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
        public Function SuccessFunction { get; set; }
        public Function FailureFunction { get; set; }
        public Function SubmitToTextractFunction { get; init; }
        public Function SubmitToTextractExpenseFunction { get; init; }
        public Function ProcessTextractQueryFunction { get; init; }
        public Function ProcessTextractExpenseFunction { get; set; }
        public Queue SendFailureQueue { get; init; }
        public Queue SendSuccessQueue { get; init; }


    }
}
