using DocProcessing.Shared.Exceptions;

namespace InitializeProcessing
{
    public class InitializationException : ProcessingExceptionBase
    {
        public InitializationException(string docKey, string message) : base(docKey, message)
        {
        }
    }
}
