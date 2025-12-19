# CLAUDE.md

This file provides comprehensive guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**IconCommander** is a Windows Forms desktop application built with .NET Framework 4.8 that provides comprehensive icon and image library management with database-driven organization, tagging, and export capabilities.

**Core Concept:**
The application maintains a SQLite database of icons/images organized into Collections and Veins. Users can search, filter, tag, and export icons to their development projects (Windows or Web). The system supports merging icons to create composite images and maintains a buffer space for working with selected icons.

**Key Features:**
- Database-driven icon library management
- Collection and Vein organization (hierarchical grouping)
- Bulk import from folders with automatic tagging
- JSON-based keyword mapping support
- Project-based icon export (Windows/Web projects)
- Icon merging functionality (overlay composition)
- Theme management with persistence
- Binary storage with SHA256 deduplication

**Solution Structure:**
- **IconCommander**: Main Windows Forms application
- **KeywordLoader**: Console utility for bulk data operations (legacy)

---

## Directory Structure

```
IconCommander/
├── Program.cs                          # Application entry point (STAThread)
├── MainForm.cs                         # Main window (370 lines)
├── MainForm.Designer.cs                # Designer-generated UI code
├── ComboboxItem.cs                     # Simple display/value helper class
├── App.config                          # Configuration (theme, database path)
├── packages.config                     # NuGet package references
│
├── DataAccess/                         # Database layer
│   ├── SqliteConnector.cs              # Core SQLite wrapper (1,546 lines)
│   └── SqliteConnector.Custom.cs       # Icon-specific queries (256 lines)
│
├── Forms/                              # Dialog and child forms
│   ├── CollectionsForm.cs              # Collections CRUD (266 lines)
│   ├── CollectionsForm.Designer.cs
│   ├── VeinsForm.cs                    # Veins CRUD (352 lines)
│   ├── VeinsForm.Designer.cs
│   ├── VeinImport.cs                   # Complex import dialog (673 lines)
│   └── VeinImport.Designer.cs
│
├── Models/                             # Data model classes
│   ├── Project.cs                      # Project entity (POCO)
│   ├── Mapping.cs                      # Keyword/Icon mapping helper
│   └── IconexMapper.cs                 # Complex mapping container
│
├── Properties/                         # Assembly metadata
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs
│   └── Settings.settings
│
└── Resources/                          # UI icons (PNG files)
    ├── Bookmark.png, DelBookmark.png
    ├── Next.png, Previous.png
    ├── Open.png, Save.png, close_32x32.png
    ├── Search.png, Comment.png, UnComment.png
    ├── RedAlert.png, Stop.png, Book_32.png
    ├── cancel.png, more_imports.png
    └── VERT_pens.ico                   # Application icon
```

---

## Core Components (Detailed)

### 1. MainForm.cs (370 lines)
**Location:** `IconCommander/MainForm.cs`

**Purpose:** Application root window providing main navigation and orchestration

**Key Responsibilities:**
- Theme management with persistence (ZidThemes)
- Database connection initialization
- Project lifecycle management
- Navigation to child forms
- Status bar updates

**Important Properties:**
```csharp
public Project SelectedProject { get; set; }  // Current active project
private SqliteConnector conector;             // Database connection
private ThemeManager themeManager1;           // UI theme engine
```

**Key Methods:**

| Method | Line | Description |
|--------|------|-------------|
| `MainForm_Load()` | ~50 | Load saved theme from config, rebuild theme menu |
| `LoadDbConnection()` | ~90 | Initialize SQLite connection with connection string builder |
| `createProjectToolStripMenuItem_Click()` | ~150 | Launch UIGenerator for project creation |
| `openProjectToolStripMenuItem_Click()` | ~180 | Show project selection dialog |
| `manageCollectionsToolStripMenuItem_Click()` | ~210 | Open CollectionsForm dialog |
| `manageVeinsToolStripMenuItem_Click()` | ~240 | Open VeinsForm dialog |
| `importVeinToolStripMenuItem_Click()` | ~270 | Launch VeinImport wizard |

**Menu Structure:**
```
┌─ Project
│  ├─ Create Project...
│  └─ Open Project...
├─ Icons
│  ├─ Collections > Manage...
│  ├─ Veins > Manage... / Import Vein...
│  └─ Icons > Manage... / Import Icons...
├─ Data Base
│  └─ Open Data Base...
└─ View
   └─ Theme (dynamically populated from ZidThemes)
```

**Connection String Configuration:**
```csharp
SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder
{
    DataSource = DatabaseFile,
    DefaultTimeout = 5000,
    SyncMode = SynchronizationModes.Off,    // Performance optimization
    JournalMode = SQLiteJournalModeEnum.Memory,  // Faster writes
    ReadOnly = false
};
```

---

### 2. SqliteConnector.cs (1,546 lines)
**Location:** `IconCommander/DataAccess/SqliteConnector.cs`

**Purpose:** Comprehensive SQLite database abstraction layer with response pattern

**Architecture Pattern:**
All methods return `SqliteResponse<T>` providing:
- Result/Results properties
- Error collection
- Execution metrics
- Success/failure flags

**Core Methods:**

| Method | Returns | Purpose |
|--------|---------|---------|
| `ExecuteNonQuery(sql, params)` | `SqliteResponse<int>` | INSERT/UPDATE/DELETE operations |
| `ExecuteScalar(sql, params)` | `SqliteResponse<object>` | Single value queries |
| `ExecuteDataSet(sql, params)` | `SqliteResponse<DataSet>` | Multi-table result sets |
| `ExecuteTable(sql, params)` | `SqliteResponse<DataTable>` | Single table queries |
| `ExecuteColumn(sql, params)` | `SqliteResponse<List<string>>` | Single column as list |
| `CreateTableInSQL(dataTable, tableName)` | void | Create table from DataTable schema |
| `GetTableScript(tableName)` | string | Generate CREATE TABLE DDL |
| `AsyncExecuteDataSet(sql, params)` | void | Background thread execution |
| `CancelExecute()` | void | Cancel async operations |

**Transaction Support:**
```csharp
// Private transaction methods
private void BeginTransaction()
private void CommitTransaction()
private void RollbackTransaction()

// Automatic transaction wrapping for critical operations
// Exception handling triggers automatic rollback
```

**Response Pattern Example:**
```csharp
var response = connector.ExecuteTable("SELECT * FROM Icons WHERE Vein = @vein",
    new SQLiteParameter("@vein", veinId));

if (response.IsOK)
{
    DataTable icons = response.Result;
    // Process data
}
else
{
    foreach (var error in response.Errors)
        Log($"Error: {error.Message}");
}
```

