using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models;
using Newtonsoft.Json;

namespace CompetitionPlatform.Helpers
{
    public static class NotificationMessageHelper
    {
        private const string MailType = "CompetitionPlatformMail:";

        public static string GenerateInitiativeMessage(IProjectData project, string userName, string email)
        {
            var model = new Initiative
            {
                FirstName = GetFirstNameFromFullName(userName),
                ProjectId = project.Id,
                ProjectStatus = Status.Initiative.ToString(),
                ProjectAuthorName = project.AuthorFullName,
                ProjectCreatedDate = project.Created,
                ProjectDescription = project.Description,
                ProjectFirstPrize = project.BudgetFirstPlace,
                ProjectName = project.Name,
                ProjectSecondPrize = project.BudgetSecondPlace ?? 0
            };

            var messageData = new InitiativeEmailMessageData
            {
                Subject = "Initiative",
                Sender = "Competition Platform",
                Model = model
            };

            var data = new InitiativeEmailData
            {
                EmailAddress = email,
                MessageData = messageData
            };

            var initiativeEmail = new InitiativeEmail
            {
                Data = data
            };

            var message = JsonConvert.SerializeObject(initiativeEmail);

            return MailType + message;
        }

        public static string GenerateCompetitionMessage(IProjectData project, IProjectFollowData follower)
        {
            var model = new Competition
            {
                FirstName = GetFirstNameFromFullName(follower.FullName),
                ProjectId = project.Id,
                ProjectStatus = Status.CompetitionRegistration.ToString(),
                ProjectAuthorName = project.AuthorFullName,
                ProjectCompetitionDeadline = project.CompetitionRegistrationDeadline,
                ProjectCreatedDate = project.Created,
                ProjectDescription = project.Description,
                ProjectFirstPrize = project.BudgetFirstPlace,
                ProjectName = project.Name,
                ProjectSecondPrize = project.BudgetSecondPlace ?? 0
            };

            var messageData = new CompetitionEmailMessageData
            {
                Subject = "Competition",
                Sender = "Competition Platform",
                Model = model
            };

            var data = new CompetitionEmailData
            {
                EmailAddress = follower.UserId,
                MessageData = messageData
            };

            var competitionEmail = new CompetitionEmail
            {
                Data = data
            };

            var message = JsonConvert.SerializeObject(competitionEmail);

            return MailType + message;
        }

        public static string GenerateImplementationMessage(IProjectData project, IProjectFollowData follower, string templateType)
        {
            var model = new Implementation
            {
                FirstName = GetFirstNameFromFullName(follower.FullName),
                ProjectId = project.Id,
                ProjectStatus = Status.Implementation.ToString(),
                ProjectAuthorName = project.AuthorFullName,
                ProjectImplementationDeadline = project.ImplementationDeadline,
                ProjectCreatedDate = project.Created,
                ProjectDescription = project.Description,
                ProjectFirstPrize = project.BudgetFirstPlace,
                ProjectName = project.Name,
                ProjectSecondPrize = project.BudgetSecondPlace ?? 0
            };

            var messageData = new ImplementationEmailMessageData
            {
                Subject = templateType,
                Sender = "Competition Platform",
                Model = model
            };

            var data = new ImplementationEmailData
            {
                EmailAddress = follower.UserId,
                MessageData = messageData
            };

            var implementationEmail = new ImplementationEmail
            {
                Data = data
            };

            var message = JsonConvert.SerializeObject(implementationEmail);

            return MailType + message;
        }

        public static string GenerateVotingMessage(IProjectData project, IProjectFollowData follower)
        {
            var model = new Voting
            {
                FirstName = GetFirstNameFromFullName(follower.FullName),
                ProjectId = project.Id,
                ProjectStatus = Status.Voting.ToString(),
                ProjectAuthorName = project.AuthorFullName,
                ProjectVotingDeadline = project.VotingDeadline,
                ProjectCreatedDate = project.Created,
                ProjectDescription = project.Description,
                ProjectFirstPrize = project.BudgetFirstPlace,
                ProjectName = project.Name,
                ProjectSecondPrize = project.BudgetSecondPlace ?? 0
            };

            var messageData = new VotingEmailMessageData
            {
                Subject = "Voting",
                Sender = "Competition Platform",
                Model = model
            };

            var data = new VotingEmailData
            {
                EmailAddress = follower.UserId,
                MessageData = messageData
            };

            var votingEmail = new VotingEmail
            {
                Data = data
            };

            var message = JsonConvert.SerializeObject(votingEmail);

            return MailType + message;
        }

        public static string ProjectCreatedMessage(string userEmail, string userName, string projectName)
        {
            var messageData = new MessageData
            {
                Subject = "New Project Created - " + projectName,
                Text = "User " + userName + " (" + userEmail + ") " + "Has Created a new project - " + projectName
            };

            var data = new Data
            {
                BroadcastGroup = 600,
                MessageData = messageData
            };

            var plainTextBroadCast = new PlainTextBroadcast { Data = data };

            var plainTextBroadcastString = JsonConvert.SerializeObject(plainTextBroadCast);

            var returnString = "PlainTextBroadcast:" + plainTextBroadcastString;

            return returnString;
        }

        private static string GetFirstNameFromFullName(string fullName)
        {
            return fullName.Split(' ')[0];
        }
    }

    public class EmailCreatedBroadcastMessage
    {
        public PlainTextBroadcast PlainTextBroadcast;
    }

    public class PlainTextBroadcast
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public int BroadcastGroup { get; set; }
        public MessageData MessageData { get; set; }
    }

    public class MessageData
    {
        public string Subject { get; set; }
        public string Text { get; set; }
    }

    public class PlainTextEmail
    {
        public EmailData Data { get; set; }
    }

    public class EmailData
    {
        public string EmailAddress { get; set; }
        public EmailMessageData MessageData { get; set; }
    }

    public class EmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
    }

    public class CompetitionEmail
    {
        public CompetitionEmailData Data { get; set; }
    }

    public class CompetitionEmailData
    {
        public string EmailAddress { get; set; }
        public CompetitionEmailMessageData MessageData { get; set; }
    }

    public class CompetitionEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public Competition Model { get; set; }
    }

    public class InitiativeEmail
    {
        public InitiativeEmailData Data { get; set; }
    }

    public class InitiativeEmailData
    {
        public string EmailAddress { get; set; }
        public InitiativeEmailMessageData MessageData { get; set; }
    }

    public class InitiativeEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public Initiative Model { get; set; }
    }

    public class ImplementationEmail
    {
        public ImplementationEmailData Data { get; set; }
    }

    public class ImplementationEmailData
    {
        public string EmailAddress { get; set; }
        public ImplementationEmailMessageData MessageData { get; set; }
    }

    public class ImplementationEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public Implementation Model { get; set; }
    }

    public class VotingEmail
    {
        public VotingEmailData Data { get; set; }
    }

    public class VotingEmailData
    {
        public string EmailAddress { get; set; }
        public VotingEmailMessageData MessageData { get; set; }
    }

    public class VotingEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public Voting Model { get; set; }
    }
}
