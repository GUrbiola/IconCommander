using System.Collections.Generic;

namespace IconCommander.Models
{
    public class IconexMapper
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<Mapping> Mappings { get; set; }

        public IconexMapper()
        {
            Mappings = new List<Mapping>();
        }
        public IconexMapper(string from, string to)
        {
            this.From = from;
            this.To = to;
            Mappings = new List<Mapping>();
        }
    }
}
