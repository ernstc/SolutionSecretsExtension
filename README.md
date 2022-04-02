# Solution Secrets

Visual Studio extension for helping to manage the solution secrets.

Download this extension from the Visual Studio Marketplace:
* [Solution Secrets 2022](https://marketplace.visualstudio.com/items?itemName=ErnestoCianciotta.SolutionSecrets2022)
* [Solution Secrets 2019](https://marketplace.visualstudio.com/items?itemName=ErnestoCianciotta.SolutionSecrets2019)

---

As a good practices, secrets (sensitive data like passwords, connection strings, access keys, etc.) must not be committed with your code in any case and must not be deployed with the apps. That's why we must use the ***User Secrets Manager*** feature in Visual Studio that let us store secrets out of the solution folder. The User Secrets Manager hides implementation details, but essentially it stores secrets in files located in the workstation's user profile folder.

This extension adds some new features to the secrets management for the entire solution. The capabilities of this extension are accessible trough the *Solution Context Menu*:

<br/>

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/context-menu-1.png" width="367" />

<br/>

# Features

* Solution secrets synchronization across user's development workstations.
* Deletion of user secrets for when you need to dismiss secrets from a development workstation.

<br/>

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/context-menu-2.png" width="607" />

<br/>

# Why this extension?

When you change your development workstation usually you clone your project code from a remote repository and then you would like to be up and running for developing and testing you code in a matter of seconds.

But if you have managed secrets with the tool User Secrets Manager you will not be immediatly able to test your code because you will miss something very important on your new workstation: **the secret settings** that let your code work.

Using **Solution Secrets Extension** you can be immediatley ready to start developing and testing on the new workstation or you can just sync secrets between different  workstations.


# Secrets synchronization
The synchronization is implemented through a cloud repository that in this case is GitHub Gist. The secrets are encrypted and pushed on GitHub in a secret Gist, so that only you can see them. 

![Concept](https://raw.githubusercontent.com/ernstc/VisualStudioSolutionSecrets/main/Concept.png)

The encryption key is generated from a passphrase or a key file.
Once you change the development workstation, you don't have to copy any file from the old one. Just install the extension, recreate the encryption key with your passphrase or your key file, authorize access to GitHub Gists and you are ready.

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/screen-config-2022.png" width="431" />

<br/>

# Managing secrets from CLI

The **Solution Secrets** extension help you manage secrets from the Visual Studio IDE, but it is possible to manage secrets also from the CLI thanks to the .NET tool ***Visual Studio Solution Secrets*** documented [here](https://devnotes.ernstc.net/visual-studio-solution-secrets).

The extension and the .NET tool share the same settings, so you can switch from the extension to the tool and vice versa.

# Cross platform secrets sharing

Sharing secrets between Windows, macOS and Linux is possible thanks to the .NET tool ***Visual Studio Solution Secrets***. The tool is cross platform and since it shares settings and format with this extension, is possible to push secrets from Windows using the Solution Secrets extension and pulling the secrets from macOS or Linux using the CLI tool.

<br/>

---
(*) You can find the **User Secrets Manager** documentation [here](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#secret-manager).