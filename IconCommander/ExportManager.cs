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
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Could not add to resource file: {ex.Message}");
            }
        }

        /// <summary>
        /// Add file reference to .csproj project file
        /// </summary>
        private void AddToProjectFile(string projectFilePath, string relativePath, ExportResult result)
        {
            try
            {
                if (!File.Exists(projectFilePath))
                {
                    result.Warnings.Add($"Project file not found: {projectFilePath}");
                    return;
                }

                XDocument xdoc = XDocument.Load(projectFilePath);
                XNamespace ns = xdoc.Root.GetDefaultNamespace();

                // Check if file is already in project
                var existingContent = xdoc.Descendants(ns + "Content")
                    .FirstOrDefault(e => e.Attribute("Include")?.Value == relativePath);

                if (existingContent != null)
                {
                    // Already exists, no need to add
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
                    new XAttribute("Include", relativePath.Replace("/", "\\"))));

                xdoc.Save(projectFilePath);
                result.ProjectFilesUpdated++;
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Could not update project file: {ex.Message}");
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
