using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class PageClassification
{
    [JsonPropertyName("PageNumber")]
    public List<Prediction> PageNumber { get; set; }

    [JsonPropertyName("PageType")]
    public List<Prediction> PageType { get; set; }  

}
