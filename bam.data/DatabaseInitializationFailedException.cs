/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Runtime.Serialization;

namespace Bam.Data
{
	[Serializable]
	/// <summary>
	/// Exception thrown when database initialization fails.
	/// </summary>
	public class DatabaseInitializationFailedException: Exception
	{
		public DatabaseInitializationFailedException() : base() { }
		public DatabaseInitializationFailedException(string connectionName)
			: base("Failed to initialize database: {0}".Format(connectionName))
		{ }
		public DatabaseInitializationFailedException(SerializationInfo info, StreamingContext context) { }

		public DatabaseInitializationFailedException(List<string> exceptionMessages)
			: base(string.Format("Failed to initialize database:\r\n{0}", exceptionMessages.ToArray().ToDelimited(s => s, "\r\n")))
		{ }
		
	}
}
