namespace PianoViaR.Utils
{
    public static class Ensure
    {
        /// <summary>
        /// Ensures that the specified argument is not null.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argument">The argument.</param>
        [System.Diagnostics.DebuggerStepThrough]
        [JetBrains.Annotations.ContractAnnotation("halt <= argument:null")]
        public static void ArgumentNotNull(object argument, [JetBrains.Annotations.InvokerParameterName] string argumentName = null)
        {
            if (argument == null)
            {
                throw new System.ArgumentNullException(argumentName ?? nameof(argument));
            }
        }
    }
}