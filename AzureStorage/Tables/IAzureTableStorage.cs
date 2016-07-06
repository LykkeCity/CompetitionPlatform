using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage.Tables
{

    public interface IAzureTableStorage<T>  where T : ITableEntity, new()
    {

        Task InsertAsync(T item, params int[] notLogCodes);

        Task InsertOrMergeAsync(T item);

        Task<T> ReplaceAsync(string partitionKey, string rowKey, Func<T, T> item);

        Task<T> MergeAsync(string partitionKey, string rowKey, Func<T, T> item);


        Task InsertOrReplaceBatchAsync(IEnumerable<T> entites);

        Task InsertOrReplaceAsync(T item);

        Task DeleteAsync(T item);

        Task<T> DeleteAsync(string partitionKey, string rowKey);

        Task CreateIfNotExistsAsync(T item);

        Task<T> GetDataAsync(string partition, string row);

        Task<IList<T>> GetDataAsync(Func<T, bool> filter = null);

        Task<IEnumerable<T>> GetDataAsync(string partitionKey, IEnumerable<string> rowKeys, int pieceSize = 100, Func<T, bool> filter = null);

        Task<IEnumerable<T>> GetDataAsync(IEnumerable<string> partitionKeys, int pieceSize = 100, Func<T, bool> filter = null);

        Task<IEnumerable<T>> GetDataAsync(IEnumerable<Tuple<string, string>> keys, int pieceSize = 100, Func<T, bool> filter = null);

        Task<IEnumerable<T>> GetDataAsync(string partition, Func<T, bool> filter = null);

        Task<IEnumerable<T>> GetDataRowKeysOnlyAsync(IEnumerable<string> rowKeys);
        
        Task<T> FirstOrNullViaScanAsync(string partitionKey, Func<T, bool> filter);

        Task<IEnumerable<T>> ExecuteQueryAsync(TableQuery<T> rangeQuery, Func<T, Task<bool>> filter);
        Task<IEnumerable<T>> ExecuteQueryAsync(TableQuery<T> rangeQuery, Func<T, bool> filter = null);
        Task ExecuteQueryAsync(TableQuery<T> rangeQuery, Action<IEnumerable<T>> yieldResult);

    }



}
