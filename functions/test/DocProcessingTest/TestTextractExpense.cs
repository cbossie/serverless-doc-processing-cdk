using DocProcessing.Shared.Model.Textract.Expense;
using DocProcessing.Shared.Model.Textract.QueryAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocProcessingTest;

[TestClass]
[DeploymentItem(@"TestAssets\ExpenseAnalysis.json")]
public class TestTextractExpense
{

    private ExpenseResult? ExpenseResult { get; set; }
    private ExpenseDataModel? ExpenseData { get; set; }

    [TestInitialize()]
    public void Setup()
    {
        using FileStream jsonStream = File.OpenRead(@"ExpenseAnalysis.json");
        if (jsonStream is null)
        {
            throw new ArgumentNullException(nameof(jsonStream));
        }
        ExpenseResult = JsonSerializer.Deserialize<ExpenseResult>(jsonStream);
        if(ExpenseResult is null)
        {
            throw new ArgumentNullException(nameof(ExpenseResult));
        }
        ExpenseData = new ExpenseDataModel(ExpenseResult.ExpenseDocuments);
    }

    [TestMethod("Test Expense Document Count")]
    public void TestExpenseDocumentCount()
    {
        Assert.IsTrue(ExpenseResult?.ExpenseDocuments.Count == 2);            
    }

    [TestMethod("Test Expense Document Indexes")]
    public void TestExpenseDocumentIndexes()
    {
        var indexes = ExpenseData?.GetExpenseReportIndexes();
        CollectionAssert.AreEquivalent(indexes?.ToList(), new int[] { 1, 2 });
    }

    [TestMethod("Test Group IDs")]
    public void TestGroupIds()
    {
        var groupIds1 = ExpenseData?.GetGroupIds(1);
        CollectionAssert.AreEquivalent(groupIds1?.ToList(), new string[] 
            { "a0c3723b-bf53-4d02-8a23-358c63f0a6ae", "84c257f3-3d27-42d1-924f-82d50564af1e" });

        var groupIds2 = ExpenseData?.GetGroupIds(2);
        CollectionAssert.AreEquivalent(groupIds2?.ToList(), new string[] 
            { "b1c3723b-bf53-4d02-8a23-358c63f0a6ae", "95d257f3-3d27-42d1-924f-82d50564af1e" });
    }


}
