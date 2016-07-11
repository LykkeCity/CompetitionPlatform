using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface IProjectFileRepository
    {
        Task InsertProjectFile(Stream stream, string projectId);
        Task<Stream> GetProjectFile(string projectId);
    }
}
