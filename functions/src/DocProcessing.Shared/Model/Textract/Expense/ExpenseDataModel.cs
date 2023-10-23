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


    //Initialization
    private void Initialize()
    {


    }
}