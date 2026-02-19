using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconCommander
{
    /// <summary>
    /// Lightweight wrapper for ComboBox data-binding that separates the display text
    /// from the underlying value (e.g. a database ID).
    /// <see cref="ToString"/> returns <see cref="Text"/> so the ComboBox renders
    /// the friendly label automatically.
    /// </summary>
    public class ComboboxItem
    {
        /// <summary>Gets or sets the text displayed in the ComboBox drop-down.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the underlying value (e.g. a database ID) associated with this item.</summary>
        public object Value { get; set; }

        /// <summary>Returns <see cref="Text"/> so the ComboBox renders the item label correctly.</summary>
        public override string ToString() => Text;
    }
}
