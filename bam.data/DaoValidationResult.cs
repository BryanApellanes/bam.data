namespace Bam.Data
{
    public class DaoValidationResult
    {
        public DaoValidationResult(bool success = true)
        {
            this.Success = success;
        }

        public Exception Exception { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }

    }
}
