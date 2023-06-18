using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
