using System;
using System.Collections.Generic;

namespace IconCommander.DataAccess
{
    /// <summary>
    /// Generic response container supporting result lists, single result, error collection, and status messages.
    /// </summary>
    /// <typeparam name="T">Type of the result.</typeparam>
    public class SqlResponse<T>
    {
        #region Properties
        /// <summary>
        /// Gets or sets the time the response was produced.
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// Gets the execution time in a formatted string (yyyy-MM-dd HH:mm:ss.fff).
        /// </summary>
        public string ExecutionTimeString
        {
            get
            {
                return ExecutionTime.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private List<T> _Results;

        /// <summary>
        /// Gets or sets the list of results.
        /// </summary>
        public List<T> Results { get { return _Results; } set { _Results = value; } }

        /// <summary>
        /// Gets or sets the first result item. Setting will replace the Results list with a single item.
        /// </summary>
        public T Result
        {
            get
            {// Return the first result or default if no results
                if (_Results == null || Results.Count == 0)
                    return default(T);
                return Results[0];
            }
            set
            {
                if (_Results != null)
                    Results.Clear();
                else if (_Results == null)
                    _Results = new List<T>();
                Results.Add(value);
            }
        }

        /// <summary>
        /// Gets or sets the collection of errors associated with the response.
        /// </summary>
        public List<ErrorOnResponse> Errors { get; set; }

        private string _Message = String.Empty;

        /// <summary>
        /// Gets or sets the status message (e.g., "Success" or an error description).
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        /// <summary>
        /// Gets a value indicating whether an error occurred based on errors list and message.
        /// </summary>
        public bool ErrorOccured
        {
            get
            {
                if (Errors == null || Errors.Count == 0)
                    return false;
                return !Message.Equals("Success", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Gets a value indicating successful response (no error occurred).
        /// </summary>
        public bool IsOK { get { return !ErrorOccured; } }

        /// <summary>
        /// Gets a value indicating failure response (an error occurred).
        /// </summary>
        public bool IsFailure { get { return ErrorOccured; } }
        #endregion

        #region Class contructor
        /// <summary>
        /// Initializes a new instance of SqliteResponse with empty results and errors, and sets ExecutionTime.
        /// </summary>
        public SqlResponse()
        {
            Errors = new List<ErrorOnResponse>();
            Results = new List<T>();
            ExecutionTime = DateTime.Now;
        }
        #endregion

        #region Indexers of the class, by field number(position on the table) and by field name(case insensitive)
        /// <summary>
        /// Gets a field by name ("results", "result", "message") in a case-insensitive manner.
        /// </summary>
        /// <param name="fieldName">The field name to access.</param>
        /// <returns>The value of the field or null if not found.</returns>
        public object this[string fieldName]
        {
            get
            {
                string aux = fieldName.ToLower();
                switch (aux)
                {
                    case "results":
                        return Results;
                    case "result":
                        return Result;
                    case "message":
                        return Message;
                }
                return null;
            }
            set
            {
                string aux = fieldName.ToLower();
                switch (aux)
                {
                    case "results":
                        if (value is List<T>)
                        {
                            Results = (List<T>)value;
                        }
                        else
                        {
                            try
                            {
                                Results = (List<T>)value;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Data type mismatch for the field: Result - index: Results", ex);
                            }
                        }
                        break;
                    case "result":
                        if (value is T)
                        {
                            Result = (T)value;
                        }
                        else
                        {
                            try
                            {
                                Result = (T)value;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Data type mismatch for the field: Result - index: Result", ex);
                            }
                        }
                        break;
                    case "message":
                        if (value is string)
                        {
                            Message = (string)value;
                        }
                        else
                        {
                            try
                            {
                                Message = (string)value;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Data type mismatch for the field: Message - index: Message", ex);
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// Adds a single result to the Results list.
        /// </summary>
        /// <param name="result">The result to add.</param>
        public void AddResult(T result)
        {
            if (this.Results == null)
                this.Results = new List<T>();
            this.Results.Add(result);
            if (String.IsNullOrEmpty(this.Message))
                this.Message = "Success";
        }

        /// <summary>
        /// Adds a range of results to the Results list.
        /// </summary>
        /// <param name="results">The list of results to add.</param>
        public void AddResults(List<T> results)
        {
            if (this.Results == null)
                this.Results = new List<T>();
            this.Results.AddRange(results);
            if (String.IsNullOrEmpty(this.Message))
                this.Message = "Success";

        }

        /// <summary>
        /// Marks the response as failure and adds a default error using the current message.
        /// </summary>
        /// <returns>The same response instance for chaining.</returns>
        public SqlResponse<T> Fail()
        {
            this.ExecutionTime = DateTime.Now;
            this.Message = "An error occurred while processing your request.";
            this.Errors.Add(new ErrorOnResponse(this.Message));
            return this;
        }

        /// <summary>
        /// Marks the response as failure with a specific message and optional exception.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="ex">Optional exception.</param>
        /// <returns>The same response instance for chaining.</returns>
        public SqlResponse<T> Fail(string message, Exception ex = null)
        {
            this.ExecutionTime = DateTime.Now;
            this.Message = message;
            if (ex != null)
            {
                this.Errors.Add(new ErrorOnResponse(message, ex));
            }
            else
            {
                this.Errors.Add(new ErrorOnResponse(message));
            }
            return this;
        }

        /// <summary>
        /// Marks the response as failure using the provided exception.
        /// </summary>
        /// <param name="ex">Exception causing the failure.</param>
        /// <returns>The same response instance for chaining.</returns>
        public SqlResponse<T> Fail(Exception ex)
        {
            this.ExecutionTime = DateTime.Now;
            this.Message = "An error occurred while processing your request.";
            if (ex == null)
            {
                Fail("An error occurred while processing your request.");
            }
            else
            {
                this.Errors.Add(new ErrorOnResponse(this.Message, ex));
            }
            return this;
        }

        /// <summary>
        /// Marks the response as successful, setting the message.
        /// </summary>
        /// <param name="message">Optional success message (default "Success").</param>
        /// <returns>The same response instance for chaining.</returns>
        public SqlResponse<T> Success()
        {
            this.ExecutionTime = DateTime.Now;
            this.Message = "Success";
            return this;
        }

        /// <summary>
        /// Marks the response as successful with a single result and message.
        /// </summary>
        /// <param name="result">The result to store.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>The same response instance for chaining.</returns>
        public SqlResponse<T> Success(T result)
        {
            this.ExecutionTime = DateTime.Now;
            this.Result = result;
            this.Message = "Success";
            return this;
        }

        /// <summary>
        /// Marks the response as successful with a list of results and message.
        /// </summary>
        /// <param name="results">The results to store.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>The same response instance for chaining.</returns>
        public SqlResponse<T> Success(List<T> results)
        {
            this.ExecutionTime = DateTime.Now;
            this.Results = results;
            this.Message = "Success";
            return this;
        }

        /// <summary>
        /// Creates a failure response with specified message and optional exception.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="ex">Optional exception.</param>
        /// <returns>A new failure SqliteResponse instance.</returns>
        public static SqlResponse<T> Failure(string message, Exception ex = null)
        {
            return new SqlResponse<T>().Fail(message, ex);
        }

        /// <summary>
        /// Creates a failure response with optional exception.
        /// </summary>
        /// <param name="ex">Optional exception.</param>
        /// <returns>A new failure SqliteResponse instance.</returns>
        public static SqlResponse<T> Failure(Exception ex = null)
        {
            return new SqlResponse<T>().Fail(ex);
        }

        /// <summary>
        /// Creates a success response with a list of results and optional message.
        /// </summary>
        /// <param name="results">Results list.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>A new success SqliteResponse instance.</returns>
        public static SqlResponse<T> Successful(List<T> results)
        {
            return new SqlResponse<T>().Success(results);
        }

        /// <summary>
        /// Creates a success response with a single result and optional message.
        /// </summary>
        /// <param name="result">Single result.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>A new success SqliteResponse instance.</returns>
        public static SqlResponse<T> Successful(T result)
        {
            return new SqlResponse<T>().Success(result);
        }

        /// <summary>
        /// Creates a success response with no results and optional message.
        /// </summary>
        /// <param name="message">Optional success message.</param>
        /// <returns>A new success SqliteResponse instance.</returns>
        public static SqlResponse<T> Successful()
        {
            return new SqlResponse<T>().Success();
        }
    }
}