**Logging Infrastructure:**
```csharp
CreateLogTables()  // Creates SystemLog and SystemExceptions tables
RegisterLog(level, message, user, details)  // Levels: DEBUG, INFO, WARN, ERROR
```

**Performance Metrics:**
```csharp
public int RowsAffected { get; }      // Last non-query result count
public int RowsRead { get; }          // Last query row count
public TimeSpan ExecutionLapse { get; } // Last operation duration
public string LastMessage { get; }    // "OK" or error message
```

---

### 3. SqliteConnector.Custom.cs (256 lines)
**Location:** `IconCommander/DataAccess/SqliteConnector.Custom.cs`

**Purpose:** Icon-specific database operations extending SqliteConnector

**Key Methods:**

| Method | Purpose | Special Features |
|--------|---------|------------------|
| `ValidateSchema()` | Schema validation | Currently stub (returns true) |
| `Keywords_Insert(keyword)` | Add keyword | Duplicate check with EXISTS |
| `Icons_Insert(iconFile)` | Register to IconBuffer | Simple insert |
| `GetIconId(vein, name)` | Retrieve icon ID | Parameterized query |
| `GetIconBufferId(fileName)` | Get buffer ID | LIKE query on FileName |
| `RegisterIconRelationShip(iconId, keywordId)` | Link icon-keyword | IconKeywordBuffer insert |
| `RegisterIcon(vein, name, sizes, ico, icoLegacy, svg)` | Insert/update icon | UPSERT with EXISTS check |
| `SaveIconFile(iconId, fileName, ext, type, size, path, binData, isMerged, hash)` | Store file with binary | SHA256 hash duplicate check |
| `GetIconFileId(hash)` | Retrieve file ID | Hash-based lookup |
| `RegisterIconTag(iconId, tag)` | Add tag | Simple insert to IconTags |
| `GetTagsFromTempTablesFor(iconBufferId)` | Query mappings | Complex JOIN across temp tables |
| `DeleteIconsFromVein(veinId)` | Cascade delete | 3-step: Tags → Files → Icons |

**Binary Data Handling:**
```csharp
// Parameterized to prevent SQL injection and handle BLOB
cmd.Parameters.Add("@binData", DbType.Binary).Value = binData;
cmd.Parameters.AddWithValue("@hash", hash);
```

**UPSERT Pattern:**
```csharp
string existsQuery = "SELECT COUNT(*) FROM Icons WHERE Vein = @vein AND Name = @name";
if ((long)existsScalar == 0)
    // INSERT
else
    // UPDATE
```

---

### 4. CollectionsForm.cs (266 lines)
**Location:** `IconCommander/Forms/CollectionsForm.cs`

**Purpose:** CRUD interface for Collections (top-level icon groupings)

**UI Components:**
- `ZidGrid zidGrid1` - Enhanced DataGridView displaying collections
- `ToolStrip` with Insert, Update, Delete, Refresh buttons
- `ThemeManager themeManager1` - Consistent theming

**Key Methods:**

| Method | Line | Purpose |
|--------|------|---------|
| `CollectionsForm_Load()` | ~30 | Apply theme, load collections |
| `LoadCollections()` | ~50 | Execute `SELECT * FROM Collections` |
| `btnInsert_Click()` | ~80 | Launch UIGenerator insert form |
| `btnUpdate_Click()` | ~120 | Launch UIGenerator update form |
| `btnDelete_Click()` | ~160 | Confirm and delete selected collection |
| `GetSelectedId()` | ~200 | Extract ID from selected grid row |
| `GetSelectedRow()` | ~220 | Get full DataRow from selection |

**UIGenerator Integration:**
```csharp
UIGenerator formGen = new UIGenerator(table, "Collections");
formGen.SetPrimaryKey("Id");
formGen.SetExclusion("Id");  // Hide from form
formGen.SetRequired("Name");
formGen.SetFieldType("Description", FieldType.MultilineText);

if (formGen.ShowInsertDialog() == DialogResult.OK)
{
    string sql = formGen.GetInsertScript();
    connector.ExecuteNonQuery(sql);
    LoadCollections();  // Refresh grid
}
```

---

### 5. VeinsForm.cs (352 lines)
**Location:** `IconCommander/Forms/VeinsForm.cs`

**Purpose:** CRUD interface for Veins (folder-based icon sources)

**UI Components:**
- `ZidGrid zidGrid1` - Display veins with collection names
- `ToolStrip` with CRUD buttons
- Foreign key support for Collection dropdown

**Key Methods:**

| Method | Line | Purpose |
|--------|------|---------|
| `VeinsForm_Load()` | ~40 | Load collections and veins |
| `LoadVeins()` | ~70 | Complex JOIN query with collection names |
| `btnInsert_Click()` | ~110 | Create vein with folder picker |
| `btnUpdate_Click()` | ~180 | Modify vein properties |
| `btnDelete_Click()` | ~240 | Delete vein with cascade option |

**Complex Query Example:**
```sql
SELECT
    v.Id,
    v.Collection,
    c.Name as CollectionName,
    v.Name,
    v.Description,
    v.Path,
    v.IsIcon,
    v.IsImage,
    v.IsSvg
FROM Veins v
LEFT JOIN Collections c ON v.Collection = c.Id
ORDER BY v.Name
```

**Foreign Key Configuration:**
```csharp
formGen.SetForeignKey("Collection", collectionsTable);
// Creates dropdown with Collection Id/Name pairs
```

**Boolean Flags:**
- `IsIcon`: Contains .ico files
- `IsImage`: Contains .png, .jpg, .bmp, etc.
- `IsSvg`: Contains .svg files

---

### 6. VeinImport.cs (673 lines) - CRITICAL COMPONENT
**Location:** `IconCommander/Forms/VeinImport.cs`

**Purpose:** Complex wizard for bulk importing icons from folders into the database

**UI Components:**
- `ComboBox cmbVeins` - Vein selector
- `TextBox txtPath` - Selected vein path
- `AddressBar addressBar1` - Breadcrumb navigation (from ZidUtilities)
- `TextBox txtTagsFile` - Optional JSON tags file
- `TreeView tvFolders` - Recursive folder structure visualization
- `ListBox lstLog` - Operation log messages
- `Label lblFileCount, lblIconCount, lblTagCount` - Statistics
- `BackgroundWorker bgWorker` - Async import processing
- `Button btnImport` - Launch import
- `Button btnCancel` - Cancel operation

