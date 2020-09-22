# About

This is a repository for all publicly released packages, tests, and demo projects pertaining to Reflect Viewer. 

**Note that external users need to have signed an access agreement before accessing any non-public files**

**Note a valid Reflect license is required to build the Reflect Viewer.**

### Status

|Package|Latest|2019.4|unity/industrial|
|-------|------|------|-------|
|com.unity.reflect | ![ReleaseBadge](https://badges.cds.internal.unity3d.com/packages/com.unity.reflect/release-badge.svg) | ![ReleaseBadge](https://badges.cds.internal.unity3d.com/packages/com.unity.reflect/candidates-badge.svg) | ![ReleaseBadge](https://badges.cds.internal.unity3d.com/packages/com.unity.reflect/candidates-badge.svg)|

<a name="Work in progress"></a>
## Work in progress
This packages and projects are in-development and not ready for production use. The features and documentation will change before any package or demo content verified for release.

<a name="Contents"></a>
## Contents
### Packages
[com.unity.touch-framework](Packages/com.unity.touch-framework/README.md)
The touch-framework package in-development at [Official Repo](https://github.com/Unity-Technologies/VirtualProduction/blob/develop/README.md).

<a name="Installation"></a>
## Installation
This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github.

## Building Locally
[Reflect Viewer](ReflectViewer/README.md)
 - Open the ReflectViewer project
 - From the File->Build Settings, select the desired platform and build
 
## MARS and Simulator
You will be required to have MARS installed with the Simulation Environments to use the Device Simulator.  It should be installed automatically as you first open the project.

## Branching Strategy
### [develop]
The main branch where the source code always reflects a state with the latest delivered development changes for the next release, also known as the “integration branch”.

### [release]
Release branches are the release branches.

Example: 
- release/1.3.2

