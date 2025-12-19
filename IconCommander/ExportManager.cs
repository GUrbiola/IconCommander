using IconCommander.DataAccess;
using IconCommander.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml.Linq;

namespace IconCommander
{
    /// <summary>
    /// Manages icon export operations from buffer to projects
    /// </summary>
    public class ExportManager
    {
        private IIconCommanderDb connector;

        public ExportManager(IIconCommanderDb dbConnector)
        {
            connector = dbConnector;
        }

        /// <summary>
        /// Export selected icons to the specified project (simple version for IconDisplayControl)
        /// </summary>
        /// <param name="project">Target project</param>
        /// <param name="icons">List of icon data (IconFileId, FileName, Extension, BinData)</param>
        /// <returns>Export results with success count and errors</returns>
        public ExportResult ExportIcons(Project project, List<IconExportData> icons)
        {
            ExportResult result = new ExportResult();

            if (project == null)
            {
                result.Errors.Add("No project selected. Please select or create a project first.");
                return result;
            }

            if (icons == null || icons.Count == 0)
            {
                result.Errors.Add("No icons selected for export.");
                return result;
            }

            // Validate project configuration
            if (!ValidateProjectConfiguration(project, result))
                return result;

            foreach (var icon in icons)
            {
                try
                {
                    ExportSingleIcon(project, icon.IconFileId, icon.FileName, icon.Extension, icon.BinData, result);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error exporting icon {icon.FileName}: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Export selected icons from buffer to the specified project
        /// </summary>
        /// <param name="project">Target project</param>
        /// <param name="bufferRows">Selected buffer rows from query</param>
        /// <returns>Export results with success count and errors</returns>
        public ExportResult ExportSelectedIcons(Project project, List<DataRow> bufferRows)
        {
            ExportResult result = new ExportResult();

            if (project == null)
            {
                result.Errors.Add("No project selected. Please select or create a project first.");
                return result;
            }

            if (bufferRows == null || bufferRows.Count == 0)
            {
                result.Errors.Add("No icons selected for export.");
                return result;
            }

            // Validate project configuration
            if (!ValidateProjectConfiguration(project, result))
                return result;

            foreach (DataRow row in bufferRows)
            {
                try
                {
                    int iconFileId = Convert.ToInt32(row["IconFile"]);
                    string fileName = row["FileName"].ToString();
                    string extension = row["Extension"].ToString();
                    byte[] binData = (byte[])row["BinData"];

                    // Export the icon
                    ExportSingleIcon(project, iconFileId, fileName, extension, binData, result);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error exporting icon: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Validate project configuration before export
        /// </summary>
        private bool ValidateProjectConfiguration(Project project, ExportResult result)
        {
            // Check if project path exists
            if (string.IsNullOrEmpty(project.Path) || !Directory.Exists(project.Path))
            {
                result.Errors.Add($"Project path does not exist: {project.Path}");
                return false;
            }

            // Check SaveIconsTo setting
            if (string.IsNullOrEmpty(project.SaveIconsTo))
            {
                result.Errors.Add("Project SaveIconsTo setting is not configured.");
                return false;
            }

            // If saving to folder, check resource folder
            if ((project.SaveIconsTo == "Folder" || project.SaveIconsTo == "Both") &&
                !string.IsNullOrEmpty(project.ResourceFolder))
            {
                string fullFolderPath = Path.Combine(project.Path, project.ResourceFolder);
                if (!Directory.Exists(fullFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullFolderPath);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Cannot create resource folder: {ex.Message}");
                        return false;
                    }
                }
            }

            // If saving to resource file, check if file exists (for Windows projects)
            if ((project.SaveIconsTo == "File" || project.SaveIconsTo == "Both") &&
                !string.IsNullOrEmpty(project.ResourceFile))
            {
                string fullResxPath = Path.Combine(project.Path, project.ResourceFile);
                if (!File.Exists(fullResxPath))
                {
                    result.Warnings.Add($"Resource file does not exist and will be created: {fullResxPath}");
                }
            }

            return true;
        }

        /// <summary>
        /// Export a single icon file
        /// </summary>
        private void ExportSingleIcon(Project project, int iconFileId, string fileName, string extension,
            byte[] binData, ExportResult result)
        {
            // Debug: Log export settings for first file only
            if (result.SuccessCount == 0 && result.ExportedFiles.Count == 0)
            {
                result.Warnings.Add($"=== Export Settings ===");
                result.Warnings.Add($"SaveIconsTo: {project.SaveIconsTo ?? "NULL"}");
                result.Warnings.Add($"ProjectFile: {project.ProjectFile ?? "NULL"}");
                result.Warnings.Add($"UpdateProjectFile: {project.UpdateProjectFile}");
                result.Warnings.Add($"ResourceFolder: {project.ResourceFolder ?? "NULL"}");
                result.Warnings.Add($"ResourceFile: {project.ResourceFile ?? "NULL"}");
                result.Warnings.Add($"Project Path: {project.Path}");
                result.Warnings.Add($"======================");
            }

            // Ensure extension starts with a dot
            if (!string.IsNullOrEmpty(extension) && !extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            // If no extension provided, default to .png
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".png";
            }

            string targetFileName = fileName;
            string fullFilePath = null;

            // Save to folder if required
            if (project.SaveIconsTo == "Folder" || project.SaveIconsTo == "Both")
            {
                string resourceFolder = Path.Combine(project.Path, project.ResourceFolder ?? "Resources");

                // Ensure folder exists
                if (!Directory.Exists(resourceFolder))
                    Directory.CreateDirectory(resourceFolder);

                // Resolve name conflicts (returns filename WITHOUT extension)
                targetFileName = ResolveNameConflict(resourceFolder, fileName, extension);

                // Build full file path WITH extension
                string fileNameWithExtension = targetFileName + extension;
                fullFilePath = Path.Combine(resourceFolder, fileNameWithExtension);

                // Write file
                File.WriteAllBytes(fullFilePath, binData);
                result.ExportedFiles.Add(fullFilePath);

                // Update project file if configured
                if (!string.IsNullOrEmpty(project.ProjectFile))
                {
                    // Build full project file path
                    string projectFilePath = project.ProjectFile;
                    if (!Path.IsPathRooted(projectFilePath))
                    {
                        projectFilePath = Path.Combine(project.Path, projectFilePath);
                    }

                    // Build relative path for the icon file
                    string relativePath = Path.Combine(project.ResourceFolder ?? "Resources", fileNameWithExtension);

                    // Add to project file (default behavior unless UpdateProjectFile is explicitly 0)
                    if (project.UpdateProjectFile != 0) // Allow null/1 to mean "yes"
                    {
                        AddToProjectFile(projectFilePath, relativePath, result);
                    }
                }
            }

            // Save to resource file if required (Windows projects)
            if ((project.SaveIconsTo == "File" || project.SaveIconsTo == "Both") &&
                !string.IsNullOrEmpty(project.ResourceFile))
            {
                string resxPath = Path.Combine(project.Path, project.ResourceFile);
                // Use the resolved target filename for resource name
                string resourceName = (targetFileName + extension).Replace(" ", "_").Replace("-", "_").Replace(".", "_");

                AddToResourceFile(resxPath, resourceName, binData, extension, result);
            }

            // Record export in database (with extension)
            RecordExport(project.Id, iconFileId, targetFileName + extension, result);

            result.SuccessCount++;
        }

        /// <summary>
        /// Resolve file name conflicts by appending _1, _2, etc.
        /// </summary>
        private string ResolveNameConflict(string targetFolder, string fileName, string extension)
        {
            string baseName = fileName;
            int counter = 1;
            string testPath = Path.Combine(targetFolder, fileName + extension);

            while (File.Exists(testPath))
            {
                fileName = $"{baseName}_{counter}";
                testPath = Path.Combine(targetFolder, fileName + extension);
                counter++;
            }

            return fileName;
        }

        /// <summary>
        /// Add icon to .resx resource file
        /// </summary>
        private void AddToResourceFile(string resxPath, string resourceName, byte[] binData,
            string extension, ExportResult result)
        {
            try
            {
                // Create directory if needed
                string directory = Path.GetDirectoryName(resxPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Load or create resx file
                System.Resources.ResXResourceSet resxSet = null;
                Dictionary<string, object> existingResources = new Dictionary<string, object>();

                if (File.Exists(resxPath))
                {
                    // Read existing resources
                    using (var reader = new System.Resources.ResXResourceReader(resxPath))
                    {
                        reader.UseResXDataNodes = true;
                        foreach (System.Collections.DictionaryEntry entry in reader)
                        {
                            var dataNode = (System.Resources.ResXDataNode)entry.Value;
                            existingResources[entry.Key.ToString()] = dataNode;
                        }
                    }
                }

                // Check for name conflict
                string finalResourceName = resourceName;
                int counter = 1;
                while (existingResources.ContainsKey(finalResourceName))
                {
                    finalResourceName = $"{resourceName}_{counter}";
                    counter++;
                }

                // Add new resource
                using (var writer = new System.Resources.ResXResourceWriter(resxPath))
                {
                    // Write existing resources
                    foreach (var kvp in existingResources)
                    {
                        writer.AddResource((System.Resources.ResXDataNode)kvp.Value);
                    }

                    // Add new icon resource
                    using (var ms = new MemoryStream(binData))
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        writer.AddResource(finalResourceName, img);
                    }

                    writer.Generate();
                }

                result.ResourcesAdded.Add(finalResourceName);

                // Update the corresponding .Designer.cs file
                UpdateDesignerFile(resxPath, finalResourceName, extension, result);
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Could not add to resource file: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the .Designer.cs file to add strongly-typed property for the resource
        /// </summary>
        private void UpdateDesignerFile(string resxPath, string resourceName, string extension, ExportResult result)
        {
            try
            {
                // Find the Designer.cs file
                string designerPath = resxPath.Replace(".resx", ".Designer.cs");

                if (!File.Exists(designerPath))
                {
                    result.Warnings.Add($"Designer file not found: {designerPath}. Resource added but not accessible in code.");
                    return;
                }

                // Read the designer file
                string designerContent = File.ReadAllText(designerPath);
                string originalContent = designerContent;

                // Check if property already exists (check for various access modifiers)
                if (designerContent.Contains($"static System.Drawing.Bitmap {resourceName} {{") ||
                    designerContent.Contains($"static System.Drawing.Icon {resourceName} {{") ||
                    designerContent.Contains($"static System.Drawing.Bitmap {resourceName}{{") ||
                    designerContent.Contains($"static System.Drawing.Icon {resourceName}{{"))
                {
                    // Already exists, no need to add
                    return;
                }

                // Determine the return type based on extension
                string returnType = "System.Drawing.Bitmap";
                if (extension.ToLower() == ".ico")
                {
                    returnType = "System.Drawing.Icon";
                }

                // Generate the property code with proper indentation
                string propertyCode = $@"
        /// <summary>
        ///   Looks up a localized resource of type {returnType}.
        /// </summary>
        internal static {returnType} {resourceName} {{
            get {{
                object obj = ResourceManager.GetObject(""{resourceName}"", resourceCulture);
                return (({returnType})(obj));
            }}
        }}";

                // Find the insertion point - look for the last property closing brace before class end
                // Strategy: Find all occurrences of "        }" (8 spaces + brace) which are property closings
                int lastPropertyEnd = -1;
                int searchIndex = 0;

                while (true)
                {
                    int nextIndex = designerContent.IndexOf("        }", searchIndex);
                    if (nextIndex < 0)
                        break;

                    // Check if this is followed by either another property or the class end
                    int afterBrace = nextIndex + "        }".Length;
                    if (afterBrace < designerContent.Length)
                    {
                        // This looks like a property end
                        lastPropertyEnd = afterBrace;
                    }

                    searchIndex = nextIndex + 1;
                }

                if (lastPropertyEnd > 0)
                {
                    // Insert after the last property
                    designerContent = designerContent.Insert(lastPropertyEnd, "\n" + propertyCode);
                }
                else
                {
                    // Fallback: Find the class closing brace (4 spaces + })
                    // Look for the pattern: 4 spaces, }, newline, } (namespace end)
                    int namespaceEnd = designerContent.LastIndexOf("\n}");
                    if (namespaceEnd > 0)
                    {
                        // Go backwards to find the class end
                        int classEnd = designerContent.LastIndexOf("    }", namespaceEnd);
                        if (classEnd > 0)
                        {
                            designerContent = designerContent.Insert(classEnd, propertyCode + "\n");
                        }
                    }
                }

                // Only write if we actually modified the content
                if (designerContent != originalContent)
                {
                    File.WriteAllText(designerPath, designerContent);
                    // Success message, not warning
                    result.Warnings.Add($"✓ Added '{resourceName}' property to {Path.GetFileName(designerPath)}");
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Could not update Designer file: {ex.Message}");
            }
        }

        /// <summary>
        /// Add file reference to .csproj project file
        /// </summary>
        private void AddToProjectFile(string projectFilePath, string relativePath, ExportResult result)
        {
            try
            {
                // Normalize the relative path to use backslashes (Windows style)
                relativePath = relativePath.Replace("/", "\\");

                if (!File.Exists(projectFilePath))
                {
                    result.Warnings.Add($"⚠ Project file not found: {projectFilePath}");
                    return;
                }

                XDocument xdoc = XDocument.Load(projectFilePath);
                XNamespace ns = xdoc.Root.GetDefaultNamespace();

                // Normalize existing paths for comparison
                var existingContent = xdoc.Descendants(ns + "Content")
                    .FirstOrDefault(e => {
                        string existing = e.Attribute("Include")?.Value;
                        if (existing == null) return false;

                        // Normalize for comparison (case-insensitive, backslashes)
                        string normalizedExisting = existing.Replace("/", "\\");
                        string normalizedNew = relativePath.Replace("/", "\\");

                        return string.Equals(normalizedExisting, normalizedNew, StringComparison.OrdinalIgnoreCase);
                    });

                if (existingContent != null)
                {
                    // Already exists, no need to add
                    result.Warnings.Add($"Already in project: {relativePath}");
                    return;
                }

                // Find or create ItemGroup for Content
                var itemGroup = xdoc.Descendants(ns + "ItemGroup")
                    .FirstOrDefault(g => g.Elements(ns + "Content").Any());

                if (itemGroup == null)
                {
                    // Create new ItemGroup
                    itemGroup = new XElement(ns + "ItemGroup");
                    xdoc.Root.Add(itemGroup);
                }

                // Add Content element
                itemGroup.Add(new XElement(ns + "Content",
                    new XAttribute("Include", relativePath)));

                xdoc.Save(projectFilePath);
                result.ProjectFilesUpdated++;
                result.Warnings.Add($"✓ Added to project: {relativePath}");
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"⚠ Could not update project file: {ex.Message}");
            }
        }

        /// <summary>
        /// Record export in ProjectIcons table
        /// </summary>
        private void RecordExport(int projectId, int iconFileId, string exportedName, ExportResult result)
        {
            try
            {
                string sql = @"
                    INSERT INTO ProjectIcons (Project, IconFile, SentToProject, DateSent)
                    VALUES (@project, @iconFile, 1, @dateSent)";

                var parameters = new Dictionary<string, string>
                {
                    { "@project", projectId.ToString() },
                    { "@iconFile", iconFileId.ToString() },
                    { "@dateSent", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                var response = connector.ExecuteNonQuery(sql, parameters);

                if (!response.IsOK)
                {
                    result.Warnings.Add($"Could not record export in database for {exportedName}");
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Database error recording export: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Simple icon data for export
    /// </summary>
    public class IconExportData
    {
        public int IconFileId { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public byte[] BinData { get; set; }
    }

    /// <summary>
    /// Result of export operation
    /// </summary>
    public class ExportResult
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> ExportedFiles { get; set; }
        public List<string> ResourcesAdded { get; set; }
        public int ProjectFilesUpdated { get; set; }

        public ExportResult()
        {
            SuccessCount = 0;
            Errors = new List<string>();
            Warnings = new List<string>();
            ExportedFiles = new List<string>();
            ResourcesAdded = new List<string>();
            ProjectFilesUpdated = 0;
        }

        public bool HasErrors => Errors.Count > 0;
        public bool IsSuccess => SuccessCount > 0 && !HasErrors;
    }
}
