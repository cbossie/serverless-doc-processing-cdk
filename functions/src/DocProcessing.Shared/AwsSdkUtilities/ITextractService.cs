using DocProcessing.Shared.Model.Textract;

namespace DocProcessing.Shared.AwsSdkUtilities;

public interface ITextractService
{
    public Task<TextractDataModel> GetBlocksForAnalysis(string bucket, string key);

}