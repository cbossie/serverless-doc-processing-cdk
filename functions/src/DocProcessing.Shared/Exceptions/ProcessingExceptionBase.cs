namespace DocProcessing.Shared.Exceptions;

public abstract class ProcessingExceptionBase : Exception
{
    public string Id { get; set; }
    public ProcessingExceptionBase(string id, string message)
        : base(message)
    {
        Id = id;
    }
}
