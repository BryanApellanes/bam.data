namespace Bam.Data
{
    /// <summary>
    /// Holds user credentials for authenticating with a database.
    /// </summary>
    public class DatabaseCredentials
    {
        /// <summary>
        /// Gets or sets the user ID for database authentication.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password for database authentication.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
