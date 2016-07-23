using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface IProjectFileInfoData
    {
        string ProjectId { get;}
        string FileName { get; set; }
        string ContentType { get; set; }
    }
    public interface IProjectFileInfoRepository
    {
        Task<IProjectFileInfoData> GetAsync(string projectId);
        Task SaveAsync(IProjectFileInfoData fileInfoData);
    }
}
