using DocProcessing.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Service;

public interface IDataService
{
    Task<IEnumerable<QueryData>> GetAllQueries();
    Task<IEnumerable<QueryData>> GetQueries(IEnumerable<string> queryKeys);

    string GenerateId(string? id = null);

    Task<T> GetData<T>(string id);

    Task<T> SaveData<T>(T data);
}