**Key Methods:**

| Method | Lines | Purpose |
|--------|-------|---------|
| `LoadVeins()` | ~60-80 | Populate ComboBox from Veins table |
| `LoadVeinData()` | ~90-130 | Load selected vein details |
| `LoadBreadCrumb()` | ~140-180 | Build AddressBar from path |
| `LoadTreeView()` | ~190-220 | Initialize folder tree |
| `GetNodesFor(path)` | ~230-456 | **Recursive directory scanner** |
| `btnImport_Click()` | ~480-520 | Validation and worker launch |
| `CleanTempTables()` | ~530-560 | Clear KeywordBuffer, IconBuffer, IconKeywordBuffer |
| `bgWorker_DoWork()` | ~570-828 | **MAIN IMPORT LOGIC** |
| `bgWorker_ProgressChanged()` | ~840-860 | Update ListBox log |
| `bgWorker_RunWorkerCompleted()` | ~870-900 | Cleanup and reload |

**Import Algorithm (bgWorker_DoWork):**

```
1. OPTIONAL JSON PARSING (if tags file provided)
   ├─ Deserialize JSON to IconexMapper structures:
   │  ├─ keywordsToIconNameIds: List<Mapping> { Text, References }
   │  ├─ iconNamesToKeywordIds: List<Mapping>
   │  └─ searchKeywordsToKeywordIds: List<Mapping>
   ├─ Populate temp tables:
   │  ├─ KeywordBuffer(Id, Keyword)
   │  ├─ IconBuffer(Id, IconFile)
   │  └─ IconKeywordBuffer(IconBuffer, KeywordBuffer)
   └─ Report progress: "Loaded X keywords, Y icons, Z relationships"

2. DIRECTORY SCANNING
   ├─ Get all files recursively from vein path
   ├─ Filter by extensions:
   │  └─ bmp, gif, jpg, jpeg, png, tiff, tif, ico, wmf, emf, svg
   └─ Report total files found

3. FOR EACH FILE (with cancellation checks every 100 items or 1 second)
   ├─ Extract metadata:
   │  ├─ fileName = Path.GetFileNameWithoutExtension()
   │  ├─ extension = Path.GetExtension().ToLower()
   │  ├─ hash = Crypter.ToSHA256(filePath)
   │  ├─ type = DetermineType(extension)  // "Icon", "SVG", "Image", "*"
   │  └─ binData = File.ReadAllBytes(filePath)
   │
   ├─ Get dimensions:
   │  ├─ If SVG:
   │  │  ├─ Parse XML: XDocument.Load(filePath)
   │  │  ├─ Extract width/height attributes from <svg> element
   │  │  ├─ Strip "px" suffix if present
   │  │  └─ size = width * height
   │  └─ Else:
   │     ├─ Image img = Image.FromFile(filePath)
   │     └─ size = img.Width * img.Height
   │
   ├─ Check for duplicates:
   │  └─ existingIconFileId = connector.GetIconFileId(hash)
   │
   ├─ If new file:
   │  ├─ iconId = connector.GetIconId(veinId, fileName)
   │  ├─ If icon doesn't exist:
   │  │  └─ connector.RegisterIcon(veinId, fileName, "", 0, 0, 0)
   │  └─ connector.SaveIconFile(iconId, fileName, extension, type,
   │                             size, filePath, binData, 0, hash)
   │
   ├─ TAG EXTRACTION:
   │  ├─ If JSON tags provided:
   │  │  └─ tags = connector.GetTagsFromTempTablesFor(iconBufferId)
   │  └─ Else:
   │     └─ tags = fileName.Split(' ', '_', '-')  // Parse filename
   │
   ├─ Register tags:
   │  └─ foreach tag in tags:
   │     └─ connector.RegisterIconTag(iconId, tag)
   │
   └─ Report progress:
      ├─ Update lblFileCount
      ├─ Calculate percentage
      └─ Add to lstLog every 100 items

4. COMPLETION
   ├─ Report summary statistics
   ├─ Clear temp tables (CleanTempTables)
   └─ Reload veins list
```

**JSON Format (Iconex):**
```json
{
  "From": "iconex_keywords",
  "To": "IconCommander",
  "Mappings": [
    {
      "Text": "account",
      "References": [123, 456, 789]
    }
  ]
}
```

**File Type Detection:**
```csharp
if (extension == ".ico") return "Icon";
if (extension == ".svg") return "SVG";
if (new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }.Contains(extension))
    return "Image";
return "*";
```

**SVG Dimension Parsing:**
```csharp
XDocument xdoc = XDocument.Load(filePath);
var widthAttr = xdoc.Root.Attribute("width")?.Value.Replace("px", "");
var heightAttr = xdoc.Root.Attribute("height")?.Value.Replace("px", "");
int width = int.Parse(widthAttr);
int height = int.Parse(heightAttr);
```

**Cancellation Support:**
```csharp
if (bgWorker.CancellationPending)
{
    e.Cancel = true;
    ReportProgress("Import cancelled by user");
    return;
}
```

---

### 7. Model Classes

#### Project.cs (20 lines)
**Location:** `IconCommander/Models/Project.cs`

```csharp
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Type { get; set; }           // "Web" or "Windows"
    public string ResourceFile { get; set; }   // Path to .resx file
    public string ResourceFolder { get; set; } // Resource folder path
    public string SaveIconsTo { get; set; }    // "Folder", "File", or "Both"
    public string ProjectFile { get; set; }    // Path to .csproj
    public int UpdateProjectFile { get; set; } // 1=true, 0=false
}
```

#### Mapping.cs (16 lines)
**Location:** `IconCommander/Models/Mapping.cs`

```csharp
public class Mapping
{
    public string Text { get; set; }      // Keyword or icon name
    public List<int> References { get; set; }  // IDs of related items
}
```

#### IconexMapper.cs (22 lines)
**Location:** `IconCommander/Models/IconexMapper.cs`

```csharp
public class IconexMapper
{
    public string From { get; set; }      // Source system identifier
    public string To { get; set; }        // Target system identifier
    public List<Mapping> Mappings { get; set; }  // Keyword mappings
}
```

#### ComboboxItem.cs (15 lines)
**Location:** `IconCommander/ComboboxItem.cs`

```csharp
public class ComboboxItem
{
    public string Text { get; set; }   // Display text
    public object Value { get; set; }  // Underlying value
    public override string ToString() => Text;
}
```

---

## Database Schema (Detailed)

### Table Relationships

