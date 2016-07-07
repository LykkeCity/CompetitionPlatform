using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories
{
    interface IProjectFileRepository
    {
        Task<string> InsertAttachment(Stream stream);
        Task<Stream> GetAttachment(string fileId);
    }
}
