using DocProcessing.Shared.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.AwsSdkUtilities;

public interface ITextractService
{
    public Task<List<Model.Textract.Block>> GetBlocksForAnalysis(string JobId, string bucket, string prefix);
}