```
Collections (1) ───< (N) Veins
                            │
                            └─< (N) Icons
                                     │
                                     ├─< (N) IconFiles
                                     │        │
                                     │        ├─< (N) IconBuffer
                                     │        ├─< (N) ProjectIcons
                                     │        └─< (N) MergedLicenses
                                     │
                                     └─< (N) IconTags

Projects (1) ───< (N) ProjectIcons

Licenses (1) ───< (N) Collections
             └───< (N) MergedLicenses

MergeRecipes references:
  - BigIcon → IconFiles(Id)
  - SmallIcon → IconFiles(Id)
  - IconResult → IconFiles(Id)
```

### Core Tables (Detailed)

#### Projects
**Purpose:** Store project configurations for icon export

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Name | TEXT NOT NULL | Project display name |
| Path | TEXT NOT NULL | Root project directory |
| Type | TEXT DEFAULT 'Web' | "Web" or "Windows" |
| ResourceFile | TEXT | Path to .resx file (Windows projects) |
| ResourceFolder | TEXT | Resource folder within project |
| SaveIconsTo | TEXT DEFAULT 'Folder' | "Folder", "File", or "Both" |
| ProjectFile | TEXT | Path to .csproj file |
| UpdateProjectFile | INTEGER DEFAULT 1 | Auto-update project XML flag |

#### Collections
**Purpose:** Top-level grouping for icon sets (e.g., "Font Awesome", "Material Design")

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Name | TEXT NOT NULL | Collection name |
| Description | TEXT | Long description |
| Comments | TEXT | Internal notes |
| License | INTEGER FK → Licenses | License reference |

#### Veins
**Purpose:** Folder-based icon sources within collections

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Collection | INTEGER NOT NULL FK | Parent collection |
| Name | TEXT NOT NULL | Vein name |
| Description | TEXT | Description |
| Path | TEXT NOT NULL | Folder path on disk |
| IsIcon | INTEGER DEFAULT 0 | Contains .ico files |
| IsImage | INTEGER DEFAULT 1 | Contains image files |
| IsSvg | INTEGER DEFAULT 0 | Contains .svg files |

#### Icons
**Purpose:** Logical icon entities (may have multiple files/sizes)

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Name | TEXT NOT NULL | Icon name (typically filename) |
| Sizes | TEXT | Available sizes (comma-separated) |
| Ico | INTEGER | Has .ico format |
| IcoLegacy | INTEGER | Has legacy .ico format |
| Svg | INTEGER | Has .svg format |
| Vein | INTEGER NOT NULL FK | Parent vein |

#### IconFiles
**Purpose:** Individual icon files with binary data

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Icon | INTEGER NOT NULL FK | Parent icon |
| FileName | TEXT NOT NULL | File name without extension |
| Extension | TEXT NOT NULL | File extension with dot |
| Type | TEXT DEFAULT 'Image' | "Icon", "SVG", "Image", "*" |
| Size | INTEGER NOT NULL | Width × Height |
| OriginalPath | TEXT | Source file path |
| BinData | BLOB | Binary file content |
| IsMerged | INTEGER DEFAULT 0 | Created via merge |
| Hash | TEXT NOT NULL | SHA256 for deduplication |

**Index:** `idx_IconFiles` ON `IconFiles(Icon)`

#### IconTags
**Purpose:** Searchable tags for icons

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Icon | INTEGER NOT NULL FK | Icon reference |
| Tag | TEXT NOT NULL | Tag text (lowercase) |

**Indexes:**
- `idx_IconTags` ON `IconTags(Tag)` - Fast tag lookup
- `idx_IconTags2` ON `IconTags(Icon)` - Fast icon lookup
- `idx_IconTags3` ON `IconTags(Icon, Tag)` - Composite index

#### IconBuffer
**Purpose:** Temporary working space for selected icons

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| IconFile | INTEGER NOT NULL FK | Icon file reference |
| CreationDate | TEXT | ISO8601 timestamp |

#### ProjectIcons
**Purpose:** Track icon exports to projects

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Project | INTEGER FK | Target project |
| IconFile | INTEGER NOT NULL FK | Exported icon file |
| SentToProject | INTEGER | Export success flag |
| DateSent | TEXT | ISO8601 timestamp |

#### MergeRecipes
**Purpose:** Record of merged icon configurations

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| BigIcon | INTEGER NOT NULL FK | Base icon file |
| SmallIcon | INTEGER NOT NULL FK | Overlay icon file |
| SmallIconPosition | TEXT NOT NULL | Position: "Top", "TopRight", "Right", "BottomRight", "Bottom", "BottomLeft", "Left", "TopLeft", "Center" |
| IconResult | INTEGER FK | Resulting merged icon file |

#### Licenses
**Purpose:** License definitions for icon collections

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| Name | TEXT NOT NULL | License name (e.g., "MIT", "CC BY 4.0") |
| Description | TEXT NOT NULL | Full license text |

#### MergedLicenses
**Purpose:** Track licenses for merged icons (may inherit multiple)

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER PK | Unique identifier |
| IconFile | INTEGER NOT NULL FK | Merged icon file |
| License | INTEGER NOT NULL FK | License reference |

### Temporary Import Tables

These tables are used during bulk import operations and cleared after processing:

#### KeywordBuffer
**Purpose:** Temporary keyword storage during JSON import

| Column | Type | Purpose |
|--------|------|---------|
| Id | INTEGER NOT NULL | Keyword ID from JSON |
| Keyword | TEXT NOT NULL | Keyword text |

#### IconKeywordBuffer
**Purpose:** Temporary keyword-icon relationships during import

| Column | Type | Purpose |
|--------|------|---------|
| IconBuffer | INTEGER NOT NULL | Icon buffer ID |
| KeywordBuffer | INTEGER NOT NULL | Keyword buffer ID |

---

## Key Dependencies (NuGet Packages)

### Database
- **System.Data.SQLite.Core** (1.0.119.0): SQLite ADO.NET provider

### ZidUtilities Suite (Custom Library)
- **ZidUtilities.CommonCode** (1.0.3): Base utilities
- **ZidUtilities.CommonCode.Win** (1.0.14):
  - `ThemeManager`: UI theming engine (supports multiple themes)
  - `UIGenerator`: Dynamic form generation from DataTables
  - `ZidGrid`: Enhanced DataGridView with sorting/filtering
  - `AddressBar`: Breadcrumb navigation control
  - `MessageBoxDialog`: Theme-aware message boxes
  - `SingleSelectionDialog`: Item selection dialog
  - `FormatConfig`: Field format configuration
