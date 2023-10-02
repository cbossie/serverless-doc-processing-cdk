using Amazon.CDK.AWS.SES.Actions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessDocProcessing.Constructs
{
    internal class DocProcessingStepFunctionProps : StateMachineProps
    {
        public Function InitializeFunction { get; init; }
        public Function SubmitToTextractFunction { get; init; }
        public Function ProcessTextractResultFunction { get; init; }
        public Queue SendFailureQueue { get; init; }
        public Queue SendSuccessQueue { get; init; }


    }
}
