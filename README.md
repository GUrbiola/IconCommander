# IconCommander

A powerful Windows Forms desktop application for managing, organizing, and exporting icon and image libraries with database-driven tagging, searching, and project integration capabilities.

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-purple.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Usage Guide](#usage-guide)
- [Configuration](#configuration)
- [Database Schema](#database-schema)
- [Architecture](#architecture)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

---

## Overview

IconCommander is a comprehensive icon library management system that helps developers organize thousands of icons across multiple collections, tag them for easy discovery, and export them directly to their development projects. The application uses SQLite for efficient storage and retrieval, supports bulk imports with automatic tagging, and provides advanced features like icon merging and project integration.

### Core Concepts

- **Collections**: Top-level groupings for icon sets (e.g., "Font Awesome", "Material Design")
- **Veins**: Folder-based sources of icons within collections (linked to filesystem directories)
- **Icons**: Logical icon entities that may have multiple file formats and sizes
- **IconFiles**: Individual icon files with binary data, dimensions, and metadata
- **Tags**: Searchable keywords automatically extracted from filenames or JSON mappings
- **Projects**: Development projects (Windows or Web) where icons can be exported

---

## Key Features

### Icon Library Management
- **Database-Driven Organization**: All icons stored in SQLite with rich metadata
- **Hierarchical Structure**: Collections → Veins → Icons → IconFiles
- **Binary Storage**: Icons stored as BLOBs with SHA256 deduplication
- **Multi-Format Support**: .ico, .png, .jpg, .bmp, .gif, .svg, .tiff, .wmf, .emf
- **Automatic Tagging**: Extract tags from filenames or JSON mapping files

### Bulk Import System
- **Recursive Directory Scanning**: Import entire folder hierarchies
- **Progress Tracking**: Real-time import progress with BackgroundWorker
- **JSON Keyword Mapping**: Support for Iconex format keyword definitions
- **Duplicate Detection**: SHA256 hash-based deduplication
- **SVG Support**: Parse SVG XML for accurate dimension extraction
- **Cancellable Operations**: Cancel long-running imports

### Project Integration
- **Windows Projects**: Export to resource files (.resx) and project folders
- **Web Projects**: Export to web resource directories
- **Auto-Update Project Files**: Automatically update .csproj with new icons
- **Export Tracking**: Record all exports in ProjectIcons table

### Icon Manipulation
- **Icon Merging**: Combine icons with configurable overlay positions
- **Multiple Sizes**: Support for icons in various dimensions
- **Format Conversion**: Convert between different image formats
- **Preview System**: View icons before export

### UI Features
- **Theme Support**: Multiple UI themes with persistence (ZidThemes)
- **Dynamic Forms**: Auto-generated CRUD forms with UIGenerator
- **Enhanced Grids**: Sortable, filterable data grids (ZidGrid)
- **Breadcrumb Navigation**: Folder navigation with AddressBar control

---

## Installation

### Prerequisites

- **Operating System**: Windows 7 or later
- **.NET Framework**: 4.8 or higher
- **Visual Studio**: 2019 or later (for development)
- **NuGet**: Package manager (integrated with Visual Studio)

### Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/IconCommander.git
   cd IconCommander
   ```

2. **Restore NuGet Packages**
   ```bash
   nuget restore IconCommander.sln
   ```

3. **Build the Solution**
   ```bash
   msbuild IconCommander.sln /p:Configuration=Release /v:minimal
   ```

4. **Run the Application**
   ```bash
   .\IconCommander\bin\Release\IconCommander.exe
   ```

### First-Time Setup

On first launch, IconCommander will:
1. Create a default SQLite database (`Icons.db`)
2. Initialize the database schema with all required tables
3. Apply the default UI theme
4. Create configuration entries in `App.config`

---

## Getting Started

### Quick Start Guide

1. **Create a Collection**
   - Navigate to `Icons → Collections → Manage Collections`
   - Click the Insert button
   - Enter collection name and description
   - Click Save

2. **Import Icons from a Folder**
   - Navigate to `Icons → Veins → Import Vein`
   - Select an existing collection or create a new one
   - Enter a name for the vein
   - Browse to the folder containing your icons
   - (Optional) Provide a JSON tags file for advanced keyword mapping
   - Click Import and wait for completion

3. **Create a Project**
   - Navigate to `Project → Create Project`
   - Enter project details (name, path, type)
   - Configure export settings
   - Click Save

4. **Browse and Export Icons**
   - Browse icons from the main window
   - Select an icon and size
   - Click Export to send to your active project

---

## Usage Guide

### Managing Collections

Collections are the top-level organizational units for your icon libraries.

**Creating a Collection:**
```
Menu: Icons → Collections → Manage Collections
Button: Insert
Fields:
  - Name (required): Display name for the collection
  - Description: Detailed description
  - Comments: Internal notes
  - License: Associated license (optional)
```

**Editing a Collection:**
- Select a collection from the grid
- Click Update
- Modify fields and click Save

**Deleting a Collection:**
- Select a collection from the grid
- Click Delete
- Confirm deletion (warning: cascades to veins and icons)

### Managing Veins

Veins represent folder-based icon sources linked to your filesystem.

**Creating a Vein:**
```
Menu: Icons → Veins → Manage Veins
Button: Insert
Fields:
  - Collection (required): Parent collection (dropdown)
  - Name (required): Vein display name
  - Description: Detailed description
  - Path (required): Folder path on disk
  - IsIcon: Contains .ico files
  - IsImage: Contains image files (.png, .jpg, etc.)
  - IsSvg: Contains .svg files
```

**Best Practices:**
- Use descriptive vein names that reflect the icon style or source
- Ensure the Path points to a valid directory
- Set the correct file type flags (IsIcon, IsImage, IsSvg)

### Importing Icons

The import system supports bulk importing with automatic tagging.

**Basic Import (Filename-Based Tags):**
1. Navigate to `Icons → Veins → Import Vein`
2. Select a vein from the dropdown
3. The vein path will be loaded automatically
4. Review the folder tree to verify contents
5. Click Import
6. Monitor progress in the log window

**Advanced Import (JSON Keyword Mapping):**
1. Prepare a JSON tags file in Iconex format:
```json
{
  "From": "iconex_keywords",
  "To": "IconCommander",
  "Mappings": [
    {
      "Text": "account",
      "References": [123, 456, 789]
    },
    {
      "Text": "user",
      "References": [123, 456]
    }
  ]
}
```
2. In the Import Vein dialog, click Browse next to Tags File
3. Select your JSON file
4. The system will load keywords into temporary tables
5. Click Import
6. Icons will be tagged using the JSON mappings

**Import Process Details:**
- All files are scanned recursively
- Binary data is read and stored in the database
- SHA256 hashes are calculated for deduplication
- Duplicate files (same hash) are skipped
- For SVG files, dimensions are extracted from XML attributes
- For images, dimensions are extracted using System.Drawing.Image
- Tags are extracted from JSON or filename (split by space, underscore, hyphen)
- Progress is reported every 100 files or 1 second
- Operations can be cancelled at any time

### Working with Projects

Projects represent your development projects where icons will be exported.

**Creating a Windows Project:**
```
Menu: Project → Create Project
Fields:
  - Name: Project display name
  - Path: Root project directory
  - Type: "Windows"
  - ResourceFile: Path to .resx file (e.g., Properties\Resources.resx)
  - ResourceFolder: Folder for icon files (e.g., Resources\Icons)
  - SaveIconsTo: "Folder", "File", or "Both"
  - ProjectFile: Path to .csproj file
  - UpdateProjectFile: 1 (auto-update) or 0 (manual)
```

**Creating a Web Project:**
```
Fields:
  - Name: Project display name
  - Path: Root project directory
  - Type: "Web"
  - ResourceFolder: Web resource path (e.g., wwwroot/images/icons)
  - ProjectFile: Path to .csproj file
  - UpdateProjectFile: 1 (auto-update) or 0 (manual)
```

**Export Behavior:**

For **Windows Projects**:
1. Icon file is copied to `Path + ResourceFolder`
2. If `UpdateProjectFile = 1`, the .csproj is updated with a `<Content Include="...">` entry
3. If `ResourceFile` is defined, the icon is added to the .resx file as an embedded resource
4. Export is recorded in ProjectIcons table

For **Web Projects**:
1. Icon file is copied to `Path + ResourceFolder`
2. If `UpdateProjectFile = 1`, the .csproj is updated with a `<Content Include="...">` entry
3. Export is recorded in ProjectIcons table

### Merging Icons

The merge feature allows you to combine two icons (e.g., adding a badge overlay).

**Merge Process (Planned Feature):**
1. Select a base icon (BigIcon)
2. Select an overlay icon (SmallIcon)
3. Choose overlay position:
   - TopLeft, Top, TopRight
   - Left, Center, Right
   - BottomLeft, Bottom, BottomRight
4. Preview the merged result
5. Save the merged icon to the database
6. The merge recipe is recorded in MergeRecipes table

**Position Calculation:**
```
TopLeft:      (0, 0)
Top:          (centerX - smallWidth/2, 0)
TopRight:     (bigWidth - smallWidth, 0)
Left:         (0, centerY - smallHeight/2)
Center:       (centerX - smallWidth/2, centerY - smallHeight/2)
Right:        (bigWidth - smallWidth, centerY - smallHeight/2)
BottomLeft:   (0, bigHeight - smallHeight)
Bottom:       (centerX - smallWidth/2, bigHeight - smallHeight)
BottomRight:  (bigWidth - smallWidth, bigHeight - smallHeight)
```

### Searching Icons

**Search Capabilities (Planned Feature):**
- Text search by icon name
- Filter by tags (multi-select)
- Filter by collection
- Filter by vein
- Filter by file extension
- Filter by size range
- Combine multiple filters

**Search Performance:**
The database includes optimized indexes for fast searching:
- `idx_IconTags` on `IconTags(Tag)` - Fast tag lookup
- `idx_IconTags2` on `IconTags(Icon)` - Fast icon lookup
- `idx_IconTags3` on `IconTags(Icon, Tag)` - Composite index
- `idx_IconFiles` on `IconFiles(Icon)` - Fast file lookup

---

## Configuration

### App.config Settings

**Location:** `IconCommander\App.config`

```xml
<configuration>
  <appSettings>
    <!-- UI theme selection -->
    <add key="SelectedTheme" value="Default" />
    <!-- Database file path -->
    <add key="DatabaseFile" value="C:\Path\To\Icons.db" />
  </appSettings>

  <connectionStrings>
    <add name="IconDatabase"
         connectionString="Data Source=Icons.db;Version=3;DefaultTimeout=5000;SyncMode=Off;JournalMode=Memory" />
  </connectionStrings>
</configuration>
```

### Connection String Parameters

| Parameter | Value | Purpose |
|-----------|-------|---------|
| `Data Source` | Path to .db file | SQLite database location |
| `Version` | 3 | SQLite version |
| `DefaultTimeout` | 5000 | Command timeout in milliseconds |
| `SyncMode` | Off | Skip fsync for faster writes (less durability) |
| `JournalMode` | Memory | Keep journal in RAM (faster, less safe) |
| `ReadOnly` | false | Allow write operations |
| `FailIfMissing` | false | Create database if it doesn't exist |

### Performance Tuning

**For Import Operations:**
- `SyncMode=Off`: Disables fsync calls (2-3x faster writes)
- `JournalMode=Memory`: Keeps rollback journal in RAM
- Trade-off: Lower durability in case of power failure

**For Production Use:**
- `SyncMode=Normal`: Standard durability
- `JournalMode=Delete`: Standard journaling
- Trade-off: Slower writes, safer data

### Theme Configuration

Available themes (from ZidThemes):
- Default
- Blue
- Dark
- Light
- Green
- Purple

Change theme via:
```
Menu: View → Theme → [Select Theme]
```

Theme preference is saved to `App.config` and persists across sessions.

---

## Database Schema

### Core Tables

#### Projects
Stores development project configurations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Name | TEXT | NOT NULL | Project display name |
| Path | TEXT | NOT NULL | Root project directory |
| Type | TEXT | DEFAULT 'Web' | "Web" or "Windows" |
| ResourceFile | TEXT | | Path to .resx file (Windows only) |
| ResourceFolder | TEXT | | Resource folder within project |
| SaveIconsTo | TEXT | DEFAULT 'Folder' | "Folder", "File", or "Both" |
| ProjectFile | TEXT | | Path to .csproj file |
| UpdateProjectFile | INTEGER | DEFAULT 1 | Auto-update project file flag (1=yes, 0=no) |

#### Collections
Top-level groupings for icon sets.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Name | TEXT | NOT NULL | Collection name |
| Description | TEXT | | Long description |
| Comments | TEXT | | Internal notes |
| License | INTEGER | FOREIGN KEY → Licenses(Id) | License reference |

#### Veins
Folder-based icon sources within collections.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Collection | INTEGER | NOT NULL, FOREIGN KEY → Collections(Id) | Parent collection |
| Name | TEXT | NOT NULL | Vein name |
| Description | TEXT | | Description |
| Path | TEXT | NOT NULL | Folder path on disk |
| IsIcon | INTEGER | DEFAULT 0 | Contains .ico files |
| IsImage | INTEGER | DEFAULT 1 | Contains image files |
| IsSvg | INTEGER | DEFAULT 0 | Contains .svg files |

#### Icons
Logical icon entities (may have multiple files/sizes).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Name | TEXT | NOT NULL | Icon name (typically filename) |
| Sizes | TEXT | | Available sizes (comma-separated) |
| Ico | INTEGER | | Has .ico format |
| IcoLegacy | INTEGER | | Has legacy .ico format |
| Svg | INTEGER | | Has .svg format |
| Vein | INTEGER | NOT NULL, FOREIGN KEY → Veins(Id) | Parent vein |

#### IconFiles
Individual icon files with binary data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Icon | INTEGER | NOT NULL, FOREIGN KEY → Icons(Id) | Parent icon |
| FileName | TEXT | NOT NULL | File name without extension |
| Extension | TEXT | NOT NULL | File extension with dot |
| Type | TEXT | DEFAULT 'Image' | "Icon", "SVG", "Image", or "*" |
| Size | INTEGER | NOT NULL | Width × Height |
| OriginalPath | TEXT | | Source file path |
| BinData | BLOB | | Binary file content |
| IsMerged | INTEGER | DEFAULT 0 | Created via merge |
| Hash | TEXT | NOT NULL | SHA256 for deduplication |

**Index:** `idx_IconFiles` ON `IconFiles(Icon)`

#### IconTags
Searchable tags for icons.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Icon | INTEGER | NOT NULL, FOREIGN KEY → Icons(Id) | Icon reference |
| Tag | TEXT | NOT NULL | Tag text (lowercase recommended) |

**Indexes:**
- `idx_IconTags` ON `IconTags(Tag)` - Fast tag lookup
- `idx_IconTags2` ON `IconTags(Icon)` - Fast icon lookup
- `idx_IconTags3` ON `IconTags(Icon, Tag)` - Composite index

#### IconBuffer
Temporary working space for selected icons.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| IconFile | INTEGER | NOT NULL, FOREIGN KEY → IconFiles(Id) | Icon file reference |
| CreationDate | TEXT | | ISO8601 timestamp |

#### ProjectIcons
Track icon exports to projects.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Project | INTEGER | FOREIGN KEY → Projects(Id) | Target project |
| IconFile | INTEGER | NOT NULL, FOREIGN KEY → IconFiles(Id) | Exported icon file |
| SentToProject | INTEGER | | Export success flag (1=success, 0=failed) |
| DateSent | TEXT | | ISO8601 timestamp |

#### MergeRecipes
Record of merged icon configurations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| BigIcon | INTEGER | NOT NULL, FOREIGN KEY → IconFiles(Id) | Base icon file |
| SmallIcon | INTEGER | NOT NULL, FOREIGN KEY → IconFiles(Id) | Overlay icon file |
| SmallIconPosition | TEXT | NOT NULL | Position: "Top", "TopRight", "Right", "BottomRight", "Bottom", "BottomLeft", "Left", "TopLeft", "Center" |
| IconResult | INTEGER | FOREIGN KEY → IconFiles(Id) | Resulting merged icon file |

#### Licenses
License definitions for icon collections.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| Name | TEXT | NOT NULL | License name (e.g., "MIT", "CC BY 4.0") |
| Description | TEXT | NOT NULL | Full license text |

#### MergedLicenses
Track licenses for merged icons (may inherit multiple).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY | Unique identifier |
| IconFile | INTEGER | NOT NULL, FOREIGN KEY → IconFiles(Id) | Merged icon file |
| License | INTEGER | NOT NULL, FOREIGN KEY → Licenses(Id) | License reference |

### Temporary Import Tables

These tables are used during bulk import operations and cleared afterward:

#### KeywordBuffer
Temporary keyword storage during JSON import.

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER NOT NULL | Keyword ID from JSON |
| Keyword | TEXT NOT NULL | Keyword text |

#### IconKeywordBuffer
Temporary keyword-icon relationships during import.

| Column | Type | Description |
|--------|------|-------------|
| IconBuffer | INTEGER NOT NULL | Icon buffer ID |
| KeywordBuffer | INTEGER NOT NULL | Keyword buffer ID |

---

## Architecture

### Design Patterns

#### 1. Response Pattern
All database operations return `SqliteResponse<T>` objects:

```csharp
public class SqliteResponse<T>
{
    public T Result { get; set; }                    // Single result
    public List<T> Results { get; set; }             // Multiple results
    public List<ErrorOnResponse> Errors { get; set; }
    public bool IsOK => Errors == null || Errors.Count == 0;
    public bool IsFailure => !IsOK;
    public DateTime ExecutionTime { get; set; }
    public int RowsAffected { get; set; }
    public TimeSpan ExecutionLapse { get; set; }
}
```

**Usage Example:**
```csharp
var response = connector.ExecuteTable("SELECT * FROM Icons WHERE Vein = @vein",
    new SQLiteParameter("@vein", veinId));

if (response.IsOK)
{
    DataTable icons = response.Result;
    // Process data
    Console.WriteLine($"Loaded {response.RowsRead} icons in {response.ExecutionLapse.TotalMilliseconds}ms");
}
else
{
    foreach (var error in response.Errors)
        MessageBox.Show($"Error: {error.Message}");
}
```

#### 2. UIGenerator Pattern
Dynamic form generation for CRUD operations:

```csharp
// Get table schema
var schemaResponse = connector.ExecuteTable("SELECT * FROM Collections LIMIT 0");
DataTable schema = schemaResponse.Result;

// Configure form generator
UIGenerator formGen = new UIGenerator(schema, "Collections");
formGen.SetPrimaryKey("Id");
formGen.SetExclusion("Id");  // Hide from form
formGen.SetRequired("Name");
formGen.SetFieldType("Description", FieldType.MultilineText);
formGen.SetForeignKey("License", licensesTable);

// Show insert dialog
if (formGen.ShowInsertDialog() == DialogResult.OK)
{
    string sql = formGen.GetInsertScript();
    var result = connector.ExecuteNonQuery(sql);
    if (result.IsOK)
        LoadCollections();  // Refresh grid
}

// Show update dialog
if (formGen.ShowUpdateDialog(selectedRow) == DialogResult.OK)
{
    string sql = formGen.GetUpdateScript();
    connector.ExecuteNonQuery(sql);
}
```

#### 3. Theme Management Pattern
Consistent theming across all forms:

```csharp
// In Form Designer
private ThemeManager themeManager1;

// In Form_Load
private void Form_Load(object sender, EventArgs e)
{
    // Load saved theme
    string themeName = ConfigurationManager.AppSettings["SelectedTheme"];
    themeManager1.Theme = (ZidThemes)Enum.Parse(typeof(ZidThemes), themeName);
    themeManager1.ApplyTheme();
}

// Save theme preference
private void SaveTheme(ZidThemes selectedTheme)
{
    Configuration config = ConfigurationManager.OpenExeConfiguration(
        ConfigurationUserLevel.None);
    config.AppSettings.Settings["SelectedTheme"].Value = selectedTheme.ToString();
    config.Save();
    ConfigurationManager.RefreshSection("appSettings");
}
```

#### 4. BackgroundWorker Pattern
Asynchronous operations with progress reporting:

```csharp
// Configure BackgroundWorker
bgWorker.WorkerReportsProgress = true;
bgWorker.WorkerSupportsCancellation = true;
bgWorker.DoWork += bgWorker_DoWork;
bgWorker.ProgressChanged += bgWorker_ProgressChanged;
bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

// DoWork handler (runs on background thread)
private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
{
    for (int i = 0; i < items.Count; i++)
    {
        // Check for cancellation
        if (bgWorker.CancellationPending)
        {
            e.Cancel = true;
            return;
        }

        ProcessItem(items[i]);

        // Report progress
        int percentComplete = (i + 1) * 100 / items.Count;
        bgWorker.ReportProgress(percentComplete, $"Processed {i + 1} of {items.Count}");
    }
}

// ProgressChanged handler (runs on UI thread)
private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
{
    progressBar.Value = e.ProgressPercentage;
    statusLabel.Text = e.UserState.ToString();
}

// RunWorkerCompleted handler (runs on UI thread)
private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
{
    if (e.Cancelled)
        MessageBox.Show("Operation cancelled");
    else if (e.Error != null)
        MessageBox.Show($"Error: {e.Error.Message}");
    else
        MessageBox.Show("Operation completed successfully");
}
```

#### 5. Parameterized Query Pattern
SQL injection prevention and BLOB handling:

```csharp
// Simple parameters
string sql = "SELECT * FROM Icons WHERE Vein = @vein AND Name LIKE @name";
var parameters = new[]
{
    new SQLiteParameter("@vein", veinId),
    new SQLiteParameter("@name", $"%{searchTerm}%")
};
var response = connector.ExecuteTable(sql, parameters);

// Binary data parameters
string insertSql = @"INSERT INTO IconFiles
    (Icon, FileName, Extension, BinData, Hash)
    VALUES (@icon, @fileName, @extension, @binData, @hash)";

var insertParams = new[]
{
    new SQLiteParameter("@icon", iconId),
    new SQLiteParameter("@fileName", fileName),
    new SQLiteParameter("@extension", extension),
    new SQLiteParameter("@binData", DbType.Binary) { Value = binData },
    new SQLiteParameter("@hash", hash)
};
connector.ExecuteNonQuery(insertSql, insertParams);
```

### Component Architecture

```
MainForm (Application Root)
├── SqliteConnector (Database Layer)
│   ├── SqliteResponse<T> (Response Pattern)
│   ├── Transaction Management
│   └── Logging Infrastructure
│
├── ThemeManager (UI Theming)
│   └── ZidThemes Engine
│
├── Child Forms
│   ├── CollectionsForm (CRUD)
│   │   ├── ZidGrid (Data Display)
│   │   └── UIGenerator (Dynamic Forms)
│   │
│   ├── VeinsForm (CRUD)
│   │   ├── ZidGrid
│   │   └── UIGenerator
│   │
│   └── VeinImport (Bulk Import)
│       ├── BackgroundWorker
│       ├── AddressBar (Navigation)
│       ├── TreeView (Folder Structure)
│       └── Progress Reporting
│
└── Models
    ├── Project
    ├── Mapping
    ├── IconexMapper
    └── ComboboxItem
```

---

## Development

### Building from Source

**Prerequisites:**
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- NuGet Package Manager

**Build Commands:**
```bash
# Restore packages
nuget restore IconCommander.sln

# Build Debug
msbuild IconCommander.sln /p:Configuration=Debug /v:minimal

# Build Release
msbuild IconCommander.sln /p:Configuration=Release /v:minimal

# Clean
msbuild IconCommander.sln /t:Clean /p:Configuration=Debug
```

### Project Structure

```
IconCommander/
├── Program.cs                     # Application entry point
├── MainForm.cs                    # Main window (370 lines)
├── ComboboxItem.cs                # Helper class
│
├── DataAccess/
│   ├── SqliteConnector.cs         # Database layer (1,546 lines)
│   └── SqliteConnector.Custom.cs  # Icon queries (256 lines)
│
├── Forms/
│   ├── CollectionsForm.cs         # Collections CRUD (266 lines)
│   ├── VeinsForm.cs               # Veins CRUD (352 lines)
│   └── VeinImport.cs              # Import wizard (673 lines)
│
├── Models/
│   ├── Project.cs
│   ├── Mapping.cs
│   └── IconexMapper.cs
│
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.resx
│   └── Settings.settings
│
└── Resources/
    └── [UI Icons: Bookmark.png, Search.png, etc.]
```

### Adding a New Form

1. **Create the form in Visual Studio Designer**
   - Right-click `Forms` folder → Add → Form (Windows Forms)
   - Design UI in Visual Studio Designer

2. **Add ThemeManager component**
   - Drag `ThemeManager` from Toolbox to form
   - Set name to `themeManager1`

3. **Apply theme in Form_Load**
   ```csharp
   private void MyForm_Load(object sender, EventArgs e)
   {
       string themeName = ConfigurationManager.AppSettings["SelectedTheme"];
       themeManager1.Theme = (ZidThemes)Enum.Parse(typeof(ZidThemes), themeName);
       themeManager1.ApplyTheme();
   }
   ```

4. **Pass SqliteConnector from MainForm**
   ```csharp
   public class MyForm : Form
   {
       private SqliteConnector connector;

       public MyForm(SqliteConnector connector)
       {
           InitializeComponent();
           this.connector = connector;
       }
   }

   // In MainForm
   MyForm form = new MyForm(this.connector);
   form.ShowDialog();
   ```

### Adding Database Methods

Add custom methods to `SqliteConnector.Custom.cs`:

```csharp
public partial class SqliteConnector
{
    public SqliteResponse<int> MyTable_Insert(string field1, string field2)
    {
        string sql = "INSERT INTO MyTable (Field1, Field2) VALUES (@field1, @field2)";
        var parameters = new[]
        {
            new SQLiteParameter("@field1", field1),
            new SQLiteParameter("@field2", field2)
        };
        return ExecuteNonQuery(sql, parameters);
    }

    public SqliteResponse<DataTable> MyTable_GetAll()
    {
        string sql = "SELECT * FROM MyTable ORDER BY Field1";
        return ExecuteTable(sql);
    }

    public SqliteResponse<int> MyTable_Update(int id, string field1, string field2)
    {
        string sql = @"UPDATE MyTable
                       SET Field1 = @field1, Field2 = @field2
                       WHERE Id = @id";
        var parameters = new[]
        {
            new SQLiteParameter("@id", id),
            new SQLiteParameter("@field1", field1),
            new SQLiteParameter("@field2", field2)
        };
        return ExecuteNonQuery(sql, parameters);
    }

    public SqliteResponse<int> MyTable_Delete(int id)
    {
        string sql = "DELETE FROM MyTable WHERE Id = @id";
        return ExecuteNonQuery(sql, new SQLiteParameter("@id", id));
    }
}
```

### Working with Binary Data

**Save binary file to database:**
```csharp
// Read file
byte[] binData = File.ReadAllBytes(filePath);
string hash = Crypter.ToSHA256(filePath);

// Check for duplicates
var existingId = connector.GetIconFileId(hash);
if (existingId.IsOK && existingId.Result != null)
{
    Console.WriteLine($"File already exists with ID {existingId.Result}");
    return;
}

// Save new file
string sql = @"INSERT INTO IconFiles
    (Icon, FileName, Extension, BinData, Hash, Size)
    VALUES (@icon, @fileName, @extension, @binData, @hash, @size)";

var parameters = new[]
{
    new SQLiteParameter("@icon", iconId),
    new SQLiteParameter("@fileName", Path.GetFileNameWithoutExtension(filePath)),
    new SQLiteParameter("@extension", Path.GetExtension(filePath)),
    new SQLiteParameter("@binData", DbType.Binary) { Value = binData },
    new SQLiteParameter("@hash", hash),
    new SQLiteParameter("@size", binData.Length)
};

var response = connector.ExecuteNonQuery(sql, parameters);
```

**Load binary file from database:**
```csharp
var response = connector.ExecuteTable(
    "SELECT BinData, Extension FROM IconFiles WHERE Id = @id",
    new SQLiteParameter("@id", iconFileId));

if (response.IsOK && response.Result.Rows.Count > 0)
{
    DataRow row = response.Result.Rows[0];
    byte[] binData = (byte[])row["BinData"];
    string extension = row["Extension"].ToString();

    // Display in PictureBox
    using (MemoryStream ms = new MemoryStream(binData))
    {
        Image img = Image.FromStream(ms);
        pictureBox1.Image = img;
    }

    // Or save to file
    File.WriteAllBytes($"exported_icon{extension}", binData);
}
```

### Testing Import Operations

**Test with sample folder structure:**
```
D:\TestIcons\
├── MaterialDesign\
│   ├── account.png
│   ├── settings.png
│   └── home.svg
└── FontAwesome\
    ├── fa-user.png
    ├── fa-gear.png
    └── fa-house.png
```

**Steps:**
1. Create a Collection: "Test Collection"
2. Create a Vein: "MaterialDesign" → Path: `D:\TestIcons\MaterialDesign`
3. Import Vein → Select vein → Import
4. Verify in database:
   ```sql
   SELECT i.Name, COUNT(if.Id) as FileCount, GROUP_CONCAT(t.Tag) as Tags
   FROM Icons i
   LEFT JOIN IconFiles if ON i.Id = if.Icon
   LEFT JOIN IconTags t ON i.Id = t.Icon
   WHERE i.Vein = (SELECT Id FROM Veins WHERE Name = 'MaterialDesign')
   GROUP BY i.Id
   ```

---

## Troubleshooting

### Common Issues

#### Database Locked
**Problem:** "Database is locked" error during operations

**Causes:**
- Another process has the database open
- Transaction not committed
- Background operation in progress

**Solutions:**
```csharp
// Increase timeout in connection string
SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder
{
    DefaultTimeout = 10000,  // Increase to 10 seconds
    // ...
};

// Ensure transactions are committed
try
{
    BeginTransaction();
    // Operations
    CommitTransaction();
}
catch
{
    RollbackTransaction();
    throw;
}
```

#### Import Fails on Large Directories
**Problem:** Import hangs or crashes with many files

**Causes:**
- Memory exhaustion from loading all files
- No progress reporting for large batches

**Solutions:**
- Process files in batches of 1000
- Report progress more frequently
- Use streaming for large files:
```csharp
using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
{
    byte[] buffer = new byte[fs.Length];
    fs.Read(buffer, 0, buffer.Length);
    // Process buffer
}
```

#### SVG Dimension Parsing Fails
**Problem:** SVG files show Size = 0

**Causes:**
- Missing width/height attributes
- Viewbox-only SVG files

**Solutions:**
```csharp
XDocument xdoc = XDocument.Load(filePath);
XElement svgElement = xdoc.Root;

// Try width/height attributes
var widthAttr = svgElement.Attribute("width")?.Value.Replace("px", "");
var heightAttr = svgElement.Attribute("height")?.Value.Replace("px", "");

if (string.IsNullOrEmpty(widthAttr) || string.IsNullOrEmpty(heightAttr))
{
    // Fall back to viewBox
    var viewBox = svgElement.Attribute("viewBox")?.Value;
    if (!string.IsNullOrEmpty(viewBox))
    {
        var parts = viewBox.Split(' ');
        if (parts.Length == 4)
        {
            widthAttr = parts[2];
            heightAttr = parts[3];
        }
    }
}

int width = int.Parse(widthAttr ?? "0");
int height = int.Parse(heightAttr ?? "0");
```

#### Theme Not Applying
**Problem:** Form shows default theme instead of saved theme

**Causes:**
- Theme not applied in Form_Load
- ThemeManager component missing
- Invalid theme name in config

**Solutions:**
```csharp
// Verify theme name is valid
private void LoadTheme()
{
    try
    {
        string themeName = ConfigurationManager.AppSettings["SelectedTheme"];
        if (Enum.TryParse(themeName, out ZidThemes theme))
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();
        }
        else
        {
            // Fall back to default
            themeManager1.Theme = ZidThemes.Default;
            themeManager1.ApplyTheme();
        }
    }
    catch
    {
        themeManager1.Theme = ZidThemes.Default;
        themeManager1.ApplyTheme();
    }
}
```

#### JSON Import Not Working
**Problem:** JSON tags file doesn't populate tags

**Causes:**
- Invalid JSON format
- Incorrect mapping structure
- Temporary tables not cleared

**Solutions:**
```csharp
// Validate JSON structure
try
{
    string json = File.ReadAllText(jsonPath);
    IconexMapper mapper = JsonConvert.DeserializeObject<IconexMapper>(json);

    if (mapper.Mappings == null || mapper.Mappings.Count == 0)
        throw new Exception("No mappings found in JSON");

    // Clear temp tables before import
    connector.ExecuteNonQuery("DELETE FROM KeywordBuffer");
    connector.ExecuteNonQuery("DELETE FROM IconBuffer");
    connector.ExecuteNonQuery("DELETE FROM IconKeywordBuffer");

    // Process mappings
    // ...
}
catch (JsonException ex)
{
    MessageBox.Show($"Invalid JSON format: {ex.Message}");
}
```

### Performance Optimization

#### Slow Queries
**Problem:** Queries take too long with large datasets

**Solutions:**
```sql
-- Add indexes for frequently queried columns
CREATE INDEX IF NOT EXISTS idx_IconTags ON IconTags(Tag);
CREATE INDEX IF NOT EXISTS idx_IconTags2 ON IconTags(Icon);
CREATE INDEX IF NOT EXISTS idx_IconTags3 ON IconTags(Icon, Tag);
CREATE INDEX IF NOT EXISTS idx_IconFiles ON IconFiles(Icon);

-- Use EXPLAIN QUERY PLAN to analyze queries
EXPLAIN QUERY PLAN
SELECT i.* FROM Icons i
JOIN IconTags t ON i.Id = t.Icon
WHERE t.Tag = 'account';
```

#### Large Database File
**Problem:** Database file grows very large

**Solutions:**
```sql
-- Run VACUUM to reclaim space
VACUUM;

-- Enable auto-vacuum in connection string
JournalMode=WAL;AutoVacuum=Full;
```

#### Memory Issues During Import
**Problem:** Out of memory exceptions during bulk import

**Solutions:**
```csharp
// Process in smaller batches
const int BATCH_SIZE = 100;
for (int i = 0; i < files.Length; i++)
{
    ProcessFile(files[i]);

    if (i % BATCH_SIZE == 0)
    {
        GC.Collect();  // Force garbage collection
        GC.WaitForPendingFinalizers();
    }
}

// Dispose images after processing
Image img = Image.FromFile(filePath);
try
{
    int size = img.Width * img.Height;
    // Use size
}
finally
{
    img.Dispose();
}
```

---

## Contributing

### Contribution Guidelines

We welcome contributions to IconCommander! Please follow these guidelines:

1. **Fork the Repository**
   ```bash
   git clone https://github.com/yourusername/IconCommander.git
   cd IconCommander
   git checkout -b feature/my-new-feature
   ```

2. **Follow Coding Standards**
   - Use C# naming conventions (PascalCase for methods, camelCase for fields)
   - Add XML documentation comments for public methods
   - Follow existing patterns (Response pattern, UIGenerator, etc.)
   - Place controls in Designer, not at runtime

3. **Write Tests**
   - Add unit tests for new database methods
   - Test with various file formats and sizes
   - Verify theme application on new forms

4. **Update Documentation**
   - Update CLAUDE.md with new features
   - Add examples to README.md
   - Document database schema changes

5. **Submit Pull Request**
   - Describe what your changes do
   - Reference related issues
   - Include screenshots for UI changes

### Development Priorities

**High Priority:**
1. Icon export engine (Windows/Web projects)
2. Icon merge drawing functionality
3. Search/filter interface
4. Icon buffer browsing UI

**Medium Priority:**
1. License management UI
2. Project file XML manipulation
3. Recent projects functionality
4. Batch icon operations

**Low Priority:**
1. Icon format conversion
2. Advanced search operators
3. Plugin system
4. REST API for external tools

### Code Review Checklist

Before submitting a pull request, ensure:
- [ ] All new forms have ThemeManager component
- [ ] Database operations use parameterized queries
- [ ] Long operations use BackgroundWorker
- [ ] Response pattern used for all database methods
- [ ] No SQL injection vulnerabilities
- [ ] Binary data uses DbType.Binary parameter
- [ ] Transactions committed/rolled back properly
- [ ] Progress reporting for operations >1 second
- [ ] Cancellation support for long operations
- [ ] CLAUDE.md updated with new features

---

## License

This project is licensed under the MIT License. See LICENSE file for details.

---

## Support

For issues, questions, or feature requests:
- **GitHub Issues**: https://github.com/yourusername/IconCommander/issues
- **Email**: support@iconcommander.com
- **Documentation**: See CLAUDE.md for detailed technical documentation

---

## Acknowledgments

- **ZidUtilities**: UI component library providing ThemeManager, UIGenerator, ZidGrid, and AddressBar
- **SQLite**: High-performance embedded database
- **Newtonsoft.Json**: JSON serialization/deserialization
- **ICSharpCode.TextEditor**: Syntax-highlighting text editor

---

## Version History

### Version 1.0.0 (Current)
- Initial release
- Collections and Veins CRUD
- Bulk import with JSON keyword mapping
- Theme management
- Project configuration
- Database-driven icon storage with SHA256 deduplication

### Planned Features (v1.1.0)
- Icon export to Windows/Web projects
- Icon merge functionality
- Search and filter UI
- Icon buffer management
- License tracking

---

**Built with ❤️ for developers who value organized icon libraries**
