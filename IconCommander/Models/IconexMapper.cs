using System.Collections.Generic;

namespace IconCommander.Models
{
    /// <summary>
    /// Root object for Iconex-format JSON keyword mapping files.
    /// Deserialised by <c>Newtonsoft.Json</c> during vein imports to populate the
    /// temporary <c>KeywordBuffer</c>, <c>IconBuffer</c>, and <c>IconKeywordBuffer</c> tables.
    /// </summary>
    /// <example>
    /// Expected JSON structure:
    /// <code>
    /// {
    ///   "From": "iconex_keywords",
    ///   "To":   "IconCommander",
    ///   "Mappings": [
    ///     { "Text": "account", "References": [123, 456] }
    ///   ]
    /// }
    /// </code>
    /// </example>
    public class IconexMapper
    {
        /// <summary>Gets or sets the identifier of the system that produced this mapping file.</summary>
        public string From { get; set; }

        /// <summary>Gets or sets the identifier of the target system (should be <c>"IconCommander"</c>).</summary>
        public string To { get; set; }

        /// <summary>Gets or sets the list of keyword-to-icon (or icon-to-keyword) mapping entries.</summary>
        public List<Mapping> Mappings { get; set; }

        /// <summary>Initialises a new <see cref="IconexMapper"/> with an empty mappings list.</summary>
        public IconexMapper()
        {
            Mappings = new List<Mapping>();
        }

        /// <summary>Initialises a new <see cref="IconexMapper"/> with the specified source and target identifiers.</summary>
        /// <param name="from">Source system identifier.</param>
        /// <param name="to">Target system identifier.</param>
        public IconexMapper(string from, string to)
        {
            this.From = from;
            this.To = to;
            Mappings = new List<Mapping>();
        }
    }
}
