using System;
using System.Collections.Generic;

namespace IconCommander.Models
{
    /// <summary>
    /// Associates a keyword or icon name with a list of related integer IDs.
    /// Used as an element inside <see cref="IconexMapper.Mappings"/> when deserialising
    /// Iconex-format JSON files during vein imports.
    /// </summary>
    public class Mapping
    {
        /// <summary>Gets or sets the keyword or icon name for this mapping entry.</summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the list of IDs that reference this keyword or icon.
        /// Typically these are icon IDs or keyword IDs from the source Iconex system.
        /// </summary>
        public List<int> References { get; set; }

        /// <summary>Initialises a new <see cref="Mapping"/> with an empty text and an empty references list.</summary>
        public Mapping()
        {
            this.Text = String.Empty;
            this.References = new List<int>();
        }
    }
}
