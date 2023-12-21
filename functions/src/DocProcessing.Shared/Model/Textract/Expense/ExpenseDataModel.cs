using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class ExpenseDataModel
{


    Dictionary<int, ExpenseDocument> ExpenseDocuments { get; }

    public ExpenseDataModel(IEnumerable<ExpenseDocument> docs)
    {
        ExpenseDocuments = docs.ToDictionary(a => a.ExpenseIndex.GetValueOrDefault()) ?? new();
        if (ExpenseDocuments.Count > 0)
        {
            Initialize();
        }
    }

    public IEnumerable<int> GetExpenseReportIndexes() => ExpenseDocuments.Keys;

    public IEnumerable<SummaryField> GetScalarSummaryFields(int expenseDocId)
    {
        if(!ExpenseDocuments.ContainsKey(expenseDocId))
        {
            return Enumerable.Empty<SummaryField>();
        }

        return ExpenseDocuments[expenseDocId].SummaryFields.Where(g => g.GroupProperties is null);
    }

    public IEnumerable<string> GetGroupIds(int expenseDocId)
    {
        if (!ExpenseDocuments.ContainsKey(expenseDocId))
        {
            return Enumerable.Empty<string>();
        }

        return ExpenseDocuments[expenseDocId].SummaryFields
            .Where(g => g.GroupProperties is not null)
            .SelectMany(g => g.GroupProperties)
            .Select(a => a.Id)
            .ToHashSet();
    }
   

    //Initialization
    private void Initialize()
    {


    }
}