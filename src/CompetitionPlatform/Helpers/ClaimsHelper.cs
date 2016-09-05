using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using CompetitionPlatform.Models;

namespace CompetitionPlatform.Helpers
{
    public static class ClaimsHelper
    {
        public static CompetitionPlatformUser GetUser(IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            IEnumerable<Claim> claims = claimsIdentity.Claims;

            var claimsList = claims as IList<Claim> ?? claims.ToList();

            var firstName = claimsList.Where(c => c.Type == ClaimTypes.GivenName)
                   .Select(c => c.Value).SingleOrDefault();

            var lastName = claimsList.Where(c => c.Type == ClaimTypes.Surname)
                   .Select(c => c.Value).SingleOrDefault();

            var email = claimsList.Where(c => c.Type == ClaimTypes.Email)
                   .Select(c => c.Value).SingleOrDefault();

            var documents = claimsList.Where(c => c.Type == "documents")
                .Select(c => c.Value).SingleOrDefault();

            var user = new CompetitionPlatformUser
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Documents = documents
            };

            return user;
        }
    }
}
