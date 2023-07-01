using DocProcessing.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestartStepFunction.Exceptions;

public class RestartStepFunctionException : Exception
{
	public RestartStepFunctionException(string message)
		: base(message)
	{

	}
}
