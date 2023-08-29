using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using SolutionSecrets.Core.Repository;

namespace SolutionSecrets.Core.Commands
{
    public class PullCommand
    {
        private readonly IEnvironment _environment;

        public PullCommand(IEnvironment environment)
        {
            _environment = environment;
        }


        public async Task Execute(string solutionFullName)
        {
            SolutionFile solution = new SolutionFile(solutionFullName);

            ICollection<SecretFile> secretFiles = solution.GetProjectsSecretFiles();
            if (secretFiles.Count == 0)
            {
                await _environment.ShowStatusMessageAsync("No secrets found.");
                return;
            }

            var synchronizationSettings = solution.CustomSynchronizationSettings;

            // Select the repository for the curront solution
            IRepository repository = Context.Current.GetRepository(synchronizationSettings) ?? Context.Current.Repository;

            try
            {
                await _environment.ShowStatusMessageAsync($"Checking authorization to {repository.RepositoryTypeFullName}...");

                if (repository is AzureKeyVaultRepository azureKvRepository)
                {
                    if (!await azureKvRepository.IsReady())
                    {
                        try
                        {
                            await azureKvRepository.AuthorizeAsync();
                        }
                        catch (AuthenticationFailedException)
                        {
                            await _environment.ShowDialogMessageAsync($"Azure authentication failed. Check your credential in\nTools -> Options -> Azure Service Authentication.");
                            await _environment.ShowStatusMessageAsync(String.Empty);
                            return;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            await _environment.ShowStatusMessageAsync("Unauthorized access to Azure Key Vault.");
                            return;
                        }
                        catch (Exception)
                        {
                            await _environment.ShowStatusMessageAsync("Error while pulling secrets for the solution.");
                            return;
                        }
                    }

                    if (!await azureKvRepository.IsReady())
                    {
                        await _environment.ShowDialogMessageAsync($"Access denied to Azure Key Vault {azureKvRepository.RepositoryName}.");
                        await _environment.OpenOptionPageAsync(OptionPage.AzureKeyVaultOptionPage);
                        await _environment.ShowStatusMessageAsync("Error while pulling secrets for the solution.");
                        return;
                    }
                }
                else if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
                {
                    await _environment.ShowDialogMessageAsync("You need to configure the solution secrets synchronization before using the Push command.");
                    await _environment.OpenOptionPageAsync(OptionPage.GitHubOptionPage);
                    await _environment.ShowStatusMessageAsync("Error while pulling secrets for the solution.");
                    return;
                }

                await _environment.ShowStatusMessageAsync($"Pulling secrets from {repository.RepositoryTypeFullName} for the solution: {solution.Name}...");

                var repositoryFiles = await repository.PullFilesAsync(solution);
                if (repositoryFiles.Count == 0)
                {
                    await _environment.ShowStatusMessageAsync("Failed, secrets not found.");
                    return;
                }

                // Validate header file
                HeaderFile header = null;
                foreach (var file in repositoryFiles)
                {
                    if (file.name == "secrets" && file.content != null)
                    {
                        try
                        {
                            header = JsonSerializer.Deserialize<HeaderFile>(file.content);
                        }
                        catch
                        { }
                        break;
                    }
                }

                if (header == null)
                {
                    await _environment.ShowStatusMessageAsync("Error while pulling secrets for the solution.");
                    return;
                }

                if (!header.IsVersionSupported())
                {
                    await _environment.ShowStatusMessageAsync("Secrets format is not compatible.");
                    return;
                }

                bool failed = false;
                foreach (var repositoryFile in repositoryFiles)
                {
                    if (repositoryFile.name != "secrets")
                    {
                        if (repositoryFile.content == null)
                        {
                            continue;
                        }

                        Dictionary<string, string> remoteSecretFiles = null;

                        try
                        {
                            remoteSecretFiles = JsonSerializer.Deserialize<Dictionary<string, string>>(repositoryFile.content);
                        }
                        catch
                        {
                            await _environment.ShowStatusMessageAsync("Error while pulling secrets for the solution.");
                        }

                        if (remoteSecretFiles == null)
                        {
                            failed = true;
                            break;
                        }

                        foreach (var remoteSecretFile in remoteSecretFiles)
                        {
                            string secretFileName = remoteSecretFile.Key;

                            // This check is for compatibility with version 1.0.x
                            if (secretFileName == "content")
                            {
                                secretFileName = "secrets.json";
                            }

                            foreach (var localSecretFile in secretFiles)
                            {
                                if (localSecretFile.ContainerName == repositoryFile.name
                                    && localSecretFile.Name == secretFileName)
                                {
                                    localSecretFile.Content = remoteSecretFile.Value;

                                    bool isFileOk = true;
                                    if (repository.EncryptOnClient)
                                    {
                                        isFileOk = localSecretFile.Decrypt();
                                    }

                                    if (isFileOk)
                                    {
                                        solution.SaveSecretSettingsFile(localSecretFile);
                                    }
                                    else
                                    {
                                        failed = true;
                                    }
                                    break;
                                }
                            }
                        }

                        if (failed)
                        {
                            break;
                        }
                    }
                }

                if (!failed)
                    await _environment.ShowStatusMessageAsync($"Secrets pulled successfully from {repository.RepositoryTypeFullName}.");
                else
                    await _environment.ShowStatusMessageAsync("Secrets pull has failed!");
            }
            catch
            {
                await _environment.ShowStatusMessageAsync("Error while pulling secrets for the solution.");
            }
        }
    }
}
