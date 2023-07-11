using Amazon.S3.Model;
using Amazon.Textract.Model;
using DocProcessing.Shared.Model.Textract;
using System.Text.Json;


namespace DocProcessingTest;

[TestClass]
[DeploymentItem(@"TestAssets\TextractResults.json")]
public class TestTextractResult
{
    private TextractDataModel TextractData { get; set; }
    private TextractAnalysisResult TextractResult { get; set; }

    [TestInitialize()]
    public async Task Setup()
    {
        using FileStream jsonStream = File.OpenRead(@"TextractResults.json");
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
        var queryResultPatientName = TextractData.GetQueryResults("patientname");

        Assert.IsTrue(queryResultPatientName.Count() == 2); 

        Assert.AreEqual(queryResultPatientName.Where(a => a.Text == "Edward Sang").Count(), 1);

        Assert.AreEqual(queryResultPatientName.Where(a => a.Text == "Denis Roegel").Count(), 1);

        var queryResultDateOfService = TextractData.GetQueryResults("dateofservice");

        Assert.AreEqual(queryResultDateOfService.Where(a => a.Text == "20 July 1865").Count(), 1);

        Assert.AreEqual(queryResultDateOfService.Where(a => a.Text == "11 january 2021").Count(), 1);
    }

    [TestMethod("Get Invalid Query Result")]
    public void TestInvalidQueryResults()
    {
        var queryResultPatientName = TextractData.GetQueryResults("baadvalue");

        Assert.IsTrue(queryResultPatientName.Count() == 0);
    }

}