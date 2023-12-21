using Amazon.S3.Model.Internal.MarshallTransformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class ExpenseDataModel
{
    Dictionary<int, ExpenseDocument> ExpenseDocuments { get; }
    private Dictionary<int, HashSet<string>> _groupSummaryFields = new();

    public ExpenseDataModel(IEnumerable<ExpenseDocument> docs)
    {
        ExpenseDocuments = docs.ToDictionary(a => a.ExpenseIndex.GetValueOrDefault()) ?? new();
        if (ExpenseDocuments.Count > 0)
        {
            Initialize();
        }
    }

    public IEnumerable<int> GetExpenseReportIndexes() => ExpenseDocuments.Keys;

    public IEnumerable<string> GetGroupIds(int expenseDocId)
    {
        if (!ExpenseDocuments.ContainsKey(expenseDocId))
        {
            return Enumerable.Empty<string>();
        }

        if (!_groupSummaryFields.ContainsKey(expenseDocId))
        {
            _groupSummaryFields[expenseDocId] = ExpenseDocuments[expenseDocId].SummaryFields
            .Where(g => g.GroupProperties is not null)
            .SelectMany(g => g.GroupProperties)
            .Select(a => a.Id)
            .ToHashSet();
        };
        
        return _groupSummaryFields[expenseDocId];
    }

    public IEnumerable<string> GetTypesForGroup(int expenseDocId, string groupId)
    {
        if (!ExpenseDocuments.ContainsKey(expenseDocId) || !GetGroupIds(expenseDocId).Contains(groupId))
        {
            return Enumerable.Empty<string>();
        }

        return ExpenseDocuments[expenseDocId].SummaryFields
            .Where(g => g.GroupProperties is not null && g.GroupProperties.Any(a => a.Id == groupId))
            .SelectMany(g => g.GroupProperties)
            .SelectMany(g => g.Types)
            .ToHashSet();            
    }
   
    public IEnumerable<SummaryField> GetGroupSummaryFields(int expenseDocId, string groupId, string type)
    {
        if (!ExpenseDocuments.ContainsKey(expenseDocId) || !GetGroupIds(expenseDocId).Contains(groupId))
        {
            return Enumerable.Empty<SummaryField>();
        }

        return ExpenseDocuments[expenseDocId].SummaryFields
            .Where(g => g.GroupProperties is not null && g.GroupProperties.Any(a => a.Id == groupId && a.Types.Any(b => b == type)));         
    }

    public IEnumerable<SummaryField> GetScalarSummaryFields(int expenseDocId)
    {
        if (!ExpenseDocuments.ContainsKey(expenseDocId))
        {
            return Enumerable.Empty<SummaryField>();
        }

        return ExpenseDocuments[expenseDocId].SummaryFields.Where(g => g.GroupProperties is null);
    }

    //Initialization
    private void Initialize()
    {


    }
}