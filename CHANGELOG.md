## March 2023 Release (version 2.0.0)
 This version allows the use of different types of repositories. Starting with this release, any solution can use a different repository to store its secret settings.

**Azure Key Vault** is the new supported repository. Azure Key Vault is the recommended repository to use for scenarios where the solution secrets can be shared within the development team.

### New Features

* Added **Azure Key Vault** as an alternative repository for storing solution secrets.

---

## April 2022 Release (version 1.1.2)

### Features

* Extended secrets management to ASP.NET MVC 5 projects.
* Added support to projects that share the same secret files.

---

## April 2022 Release (version 1.0.2)

Initial release.
Visual Studio extension that allows to synchronize solution secrets across different development workstations.

### Features

* Pushing encrypted secrets to a remote repository.
* Pulling ecrypted secrets from a remote repository.
* Implemented GitHub Gist as remote secrets repository.
* Deletion of user secrets for when you need to dismiss secrets from a development workstation.
