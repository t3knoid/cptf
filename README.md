# CPTF
CPTF is an acronym for Copy Test Files. It's a tool I use at work stage test files. It's function is very specific to my needs. Although settings can be modified in the app configuration file.

A typical operation is coping a corpus of files with the following path structure: 

<TestDataRepositoryRoot>\<TestData>\data

to the following:

<ProjectFolderRoot>\<ProjectFolder>\<TestData>

This tool uses RoboSharp to perform the heavy lifting of performing parallel copy using RoboCopy. One interesting part of the project is how to shutdown threads gracefully in a Win32 console application.

# Requirements
The tool requires that the settings in the application config file are set:
```xml
            <setting name="DestinationRootDir" serializeAs="String">
                <value>d:\test\src</value>
            </setting>
            <setting name="TestDataRepoRootDir" serializeAs="String">
                <value>D:\test\src\_testdata</value>
            </setting>
```

# Usage
CPTF expects two parameters, the name of the test data to copy and the project where the data is copied to:

cptf.exe -project project1 -testdata testdata1

# Downloads
Download the latest build [here](https://github.com/t3knoid/cptf/releases/tag/latest).
