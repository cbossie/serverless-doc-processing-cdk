using DocProcessing.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InitializeProcessing
{
    public class InitializationException : ProcessingExceptionBase
    {
        public InitializationException(string docKey, string message) : base(docKey, message)
        {
        }
    }
}
