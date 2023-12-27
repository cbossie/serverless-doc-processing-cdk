namespace DocProcessing.Shared.Exceptions
{
    public class FileTypeException : ProcessingExceptionBase
    {
        public FileTypeException(string id, string message) : base(id, message) { }
    }
}

