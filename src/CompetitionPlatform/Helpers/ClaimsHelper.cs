using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CompetitionPlatform.Helpers
{
    public static class ClaimsHelper
    {
        public static UserModel GetUser(IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            var claims = claimsIdentity.Claims;

            var claimsList = claims as IList<Claim> ?? claims.ToList();

            var firstName = claimsList.Where(c => c.Type == ClaimTypes.GivenName)
                .Select(c => c.Value).SingleOrDefault();

            var lastName = claimsList.Where(c => c.Type == ClaimTypes.Surname)
                .Select(c => c.Value).SingleOrDefault();

            var email = claimsList.Where(c => c.Type == ClaimTypes.Email)
                .Select(c => c.Value).SingleOrDefault();

            var documents = claimsList.Where(c => c.Type == "documents")
                .Select(c => c.Value).SingleOrDefault();

            var id = claimsList.Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault();

            var user = new UserModel
            {
                Id = id,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Documents = documents
            };

            return user;
        }

        public static string GetFirstName(IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            var claims = claimsIdentity.Claims;

            var claimsList = claims as IList<Claim> ?? claims.ToList();

            var firstName = claimsList.Where(c => c.Type == ClaimTypes.GivenName)
                .Select(c => c.Value)
                .SingleOrDefault();

            return firstName;
        }

        public static ClaimsIdentity UpdateFirstNameClaim(IIdentity identity, string name)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            var claims = claimsIdentity.Claims;
            var claimsList = claims as IList<Claim> ?? claims.ToList();
            var firstName = claimsList.SingleOrDefault(c => c.Type == ClaimTypes.GivenName);
            claimsIdentity.RemoveClaim(firstName);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, name));

            return claimsIdentity;
        }
    }
}