using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Exceptions
{
    public class FileTypeException : ProcessingExceptionBase
    {
        public FileTypeException(string id, string message) : base(id, message) { }
    }
}

