using System;
using CompetitionPlatform.Models;

namespace CompetitionPlatform.Helpers
{
    public static class StatusHelper
    {
        public static Status GetProjectStatusFromString(string status)
        {
            if (status == "CompetitionRegistration") return Status.Registration;
            if (status == "Implementation") return Status.Submission;
            return (Status)Enum.Parse(typeof(Status), status, true);
        }
    }
}
