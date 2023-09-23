using Amazon.Textract.Model;

namespace DocProcessing.Shared.AwsSdkUtilities
{
    public class TextractResult
    {
        private List<Block> Blocks { get; } = new();

        public TextractResult(IEnumerable<Block> blocks)
        {
            Blocks.AddRange(blocks);
        }
    }
}
