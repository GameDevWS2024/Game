# 2D Game Development Project

## Project Overview

This project is a 2D game developed as part of the Games and Visual Effects course during the winter term of 2023/2025.

---

## Setting Up for Development

To get started with the game development, follow these steps:

1. **Download Godot (Mono Version)**:  
   [Godot Download](https://godotengine.org/download)  
   *Ensure you download the Mono version for C# support.*

2. **Install .NET 8**:  
   [.NET 8 Download](https://dotnet.microsoft.com/en-us/download)

3. **Install Git**:  
   [Git Download](https://git-scm.com/downloads)  
   *If you haven’t installed it yet, make sure to do so!*

4. **Set Up SSH Key**:  
   [Setup SSH](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/checking-for-existing-ssh-keys)  
   *Ensure you have an SSH key for accessing the repository.*

5. **Install Git Large File Storage (LFS)**:  
   [Guide to Install Git LFS](https://github.com/git-lfs/git-lfs?utm_source=gitlfs_site&utm_medium=installation_link&utm_campaign=gitlfs#installing)

6. **Get a Gemini API key**
   [Get a key](https://ai.google.dev/gemini-api/docs/api-key)
---

## Useful Resources

- **Godot Documentation**:  
  [Godot Docs](https://docs.godotengine.org/en/stable/index.html)

- **C# Documentation**:  
  [C# Docs](https://learn.microsoft.com/dotnet/csharp/)

---

## GitHub LFS

### Why Use Git LFS?
Git LFS (Large File Storage) is essential for managing large files such as textures and audio assets. After installing Git LFS, you can continue using Git as usual.

### Adding New File Types to LFS
To track specific file types, use the following commands:

```bash
git lfs track "myfile.myending"
git lfs track "*.myending"
```

### Further Help
For additional assistance, you can use:
```bash
git lfs help <command>
git lfs <command> -h
```

---

## Guidelines

- **Branching**: 
  - You **cannot push** directly to the `main` branch. 
  - Create a **branch** and then submit a **Pull Request (PR)**.

- **Merging**:
  - You **cannot merge** into the `main` branch without a PR that has one or more reviews and where all checks have passed.

- **C#**
  - We decided to use C# exlusively in Godot, so you can't use gdscript.
  - [C# specific Godot Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html)

- **C# Formatting**:
  - We adhere to standard C# formatting guidelines. 
  - Format the project with:
    ```bash 
    dotnet format Game.sln
    ```
  - You may also configure your editor to format on save. Ensure that your code is properly formatted before making a PR, as unformatted code may cause tests to fail.

- **File Placement**:
  - `.tscn` (scene) files **must** be placed inside the `scenes` folder.
  - `.cs` (C# script) files **must** be placed inside the `scripts` folder.
  - Assets **must** be located within the `assets` folder.

---
