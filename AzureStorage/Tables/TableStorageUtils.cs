using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage.Tables
{
    public enum ToIntervalOption
    {
        IncludeTo,
        ExcludeTo
    }

    public enum RowKeyDateTimeFormat
    {
        Iso,
        Short
    }


    public static class TableStorageUtils
    {



        public const int Conflict = 409;

        public static string ToDateTimeMask(this RowKeyDateTimeFormat format)
        {
            return format == RowKeyDateTimeFormat.Short ? "yyyyMMddHHmmss" : "yyyy-MM-dd HH:mm:ss";
        }

        public static string ToDateTimeSuffix(this int value, RowKeyDateTimeFormat format)
        {
            return format == RowKeyDateTimeFormat.Short ? value.ToString("000") : '.'+value.ToString("000") ;
        }

        public static bool HandleStorageException(this StorageException storageException, IEnumerable<int> notLogCodes)
        {
            return notLogCodes.Any(notLogCode => storageException.RequestInformation.HttpStatusCode == notLogCode);
        }


        #region Inserts

        public static async Task<T> InsertAndGenerateRowKeyAsDateTimeAsync<T>(this IAzureTableStorage<T> table, T entity, DateTime dateTime, RowKeyDateTimeFormat rowKeyDateTimeFormat = RowKeyDateTimeFormat.Iso) where T : ITableEntity, new()
        {
            var dt = dateTime.ToString(rowKeyDateTimeFormat.ToDateTimeMask());
            var no = 0;

            while (true)
            {
                entity.RowKey = dt + no.ToDateTimeSuffix(rowKeyDateTimeFormat);

                try
                {
                    await table.InsertAsync(entity, Conflict);
                    return entity;

                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions[0] is StorageException)
                    {
                        var se = e.InnerExceptions[0] as StorageException;
                        if (se.RequestInformation.HttpStatusCode != Conflict)
                            throw;
                    }
                    else throw;
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode != Conflict)
                        throw;
                }

                if (no == 999)
                    throw new Exception("Can not insert record: " + PrintItem(entity));
                no++;
            }

        }

        public static async Task<T> InsertAndCheckRowKeyAsync<T>(this IAzureTableStorage<T> table, T entity, Func<string> generateRowKey) where T : ITableEntity, new()
        {
            var no = 0;

            while (true)
            {
                entity.RowKey = generateRowKey();

                try
                {
                    await table.InsertAsync(entity);
                    return entity;

                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions[0] is StorageException)
                    {
                        var se = e.InnerExceptions[0] as StorageException;
                        if (se.RequestInformation.HttpStatusCode != Conflict)
                            throw;
                    }
                    else throw;
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode != Conflict)
                        throw;
                }
                if (no == 999)
                    throw new Exception("Can not insert record InsertAndCheckRowKey: " + PrintItem(entity));
                no++;
            }

        }


        public static async Task<T> InsertAndGenerateRowKeyAsTimeAsync<T>(this IAzureTableStorage<T> table, T entity, DateTime dateTime) where T : ITableEntity, new()
        {
            var dt = dateTime.ToString("HH:mm:ss");
            var no = 0;

            while (true)
            {
                entity.RowKey = dt + '.' + no.ToString("000");

                try
                {
                    await table.InsertAsync(entity);
                    return entity;

                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions[0] is StorageException)
                    {
                        var se = e.InnerExceptions[0] as StorageException;
                        if (se.RequestInformation.HttpStatusCode != Conflict)
                            throw;
                    }
                    else throw;
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode != Conflict)
                        throw;
                }
                if (no == 999)
                    throw new Exception("Can not insert record: " + PrintItem(entity));
                no++;
            }

        }

        #endregion Inserts


        public static class QueryGenerator<T> where T : ITableEntity, new()
        {
            private static string GenerateRowFilterString(string rowKeyFrom, string rowKeyTo, ToIntervalOption intervalOption)
            {
                if (intervalOption == ToIntervalOption.IncludeTo)
                    return
                        "RowKey " + QueryComparisons.GreaterThanOrEqual + " '" + rowKeyFrom + "' and " +
                        "RowKey " + QueryComparisons.LessThanOrEqual + " '" + rowKeyTo + "'";

                return
                    "RowKey " + QueryComparisons.GreaterThanOrEqual + " '" + rowKeyFrom + "' and " +
                    "RowKey " + QueryComparisons.LessThan + " '" + rowKeyTo + "'";

            }

            private static string ConvertDateTimeToString(DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-dd") + " 00:00:00.000";
            }

            public static class PartitionKeyOnly
            {


                public static TableQuery<T> GetTableQuery(string partitionKey)
                {
                    var sqlFilter =
                        "PartitionKey " + QueryComparisons.Equal + " '" + partitionKey + "'";

                    return new TableQuery<T>().Where(sqlFilter);
                }

                /// <summary>
                /// Генерация запроса-диапазона только по PartitionKey
                /// </summary>
                /// <param name="from">Partition from</param>
                /// <param name="to">Partition to</param>
                /// <param name="intervalOption">Включить участок to</param>
                /// <returns></returns>
                public static TableQuery<T> BetweenQuery(string @from, string to, ToIntervalOption intervalOption)
                {
                    var sqlFilter = intervalOption == ToIntervalOption.IncludeTo

                            ? "PartitionKey " + QueryComparisons.GreaterThanOrEqual + " '" + @from + "' and " +
                              "PartitionKey " + QueryComparisons.LessThanOrEqual + " '" + to + "'"

                            : "PartitionKey " + QueryComparisons.GreaterThanOrEqual + " '" + @from + "' and " +
                              "PartitionKey " + QueryComparisons.LessThan + " '" + to + "'";

                    return new TableQuery<T>().Where(sqlFilter);
                }

                public static TableQuery<T> BetweenQuery(DateTime @from, DateTime to, ToIntervalOption intervalOption)
                {
                    return BetweenQuery(ConvertDateTimeToString(@from), ConvertDateTimeToString(to), intervalOption);
                }



            }

            public static class RowKeyOnly
            {
                public static TableQuery<T> BetweenQuery(string rowKeyFrom, string rowKeyTo, ToIntervalOption intervalOption)
                {
                    var sqlFilter = GenerateRowFilterString(rowKeyFrom, rowKeyTo, intervalOption);
                    return new TableQuery<T>().Where(sqlFilter);
                }

                public static TableQuery<T> BetweenQuery(DateTime from, DateTime to, ToIntervalOption intervalOption)
                {
                    var sqlFilter = GenerateRowFilterString(ConvertDateTimeToString(@from), ConvertDateTimeToString(to), intervalOption);
                    return new TableQuery<T>().Where(sqlFilter);
                }

                public static TableQuery<T> GetTableQuery(string rowKey)
                {
                    var sqlFilter = "RowKey " + QueryComparisons.Equal + " '" + rowKey + "'";
                                    return new TableQuery<T>().Where(sqlFilter);

                }

                public static TableQuery<T> GetTableQuery(IEnumerable<string> rowKeys)
                {
                    var queryString = new StringBuilder();
                    foreach (var rowKey in rowKeys)
                    {
                        if (queryString.Length != 0)
                            queryString.Append(" or ");

                        queryString.Append("RowKey " + QueryComparisons.Equal + " '" + rowKey + "'");
                    }
                    return new TableQuery<T>().Where(queryString.ToString());
                }

            }


            public static TableQuery<T> BetweenQuery(string partitionKey, string rowKeyFrom, string rowKeyTo, ToIntervalOption intervalOption)
            {

                var sqlFilter = "PartitionKey " + QueryComparisons.Equal + " '" + partitionKey + "' and " +
                                GenerateRowFilterString(rowKeyFrom, rowKeyTo, intervalOption);

                return new TableQuery<T>().Where(sqlFilter);
            }

            public static TableQuery<T> BetweenQuery(string partitionKey, DateTime rowKeyFrom, DateTime rowKeyTo, ToIntervalOption intervalOption)
            {
                var sqlFilter = "PartitionKey " + QueryComparisons.Equal + " '" + partitionKey + "' and " +
                                GenerateRowFilterString(ConvertDateTimeToString(rowKeyFrom), ConvertDateTimeToString(rowKeyTo), intervalOption);

                return new TableQuery<T>().Where(sqlFilter);
            }

            public static TableQuery<T> BetweenQuery(IEnumerable<string> partitionKeys, DateTime rowKeyFrom, DateTime rowKeyTo, ToIntervalOption intervalOption)
            {
                var partitions = new StringBuilder();
                foreach (var partitionKey in partitionKeys)
                {
                    if (partitions.Length>0)
                        partitions.Append(" or ");

                    partitions.Append("PartitionKey " + QueryComparisons.Equal + " '" + partitionKey + "'");
                }

                var sqlFilter = "("+partitions+ ") and " +
                                GenerateRowFilterString(ConvertDateTimeToString(rowKeyFrom), ConvertDateTimeToString(rowKeyTo), intervalOption);

                return new TableQuery<T>().Where(sqlFilter);
            }


            public static TableQuery<T> MultiplePartitionKeys(params string[] partitionKeys)
            {

                var partitionKeysString = new StringBuilder();

                foreach (var rowKey in partitionKeys)
                {
                    if (partitionKeysString.Length > 0)
                        partitionKeysString.Append(" or ");
                    partitionKeysString.Append("PartitionKey " + QueryComparisons.Equal + " '" + rowKey + "'");
                }
                var sqlFilter = partitionKeysString.ToString();
                return new TableQuery<T>().Where(sqlFilter);
            }


            public static TableQuery<T> MultipleRowKeys(string partitionKey, params string[] rowKeys)
            {

                var rowKeysString = new StringBuilder();

                foreach (var rowKey in rowKeys)
                {
                    if (rowKeysString.Length > 0)
                        rowKeysString.Append(" or ");
                    rowKeysString.Append("RowKey " + QueryComparisons.Equal + " '" + rowKey + "'");
                }
                var sqlFilter = "PartitionKey " + QueryComparisons.Equal + " '" + partitionKey + "' and (" + rowKeysString + ")";
                return new TableQuery<T>().Where(sqlFilter);
            }

            public static TableQuery<T> MultipleKeys(IEnumerable<Tuple<string,string>> keys)
            {

                var sqlFilter = new StringBuilder();

                foreach (var key in keys)
                {
                    sqlFilter.Append("PartitionKey " + QueryComparisons.Equal + " '" + key.Item1 + "' and RowKey " + QueryComparisons.Equal + " '" + key.Item2 + "'");

                    if (sqlFilter.Length > 0)
                        sqlFilter.Append(" or ");
                }
                return new TableQuery<T>().Where(sqlFilter.ToString());
            }

            public static TableQuery<T> RangeQuery(string partitionFrom, string partitionTo, string rowKey,
                ToIntervalOption intervalOption)
            {
                var sqlFilter = intervalOption == ToIntervalOption.IncludeTo

                    ? "PartitionKey " + QueryComparisons.GreaterThanOrEqual + " '" + partitionFrom + "' and " +
                      "PartitionKey " + QueryComparisons.LessThanOrEqual + " '" + partitionTo + "'"

                    : "PartitionKey " + QueryComparisons.GreaterThanOrEqual + " '" + partitionFrom + "' and " +
                      "PartitionKey " + QueryComparisons.LessThan + " '" + partitionTo + "'";

                return
                    new TableQuery<T>().Where(sqlFilter + " and RowKey " + QueryComparisons.Equal + " '" + rowKey + "'");

            }
        }

        public static string PrintItem(object item)
        {
            if (item is string)
                return item as string;

            var stringBuilder = new StringBuilder();

            foreach (var propertyInfo in item.GetType().GetProperties().Where(propertyInfo => propertyInfo.CanRead && propertyInfo.CanWrite))

                stringBuilder.Append(propertyInfo.Name + "=[" + propertyInfo.GetValue(item, null) + "];");

            return stringBuilder.ToString();
        }

        public static IEnumerable<T> ApplyFilter<T>(IEnumerable<T> data, Func<T, bool> filter)
        {
            return filter == null ? data : data.Where(filter);
        }




        public static async Task<T> ModifyOrCreateAsync<T>(this IAzureTableStorage<T> tableStorage,
            string partitionKey, string rowKey, Func<T> create, Action<T> update) where T : ITableEntity, new()
        {

            for (var i = 0; i < 15; i++)
            {
                try
                {

                    var entity = await tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
                    {
                        update(itm);
                        return itm;
                    });

                    if (entity != null) return entity;

                    entity = create();
                    await tableStorage.InsertAsync(entity);
                    return entity;
                }
                catch (Exception)
                {

                }
            }
            throw new Exception("Can not modify or update entity: "+PrintItem(create()));
        }



        public static Task<IEnumerable<T>> WhereAsync<T>(this IAzureTableStorage<T> tableStorage, string partitionKey,
            DateTime from, DateTime to, ToIntervalOption intervalOption, Func<T, bool> filter = null)
            where T : ITableEntity, new()
        {
            var rangeQuery = QueryGenerator<T>.BetweenQuery(partitionKey, from, to, intervalOption);

            return filter == null
                ? tableStorage.ExecuteQueryAsync(rangeQuery)
                : tableStorage.ExecuteQueryAsync(rangeQuery, filter);

        }


        public static Task<IEnumerable<T>> WhereAsync<T>(this IAzureTableStorage<T> tableStorage, string partitionKey,
            int year , int month, Func<T, bool> filter = null)
            where T : ITableEntity, new()
        {

            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);

            var rangeQuery = QueryGenerator<T>.BetweenQuery(partitionKey, from, to, ToIntervalOption.ExcludeTo);

            return filter == null
                ? tableStorage.ExecuteQueryAsync(rangeQuery)
                : tableStorage.ExecuteQueryAsync(rangeQuery, filter);

        }

        public static Task WhereAsync<T>(this IAzureTableStorage<T> tableStorage, string partitionKey,
            int year, int month, Action<IEnumerable<T>> chunk = null)
            where T : ITableEntity, new()
        {

            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);

            var rangeQuery = QueryGenerator<T>.BetweenQuery(partitionKey, from, to, ToIntervalOption.ExcludeTo);

            return tableStorage.ExecuteQueryAsync(rangeQuery, chunk);

        }



        public async static Task<IEnumerable<T>> WhereAsync<T>(this IAzureTableStorage<T> tableStorage, IEnumerable<string> partitionKeys, DateTime from, DateTime to,
    ToIntervalOption intervalOption, Func<T, bool> filter = null)
    where T : ITableEntity, new()
        {
            var result = new List<T>();

            await Task.WhenAll(
                partitionKeys.Select(partitionKey => tableStorage.WhereAsync(partitionKey, from, to, intervalOption, filter)
                    .ContinueWith(task =>
                    {
                        lock (result) result.AddRange(task.Result);
                    }))
                );

            return result;
        }


    

        public static Task<IEnumerable<T>> GetDataRowKeyOnlyAsync<T>(this IAzureTableStorage<T> tableStorage, string rowKey)
                        where T : ITableEntity, new()
        {
            var query = QueryGenerator<T>.RowKeyOnly.GetTableQuery(rowKey);
            return tableStorage.ExecuteQueryAsync(query);
        }


        public static Task<T> ReplaceAsync<T>(this IAzureTableStorage<T> tableStorage, T item,
            Func<T, T> updateAction) where T : ITableEntity, new()
        {
            return tableStorage.ReplaceAsync(item.PartitionKey, item.RowKey, updateAction);
        }


        public static async Task<IEnumerable<T>> ScanAndGetList<T>(this IAzureTableStorage<T> tableStorage, string partitionKey, Func<T, bool> condition) 
            where T : class, ITableEntity, new() 
        {
            var result = new List<T>();
            
            var query = QueryGenerator<T>.PartitionKeyOnly.GetTableQuery(partitionKey);

            await tableStorage.ExecuteQueryAsync(query, items =>
            {
               var itemsToAdd = items.Where(condition);
                result.AddRange(itemsToAdd);
            });

            return result;
        }



        public static async Task<T> InsertOrModifyAsync<T>(this IAzureTableStorage<T> tableStorage, string partitionKey,
            string rowKey, Func<T> createNew, Func<T, T> modify)
            where T : class, ITableEntity, new()
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    var result = await tableStorage.ReplaceAsync(partitionKey,
                        rowKey, modify);

                    if (result != null)
                        return result;

                    result = createNew();
                    await tableStorage.InsertAsync(result);
                    return result;
                }
                catch (Exception)
                {

                }
            }

            throw new Exception("Can not insert or modify entity");
        }

    }
    

}
