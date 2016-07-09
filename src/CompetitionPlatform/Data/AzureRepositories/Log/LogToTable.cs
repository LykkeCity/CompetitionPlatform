using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.WindowsAzure.Storage.Table;
using AzureStorage.Tables;

namespace CompetitionPlatform.Data.AzureRepositories.Log
{
    public class LogEntity : TableEntity
    {

        public static string GeneratePartitionKey(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public DateTime DateTime { get; set; }
        public string Level { get; set; }
        public string Component { get; set; }
        public string Process { get; set; }
        public string Context { get; set; }
        public string Type { get; set; }
        public string Stack { get; set; }
        public string Msg { get; set; }

        public static LogEntity Create(string level, string component, string process, string context, string type, string stack, string msg, DateTime dateTime)
        {
            return new LogEntity
            {
                PartitionKey = GeneratePartitionKey(dateTime),
                DateTime = dateTime,
                Level = level,
                Component = component,
                Process = process,
                Context = context,
                Type = type,
                Stack = stack,
                Msg = msg
            };
        }

    }

    public class LogToTable : ILog
    {
        private readonly IAzureTableStorage<LogEntity> _tableStorage;

        public LogToTable(IAzureTableStorage<LogEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }


        private async Task Insert(string level, string component, string process, string context, string type, string stack,
            string msg, DateTime? dateTime)
        {
            var dt = dateTime ?? DateTime.UtcNow;
            var newEntity = LogEntity.Create(level, component, process, context, type, stack, msg, dt);
            await _tableStorage.InsertAndGenerateRowKeyAsTimeAsync(newEntity, dt);
        }

        public Task WriteInfo(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("info", component, process, context, null, null, info, dateTime);
        }

        public Task WriteWarning(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("warning", component, process, context, null, null, info, dateTime);
        }

        public Task WriteError(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("error", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        public Task WriteFatalError(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("fatalerror", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        public int Count { get { return 0; } }
    }
}
