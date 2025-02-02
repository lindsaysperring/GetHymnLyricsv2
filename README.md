# GetHymnLyrics v2

A cross-platform desktop application for managing and organizing hymn lyrics, built with Avalonia UI and .NET.

## Features

- **Hymn Management**: Add, edit, and delete hymns with comprehensive metadata
- **Structured Organization**: Manage hymns with detailed section organization (verses, chorus, etc.)
- **Copyright Management**: Track copyright information for both lyrics and music
- **Search Functionality**: Quickly find hymns by title or number
- **Clipboard Support**: Copy formatted hymn text to clipboard for use in other applications
- **Cross-Platform**: Runs on both Windows and macOS

## Technologies

- **Framework**: .NET
- **UI Framework**: Avalonia UI
- **Architecture**: MVVM (Model-View-ViewModel)
- **Data Storage**: XML-based storage format
- **Dependencies**: 
  - CommunityToolkit.Mvvm for MVVM implementation
  - Avalonia for cross-platform UI

## Installation

1. Download the latest release for your platform (Windows or macOS)
2. Extract the downloaded archive
3. Run the application:
   - Windows: Run `GetHymnLyricsv2.exe`
   - macOS: Run `GetHymnLyricsv2.app`

## Usage

### Managing Hymns

1. **Add a New Hymn**:
   - Click the "Add" button
   - Fill in the hymn details (title, number, authors)
   - Add sections (verses, chorus) as needed

2. **Edit a Hymn**:
   - Select a hymn from the list
   - Modify the details in the editor
   - Changes are saved automatically

3. **Delete a Hymn**:
   - Select a hymn from the list
   - Click the "Delete" button

### Working with Sections

1. **Add Sections**:
   - Select a hymn
   - Add verses, chorus, or other sections
   - Arrange sections in the desired order

2. **Copy to Clipboard**:
   - Select a hymn
   - Click "Copy to Clipboard"
   - Paste the formatted text where needed

### Search

- Use the search box to find hymns by:
  - Title
  - Hymn number

## Development

### Prerequisites

- .NET SDK 6.0 or later
- Visual Studio 2022 or Visual Studio Code

### Building from Source

1. Clone the repository
2. Open `GetHymnLyricsv2.sln` in Visual Studio or the project folder in VS Code
3. Restore NuGet packages
4. Build the solution

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.txt](LICENSE.txt) file for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a pull request
