## August 2024 Release (version 2.1.3)

This is a maintenance release.

### Fixes

* Updated dependencies for fixing security vulnerabilities.

---

## October 2023 Release (version 2.1.2)

This is a maintenance release.

### Fixes

* Updated dependencies for fixing security vulnerability CVE-2023-36414.

---

## August 2023 (version 2.1.1)

### Fixes

* Fixed behaviour in case the user want to use only Azure Key Vault. The absence of the the encryption key and the GitHub authorization does not block anymore the commands pull and push.
* Other minor fixes.

---

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