- **ZidUtilities.CommonCode.Files** (1.0.4): File operations
- **ZidUtilities.CommonCode.ICSharpTextEditor** (1.0.8): Text editor integration

### JSON & Data Processing
- **Newtonsoft.Json** (13.0.4): JSON serialization/deserialization for Iconex format
- **ClosedXML** (0.87.1): Excel file manipulation
- **DocumentFormat.OpenXml** (2.5): Office Open XML support
- **ExcelDataReader** (3.8.0): Excel file reading
- **ExcelDataReader.DataSet** (3.8.0): Excel to DataSet conversion

### UI Components
- **ICSharpCode.TextEditor** (3.2.1.6466): Syntax-highlighting text editor

### Utilities
- **System.ValueTuple** (4.5.0): Tuple support for .NET Framework

---

## Architecture Patterns

### 1. Response Pattern
All database operations return strongly-typed responses:

```csharp
public class SqliteResponse<T>
{
    public T Result { get; set; }              // Single result
    public List<T> Results { get; set; }       // Multiple results
    public List<ErrorOnResponse> Errors { get; set; }
    public bool IsOK => Errors == null || Errors.Count == 0;
    public bool IsFailure => !IsOK;
    public DateTime ExecutionTime { get; set; }
}

// Usage
var response = connector.ExecuteTable("SELECT * FROM Icons");
if (response.IsOK)
    BindGrid(response.Result);
else
    ShowErrors(response.Errors);
```

### 2. UIGenerator Pattern
Dynamic form generation for CRUD operations:

```csharp
UIGenerator formGen = new UIGenerator(dataTable, "TableName");
formGen.SetPrimaryKey("Id");
formGen.SetExclusion("Id", "CreatedDate");  // Hide fields
formGen.SetRequired("Name", "Path");         // Validation
formGen.SetFieldType("Description", FieldType.MultilineText);
formGen.SetFieldFormat("Path", FieldFormat.Folder);  // Folder picker
formGen.SetForeignKey("Collection", collectionsTable);  // Dropdown

// Show dialogs
if (formGen.ShowInsertDialog() == DialogResult.OK)
{
    string sql = formGen.GetInsertScript();
    connector.ExecuteNonQuery(sql);
}

if (formGen.ShowUpdateDialog(selectedRow) == DialogResult.OK)
{
    string sql = formGen.GetUpdateScript();
    connector.ExecuteNonQuery(sql);
}
```

### 3. Theme Management Pattern
Consistent theming across all forms:

```csharp
private ThemeManager themeManager1;

// In Form_Load
themeManager1.Theme = (ZidThemes)Enum.Parse(typeof(ZidThemes),
    ConfigurationManager.AppSettings["SelectedTheme"]);
themeManager1.ApplyTheme();

// Save theme preference
Configuration config = ConfigurationManager.OpenExeConfiguration(
    ConfigurationUserLevel.None);
config.AppSettings.Settings["SelectedTheme"].Value = selectedTheme.ToString();
config.Save();
```

### 4. BackgroundWorker Pattern
Asynchronous operations with progress reporting:

```csharp
bgWorker.WorkerReportsProgress = true;
bgWorker.WorkerSupportsCancellation = true;
bgWorker.DoWork += bgWorker_DoWork;
bgWorker.ProgressChanged += bgWorker_ProgressChanged;
bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

// In DoWork handler
for (int i = 0; i < items.Count; i++)
{
    if (bgWorker.CancellationPending)
    {
        e.Cancel = true;
        return;
    }

    ProcessItem(items[i]);

    int percentComplete = (i + 1) * 100 / items.Count;
    bgWorker.ReportProgress(percentComplete, $"Processed {i + 1} of {items.Count}");
}
```

### 5. Parameterized Query Pattern
SQL injection prevention and BLOB handling:

```csharp
string sql = "INSERT INTO IconFiles (Icon, FileName, BinData, Hash) " +
             "VALUES (@icon, @fileName, @binData, @hash)";

var parameters = new SQLiteParameter[]
{
    new SQLiteParameter("@icon", iconId),
    new SQLiteParameter("@fileName", fileName),
    new SQLiteParameter("@binData", DbType.Binary) { Value = binData },
    new SQLiteParameter("@hash", hash)
};

connector.ExecuteNonQuery(sql, parameters);
```

---

## Configuration & Settings

### App.config
**Location:** `IconCommander/App.config`

```xml
<configuration>
  <appSettings>
    <add key="SelectedTheme" value="Default" />
    <add key="DatabaseFile" value="C:\Path\To\Icons.db" />
  </appSettings>
  <connectionStrings>
    <add name="IconDatabase"
         connectionString="Data Source=Icons.db;Version=3;DefaultTimeout=5000;SyncMode=Off;JournalMode=Memory" />
  </connectionStrings>
</configuration>
```

### Connection String Builder
```csharp
SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder
{
    DataSource = DatabaseFile,
    Version = 3,
    DefaultTimeout = 5000,                      // 5 second timeout
    SyncMode = SynchronizationModes.Off,        // Performance: skip fsync
    JournalMode = SQLiteJournalModeEnum.Memory, // Keep journal in memory
    ReadOnly = false,
    FailIfMissing = false                       // Create if doesn't exist
};
```

---

## Data Flow Diagrams

### Icon Import Flow (VeinImport)
```
User Action: Select Vein + Optional JSON Tags File
    ↓
[Validate Selection] → Show error if invalid
    ↓
[Launch BackgroundWorker]
    ↓
┌─────────────────────────────────────────────────────────┐
│ bgWorker_DoWork (Background Thread)                     │
├─────────────────────────────────────────────────────────┤
│ 1. IF JSON file provided:                               │
│    ├─ Parse JSON (Newtonsoft.Json)                      │
│    ├─ Populate KeywordBuffer table                      │
│    ├─ Populate IconBuffer table                         │
│    └─ Populate IconKeywordBuffer table                  │
│                                                          │
│ 2. Scan vein directory recursively                      │
│    └─ Filter: .bmp, .gif, .jpg, .png, .ico, .svg, etc. │
│                                                          │
│ 3. FOR EACH FILE:                                       │
│    ├─ Extract: fileName, extension                      │
│    ├─ Calculate: SHA256 hash                            │
│    ├─ Determine: type ("Icon", "SVG", "Image")         │
│    ├─ Read: binary data                                 │
│    ├─ Get dimensions:                                   │
│    │  ├─ SVG: Parse XML for width/height attributes    │
│    │  └─ Image: Use System.Drawing.Image               │
│    ├─ Check duplicate: GetIconFileId(hash)             │
│    ├─ IF NEW:                                           │
│    │  ├─ RegisterIcon(veinId, fileName, ...)           │
│    │  └─ SaveIconFile(iconId, binData, hash, ...)      │
│    ├─ Extract tags:                                     │
│    │  ├─ From JSON: GetTagsFromTempTablesFor()         │
│    │  └─ OR from filename: Split by space/underscore   │
│    └─ RegisterIconTag(iconId, tag) for each tag        │
│                                                          │
│ 4. Report progress every 100 items or 1 second         │
│ 5. Check cancellation periodically                      │
└─────────────────────────────────────────────────────────┘
    ↓
[bgWorker_RunWorkerCompleted]
    ├─ Show completion message
    ├─ CleanTempTables() → Clear KeywordBuffer, IconBuffer, IconKeywordBuffer
    └─ LoadVeins() → Refresh vein list
```

