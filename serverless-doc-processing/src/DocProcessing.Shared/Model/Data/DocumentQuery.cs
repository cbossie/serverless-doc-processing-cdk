﻿using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Data;

[DynamoDBTable(Constants.ResourceNames.QUERY_DATA_TABLE)]
public class DocumentQuery
{
    [JsonPropertyName("query")]
    [DynamoDBHashKey("query")]
    public string QueryId { get; set; }

    [DynamoDBProperty("queryText")]
    [JsonPropertyName("queryText")]
    public virtual string QueryText { get; set; }

    [DynamoDBProperty("result")]
    [JsonPropertyName("result")]
    public string Result { get; set; }

    [DynamoDBProperty("confidence")]
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [DynamoDBProperty("isValid")]
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [DynamoDBProperty("processed")]
    [JsonPropertyName("processed")]
    public bool Processed { get; set; }

}