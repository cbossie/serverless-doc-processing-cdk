using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Model.Internal.MarshallTransformations;
using DocProcessing.Shared.Model.Data;
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

        public async  Task<IEnumerable<DocumentQuery>> GetAllQueries()
        {
            var asyncData = DbContext.ScanAsync<DocumentQuery>(Enumerable.Empty<ScanCondition>());
            return await asyncData.GetRemainingAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<DocumentQuery>> GetQueries(IEnumerable<string> queryKeys)
        {
            if(queryKeys is null || !queryKeys.Any())
            {
                return await GetAllQueries().ConfigureAwait(false);
            }

            var batchGet = DbContext.CreateBatchGet<DocumentQuery>();
            foreach(var key in queryKeys) { batchGet.AddKey(key); }

            await batchGet.ExecuteAsync().ConfigureAwait(false);

            return batchGet.Results;

        }

        public string GenerateId(string? id = null)
        {
            return id ?? Guid.NewGuid().ToString("N");
        }

        public async Task<T> SaveData<T>(T data)
        {
            await DbContext.SaveAsync(data).ConfigureAwait(false);

            return data;
        }

        public async Task<T> GetData<T>(string id)
        {
            return await DbContext.LoadAsync<T>(id).ConfigureAwait(false);
        }
    }
}
