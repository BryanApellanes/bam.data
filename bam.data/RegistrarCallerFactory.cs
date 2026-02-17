/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.Logging;

namespace Bam.Data
{
    /// <summary>
    /// Factory for creating IRegistrarCaller instances from assembly-qualified type names.
    /// </summary>
    public class RegistrarCallerFactory
    {
        /// <summary>
        /// Attempts to create an IRegistrarCaller from the specified assembly-qualified type name.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified type name of the registrar caller.</param>
        /// <param name="result">The created IRegistrarCaller, or null if creation failed.</param>
        /// <returns>True if the caller was created successfully.</returns>
        public bool TryCreateRegistrarCaller(string assemblyQualifiedName, out IRegistrarCaller result)
        {
            bool success = false;
            result = null!;
            try
            {
                result = CreateRegistrarCaller(assemblyQualifiedName);
                success = result != null;
            }
            catch (Exception ex)
            {
                Log.AddEntry("An error occurred trying to create RegistrarCaller: {0}", ex, ex.Message);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Creates an IRegistrarCaller from the specified assembly-qualified type name.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified type name of the registrar caller.</param>
        /// <returns>The created IRegistrarCaller, or null if the type was not found.</returns>
        public IRegistrarCaller CreateRegistrarCaller(string assemblyQualifiedName)
        {
            Type? returnType = Type.GetType(assemblyQualifiedName);
            IRegistrarCaller? result = null;
            if (returnType != null)
            {
                result = returnType.Construct<IRegistrarCaller>();
            }

            return result!;
        }
    }
}
