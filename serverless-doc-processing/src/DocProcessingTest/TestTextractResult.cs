using Amazon.Textract.Model;
using DocProcessing.Shared.Model.Textract;
using System.Text.Json;


namespace DocProcessingTest;

[TestClass]
public class TestTextractResult
{
    private TextractDataModel TextractData { get; set; }


    public TestTextractResult()
    {

    }

    [ClassInitialize()]
    public async Task Setup()
    { 
        using FileStream jsonStream = File.OpenRead(@"TestAssets/TextractResults.json");

        try
        {
            var blocks = JsonSerializer.Deserialize<TextractAnalysisResult>(jsonStream);

            TextractData = new TextractDataModel();
            TextractData.TextractBlocks.AddRange(blocks.Blocks);
        }
        catch(Exception ex)
        {

            int r = 5;
        }

        int a = 3;
    }


    [TestMethod]
    public void TestGetQueryData()
    {
        Assert.IsTrue(TextractData.TextractBlocks.Count == 10000);

    }

}