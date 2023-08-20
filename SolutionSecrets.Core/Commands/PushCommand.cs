using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using SolutionSecrets.Core.Repository;

namespace SolutionSecrets.Core.Commands
{
    public class PushCommand
    {
        private readonly IEnvironment _environment;

        public PushCommand(IEnvironment environment)
        {
            _environment = environment;
        }


        public async Task Execute(string solutionFullName)
        {
            SolutionFile solution = new SolutionFile(solutionFullName);

            var secretFiles = solution.GetProjectsSecretFiles();
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
                        catch (AuthenticationFailedException ex)
                        {
                            await _environment.ShowDialogMessageAsync($"Azure authentication failed. Check your credential in\nTools -> Options -> Azure Service Authentication.");
                            await _environment.ShowStatusMessageAsync(String.Empty);
                            return;
                        }
                        catch (Exception)
                        {
                            await _environment.ShowStatusMessageAsync("Error while pushing secrets for the solution.");
                            return;
                        }
                    }

                    if (!await azureKvRepository.IsReady())
                    {
                        await _environment.ShowDialogMessageAsync($"Access denied to Azure Key Vault {azureKvRepository.RepositoryName}.");
                        await _environment.OpenOptionPageAsync(OptionPage.AzureKeyVaultOptionPage);
                        await _environment.ShowStatusMessageAsync("Error while pushing secrets for the solution.");
                        return;
                    }
                }
                else if (!await Context.Current.Cipher.IsReady() || !await repository.IsReady())
                {
                    await _environment.ShowDialogMessageAsync("You need to configure the solution secrets synchronization before using the Push command.");
                    await _environment.OpenOptionPageAsync(OptionPage.GitHubOptionPage);
                    await _environment.ShowStatusMessageAsync("Error while pushing secrets for the solution.");
                    return;
                }

                var headerFile = new HeaderFile
                {
                    visualStudioSolutionSecretsVersion = Versions.CurrentVersion.ToString(),
                    lastUpload = DateTime.UtcNow,
                    solutionFile = solution.Name,
                    solutionGuid = solution.Uid
                };

                var files = new List<(string name, string content)>
                {
                    ("secrets", JsonSerializer.Serialize(headerFile))
                };

                await _environment.ShowStatusMessageAsync($"Checking secrets for the solution: {solution.Name}...");

                var repositoryFiles = await repository.PullFilesAsync(solution);

                var secrets = new Dictionary<string, Dictionary<string, string>>();

                bool isEmpty = true;
                bool failed = false;
                foreach (var secretFile in secretFiles)
                {
                    if (secretFile.Content != null)
                    {
                        isEmpty = false;
                        bool isFileOk = true;

                        if (repository.EncryptOnClient)
                        {
                            isFileOk = secretFile.Encrypt();
                        }

                        if (isFileOk)
                        {
                            if (!secrets.ContainsKey(secretFile.ContainerName))
                            {
                                secrets.Add(secretFile.ContainerName, new Dictionary<string, string>());
                            }
                            secrets[secretFile.ContainerName].Add(secretFile.Name, secretFile.Content);
                        }
                        else
                        {
                            failed = true;
                            break;
                        }
                    }
                }

                foreach (var group in secrets)
                {
                    string groupContent = JsonSerializer.Serialize(group.Value);
                    files.Add((group.Key, groupContent));
                }

                // Merge remote files with local ones. Local files have priority.
                files.AddRange(
                    repositoryFiles.Where(rf => !files.Any(f => f.name == rf.name))
                    );

                bool isChanged = false;
                if (!isEmpty)
                {
                    if (files.Count != repositoryFiles.Count)
                    {
                        isChanged = true;
                    }
                    else
                    {
                        foreach (var file in files)
                        {
                            if (file.name == "secrets")
                            {
                                continue;
                            }

                            var fileContent = JsonSerializer.Deserialize<Dictionary<string, string>>(file.content);
                            foreach (var repositoryFile in repositoryFiles)
                            {
                                if (repositoryFile.name == file.name)
                                {
                                    var repositoryFileContent = JsonSerializer.Deserialize<Dictionary<string, string>>(repositoryFile.content);
                                    foreach (var secret in repositoryFileContent)
                                    {
                                        if (!fileContent.ContainsKey(secret.Key)
                                            || fileContent[secret.Key] != secret.Value)
                                        {
                                            isChanged = true;
                                            goto exitChangeCheck;
                                        }
                                    }
                                }
                            }
                        }
                    exitChangeCheck:;
                    }
                }

                if (!isEmpty && isChanged && !failed)
                {
                    await _environment.ShowStatusMessageAsync($"Pushing secrets to {repository.RepositoryTypeFullName} for the solution: {solution.Name}...");
                    if (!await repository.PushFilesAsync(solution, files))
                    {
                        failed = true;
                    }
                }

                if (isEmpty)
                {
                    await _environment.ShowStatusMessageAsync("Cannot find local secrets for this solution.");
                }
                else if (!isChanged)
                {
                    await _environment.ShowStatusMessageAsync("Secrets already updated.");
                }
                else if (failed)
                {
                    await _environment.ShowStatusMessageAsync("Secrets pushing has failed!");
                }
                else
                {
                    await _environment.ShowStatusMessageAsync($"Secrets pushed successfully to {repository.RepositoryTypeFullName}.");
                }
            }
            catch
            {
                await _environment.ShowStatusMessageAsync("Error while pushing secrets for the solution.");
            }
        }
    }
}
