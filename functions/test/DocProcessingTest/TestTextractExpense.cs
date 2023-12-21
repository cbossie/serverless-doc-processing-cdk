﻿using DocProcessing.Shared.Model.Textract.Expense;
using DocProcessing.Shared.Model.Textract.QueryAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.Routing;

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
        if (ExpenseResult is null)
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

    [TestMethod("Test Scalar Summary Fields")]
    public void TestScalarSummaryFields()
    {
        var summaryFields1 = ExpenseData?.GetScalarSummaryFields(1);
        Assert.IsTrue(summaryFields1?.Count() == 18);

        var summaryFields2 = ExpenseData?.GetScalarSummaryFields(2);
        Assert.IsTrue(summaryFields2?.Count() == 18);
    }

    [TestMethod("Test Group Summary Fields")]
    public void TestGroupSummaryFields()
    {
        var groupSummaryFields1a = ExpenseData?.GetGroupSummaryFields(1, "a0c3723b-bf53-4d02-8a23-358c63f0a6ae", "RECEIVER_SHIP_TO");
        Assert.IsTrue(groupSummaryFields1a?.Count() == 7);

        var groupSummaryFields1b = ExpenseData?.GetGroupSummaryFields(1, "84c257f3-3d27-42d1-924f-82d50564af1e", "VENDOR");
        Assert.IsTrue(groupSummaryFields1b?.Count() == 6);

        var groupSummaryFields2a = ExpenseData?.GetGroupSummaryFields(2, "b1c3723b-bf53-4d02-8a23-358c63f0a6ae", "RECEIVER_SHIP_TO_2");
        Assert.IsTrue(groupSummaryFields2a?.Count() == 7);

        var groupSummaryFields2b = ExpenseData?.GetGroupSummaryFields(2, "95d257f3-3d27-42d1-924f-82d50564af1e", "VENDOR_2");
        Assert.IsTrue(groupSummaryFields2b?.Count() == 6);

        var groupSummaryFieldsfail1 = ExpenseData?.GetGroupSummaryFields(2, "FailedValue", "FAILED_VALUE");
        Assert.IsTrue(groupSummaryFieldsfail1?.Count() == 0);

        var groupSummaryFieldsfail2 = ExpenseData?.GetGroupSummaryFields(1, "95d257f3-3d27-42d1-924f-82d50564af1e" ,"VENDOR_2");
        Assert.IsTrue(groupSummaryFieldsfail2?.Count() == 0);

        var groupSummaryFieldsfail3 = ExpenseData?.GetGroupSummaryFields(0, "FailedValue", "VENDOR");
        Assert.IsTrue(groupSummaryFieldsfail3?.Count() == 0);

    }

    [TestMethod("Test Get Types For Group")]
    public void TestGetTypesForGroup()
    {
        var group1Data = ExpenseData?.GetTypesForGroup(1, "a0c3723b-bf53-4d02-8a23-358c63f0a6ae");
        CollectionAssert.AreEquivalent(group1Data?.ToList(), new string[] { "RECEIVER_SHIP_TO" }); ;

        var group2Data = ExpenseData?.GetTypesForGroup(1, "84c257f3-3d27-42d1-924f-82d50564af1e");
        CollectionAssert.AreEquivalent(group2Data?.ToList(), new string[] { "VENDOR" }); ;

        var group3Data = ExpenseData?.GetTypesForGroup(2, "b1c3723b-bf53-4d02-8a23-358c63f0a6ae");
        CollectionAssert.AreEquivalent(group3Data?.ToList(), new string[] { "RECEIVER_SHIP_TO_2" }); ;

        var group4Data = ExpenseData?.GetTypesForGroup(2, "95d257f3-3d27-42d1-924f-82d50564af1e");
        CollectionAssert.AreEquivalent(group4Data?.ToList(), new string[] { "VENDOR_2" }); ;

        var group5Data = ExpenseData?.GetTypesForGroup(1, "FAIL_VALUE");
        Assert.IsTrue(group5Data?.Count() == 0);
    }

}