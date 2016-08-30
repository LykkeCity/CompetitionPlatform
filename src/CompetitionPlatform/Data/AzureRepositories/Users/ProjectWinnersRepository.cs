using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using AzureStorage.Tables;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class WinnerEntity : TableEntity, IWinnerData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string userId)
        {
            return userId;
        }

        public static WinnerEntity Create(IWinnerData src)
        {
            var result = new WinnerEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(src.WinnerId),
                FullName = src.FullName,
                Place = src.Place,
                Result = src.Result,
                Votes = src.Votes,
                Score = src.Score
            };

            return result;
        }

        public string ProjectId { get; set; }
        public string WinnerId { get; set; }
        public string FullName { get; set; }
        public int Place { get; set; }
        public string Result { get; set; }
        public int Votes { get; set; }
        public int Score { get; set; }
    }

    public class ProjectWinnersRepository : IProjectWinnersRepository
    {
        private readonly IAzureTableStorage<WinnerEntity> _winnersStorage;

        public ProjectWinnersRepository(IAzureTableStorage<WinnerEntity> winnersStorage)
        {
            _winnersStorage = winnersStorage;
        }

        public async Task<IWinnerData> GetAsync(string projectId, string userId)
        {
            var partitionKey = WinnerEntity.GeneratePartitionKey(projectId);
            var rowKey = WinnerEntity.GenerateRowKey(userId);

            return await _winnersStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IWinnerData>> GetWinnersAsync(string projectId)
        {
            var partitionKey = WinnerEntity.GeneratePartitionKey(projectId);

            return await _winnersStorage.GetDataAsync(partitionKey);
        }

        public async Task SaveAsync(IWinnerData winnerData)
        {
            var newEntity = WinnerEntity.Create(winnerData);

            await _winnersStorage.InsertAsync(newEntity);
        }
    }
}
