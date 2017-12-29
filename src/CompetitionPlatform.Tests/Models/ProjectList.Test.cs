using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models.ProjectModels;
using Xunit;
using Moq;

namespace CompetitionPlatform.Tests
{
    public class ProjectListTest
    {
        private Mock<IProjectRepository> mockRepo;

        public ProjectListTest()
        {
            // Set up project data for testing. Using Moq to avoid creating an entire mock version of ProjectRepository
            // when only the getProjectsAsync method is needed here
            var Project1 = new Mock<IProjectData>();
            Project1.Setup(x => x.AuthorId).Returns("1");
            Project1.Setup(x => x.Id).Returns("Project1");
            Project1.Setup(x => x.ProjectStatus).Returns(Models.Status.Draft.ToString());

            var Project2 = new Mock<IProjectData>();
            Project2.Setup(x => x.Id).Returns("Project2");
            Project2.Setup(x => x.AuthorId).Returns("2");
            Project2.Setup(x => x.ProjectStatus).Returns(Models.Status.Submission.ToString());

            mockRepo = new Mock<IProjectRepository>();
            mockRepo.Setup(x => x.GetProjectsAsync()).ReturnsAsync(new List<IProjectData> { Project1.Object, Project2.Object });
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
    }
}
