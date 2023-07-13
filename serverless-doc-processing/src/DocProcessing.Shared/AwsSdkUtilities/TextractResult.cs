using Amazon.Textract.Model;
using DocProcessing.Shared.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.AwsSdkUtilities
{
    public class TextractResult
    {
        private List<Block> Blocks { get; } = new();

        private Dictionary<string, DocumentQuery> QueryResults { get; } = new();

        public TextractResult(IEnumerable<Block> blocks)
        {
            Blocks.AddRange(blocks);
        }

        private void Initialize()
        {
            var queryResults = Blocks
                .Where(b => b.BlockType == Amazon.Textract.BlockType.QUERY)
                .GroupJoin(
                    Blocks.Where(b => b.BlockType == Amazon.Textract.BlockType.QUERY_RESULT),
                    b => b.Relationships?.FirstOrDefault()?.Ids?.FirstOrDefault(),
                    c => c.Id,
                    (l, r) => {
                        return l.Text;
                    });
        }


    }
}
