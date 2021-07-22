# Inxi.NET

![GitHub repo size](https://img.shields.io/github/repo-size/EoflaOE/Inxi.NET?color=purple&label=size) [![GitHub All Releases](https://img.shields.io/github/downloads/EoflaOE/Inxi.NET/total?color=purple&label=d/l)](https://github.com/EoflaOE/Inxi.NET/releases) [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/EoflaOE/Inxi.NET?color=purple&include_prereleases&label=github)](https://github.com/EoflaOE/Inxi.NET/releases/latest) [![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Inxi.NET?color=purple)](https://www.nuget.org/packages/Inxi.NET/)

Inxi.NET is the Linux-only hardware information frontend using Inxi as its backend for getting system information.

Please note that macOS support is very experimental, and a lot of features are not available.

## Build Status

To look at build statuses for all branches, go to the main branch (master).

## System Requirements

To run any project that use this library, we recommend that you have:

### Windows systems

1. Windows 7 or later (Windows 10 recommended)
2. .NET Framework 4.5 or later, or .NET Core 2.1 or 3.1, or .NET 5.0

* If your app uses Inxi.NET, uses x86, and uses one of .NET Core 2.1, 3.1, or .NET 5.0, at least a 2.00 GHz processor with SSE2 is required.

### Linux systems

1. Mono 5.10 or higher (6.0 or higher is recommended) or dotnet with .NET Core 2.1 or 3.1 or .NET 5.0

## How to install

This section covers how to install Inxi.NET on your project. Please scroll down to your system below.

### Windows systems (Recommended)

1. Open Visual Studio to any project, and open the NuGet package manager
2. Search for `Inxi.NET` and install it there

### Windows systems (Alternative)

1. Download the Inxi.NET library files [here](https://github.com/EoflaOE/Inxi.NET/releases).
2. Unzip the file to any directory
3. Open Visual Studio to any project, and add a reference to Inxi.NET

## How to Build

This section covers how to build Inxi.NET on your system. Please scroll down to your platform below.

### Visual Studio 2019 16.8+

1. Open Visual Studio
2. Press `Clone a repository`
3. In Repository Location, enter `https://github.com/EoflaOE/Inxi.NET.git`
4. Wait until it clones. It might take a few minutes depending on your Internet connection.
5. Press `Solution Explorer`, then press `Switch Views`
6. Click on `Inxi.NET.sln`
7. Press `Start` or press `Build > Build Solution`

### JetBrains Rider (64-bit)

1. Install Mono Runtime, Git, and `libmono-microsoft-visualbasic10.0-cil`.
2. Install JetBrains Rider.
3. After installation, open JetBrains Rider, and follow the configuration steps.
4. When the main menu opens, choose `Check out from Version Control` and then `Git`.
5. Write on the URL `https://github.com/EoflaOE/Inxi.NET.git` and press `Test` to verify your connectivity.
6. Press Clone, and git will download the repo, then Rider will open up. It might take a few minutes depending on your Internet connection.
7. Click on the hammer button to build.

### MonoDevelop

1. Install Mono Runtime, `libmono-microsoft-visualbasic10.0-cil`, and MonoDevelop.
2. After installation, extract the source code, open MonoDevelop, and click on `Open...`
3. Click on the `Build` menu bar, and click on build button to compile.

## Credits

**EoflaOE:** Owner of Inxi.NET

## Open Source Software

Below entries are the open source software that is used by Inxi.NET. They are required for execution.

### Inxi

Source code: https://github.com/smxi/inxi

Copyright (c) 2018 - present, smxi.

License (GNU GPL-3.0): https://github.com/smxi/inxi/blob/master/LICENSE.txt

### Newtonsoft.Json

Source code: https://github.com/JamesNK/Newtonsoft.Json

Copyright (c) 2007, James Newton-King

License (MIT): https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md

## License

    Inxi.NET - .NET Frontend for Inxi
    Copyright (C) 2020-2021  EoflaOE

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

