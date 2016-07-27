using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public interface IProjectResultRepository
    {
        Task InsertProjectResult(Stream stream, string projectId, string userName);
        Task<Stream> GetProjectResult(string projectId, string userName);
    }
}
