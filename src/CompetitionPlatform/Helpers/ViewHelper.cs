using System;

namespace CompetitionPlatform.Helpers
{
    public static class ViewHelper
    {
        public static string GetAvatarOrDefault(string avatar)
        {
            return string.IsNullOrEmpty(avatar) ? "/public/img/user_default.svg" : avatar;
        }

        public static string GetProjectStageDates(DateTime startDate, DateTime endDate)
        {
            //If years are different, display them for both dates
            var sameYear = startDate.Year == endDate.Year;

            if (sameYear)
                return startDate.ToString("MMM d") + " - " + endDate.ToString("MMM d, yyyy");
            return startDate.ToString("MMM d, yyyy") + " - " + endDate.ToString("MMM d, yyyy");
        }
    }
}
