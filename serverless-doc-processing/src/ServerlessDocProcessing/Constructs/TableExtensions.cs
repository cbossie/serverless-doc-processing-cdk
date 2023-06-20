using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessDocProcessing.Constructs
{
    public static class TableExtensions
    {
        public static void GrantDocumentObjectModelPermissions(this Table tbl, IGrantable source)
        {
            tbl.GrantReadWriteData(source);
            tbl.Grant(source, "dynamodb:DescribeTable");
        }
    }
}
