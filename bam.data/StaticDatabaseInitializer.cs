/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.Logging;

namespace Bam.Data
{
	/// <summary>
	/// A database initializer that uses a static dictionary of pre-configured database instances.
	/// </summary>
	public class StaticDatabaseInitializer: Loggable, IDatabaseInitializer
	{
		/// <summary>
		/// Initializes a new StaticDatabaseInitializer with an empty database dictionary.
		/// </summary>
		public StaticDatabaseInitializer()
		{
			_databasesByConnectionName = new Dictionary<string, Database>();
		}

		Dictionary<string, Database> _databasesByConnectionName;
		/// <summary>
		/// Gets the dictionary mapping connection names to their pre-configured database instances.
		/// </summary>
		public Dictionary<string, Database> DatabasesByConnectionName
		{
			get
			{
				return _databasesByConnectionName;
			}
		}

		/// <summary>
		/// Gets or sets the last connection name that was requested.
		/// </summary>
		public string LastConnectionName { get; set; }

		/// <summary>
		/// Occurs when a database is not found for the requested connection name.
		/// </summary>
		[Verbosity(VerbosityLevel.Information, SenderMessageFormat="No database was added for the connection named {LastConnectionName}")]
		public event EventHandler DatabaseNotFound;

		#region IDatabaseInitializer Members

		/// <summary>
		/// Initializes a database for the specified connection name by looking it up in the static dictionary.
		/// </summary>
		/// <param name="connectionName">The connection name to look up.</param>
		/// <returns>A result containing the database if found, or an exception if not.</returns>
		public DatabaseInitializationResult Initialize(string connectionName)
		{
			LastConnectionName = connectionName;
			if(DatabasesByConnectionName.ContainsKey(connectionName))
			{
				return new DatabaseInitializationResult(DatabasesByConnectionName[connectionName], null);
			}
			else
			{
				FireEvent(DatabaseNotFound, EventArgs.Empty);
			}

			return new DatabaseInitializationResult(null, new DatabaseInitializationFailedException(connectionName));
		}

		/// <summary>
		/// Not implemented. Throws NotImplementedException.
		/// </summary>
		/// <param name="types">The types to ignore.</param>
		public void Ignore(params Type[] types)
		{
			throw new NotImplementedException("{0} doesn't implement Ignore".Format(typeof(StaticDatabaseInitializer).FullName));
		}

		/// <summary>
		/// Not implemented. Throws NotImplementedException.
		/// </summary>
		/// <param name="connectionNames">The connection names to ignore.</param>
		public void Ignore(params string[] connectionNames)
		{
			throw new NotImplementedException("{0} doesn't implement Ignore".Format(typeof(StaticDatabaseInitializer).FullName));
		}

		#endregion
	}
}
