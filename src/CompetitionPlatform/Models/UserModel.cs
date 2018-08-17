using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Helpers;

namespace CompetitionPlatform.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string GetFullName()
        {
            return FirstName + " " + LastName;
        }

        public static UserModel GetAuthenticatedUser(IIdentity identity)
        {
            return ClaimsHelper.GetUser(identity);
        }

        public static async void GenerateStreamsId(IStreamsIdRepository streamsIdRepository, string clientId)
        {
            var streamsId = await streamsIdRepository.GetOrCreateAsync(clientId);

            if (streamsId == null)
            {
                var newEntity = new StreamsIdEntity
                {
                    ClientId = clientId
                };

                await streamsIdRepository.SaveAsync(newEntity);
            }
        }
    }
}