### Project Icon Export Flow (Planned)
```
User Action: Select Icon → Choose Size → Click Export
    ↓
[Get Active Project from Projects table]
    ↓
[Determine Project Type: "Windows" or "Web"]
    ↓
┌──────────────────────────────────────────────────────────┐
│ IF Type = "Windows"                                      │
├──────────────────────────────────────────────────────────┤
│ 1. Save icon file to ResourceFolder                      │
│    └─ Path from Projects.Path + Projects.ResourceFolder │
│                                                           │
│ 2. IF UpdateProjectFile = 1:                            │
│    ├─ Load ProjectFile as XML (Load .csproj)            │
│    ├─ Add <Content Include="path\to\icon.png" />        │
│    └─ Save XML                                           │
│                                                           │
│ 3. IF ResourceFile is defined:                          │
│    ├─ Load .resx file                                    │
│    ├─ Add icon as embedded resource                      │
│    └─ Save .resx                                         │
└──────────────────────────────────────────────────────────┘
    OR
┌──────────────────────────────────────────────────────────┐
│ IF Type = "Web"                                          │
├──────────────────────────────────────────────────────────┤
│ 1. Save icon file to ResourceFolder                      │
│                                                           │
│ 2. IF UpdateProjectFile = 1:                            │
│    ├─ Load ProjectFile as XML                           │
│    ├─ Add <Content Include="path\to\icon.png" />        │
│    └─ Save XML                                           │
└──────────────────────────────────────────────────────────┘
    ↓
[Record in ProjectIcons table]
    ├─ Project ID
    ├─ IconFile ID
    ├─ SentToProject = 1
    └─ DateSent = DateTime.Now (ISO8601)
```

### Icon Merge Flow (Planned)
```
User Action: Select BigIcon + SmallIcon + Position → Click Merge
    ↓
[Load both icon files from IconFiles.BinData]
    ↓
[Create Graphics object from BigIcon dimensions]
    ↓
┌──────────────────────────────────────────────────────────┐
│ Calculate SmallIcon Position                             │
├──────────────────────────────────────────────────────────┤
│ Position Options (Clockwise):                            │
│ ┌─────────────────────────────────┐                      │
│ │ TopLeft    Top    TopRight      │                      │
│ │                                  │                      │
│ │ Left     Center     Right        │                      │
│ │                                  │                      │
│ │ BottomLeft Bottom BottomRight   │                      │
│ └─────────────────────────────────┘                      │
│                                                           │
│ Calculate (x, y) coordinates for SmallIcon placement     │
└──────────────────────────────────────────────────────────┘
    ↓
[Draw BigIcon at (0, 0)]
    ↓
[Draw SmallIcon at calculated position]
    ↓
[Save merged image to byte array]
    ↓
[Calculate new SHA256 hash]
    ↓
[Create Icon record in default Collection > Vein]
    ↓
[Save IconFile with IsMerged = 1]
    ↓
[Record MergeRecipe]
    ├─ BigIcon ID
    ├─ SmallIcon ID
    ├─ SmallIconPosition
    └─ IconResult ID
    ↓
[Add to IconBuffer]
    ↓
[Show merged icon in UI]
```

---

## Feature Implementation Status

### ✅ Fully Implemented
- **Database Layer**: Complete SQLite abstraction with response pattern
- **Theme Management**: Persistent theme selection with ZidThemes
- **Collections CRUD**: Full create/read/update/delete with UIGenerator
- **Veins CRUD**: Full CRUD with foreign key support
- **Vein Import**: Complex bulk import with:
  - Recursive directory scanning
  - JSON keyword mapping support (Iconex format)
  - Binary file storage with BLOB handling
  - SHA256 deduplication
  - SVG dimension parsing via XML
  - Image dimension extraction
  - Progress reporting with BackgroundWorker
  - Cancellation support
  - Automatic tagging (from JSON or filename)
- **Project Management**: Create/open projects with UIGenerator
- **Error Handling**: Comprehensive exception tracking and logging

### 🚧 Partially Implemented
- **Icon Management**: Database schema ready, UI stubs exist, CRUD logic missing
- **Icon Buffer**: Table exists, import populates it, no browsing UI
- **Merge Functionality**: MergeRecipes table ready, UI buttons exist, drawing logic missing

### ❌ Not Implemented
- **Icon Export to Projects**: Complete export engine missing
  - File copy to project folders
  - .csproj XML manipulation
  - .resx resource file updates
- **Icon Merge Drawing**: Image composition logic missing
  - Position calculation algorithms
  - Graphics drawing and compositing
  - Merged icon storage
- **Search/Filter Interface**: No UI for searching by tags/collection/vein
- **License Management**: Tables exist, no UI
- **Recent Projects**: Menu items exist, no functionality

---

## Extension Points for Future Development

### 1. Icon Export Engine
**Files to modify:** `MainForm.cs`, new `ExportManager.cs`

**Implementation tasks:**
- Create `ExportManager` class with methods:
  - `ExportToWindowsProject(Project, IconFile)`: 3-step export
  - `ExportToWebProject(Project, IconFile)`: 2-step export
  - `UpdateProjectXml(projectFilePath, iconPath)`: XML manipulation
  - `UpdateResourceFile(resxPath, iconPath, iconName)`: .resx update
- Use `System.Xml.Linq` for .csproj manipulation
- Use `System.Resources.ResXResourceWriter` for .resx updates

### 2. Icon Merge Functionality
**Files to modify:** New `MergeForm.cs`, new `IconMerger.cs`

**Implementation tasks:**
- Create `MergeForm` dialog:
  - Two PictureBox controls for BigIcon/SmallIcon selection
  - Position selector (9 radio buttons in 3×3 grid)
  - Preview PictureBox
  - Merge button
