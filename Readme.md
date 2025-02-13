# QuickLook GitViewer Plugin

## Overview

The QuickLook GitViewer Plugin is a plugin for the QuickLook application that allows users to view Git repository information directly within QuickLook. This plugin provides a convenient way to quickly access and visualize Git repository details without leaving the QuickLook interface.

## Features

- Detects Git repositories and displays relevant information.
- Highlights Git commands, file changes, branch information, and status information.
- Provides clickable URLs for remote repositories.
- Displays repository information in a user-friendly HTML format.

## Installation

1. Download the latest release of the QuickLook GitViewer Plugin.
2. Copy the plugin files to the QuickLook plugins directory:
```
C:\Users<YourUsername>\AppData\Local\Programs\QuickLook\QuickLook.Plugin
```
3. Restart QuickLook to load the new plugin.

## Usage

1. Open QuickLook and navigate to a directory containing a Git repository.
2. QuickLook will automatically detect the Git repository and display the relevant information.
3. Use the QuickLook interface to view the repository details, including remote URLs, status, and file changes.

## Development

### Prerequisites

- Visual Studio 2022
- .NET Framework 4.6.2

### Building the Plugin

1. Clone the repository:
```git clone```
2. Open the solution file (`.sln`) in Visual Studio 2022.
3. Build the solution to generate the plugin DLL.

### Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [QuickLook](https://github.com/QL-Win/QuickLook) - The QuickLook application.
- [Paddy Xu](https://github.com/xupefei) - Original author of QuickLook.

## Contact

For any questions or issues, please open an issue on the [GitHub repository](<repository-url>).

