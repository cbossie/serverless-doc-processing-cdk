﻿using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Data.Expense;
public class DocumentExpenseValue
{

    [DynamoDBProperty("Text")]
    [JsonPropertyName("Text")]
    public string Text { get; set; }

    [DynamoDBProperty("Confidence")]
    [JsonPropertyName("Confidence")]
    public double Confidence { get; set; }

}
