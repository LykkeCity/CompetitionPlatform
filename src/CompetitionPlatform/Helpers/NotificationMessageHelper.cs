using System.Collections.Generic;
using System.Linq;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models;
using Newtonsoft.Json;

namespace CompetitionPlatform.Helpers
{
    public static class NotificationMessageHelper
    {
        private const string MailType = "CompetitionPlatformMail:";
        private const string EmailSender = "Lykke Streams";

        public static string GenerateRegistrationMessage(string firstName, string email)
        {
            var model = new RegistrationEmailModel
            {
                FirstName = firstName
            };

            var messageData = new RegistrationEmailMessageData
            {
                Subject = "Registration",
                Sender = EmailSender,
                Model = model
            };

            var data = new RegistrationEmailData
            {
                EmailAddress = email,
                MessageData = messageData
            };

            var registrationEmail = new RegistrationEmail
            {
                Data = data
            };

            var message = JsonConvert.SerializeObject(registrationEmail);

            return MailType + message;
        }

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
                Sender = EmailSender,
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
                Sender = EmailSender,
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
                Sender = EmailSender,
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
                Sender = EmailSender,
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

        public static string GenerateArchiveMessage(IProjectData project, IProjectFollowData follower, int participantsCount, int resultsCount, IEnumerable<IWinnerData> winners)
        {
            var duration = (project.VotingDeadline - project.Created).Days;

            var firstPlaceWinnerName = "";
            var firstPlaceWinnerId = "";
            var firstPlaceWinnerScore = 0;
            var firstPlaceWinnerVotes = 0;

            var secondPlaceWinnerName = "";
            var secondPlaceWinnerId = "";
            var secondPlaceWinnerScore = 0;
            var secondPlaceWinnerVotes = 0;

            var thirdPlaceWinnerName = "";
            var thirdPlaceWinnerId = "";
            var thirdPlaceWinnerScore = 0;
            var thirdPlaceWinnerVotes = 0;

            var fourthPlaceWinnerName = "";
            var fourthPlaceWinnerId = "";
            var fourthPlaceWinnerScore = 0;
            var fourthPlaceWinnerVotes = 0;

            var winnersData = winners as IWinnerData[] ?? winners.ToArray();

            for (int i = 1; i <= winnersData.Count(); i++)
            {
                if (i == 1)
                {
                    firstPlaceWinnerName = winnersData[0].FullName;
                    firstPlaceWinnerId = winnersData[0].WinnerId;
                    firstPlaceWinnerScore = winnersData[0].Score;
                    firstPlaceWinnerVotes = winnersData[0].Votes;
                }
                if (i == 2)
                {
                    secondPlaceWinnerName = winnersData[1].FullName;
                    secondPlaceWinnerId = winnersData[1].WinnerId;
                    secondPlaceWinnerScore = winnersData[1].Score;
                    secondPlaceWinnerVotes = winnersData[1].Votes;
                }
                if (i == 3)
                {
                    thirdPlaceWinnerName = winnersData[2].FullName;
                    thirdPlaceWinnerId = winnersData[2].WinnerId;
                    thirdPlaceWinnerScore = winnersData[2].Score;
                    thirdPlaceWinnerVotes = winnersData[2].Votes;
                }
                if (i == 4)
                {
                    fourthPlaceWinnerName = winnersData[3].FullName;
                    fourthPlaceWinnerId = winnersData[3].WinnerId;
                    fourthPlaceWinnerScore = winnersData[3].Score;
                    fourthPlaceWinnerVotes = winnersData[3].Votes;
                }
            }

            var model = new Archive
            {
                FirstName = GetFirstNameFromFullName(follower.FullName),
                ProjectId = project.Id,
                ProjectCreatedDate = project.Created,
                ProjectAuthorName = project.AuthorFullName,
                ProjectName = project.Name,
                ProjectStatus = Status.Archive.ToString(),
                ProjectDescription = project.Description,
                ProjectFirstPrize = project.BudgetFirstPlace,
                ProjectSecondPrize = project.BudgetSecondPlace ?? 0,
                ParticipantCount = participantsCount,
                ResultCount = resultsCount,
                WinnerCount = winnersData.Count(),
                Duration = duration,
                FirstPlaceWinnerName = firstPlaceWinnerName,
                FirstPlaceWinnerId = firstPlaceWinnerId,
                FirstPlaceWinnerScore = firstPlaceWinnerScore,
                FirstPlaceWinnerVotes = firstPlaceWinnerVotes,
                SecondPlaceWinnerName = secondPlaceWinnerName,
                SecondPlaceWinnerId = secondPlaceWinnerId,
                SecondPlaceWinnerScore = secondPlaceWinnerScore,
                SecondPlaceWinnerVotes = secondPlaceWinnerVotes,
                ThirdPlaceWinnerName = thirdPlaceWinnerName,
                ThirdPlaceWinnerId = thirdPlaceWinnerId,
                ThirdPlaceWinnerScore = thirdPlaceWinnerScore,
                ThirdPlaceWinnerVotes = thirdPlaceWinnerVotes,
                FourthPlaceWinnerName = fourthPlaceWinnerName,
                FourthPlaceWinnerId = fourthPlaceWinnerId,
                FourthPlaceWinnerScore = fourthPlaceWinnerScore,
                FourthPlaceWinnerVotes = fourthPlaceWinnerVotes
            };

            var messageData = new ArchiveEmailMessageData
            {
                Subject = "Archive",
                Sender = EmailSender,
                Model = model
            };

            var data = new ArchiveEmailData
            {
                EmailAddress = follower.UserId,
                MessageData = messageData
            };

            var archiveEmail = new ArchiveEmail
            {
                Data = data
            };

            var message = JsonConvert.SerializeObject(archiveEmail);

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

    public class RegistrationEmail
    {
        public RegistrationEmailData Data { get; set; }
    }

    public class RegistrationEmailData
    {
        public string EmailAddress { get; set; }
        public RegistrationEmailMessageData MessageData { get; set; }
    }

    public class RegistrationEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public RegistrationEmailModel Model { get; set; }
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

    public class ArchiveEmail
    {
        public ArchiveEmailData Data { get; set; }
    }

    public class ArchiveEmailData
    {
        public string EmailAddress { get; set; }
        public ArchiveEmailMessageData MessageData { get; set; }
    }

    public class ArchiveEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public Archive Model { get; set; }
    }
}
