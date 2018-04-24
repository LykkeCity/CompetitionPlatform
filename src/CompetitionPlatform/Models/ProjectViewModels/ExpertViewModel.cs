using CompetitionPlatform.Data.AzureRepositories.Expert;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ExpertViewModel : IProjectExpertData
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserIdentifier { get; set; }
        public string StreamsId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }


        public static ExpertViewModel Create(IProjectExpertData data)
        {
            return new ExpertViewModel{ 
                ProjectId = data.ProjectId,
                UserId = data.UserId,
                UserIdentifier = data.UserIdentifier,
                //StreamsId = data.StreamsId,
                FullName = data.FullName,
                Description = data.Description,
                Priority = data.Priority
            };
        }
    }
}