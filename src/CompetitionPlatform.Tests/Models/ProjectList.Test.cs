using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models.ProjectModels;
using Xunit;
using Moq;

namespace CompetitionPlatform.Tests
{
    public class ProjectListTest
    {
        private Mock<IProjectRepository> mockRepo;
        private Mock<IProjectData> Project1;
        private Mock<IProjectData> Project2;

        public ProjectListTest()
        {
            // Set up project data for testing. Using Moq to avoid creating an entire mock version of ProjectRepository
            // when only the getProjectsAsync method is needed here
            Project1 = new Mock<IProjectData>();
            Project1.Setup(x => x.AuthorId).Returns("1");
            Project1.Setup(x => x.Id).Returns("Project1");
            Project1.Setup(x => x.Category).Returns("Blockchain");
            Project1.Setup(x => x.BudgetFirstPlace).Returns(1000);
            Project1.Setup(x => x.LastModified).Returns(new DateTime(2017, 12, 01));
            Project1.Setup(x => x.ProjectStatus).Returns(Models.Status.Draft.ToString());

            Project2 = new Mock<IProjectData>();
            Project2.Setup(x => x.Id).Returns("Project2");
            Project2.Setup(x => x.AuthorId).Returns("2");
            Project2.Setup(x => x.BudgetFirstPlace).Returns(2000);
            Project2.Setup(x => x.LastModified).Returns(new DateTime(2017, 12, 02));
            Project2.Setup(x => x.Category).Returns("Design");
            Project2.Setup(x => x.ProjectStatus).Returns(Models.Status.Submission.ToString());

            mockRepo = new Mock<IProjectRepository>();
            mockRepo.Setup(x => x.GetProjectsAsync(null)).ReturnsAsync(new List<IProjectData> { Project1.Object, Project2.Object });
        }
        [Fact]
        public async Task ProjectListShouldBuildList()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .GetProjects();
            Assert.Equal(2, projectList.Count());
            Assert.Equal("Project1", projectList.First().Id);
            Assert.Equal("Project2", projectList.Last().Id);
        }
        [Fact]
        public async Task ProjectListShouldFilterByAuthor()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByAuthorId("1")
                .GetProjects();
            Assert.Equal(1, projectList.Count());
            Assert.Equal("Project1", projectList.First().Id);
        }
        [Fact]
        public async Task ProjectListShouldRemoveDrafts()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .RemoveDrafts()
                .GetProjects();
            Assert.Equal(1, projectList.Count());
            Assert.Equal("Project2", projectList.First().Id);
        }

        [Fact]
        public async Task ProjectListShouldFilterByFollowing()
        {
            var mockFoundProject = new Mock<IProjectFollowData>();
            var mockFollowRepo = new Mock<IProjectFollowRepository>();
            mockFollowRepo.Setup(x => x.GetAsync("1", "Project1")).ReturnsAsync(mockFoundProject.Object);
            mockFollowRepo.Setup(x => x.GetAsync("1", "Project2")).ReturnsAsync((IProjectFollowData)null);

            var projectList = await ProjectList.CreateProjectList(mockRepo.Object);
            var projects = (await projectList.FilterByFollowing("1", mockFollowRepo.Object)).GetProjects();
            Assert.Equal(1, projects.Count());
            Assert.Equal("Project1", projects.First().Id);
        }

        [Fact]
        public async Task ProjectListShouldFilterByParticipating()
        {
            var mockFoundProject = new Mock<IProjectParticipateData>();
            var mockParticipatingRepo = new Mock<IProjectParticipantsRepository>();
            mockParticipatingRepo.Setup(x => x.GetAsync("1", "Project1")).ReturnsAsync(mockFoundProject.Object);
            mockParticipatingRepo.Setup(x => x.GetAsync("1", "Project2")).ReturnsAsync((IProjectParticipateData)null);

            var projectList = await ProjectList.CreateProjectList(mockRepo.Object);
            var projects = (await projectList.FilterByParticipating("1", mockParticipatingRepo.Object)).GetProjects();
            Assert.Equal(1, projects.Count());
            Assert.Equal("Project1", projects.First().Id);
        }

        [Fact]
        public async Task ProjectListShouldReturnAllProjectsOnAllStatusFilter()
        {  
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByStatus("All")
                .GetProjects();
            Assert.Equal(2, projectList.Count());
        }
        [Fact]
        public async Task ProjectListShouldReturnAllProjectsOnNullStatusFilter()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByStatus((string)null)
                .GetProjects();
            Assert.Equal(2, projectList.Count());
        }
        [Fact]
        public async Task ProjectListShouldFilterByStatus()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByStatus(Models.Status.Submission.ToString())
                .GetProjects();
            Assert.Equal(1, projectList.Count());
            Assert.Equal("Project2", projectList.First().Id);
        }
        [Fact]
        public async Task ProjectListShouldReturnAllProjectsOnAllCategoryFilter()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByCategory("All")
                .GetProjects();
            Assert.Equal(2, projectList.Count());
        }
        [Fact]
        public async Task ProjectListShouldReturnAllProjectsOnNullCategoryFilter()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByCategory((string)null)
                .GetProjects();
            Assert.Equal(2, projectList.Count());
        }
        [Fact]
        public async Task ProjectListShouldFilterByCategory()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByCategory("Blockchain")
                .GetProjects();
            Assert.Equal(1, projectList.Count());
            Assert.Equal("Project1", projectList.First().Id);
        }
        [Fact]
        public async Task ProjectListShouldFilterByCurrentProjects()
        {
            var Project3 = new Mock<IProjectData>();
            Project3.Setup(x => x.Id).Returns("Project3");
            Project3.Setup(x => x.AuthorId).Returns("3");
            Project3.Setup(x => x.ProjectStatus).Returns(Models.Status.Initiative.ToString());

            mockRepo.Setup(x => x.GetProjectsAsync(null)).ReturnsAsync(new List<IProjectData> { Project1.Object, Project2.Object, Project3.Object });

            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .FilterByCurrentProjects()
                .GetProjects();
            Assert.Equal(2, projectList.Count());
            Assert.Equal("Project1", projectList.First().Id);
            Assert.Equal("Project2", projectList.Last().Id);
        }
        [Fact]
        public async Task ProjectListShouldOrderByPrizeAscending()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .OrderByPrize("Ascending")
                .GetProjects();
            Assert.Equal(2, projectList.Count());
            Assert.Equal("Project1", projectList.First().Id);
            Assert.Equal("Project2", projectList.Last().Id);
        }
        [Fact]
        public async Task ProjectListShouldOrderByPrizeDescending()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .OrderByPrize("Descending")
                .GetProjects();
            Assert.Equal(2, projectList.Count());
            Assert.Equal("Project2", projectList.First().Id);
            Assert.Equal("Project1", projectList.Last().Id);
        }
        [Fact]
        public async Task ProjectListShouldOrderByLastModifiedAscending()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .OrderByLastModified("Ascending")
                .GetProjects();
            Assert.Equal(2, projectList.Count());
            Assert.Equal("Project1", projectList.First().Id);
            Assert.Equal("Project2", projectList.Last().Id);
        }
        [Fact]
        public async Task ProjectListShouldOrderByLastModifiedDescending()
        {
            var projectList = (await ProjectList.CreateProjectList(mockRepo.Object))
                .OrderByLastModified("Descending")
                .GetProjects();
            Assert.Equal(2, projectList.Count());
            Assert.Equal("Project2", projectList.First().Id);
            Assert.Equal("Project1", projectList.Last().Id);
        }
        public async Task ProjectListShouldUnionDistinct()
        {
            var mockRepoToUnion = new Mock<IProjectRepository>();
            var Project3 = new Mock<IProjectData>();
            Project3.Setup(x => x.Id).Returns("Project3");
            
            mockRepo.Setup(x => x.GetProjectsAsync(null)).ReturnsAsync(new List<IProjectData> { Project1.Object, Project2.Object });
            mockRepoToUnion.Setup(x => x.GetProjectsAsync(null)).ReturnsAsync(new List<IProjectData> { Project2.Object, Project3.Object });

            var projectList = await ProjectList.CreateProjectList(mockRepo.Object);
            var projectListToUnion = await ProjectList.CreateProjectList(mockRepoToUnion.Object);
            var unionProjects = projectList.DistinctUnion(projectListToUnion)
                .GetProjects();
            Assert.Equal(2, unionProjects.Count());
            Assert.Equal("Project1", unionProjects.First().Id);
            Assert.Equal("Project2", unionProjects.ElementAt(1).Id);
            Assert.Equal("Project3", unionProjects.Last().Id);
        }
    }
}
