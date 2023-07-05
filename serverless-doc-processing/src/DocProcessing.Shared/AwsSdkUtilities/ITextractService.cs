using DocProcessing.Shared.Model.Data;
using DocProcessing.Shared.Model.Textract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.AwsSdkUtilities;

public interface ITextractService
{
    public Task<TextractDataModel> GetBlocksForAnalysis(string jobId, string bucket, string prefix);
 
}