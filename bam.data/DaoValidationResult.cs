namespace Bam.Data
{
    /// <summary>
    /// Represents the result of validating a Dao instance, including success status and any error details.
    /// </summary>
    public class DaoValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaoValidationResult"/> class.
        /// </summary>
        /// <param name="success">Whether the validation succeeded.</param>
        public DaoValidationResult(bool success = true)
        {
            this.Success = success;
        }

        /// <summary>
        /// Gets or sets the exception that occurred during validation, if any.
        /// </summary>
        public Exception Exception { get; set; } = null!;

        /// <summary>
        /// Gets or sets a message describing the validation result.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the validation succeeded.
        /// </summary>
        public bool Success { get; set; }

    }
}
