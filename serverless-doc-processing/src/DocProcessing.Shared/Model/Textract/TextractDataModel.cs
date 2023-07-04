using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class TextractDataModel
{
    public List<Block> TextractBlocks { get; } = new();
}
