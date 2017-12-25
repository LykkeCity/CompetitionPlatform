using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Helpers
{
    public static class ViewHelper
    {
        public static string GetAvatarOrDefault(string avatar)
        {
            return string.IsNullOrEmpty(avatar) ? "/public/img/user_default.svg" : avatar;
        }
    }
}
