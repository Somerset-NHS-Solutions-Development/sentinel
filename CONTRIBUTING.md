# Contributing

Contributions are welcome and any help that can be offered is greatly appreciated.
Please take a moment to read the entire contributing guide.

Contributions should use the [Feature Branch Workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/feature-branch-workflow),
meaning that development should take place in `feat/` branches, with the `master` branch kept in a stable state.
When you submit pull requests, please make sure to fork from and submit back to `master`.

Please use the [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) standard for commit messages.

## Getting Started

Please see the requirements in the README file.

With those in place, you can fork the repository, clone it, and then build from Visual Studio. You will need to 
grab a copy of the appsettings.*.json files from it's current installation on ydh-watchdog, put these in the sentinel
folder (next to appsettings.json). Ensure that the appsettings.Development.json file contains the correct 
connection strings, etc. 
For development purposes you should make your own copy of the database, and alter the connection string in 
appsettings.Development.json accordingly.
It can then be tested on localhost from within Visual Studio, or deployed to a staging server if desired.

## Pull Request Checklist

Prior to submitting a pull request back to the main repository, please make sure you have completed the following steps:

1. Pull request base branch is set to `master`. All pull requests should be forked from and merged back to `master`
2. Ensure that compiler warnings/messages are minimised

## Issues

Please file your issues [here](https://gitlab.com/ydh.nhs.uk/scratch/sentinel/-/issues) and try to provide as much information in the template as possible/relevant.
