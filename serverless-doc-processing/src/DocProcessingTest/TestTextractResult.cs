using Amazon.S3.Model;
using Amazon.Textract.Model;
using DocProcessing.Shared.Model.Textract;
using System.Text.Json;


namespace DocProcessingTest;

[TestClass]
public class TestTextractResult
{
    private TextractDataModel TextractData { get; set; }
    private TextractAnalysisResult TextractResult { get; set; }


    [TestInitialize()]
    public async Task Setup()
    {
        using FileStream jsonStream = File.OpenRead(@"TestAssets/TextractResults.json");

        TextractResult = JsonSerializer.Deserialize<TextractAnalysisResult>(jsonStream);

        TextractData = new TextractDataModel(TextractResult.Blocks);

    }


    [TestMethod("Test Block Count")]
    public void TestBlockCount()
    {
        Assert.IsTrue(TextractResult.GetBlockCount() == 1000);

    }

    [TestMethod("Get Query Result")]
    public void TestQueryResults()
    {
        var queryResult = TextractData.GetQueryResults("patientname");

        Assert.IsTrue(queryResult.Count() == 2); 

        Assert.AreEqual(queryResult.Where(a => a.Text == "Edward Sang").Count(), 1);

        Assert.AreEqual(queryResult.Where(a => a.Text == "Denis Roegel").Count(), 1);
    }


}