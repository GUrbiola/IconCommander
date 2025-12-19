using System;
using System.Collections.Generic;

namespace IconCommander.Models
{
    public class Mapping
    {
        public string Text { get; set; }
        public List<int> References { get; set; }
        public Mapping()
        {
            this.Text = String.Empty;
            this.References = new List<int>();
        }
    }
}
