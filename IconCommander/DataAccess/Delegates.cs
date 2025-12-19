using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconCommander.DataAccess
{
    /// <summary>
    /// Delegate to notify start/finish of a query processing operation.
    /// </summary>
    /// <param name="Query">The SQL query being processed.</param>
    /// <param name="Time">The time of the event.</param>
    public delegate void ProcessingQuery(string Query, DateTime Time);
}
