using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InitializeProcessing.Input
{
    public class S3StepFunctionCompositeEvent
    {
        [JsonPropertyName("Event")]
        public S3ObjectCreateEvent Event { get; set; }

        [JsonPropertyName("ExecutionId")]
        public string ExecutionId { get; set; }
    }
}
