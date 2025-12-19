using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconCommander.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string ResourceFile { get; set; }
        public string ResourceFolder { get; set; }
        public string SaveIconsTo { get; set; }
        public string ProjectFile { get; set; }
        public int UpdateProjectFile { get; set; }

    }
}
