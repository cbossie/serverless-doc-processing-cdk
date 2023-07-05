using Amazon.S3.Model;
using Amazon.Textract.Model;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class TextractDataModel
{
    Dictionary<string, Block> BlockMap { get; }

    private Dictionary<string, List<Block>> Queries = new();

    public TextractDataModel(IEnumerable<Block> blocks)
    {
        BlockMap = blocks.ToDictionary(a => a.Id);
        if (BlockMap != null && BlockMap.Count > 0)
        {
            Initialize();
        }
    }

    //Initialization
    private void Initialize()
    {

        Queries = BlockMap.Values.Where(b => b.BlockType == "QUERY").GroupBy(a => a.Query.Alias).ToDictionary(a => a.Key, b => b.ToList());

    }

    public Block? GetBlock(string id)
    {
        if (BlockMap.TryGetValue(id, out var block))
        {
            return block;
        }
        return null;
    }

    public IEnumerable<Block> GetQueryResults(string queryAlias)
    {
        if (!string.IsNullOrEmpty(queryAlias) && Queries.TryGetValue(queryAlias, out var blocks))
        {
            foreach(var blockId in blocks.SelectMany(a => a.GetRelationshipsByType("ANSWER"))) 
            {
                if(BlockMap.TryGetValue(blockId, out var answerBlock))
                {
                    yield return answerBlock;
                }
                
            }

        }
    }
}
