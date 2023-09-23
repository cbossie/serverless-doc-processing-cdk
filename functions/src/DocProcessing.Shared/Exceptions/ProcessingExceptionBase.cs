namespace DocProcessing.Shared.Exceptions;

public abstract class ProcessingExceptionBase : Exception
{
    public string DocKey { get; set; }
    public ProcessingExceptionBase(string docKey, string message)
        : base(message)
    {
        DocKey = docKey;
    }
}
