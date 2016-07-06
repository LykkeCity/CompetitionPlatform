using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage.Tables
{

    public class AzureTableStorage<T>: IAzureTableStorage<T> where T : class, ITableEntity, new()
    {
        private readonly string _connstionString;
        private readonly string _tableName;
        private readonly ILog _log;

        private CloudStorageAccount _cloudStorageAccount;

        private CloudTable _table;

        private Task CreateTableIfNoExistsAsync()
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(_connstionString);
            var cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            _table = cloudTableClient.GetTableReference(_tableName);
           return _table.CreateIfNotExistsAsync();
        }

        private CloudTable GetTable()
        {
            if (_table == null)
                CreateTableIfNoExistsAsync().Wait();

            return _table;
        }


        public AzureTableStorage(string connstionString, string tableName, ILog log)
        {
            _connstionString = connstionString;
            _tableName = tableName;
            _log = log;
        }


        private async Task ExecuteQueryAsync(string processName, TableQuery<T> rangeQuery, Func<T, bool> filter, Func<IEnumerable<T>, Task> yieldData)
        {

            try
            {
                TableContinuationToken tableContinuationToken = null;
                var table = GetTable();
                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(rangeQuery, tableContinuationToken);
                    tableContinuationToken = queryResponse.ContinuationToken;
                    await yieldData(TableStorageUtils.ApplyFilter(queryResponse.Results, filter));
                }
                while (tableContinuationToken != null);

            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, processName, rangeQuery.FilterString ?? "[null]", ex).Wait();
                throw;
            }

        }

        private async Task ExecuteQueryAsync(string processName, TableQuery<T> rangeQuery, Func<T, bool> filter, Func<IEnumerable<T>, bool> yieldData)
        {

            try
            {
                TableContinuationToken tableContinuationToken = null;
                var table = GetTable();
                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(rangeQuery, tableContinuationToken);
                    tableContinuationToken = queryResponse.ContinuationToken;
                   var shouldWeContinue = yieldData(TableStorageUtils.ApplyFilter(queryResponse.Results, filter));
                    if (!shouldWeContinue)
                        break;
                }
                while (tableContinuationToken != null);

            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, processName, rangeQuery.FilterString ?? "[null]", ex).Wait();
                throw;
            }

        }

        private async Task ExecuteQueryAsync2(string processName, TableQuery<T> rangeQuery, Func<T, Task<bool>> filter, Func<T, bool> yieldData)
        {

            try
            {
                TableContinuationToken tableContinuationToken = null;
                var table = GetTable();
                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(rangeQuery, tableContinuationToken);
                    tableContinuationToken = queryResponse.ContinuationToken;

                    foreach (var itm in queryResponse.Results)
                    {
                        if (filter == null || await filter(itm))
                        {
                            var shouldWeContinue = yieldData(itm);
                            if (!shouldWeContinue)
                                return ;
                        }
                    }

                }
                while (tableContinuationToken != null);

            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, processName, rangeQuery.FilterString ?? "[null]", ex).Wait();
                throw;
            }

        }



        private void HandleException(T item, Exception ex, IEnumerable<int> notLogCodes)
        {
            var storageException = ex as StorageException;
            if (storageException != null)
            {
                if (!storageException.HandleStorageException(notLogCodes))
                {
                     // Если этот эксепшн не обработан, то логируем его
                    _log?.WriteFatalError("Table storage: " + _tableName, "Insert item",
                        TableStorageUtils.PrintItem(item), ex);
                }
            }
            else
            {
                _log?.WriteFatalError("Table storage: " + _tableName, "Insert item", TableStorageUtils.PrintItem(item), ex);
            }
        }

        public virtual async Task InsertAsync(T item, params int[] notLogCodes)
        {
            try
            {
                await GetTable().ExecuteAsync(TableOperation.Insert(item));
            }
            catch (Exception ex)
            {
                HandleException(item, ex, notLogCodes);
                throw;
            }
        }


        public async Task InsertOrMergeAsync(T item)
        {
            try
            {

                await GetTable().ExecuteAsync(TableOperation.InsertOrMerge(item));
            }
            catch (Exception ex)
            {
                if (_log != null)
                 await _log.WriteFatalError("Table storage: " + _tableName, "InsertOrMerge item", TableStorageUtils.PrintItem(item), ex);
            }
        }


        public async Task<T> ReplaceAsync(string partitionKey, string rowKey, Func<T, T> replaceAction)
        {
            object itm = "Not read";
            try
            {
                while (true)
                {
                    try
                    {
                        var entity = await GetDataAsync(partitionKey, rowKey);
                        if (entity != null)
                        {
                            var result = replaceAction(entity);
                            itm = result;
                            if (result != null)
                                await GetTable().ExecuteAsync(TableOperation.Replace(result));

                            return result;
                        }

                        return null;

                    }
                    catch (StorageException e)
                    {
                        // Если поймали precondition fall = 412, значит в другом потоке данную сущность успели поменять
                        // - нужно повторить операцию, пока не исполнится без ошибок
                        if (e.RequestInformation.HttpStatusCode != 412)
                            throw;
                    }
                }

            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, "Replace item", TableStorageUtils.PrintItem(itm), ex).Wait();
                throw;
            }
        }

        public async Task<T> MergeAsync(string partitionKey, string rowKey, Func<T, T> mergeAction)
        {
            object itm = "Not read";

            try
            {
                while (true)
                {
                    try
                    {
                        var entity = await GetDataAsync(partitionKey, rowKey);
                        if (entity != null)
                        {
                            var result = mergeAction(entity);
                            itm = result;
                            if (result != null)
                                await GetTable().ExecuteAsync(TableOperation.Merge(result));

                            return result;
                        }
                        return null;
                    }
                    catch (StorageException e)
                    {
                        // Если поймали precondition fall = 412, значит в другом потоке данную сущность успели поменять
                        // - нужно повторить операцию, пока не исполнится без ошибок
                        if (e.RequestInformation.HttpStatusCode != 412)
                            throw;
                    }
                }

            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, "Replace item", TableStorageUtils.PrintItem(itm), ex).Wait();
                throw;
            }
        }

        public Task InsertOrReplaceBatchAsync(IEnumerable<T> entites)
        {
            var operationsBatch = new TableBatchOperation();

            foreach (var entity in entites)
                operationsBatch.Add(TableOperation.InsertOrReplace(entity));

            return GetTable().ExecuteBatchAsync(operationsBatch);
        }


        public virtual async Task InsertOrReplaceAsync(T item)
        {
            try
            {
               await GetTable().ExecuteAsync(TableOperation.InsertOrReplace(item));
            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, "InsertOrReplace item", TableStorageUtils.PrintItem(item),
                    ex).Wait();
                throw;
            }
        }

        public virtual async Task DeleteAsync(T item)
        {
            try
            {
                await GetTable().ExecuteAsync(TableOperation.Delete(item));
            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, "Delete item", TableStorageUtils.PrintItem(item), ex).Wait();
                throw;
            }

        }

        public async Task<T> DeleteAsync(string partitionKey, string rowKey)
        {
            var itm = await GetDataAsync(partitionKey, rowKey);
            if (itm != null)
              await DeleteAsync(itm);
            return itm;
        }

        private TableQuery<T> CompileTableQuery(string partition)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition);
            return  new TableQuery<T>().Where(filter);
        }

        public async Task<IEnumerable<T>> GetDataAsync(string partitionKey, IEnumerable<string> rowKeys,
            int pieceSize = 15, Func<T, bool> filter = null)
        {

            var result = new List<T>();

            await Task.WhenAll(
                rowKeys.ToPieces(pieceSize).Select(piece =>
                    ExecuteQueryAsync("GetDataWithMutipleRows",
                        TableStorageUtils.QueryGenerator<T>.MultipleRowKeys(partitionKey, piece.ToArray()), filter,
                        items =>
                        {
                            lock (result)
                                result.AddRange(items);
                            return true;
                        })
                    )
                );

            return result;
        }

        public async Task<IEnumerable<T>> GetDataAsync(IEnumerable<string> partitionKeys, int pieceSize = 100, Func<T, bool> filter = null)
        {

            var result = new List<T>();

            await Task.WhenAll(
                partitionKeys.ToPieces(pieceSize).Select(piece =>
                    ExecuteQueryAsync("GetDataWithMutiplePartitionKeys",
                        TableStorageUtils.QueryGenerator<T>.MultiplePartitionKeys(piece.ToArray()), filter,
                        items =>
                        {
                            lock (result)
                                result.AddRange(items);
                            return true;
                        })
                    )
                );

            return result;
        }

        public async Task<IEnumerable<T>> GetDataAsync(IEnumerable<Tuple<string, string>> keys, int pieceSize = 100, Func<T, bool> filter = null)
        {
            var result = new List<T>();

            await Task.WhenAll(
                keys.ToPieces(pieceSize).Select(piece =>
                    ExecuteQueryAsync("GetDataWithMoltipleKeysAsync",
                        TableStorageUtils.QueryGenerator<T>.MultipleKeys(piece), filter,
                        items =>
                        {
                            lock (result)
                                result.AddRange(items);
                            return true;
                        })
                    )
                );

            return result;
        }

        public Task GetDataByChunksAsync(Func<IEnumerable<T>, Task> chunks)
        {
            var rangeQuery = new TableQuery<T>();
            return ExecuteQueryAsync("GetDataByChunksAsync", rangeQuery, null, async itms =>
            {
                await chunks(itms);
            });

        }

        public Task GetDataByChunksAsync(Action<IEnumerable<T>> chunks)
        {
            var rangeQuery = new TableQuery<T>();
            return ExecuteQueryAsync("GetDataByChunksAsync", rangeQuery, null, itms =>
            {
                chunks(itms);
                return true;
            });
        }

        public Task GetDataByChunksAsync(string partitionKey, Action<IEnumerable<T>> chunks)
        {
            var query = CompileTableQuery(partitionKey);
            return ExecuteQueryAsync(query, chunks);
        }

        public virtual async Task<T> GetDataAsync(string partition, string row)
        {

            try
            {
                var retrieveOperation = TableOperation.Retrieve<T>(partition, row);
                var retrievedResult = await GetTable().ExecuteAsync(retrieveOperation);
                return (T)retrievedResult.Result;
            }
            catch (Exception ex)
            {
                _log?.WriteFatalError("Table storage: " + _tableName, "Get item async by partId and rowId",
                    "partitionId=" + partition + "; rowId=" + row, ex).Wait();
                throw;
            }
        }



        public async Task<IEnumerable<T>> GetDataRowKeysOnlyAsync(IEnumerable<string> rowKeys)
        {
            var query = TableStorageUtils.QueryGenerator<T>.RowKeyOnly.GetTableQuery(rowKeys);
            var result = new List<T>();

            await ExecuteQueryAsync("GetDataRowKeysOnlyAsync", query, null, chunk =>
            {
                result.AddRange(chunk);
                return Task.FromResult(0);
            });

            return result;
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync(TableQuery<T> rangeQuery, Func<T, Task<bool>> filter)
        {
            var result = new List<T>();
            await ExecuteQueryAsync2("WhereAsyncc", rangeQuery, filter, itm =>
            {
                result.Add(itm);
                return true;
            });

            return result;
        }


        public async Task<IList<T>> GetDataAsync(Func<T, bool> filter = null)
        {
            var rangeQuery = new TableQuery<T>();
            var result = new List<T>();
            await ExecuteQueryAsync("GetDataAsync", rangeQuery, filter, itms =>
            {
                result.AddRange(itms);
                return true;
            });
            return result;
        }

        public virtual async Task<IEnumerable<T>> GetDataAsync(string partition, Func<T, bool> filter = null)
        {
            var rangeQuery = CompileTableQuery(partition);
            var result = new List<T>();

            await ExecuteQueryAsync("GetDataAsync", rangeQuery, filter, itms =>
            {
                result.AddRange(itms);
                return true;
            });

            return result;

        }
         
        public async Task<IEnumerable<T>> ExecuteQueryAsync(TableQuery<T> rangeQuery, Func<T, bool> filter = null)
        {
            var result = new List<T>();
            await ExecuteQueryAsync("WhereAsync", rangeQuery, filter, itms =>
            {
                result.AddRange(itms);
                return true;
            });
            return result;
        }

        public Task ExecuteQueryAsync(TableQuery<T> rangeQuery, Action<IEnumerable<T>> yieldResult)
        {
            return ExecuteQueryAsync("ExecuteAsync", rangeQuery, null, itms =>
            {
                yieldResult(itms);
                return true;
            });
        }

        public Task CreateIfNotExistsAsync(T item)
        {
           return GetTable().CreateIfNotExistsAsync();
        }

        public Task DoBatchAsync(TableBatchOperation batch)
        {
         return GetTable().ExecuteBatchAsync(batch);  
        }
        
        public async Task<T> FirstOrNullViaScanAsync(string partitionKey, Func<T, bool> filter){
            
            T result = default(T);
            var rangeQuery = CompileTableQuery(partitionKey);
            await ExecuteQueryAsync("FirstOrNullViaScanAsync", rangeQuery, null, itms =>
            {
                result = itms.FirstOrDefault(filter);
                return result == null;
            });            
            
            return result;
            
        }
        
        

    }
}
