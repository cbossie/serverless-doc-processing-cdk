using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Model.Internal.MarshallTransformations;
using DocProcessing.Shared.Data;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Service
{
    public class DataService : IDataService
    {
        IDynamoDBContext DbContext { get; }

        public DataService(IDynamoDBContext dbContext)
        {
            DbContext = dbContext;
        }

        public async  Task<IEnumerable<QueryData>> GetAllQueries()
        {
            var asyncData = DbContext.ScanAsync<QueryData>(Enumerable.Empty<ScanCondition>());
            return await asyncData.GetRemainingAsync();
        }

        public async Task<IEnumerable<QueryData>> GetQueries(IEnumerable<string> queryKeys)
        {
            if(queryKeys is null || !queryKeys.Any())
            {
                return await GetAllQueries();
            }

            var batchGet = DbContext.CreateBatchGet<QueryData>();
            foreach(var key in queryKeys) { batchGet.AddKey(key); }

            await batchGet.ExecuteAsync();

            return batchGet.Results;

        }

        public string GenerateId(string? id = null)
        {
            return id ?? Guid.NewGuid().ToString("N");
        }

        public async Task<T> SaveData<T>(T data)
        {
            await DbContext.SaveAsync(data);

            return data;
        }
    }
}