- Create `IconMerger` class with methods:
  - `CalculatePosition(bigSize, smallSize, position)`: Returns Point
  - `MergeIcons(bigIconData, smallIconData, position)`: Returns merged byte[]
  - Uses `System.Drawing.Graphics.DrawImage()`

### 3. Search Interface
**Files to modify:** `MainForm.cs` (add search panel), new `SearchHelper.cs`

**Implementation tasks:**
- Add search panel to MainForm with:
  - Text search box (icon name)
  - Tag multi-select list
  - Collection dropdown
  - Vein dropdown
  - Extension filter (checkboxes)
  - Size range sliders
- Create `SearchHelper` class:
  - `BuildSearchQuery(SearchCriteria)`: Dynamic SQL generation
  - Use indexes for performance (idx_IconTags, idx_IconFiles)
- Display results in ZidGrid with thumbnails

### 4. Icon Buffer Management
**Files to modify:** New `BufferForm.cs`

**Implementation tasks:**
- Create `BufferForm` dialog:
  - ZidGrid showing IconBuffer with icon previews
  - Export selected button
  - Remove from buffer button
  - Clear all button
  - Merge selected button (launches MergeForm)
- Display icons with thumbnails from BinData

### 5. License Management
**Files to modify:** New `LicensesForm.cs`

**Implementation tasks:**
- Similar pattern to CollectionsForm
- CRUD for Licenses table
- Display license text in multiline textbox
- Link to Collections and MergedLicenses

### 6. Project File XML Manipulation
**Files to modify:** New `ProjectFileManager.cs`

**Implementation tasks:**
```csharp
public class ProjectFileManager
{
    public void AddContentFile(string csprojPath, string relativePath)
    {
        XDocument xdoc = XDocument.Load(csprojPath);
        XNamespace ns = xdoc.Root.GetDefaultNamespace();

        // Find or create ItemGroup
        var itemGroup = xdoc.Descendants(ns + "ItemGroup")
            .FirstOrDefault(g => g.Elements(ns + "Content").Any());

        if (itemGroup == null)
        {
            itemGroup = new XElement(ns + "ItemGroup");
            xdoc.Root.Add(itemGroup);
        }

        // Add Content element
        itemGroup.Add(new XElement(ns + "Content",
            new XAttribute("Include", relativePath)));

        xdoc.Save(csprojPath);
    }
}
```

---

## Common Development Patterns

### Adding a New Form
1. Create form in `Forms/` folder
2. Add ThemeManager component in designer
3. Apply theme in Form_Load:
   ```csharp
   themeManager1.Theme = GetThemeFromConfig();
   themeManager1.ApplyTheme();
   ```
4. Pass SqliteConnector from MainForm
5. Use ZidGrid for data display
6. Use UIGenerator for CRUD dialogs

### Adding a New Database Table
1. Update schema in CLAUDE.md
2. Add custom methods to `SqliteConnector.Custom.cs`:
   ```csharp
   public SqliteResponse<int> TableName_Insert(params)
   public SqliteResponse<DataTable> TableName_GetAll()
   public SqliteResponse<int> TableName_Update(params)
   public SqliteResponse<int> TableName_Delete(int id)
   ```
3. Create form for CRUD operations
4. Update indexes if needed for performance

### Using UIGenerator for Dynamic Forms
```csharp
// Get table schema
var schemaResponse = connector.ExecuteTable($"SELECT * FROM {tableName} LIMIT 0");
DataTable schema = schemaResponse.Result;

// Configure generator
UIGenerator formGen = new UIGenerator(schema, tableName);
formGen.SetPrimaryKey("Id");
formGen.SetExclusion("Id", "CreatedDate");
formGen.SetRequired("Name");
formGen.SetFieldType("Description", FieldType.MultilineText);
formGen.SetFieldFormat("Path", FieldFormat.Folder);

// For foreign keys
var foreignData = connector.ExecuteTable("SELECT Id, Name FROM ForeignTable");
formGen.SetForeignKey("ForeignId", foreignData.Result);

// Show dialog
if (formGen.ShowInsertDialog() == DialogResult.OK)
{
    string sql = formGen.GetInsertScript();
    var result = connector.ExecuteNonQuery(sql);
    if (result.IsOK)
        RefreshGrid();
}
```

### Working with Binary Data
```csharp
// Read binary file
byte[] binData = File.ReadAllBytes(filePath);
string hash = Crypter.ToSHA256(filePath);

// Save to database
string sql = @"INSERT INTO IconFiles
    (Icon, FileName, BinData, Hash)
    VALUES (@icon, @fileName, @binData, @hash)";

var parameters = new[]
{
    new SQLiteParameter("@icon", iconId),
    new SQLiteParameter("@fileName", fileName),
    new SQLiteParameter("@binData", DbType.Binary) { Value = binData },
    new SQLiteParameter("@hash", hash)
};

connector.ExecuteNonQuery(sql, parameters);

// Load from database
var response = connector.ExecuteTable(
    "SELECT BinData FROM IconFiles WHERE Id = @id",
    new SQLiteParameter("@id", iconFileId));

if (response.IsOK && response.Result.Rows.Count > 0)
{
    byte[] data = (byte[])response.Result.Rows[0]["BinData"];
    using (MemoryStream ms = new MemoryStream(data))
    {
        Image img = Image.FromStream(ms);
        pictureBox1.Image = img;
    }
}
```

---

## Build and Development Commands

### Build
```bash
# Restore NuGet packages first
nuget restore IconCommander.sln

# Build Debug configuration
msbuild IconCommander.sln /p:Configuration=Debug /v:minimal

# Build Release configuration
msbuild IconCommander.sln /p:Configuration=Release /v:minimal
```

### Run
```bash
# Debug build
.\IconCommander\bin\Debug\IconCommander.exe

# Release build
.\IconCommander\bin\Release\IconCommander.exe
```

### Clean
```bash
msbuild IconCommander.sln /t:Clean /p:Configuration=Debug
```

---

## Important File Locations

