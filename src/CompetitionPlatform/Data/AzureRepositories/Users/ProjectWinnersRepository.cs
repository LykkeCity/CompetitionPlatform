using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

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

        internal void Update(IWinnerData src)
        {
            Place = src.Place;
            Budget = src.Budget;
            WinnerIdentifier = src.WinnerIdentifier;
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
                Score = src.Score,
                Budget = src.Budget,
                WinnerId = src.WinnerId,
                WinnerIdentifier = src.WinnerIdentifier,
                WinningScore = src.WinningScore
            };

            return result;
        }

        public string ProjectId { get; set; }
        public string WinnerId { get; set; }
        public string WinnerIdentifier { get; set; }
        public string FullName { get; set; }
        public int Place { get; set; }
        public string Result { get; set; }
        public int Votes { get; set; }
        public int Score { get; set; }
        public double? Budget { get; set; }
        public double WinningScore { get; set; }
        public string StreamsId { get; set; }
    }

    public class ProjectWinnersRepository : IProjectWinnersRepository
    {
        private readonly INoSQLTableStorage<WinnerEntity> _winnersStorage;

        public ProjectWinnersRepository(INoSQLTableStorage<WinnerEntity> winnersStorage)
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

        public async Task<int> GetWinnersCountAsync(string projectId)
        {
            var winners = await GetWinnersAsync(projectId);
            return winners.ToList().Count;
        }

        public async Task SaveAsync(IWinnerData winnerData)
        {
            var newEntity = WinnerEntity.Create(winnerData);
            await _winnersStorage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(IWinnerData winnerData)
        {
            var partitionKey = WinnerEntity.GeneratePartitionKey(winnerData.ProjectId);
            var rowKey = WinnerEntity.GenerateRowKey(winnerData.WinnerId);

            return _winnersStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(winnerData);
                return itm;
            });
        }

        public async Task DeleteAsync(string projectId, string userId)
        {
            var partitionKey = WinnerEntity.GeneratePartitionKey(projectId);
            var rowKey = WinnerEntity.GenerateRowKey(userId);

            await _winnersStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
