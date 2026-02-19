using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconCommander.Models
{
    /// <summary>
    /// Represents a development project to which icons can be exported.
    /// Projects are persisted in the <c>Projects</c> database table and configure
    /// where exported icon files and resource entries should be placed.
    /// </summary>
    public class Project
    {
        /// <summary>Gets or sets the unique database identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the display name of the project.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the root directory path of the project on disk.</summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the project type.
        /// Accepted values: <c>"Web"</c> or <c>"Windows"</c>.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the relative path to the .resx resource file within the project.
        /// Used only for Windows projects when <see cref="SaveIconsTo"/> is <c>"File"</c> or <c>"Both"</c>.
        /// </summary>
        public string ResourceFile { get; set; }

        /// <summary>
        /// Gets or sets the relative folder path (inside <see cref="Path"/>) where exported icon files are stored.
        /// </summary>
        public string ResourceFolder { get; set; }

        /// <summary>
        /// Gets or sets the export destination strategy.
        /// Accepted values: <c>"Folder"</c> (file system only), <c>"File"</c> (.resx only), or <c>"Both"</c>.
        /// </summary>
        public string SaveIconsTo { get; set; }

        /// <summary>
        /// Gets or sets the relative or absolute path to the .csproj project file.
        /// When set, the export engine can automatically add content references to this file.
        /// </summary>
        public string ProjectFile { get; set; }

        /// <summary>
        /// Gets or sets a flag controlling whether the .csproj file is updated automatically on export.
        /// <c>1</c> = auto-update; <c>0</c> = manual.
        /// </summary>
        public int UpdateProjectFile { get; set; }
    }
}
