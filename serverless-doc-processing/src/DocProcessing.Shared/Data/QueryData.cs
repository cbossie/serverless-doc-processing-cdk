using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Data
{
    [DynamoDBTable("QueryData")]
    public class QueryData
    {
        [DynamoDBHashKey("query")]
        public string QueryId { get; set; }

        [DynamoDBProperty("queryText")]
        public string QueryText { get; set; }
    }
}
