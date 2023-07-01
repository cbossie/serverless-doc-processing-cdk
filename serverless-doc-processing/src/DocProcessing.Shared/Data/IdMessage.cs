using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared;

public class IdMessage
{
    public virtual string Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
