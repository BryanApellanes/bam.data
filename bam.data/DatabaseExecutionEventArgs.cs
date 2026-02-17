using System.Data.Common;

namespace Bam.Data
{
    /// <summary>
    /// Provides data for database execution events, including the command, reader, and any exception that occurred.
    /// </summary>
    [Serializable]
    public class DatabaseExecutionEventArgs: EventArgs
    {
        /// <summary>
        /// Gets or sets the database on which the command was executed.
        /// </summary>
        public Database Database { get; set; } = null!;

        /// <summary>
        /// Gets or sets the data reader returned by the command, if applicable.
        /// </summary>
        public DbDataReader DataReader { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database command that was executed.
        /// </summary>
        public DbCommand Command { get; set; } = null!;

        /// <summary>
        /// Gets or sets an informational message about the execution.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Gets or sets the exception that occurred during execution, if any.
        /// </summary>
        public Exception Exception { get; set; } = null!;
    }
}
