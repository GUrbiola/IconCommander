using System;

namespace IconCommander.DataAccess
{
    /// <summary>
    /// Represents an error entry in a response, capturing execution time, message and exception details.
    /// </summary>
    public class ErrorOnResponse
    {
        /// <summary>
        /// Gets or sets the time when the error was recorded.
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the associated exception, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Initializes a new ErrorOnResponse with a default message.
        /// </summary>
        public ErrorOnResponse()
        {
            this.ExecutionTime = DateTime.Now;
            this.Message = "An error occurred while processing your request.";
        }

        /// <summary>
        /// Initializes a new ErrorOnResponse with a specific message and optional exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="Ex">Optional exception associated with the error.</param>
        public ErrorOnResponse(string message, Exception Ex = null)
        {
            this.ExecutionTime = DateTime.Now;
            this.Message = message;
            this.Exception = Ex == null ? new Exception(this.Message) : Ex;
        }
    }
}
