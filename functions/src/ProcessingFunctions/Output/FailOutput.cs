using DocProcessing.Shared.Model.Data.Expense;
using DocProcessing.Shared.Model.Data.Query;
using ProcessingFunctions.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProcessingFunctions.Output;

public class FailOutput : StepFunctionOutputBase
{
    [JsonPropertyName("Success")]
    public override bool Success => false;

    [JsonPropertyName("Error")]
    public ErrorMessage Error { get; set; }

}
