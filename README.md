# Solution Secrets for Visual Studio

### Visual Studio extension to synchronize solution secrets across different development workstations.

Download this extension from the Visual Studio Marketplace:
* [Solution Secrets 2022](https://marketplace.visualstudio.com/items?itemName=ErnestoCianciotta.SolutionSecrets2022)
* [Solution Secrets 2019](https://marketplace.visualstudio.com/items?itemName=ErnestoCianciotta.SolutionSecrets2019)

---

Sensitive data like passwords, connection strings, access keys, etc. should not be included in the source code that is committed in a shared repository. Secrets must not be deployed with the apps, too.

For developers that use Visual Studio, since many years ago, [**User Secrets Manager**](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#secret-manager) helps you to maintain your secrets out of the solutions folder.

Solution Secrets extension for Visual Studio leverage on User Secrets Manager feature and goes further enabling secure secrets synchronization between development workstations and operating systems.

### **Why this extension?**

When you change your development workstation, you usually clone your project code from a remote repository and then you would like to be up and running for developing and testing your code in a matter of seconds.

But if you have managed secrets with the tool User Secrets Manager you will not be immediately able to test your code because you will miss something very important on your new workstation: **the secret settings** that let your code work.

Using the **Solution Secrets extension** you can be immediately ready to start developing and testing on the new workstation or you can just sync secrets between different workstations.

### **How does it work?**

This extension adds some new features to the secrets management for the entire solution. The capabilities of this extension are accessible through the *Solution Context Menu*:

<br/>

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/context-menu-1.png" width="367" />

<br/>

Selecting the menu item "Solution Secrets", will open the submenu from where you can use the main features for managing and synching secrets:

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/context-menu-2-1.png" width="607" />

Secrets are synchronized with a remote repository that can be:

* GitHub Gists
    
* Azure Key Vault
    

### **GitHub Gists**

A "Gist" is a snippet of code that can either be public or secret. Visual Studio Solution Secrets uses only **secret** Gists.

GitHub Gists is the default repository used by Visual Studio Solution Secrets for storing solutions secrets. Secrets are collected, **encrypted** and pushed on your GitHub account in a **secret Gist**, so that only you can see them. The encryption key is generated from a passphrase or a key file that you specify during the one-time initialization phase of the tool.

GitHub Gists fits very well for personal use.

![Concept](https://raw.githubusercontent.com/ernstc/VisualStudioSolutionSecrets/main/media/github-flow.png)

For configuring Visual Studio to use GitHub Gists, go to

Tools -&gt; Options -&gt; Solutions Secrets -&gt; GitHub Gists

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/screen-options-2022-2-2.png" width="607" />

From here you can authorize access to your GitHub account and you can create the encryption key because even if your secrets are saved in a secret Gist, they can be accessed by someone that acknowledges the Gist URL, hence the secrets are encrypted before they are saved in a Gist.

### **Azure Key Vault**

Azure Key Vault is a cloud service for securely storing and accessing secrets. Secrets are encrypted at rest and can be accessed only by authorized accounts. No one else can read their contents.

Since the secrets are encrypted at rest and communication with the key vault is secure because it is enforced as HTTPS / TLS 1.2, Visual Studio Solution Secrets does not encrypt the secrets by itself before sending them to the key vault, therefore it is not necessary to use the encryption key on the local machine.

This opens the scenario in which you can share the secrets of the solution with the development team. You just need to authorize the team with read or read/write access to the Azure Key Vault secrets, so that the team can pull secrets.

Azure Key Vault fits better for enterprise use and **is the recommended way for sharing solution secrets within the team.**

You can read the Azure Key Vault documentation [**here**](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)

![Concept](https://raw.githubusercontent.com/ernstc/VisualStudioSolutionSecrets/main/media/azurekv-flow.png)

For using Azure Key Vault, you must specify the key vault URL from

Tool -&gt; Options -&gt; Solution Secrets -&gt; Azure Key Vault

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/screen-options-2022-2-3.png" width="607" />

### **Default Secrets Repository**

Now that you have seen how to configure the use of GitHub Gists and Azure Key Vault as remote secrets repository, you need to specify which one of them must be used by default. This can be done by going to

Tools -&gt; Options -&gt; Solutions Secrets

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/screen-options-2022-2-1.png" width="607" />

### **Custom Settings per Solution**

Solution Secrets v2.0 let you configure a custom remote repository for your solution. For customizing the synchronization settings go to

Solution Context Menu -&gt; Solution Secrets -&gt; Synchronization Settings...

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/context-menu-2-2.png" width="607" />

The configuration dialog will let you select the repository type GitHub Gist

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/screen-config-2022-2-1.png" width="500" />

or Azure Key Vault

<img src="https://raw.githubusercontent.com/ernstc/SolutionSecretsExtension/main/Resources/screen-config-2022-2-2.png" width="500" />

### **Conclusions**

Solution Secrets v2.0 for Visual Studio is available only for Visual Studio for Windows and is based on the same code base as the Visual Studio Solution Secrets project.

Sharing secrets between Windows, macOS and Linux is possible thanks to the .NET tool ***Visual Studio Solution Secrets***.

The tool works from the command prompt and is cross-platform. Since it shares settings and format with this extension, is possible to push secrets from Windows using the Solution Secrets for Visual Studio and pull secrets from macOS or Linux using the CLI tool.

[Read the blog post for more details.](https://devnotes.ernstc.net/visual-studio-solution-secrets-v2)