| File | Path | Lines | Purpose |
|------|------|-------|---------|
| MainForm.cs | IconCommander/MainForm.cs | 370 | Main application window |
| SqliteConnector.cs | IconCommander/DataAccess/SqliteConnector.cs | 1,546 | Database abstraction layer |
| SqliteConnector.Custom.cs | IconCommander/DataAccess/SqliteConnector.Custom.cs | 256 | Icon-specific queries |
| CollectionsForm.cs | IconCommander/Forms/CollectionsForm.cs | 266 | Collections CRUD UI |
| VeinsForm.cs | IconCommander/Forms/VeinsForm.cs | 352 | Veins CRUD UI |
| VeinImport.cs | IconCommander/Forms/VeinImport.cs | 673 | Bulk import wizard |
| Project.cs | IconCommander/Models/Project.cs | 20 | Project model |
| Mapping.cs | IconCommander/Models/Mapping.cs | 16 | Mapping model |
| IconexMapper.cs | IconCommander/Models/IconexMapper.cs | 22 | JSON mapper model |
| ComboboxItem.cs | IconCommander/ComboboxItem.cs | 15 | Combo helper |
| App.config | IconCommander/App.config | - | Configuration |
| packages.config | IconCommander/packages.config | - | NuGet references |

---

## Development Notes

### Windows Forms Designer
- All forms should use the Visual Studio Designer
- Controls should be placed at design time, not runtime
- Designer files (*.Designer.cs) are auto-generated - avoid manual edits
- Use ThemeManager component on every form for consistent theming

### Package Management
- Project uses `packages.config` (legacy NuGet format)
- All packages must target .NET Framework 4.8
- ZidUtilities packages are custom (not on NuGet.org)

### Database Best Practices
- Always use parameterized queries (SQL injection prevention)
- Use transactions for multi-step operations
- Check hash before inserting IconFiles (deduplication)
- Use indexes for frequently queried columns (Tags, Icon foreign keys)
- Store dates as ISO8601 strings (YYYY-MM-DD HH:MM:SS)

### Performance Considerations
- SQLite connection string optimizations:
  - `SyncMode=Off`: Skip fsync (faster writes, less durability)
  - `JournalMode=Memory`: Keep journal in RAM
  - `DefaultTimeout=5000`: 5-second lock timeout
- Use BackgroundWorker for long operations (import, export)
- Report progress every 100 items or 1 second
- Use indexes for tag searches (idx_IconTags on Tag column)

### Error Handling Pattern
```csharp
var response = connector.ExecuteNonQuery(sql, parameters);
if (response.IsOK)
{
    MessageBoxDialog.ShowInfo("Operation completed successfully");
}
else
{
    string errors = string.Join("\n", response.Errors.Select(e => e.Message));
    MessageBoxDialog.ShowError($"Operation failed:\n{errors}");
}
```

---

## UI Resources

### Embedded Icons
**Location:** `IconCommander/Resources/`

| File | Purpose |
|------|---------|
| Bookmark.png | Bookmark operations |
| DelBookmark.png | Delete bookmark |
| Next.png | Navigate forward |
| Previous.png | Navigate backward |
| Open.png | Open file/project |
| Save.png | Save operations |
| close_32x32.png | Close dialogs |
| Search.png | Search functionality |
| Comment.png | Add comment |
| UnComment.png | Remove comment |
| RedAlert.png | Error indicator |
| Stop.png | Cancel operations |
| Book_32.png | Documentation |
| cancel.png | Cancel button |
| more_imports.png | Additional imports |
| VERT_pens.ico | Application icon |

### Theme Support
Available themes from ZidThemes enum:
- Default
- Blue
- Dark
- Light
- Green
- Purple
(Exact theme names depend on ZidUtilities.CommonCode.Win version)

---

## Claude Code Configuration

### Command Files (Slash Commands)

Claude Code supports custom slash commands that can be invoked during conversations. Command files are stored in specific directories:

#### Command File Locations

1. **Project-specific commands:** `D:\Just For Fun\IconCommander\.claude\commands\`
   - Commands available only in this project
   - Currently no custom commands defined for this project

2. **Global commands:** `C:\Users\Gonzalo\.claude\commands\`
   - Commands available across all projects
   - Currently no custom global commands

3. **Plugin commands:** `C:\Users\Gonzalo\.claude\plugins\marketplaces\claude-plugins-official\plugins\`
   - Official plugins with built-in commands
   - Examples:
     - `commit-commands/commands/` - Git commit helpers
     - `code-review/commands/` - Code review automation
     - `pr-review-toolkit/commands/` - PR review tools

#### Command File Format

Command files are Markdown files (`.md`) with YAML front matter:

```markdown
---
allowed-tools: Bash(git add:*), Bash(git commit:*)
description: Short description of what this command does
---

## Context

- Information to gather (can use !`shell commands` for dynamic data)

## Your task

Instructions for Claude on what to do when this command is invoked.
```

#### Creating Custom Commands

To create a custom command for this project:

1. Create directory: `.claude\commands\`
2. Add a `.md` file with the command name (e.g., `build-and-run.md`)
3. Define YAML front matter with allowed tools and description
4. Write instructions for Claude

**Example custom command for this project:**

```markdown
---
allowed-tools: Bash(msbuild:*), Bash(*.exe)
description: Build and run IconCommander in Debug mode
---

## Your task

1. Build the IconCommander solution in Debug configuration
2. If build succeeds, run the application
3. Report any errors encountered
```

Save this as `.claude\commands\build-and-run.md` to use it with `/build-and-run`

### Settings Files

**Project settings:** `D:\Just For Fun\IconCommander\.claude\settings.local.json`
- Project-specific Claude Code configuration

**Global settings:** `C:\Users\Gonzalo\.claude\settings.json`
- Global Claude Code preferences

**Global instructions:** `C:\Users\Gonzalo\.claude\CLAUDE.md`
- Instructions that apply to all projects (overridden by project-specific CLAUDE.md)

---

## Summary for Claude Code

**Quick Reference:**
- **Language:** C# (.NET Framework 4.8)
- **UI:** Windows Forms with ZidUtilities theming
- **Database:** SQLite with custom abstraction layer
- **Key Pattern:** Response pattern for all DB operations
- **Main Files:** MainForm.cs (370), SqliteConnector.cs (1,546), VeinImport.cs (673)
- **Architecture:** Event-driven with BackgroundWorker for async operations

**When working on this project:**
1. Always use parameterized queries (security)
2. Follow the SqliteResponse<T> pattern for consistency
3. Add ThemeManager to all new forms
4. Use UIGenerator for CRUD dialogs (reduces boilerplate)
5. Use BackgroundWorker for operations >1 second
6. Place controls in Designer, not at runtime (per user instructions)
7. Update this CLAUDE.md when adding major features

**Current priorities for development:**
1. Icon export engine (Windows/Web projects)
2. Icon merge drawing functionality
3. Search/filter interface
4. Icon buffer browsing UI
