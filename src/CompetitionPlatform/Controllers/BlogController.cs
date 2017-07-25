using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Blog;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.BlogCategory;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using CompetitionPlatform.Models.BlogViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IBlogCategoriesRepository _blogCategoriesRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectParticipantsRepository _participantsRepository;
        private readonly IProjectResultRepository _resultRepository;
        private readonly IProjectWinnersRepository _winnersRepository;
        private readonly IUserRolesRepository _userRolesRepository;
        private readonly IBlogCommentsRepository _blogCommentsRepository;
        private readonly IBlogPictureRepository _blogPictureRepository;
        private readonly IBlogPictureInfoRepository _blogPictureInfoRepository;

        public BlogController(IBlogRepository blogRepository, IBlogCategoriesRepository blogCategoriesRepository,
            IProjectRepository projectRepository, IProjectParticipantsRepository participantsRepository,
            IProjectResultRepository resultRepository, IProjectWinnersRepository winnersRepository,
            IUserRolesRepository userRolesRepository, IBlogCommentsRepository blogCommentsRepository,
            IBlogPictureRepository blogPictureRepository, IBlogPictureInfoRepository blogPictureInfoRepository)
        {
            _blogRepository = blogRepository;
            _blogCategoriesRepository = blogCategoriesRepository;
            _projectRepository = projectRepository;
            _participantsRepository = participantsRepository;
            _resultRepository = resultRepository;
            _winnersRepository = winnersRepository;
            _userRolesRepository = userRolesRepository;
            _blogCommentsRepository = blogCommentsRepository;
            _blogPictureRepository = blogPictureRepository;
            _blogPictureInfoRepository = blogPictureInfoRepository;
        }

        public async Task<IActionResult> BlogList()
        {
            ViewBag.Blog = ViewBag.Blog != true;
            ViewBag.MyProjects = false;
            ViewBag.AllProjects = false;
            ViewBag.Faq = false;

            var viewModel = await GetBlogListViewModel();
            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var user = GetAuthenticatedUser();
            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());
            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;

            if (!isAdmin)
            {
                return View("AccessDenied");
            }

            ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
            return View("CreateBlog");
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = GetAuthenticatedUser();
            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());
            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;

            if (!isAdmin)
            {
                return View("AccessDenied");
            }

            ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
            var viewModel = await GetBlogViewModel(id);

            return View("EditBlog", viewModel);
        }

        public async Task<IActionResult> SaveBlog(BlogViewModel blog)
        {
            var user = GetAuthenticatedUser();
            blog.AuthorName = user.GetFullName();
            blog.AuthorId = user.Email;

            blog.Posted = DateTime.UtcNow;

            var idValid = Regex.IsMatch(blog.Id, @"^[a-z0-9-]+$") && !string.IsNullOrEmpty(blog.Id);

            if (!idValid)
            {
                ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
                ModelState.AddModelError("Id", "Blog Entry Url can only contain lowercase letters, numbers and the dash symbol and cannot be empty!");
                return View("CreateBlog", blog);
            }

            var blogEntry = await _blogRepository.GetAsync(blog.Id);

            if (blogEntry == null)
            {
                if (blog.ProjectId != null)
                {
                    var project = await _projectRepository.GetAsync(blog.ProjectId);

                    if (project == null)
                    {
                        ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
                        ModelState.AddModelError("ProjectId", "Project with that Id does not exist!");
                        return View("CreateBlog", blog);
                    }
                    blog.ProjectName = project.Name;
                }

                await _blogRepository.SaveAsync(blog);
                await SaveBlogPicture(blog.File, blog.Id);
                return RedirectToAction("BlogList", "Blog");
            }

            ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
            ModelState.AddModelError("Id", "Blog Entry with this Id already exists!");
            return View("CreateBlog", blog);
        }

        public async Task<IActionResult> SaveEditedBlog(BlogViewModel blog)
        {
            var existingEntry = await _blogRepository.GetAsync(blog.Id);

            blog.Posted = DateTime.UtcNow;

            var idValid = Regex.IsMatch(blog.Id, @"^[a-z0-9-]+$") && !string.IsNullOrEmpty(blog.Id);

            if (!idValid)
            {
                ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
                ModelState.AddModelError("Id", "Blog Entry Url can only contain lowercase letters, numbers and the dash symbol and cannot be empty!");
                return View("EditBlog", blog);
            }

            if (blog.ProjectId != null && blog.ProjectId != existingEntry.ProjectId)
            {
                var project = await _projectRepository.GetAsync(blog.ProjectId);

                if (project == null)
                {
                    ViewBag.BlogCategories = _blogCategoriesRepository.GetCategories();
                    ModelState.AddModelError("ProjectId", "Project with that Id does not exist!");
                    return View("EditBlog", blog);
                }
                blog.ProjectName = project.Name;
            }

            await SaveBlogPicture(blog.File, blog.Id);
            await _blogRepository.UpdateAsync(blog);
            return RedirectToAction("BlogList", "Blog");
        }

        public async Task<IActionResult> BlogDetails(string id)
        {
            ViewBag.Blog = ViewBag.Blog != true;

            var blog = await _blogRepository.GetAsync(id);

            if (blog == null)
            {
                return View("BlogNotFound");
            }

            var viewModel = await GetBlogViewModel(id);
            return View(viewModel);
        }

        private async Task<BlogViewModel> GetBlogViewModel(string id)
        {
            var blog = await _blogRepository.GetAsync(id);
            var user = GetAuthenticatedUser();
            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;
            var isAuthor = (user.Email != null) && user.Email == blog.AuthorId;

            var model = new BlogViewModel
            {
                Id = blog.Id,
                Name = blog.Name,
                Overview = blog.Overview,
                Text = blog.Text,
                ProjectId = blog.ProjectId,
                ProjectName = blog.ProjectName,
                Category = blog.Category,
                Published = blog.Published,
                Posted = blog.Posted,
                FirstResult = blog.FirstResult,
                FirstResultAuthor = blog.FirstResultAuthor,
                FirstResultComment = blog.FirstResultComment,
                SecondResult = blog.SecondResult,
                SecondResultAuthor = blog.SecondResultAuthor,
                SecondResultComment = blog.SecondResultComment,
                ThirdResult = blog.ThirdResult,
                ThirdResultAuthor = blog.ThirdResultAuthor,
                ThirdResultComment = blog.ThirdResultComment,
                FourthResult = blog.FourthResult,
                FourthResultAuthor = blog.FourthResultAuthor,
                FourthResultComment = blog.FourthResultComment,
                AuthorId = blog.AuthorId,
                AuthorName = blog.AuthorName,
                IsAdmin = isAdmin
            };

            var comments = await _blogCommentsRepository.GetBlogCommentsAsync(id);

            foreach (var comment in comments)
            {
                if (!string.IsNullOrEmpty(comment.Comment))
                {
                    comment.Comment = Regex.Replace(comment.Comment, @"\r\n?|\n", "<br />");
                }
            }

            comments = SortComments(comments);

            var commenterIsModerator = new Dictionary<string, bool>();

            foreach (var comment in comments)
            {
                var role = await _userRolesRepository.GetAsync(comment.UserId);
                var isModerator = role != null && role.Role == StreamsRoles.Admin;
                commenterIsModerator.Add(comment.Id, isModerator);
            }

            var commentsPartial = new BlogCommentPartialViewModel
            {
                BlogId = model.Id,
                UserId = user.Email,
                Comments = comments,
                IsAdmin = isAdmin,
                IsAuthor = isAuthor,
                CommenterIsModerator = commenterIsModerator,
                BlogAuthorId = blog.AuthorId
            };

            model.CommentsPartial = commentsPartial;

            var blogImageInfo = await _blogPictureInfoRepository.GetAsync(blog.Id);
            if (blogImageInfo != null)
            {
                var blogImage = await _blogPictureRepository.GetBlogPicture(blog.Id);

                byte[] bytesArray;
                using (var ms = new MemoryStream())
                {
                    blogImage.CopyTo(ms);
                    bytesArray = ms.ToArray();
                }
                model.ImageDataType = blogImageInfo.ContentType;
                model.ImageBase64 = Convert.ToBase64String(bytesArray);
            }

            if (string.IsNullOrEmpty(blog.ProjectId)) return model;
            var project = await _projectRepository.GetAsync(blog.ProjectId);

            model.ProjectBudget = project.BudgetFirstPlace;
            model.ProjectCategory = project.Category;
            model.ProjectAuthor = project.AuthorFullName;
            model.ProjectParticipants = await _participantsRepository.GetProjectParticipantsCountAsync(blog.ProjectId);
            model.ProjectResults = await _resultRepository.GetResultsCountAsync(blog.ProjectId);
            model.ProjectWinners = await _winnersRepository.GetWinnersCountAsync(blog.ProjectId);

            return model;
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }

        private async Task<BlogListIndexViewModel> GetBlogListViewModel()
        {
            var blogEntries = await _blogRepository.GetBlogsAsync();

            var compactModels = await GetCompactBlogsList(blogEntries);

            var viewModel = new BlogListIndexViewModel
            {
                BlogCategories = _blogCategoriesRepository.GetCategories(),
                BlogEntries = compactModels.OrderByDescending(x => x.Published),
                IsAdmin = await CurrentUserIsAdmin()
            };

            return viewModel;
        }

        private async Task<List<BlogCompactViewModel>> GetCompactBlogsList(IEnumerable<IBlogData> blogEntries)
        {
            var compactModels = new List<BlogCompactViewModel>();

            foreach (var blog in blogEntries)
            {
                var commentCount = await _blogCommentsRepository.GetBlogCommentsCountAsync(blog.Id);
                var compactModel = new BlogCompactViewModel
                {
                    Id = blog.Id,
                    Name = blog.Name,
                    Overview = blog.Overview,
                    Category = blog.Category,
                    Published = blog.Published,
                    CommentsCount = commentCount,
                    ProjectId = blog.ProjectId,
                    ProjectName = blog.ProjectName
                };

                var blogImageInfo = await _blogPictureInfoRepository.GetAsync(blog.Id);
                if (blogImageInfo != null)
                {
                    var blogImage = await _blogPictureRepository.GetBlogPicture(blog.Id);

                    byte[] bytesArray;
                    using (var ms = new MemoryStream())
                    {
                        blogImage.CopyTo(ms);
                        bytesArray = ms.ToArray();
                    }
                    compactModel.ImageDataType = blogImageInfo.ContentType;
                    compactModel.ImageBase64 = Convert.ToBase64String(bytesArray);
                }
                compactModels.Add(compactModel);
            }
            return compactModels;
        }

        [Authorize]
        public async Task<IActionResult> AddComment(BlogCommentPartialViewModel model)
        {
            var user = GetAuthenticatedUser();
            model.UserId = user.Email;
            model.FullName = user.GetFullName();
            model.Created = DateTime.UtcNow;
            model.LastModified = model.Created;
            model.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            if (!string.IsNullOrEmpty(model.Comment))
            {
                await _blogCommentsRepository.SaveAsync(model);
            }
            return RedirectToAction("BlogDetails", "Blog", new { id = model.BlogId });
        }

        private IEnumerable<IBlogCommentData> SortComments(IEnumerable<IBlogCommentData> comments)
        {
            var commentsData = comments as IList<IBlogCommentData> ?? comments.ToList();

            var childComments = commentsData.Where(x => x.ParentId != null).ToList();
            comments = commentsData.Where(x => x.ParentId == null).ToList();
            comments = comments.OrderBy(c => c.Created).Reverse().ToList();

            var sortedComments = new List<IBlogCommentData>();

            foreach (var comment in comments)
            {
                sortedComments.Add(comment);
                var children = childComments.Where(x => x.ParentId == comment.Id).ToList();
                children = children.OrderBy(x => x.Created).ToList();
                sortedComments.AddRange(children);
            }

            return sortedComments;
        }

        public IActionResult GetCommentReplyForm(string commentId, string blogId)
        {
            var user = GetAuthenticatedUser();
            var created = DateTime.UtcNow;

            var model = new BlogCommentPartialViewModel
            {
                UserId = user.Email,
                FullName = user.GetFullName(),
                BlogId = blogId,
                ParentId = commentId,
                Created = created,
                LastModified = created
            };

            return PartialView("~/Views/Blog/BlogCommentReplyFormPartial.cshtml", model);
        }

        [Authorize]
        public async Task<IActionResult> RemoveComment(string blogId, string commentId)
        {
            var user = GetAuthenticatedUser();

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;

            if (isAdmin)
            {
                var comment = await _blogCommentsRepository.GetBlogCommentAsync(blogId, commentId);
                comment.Deleted = true;
                await _blogCommentsRepository.UpdateAsync(comment, blogId);
            }

            return RedirectToAction("BlogDetails", "Blog", new { id = blogId });
        }

        public async Task<IActionResult> GetBlogList(string blogCategoryFilter)
        {
            var viewModel = await GetBlogListViewModel(blogCategoryFilter);
            return PartialView("BlogListPartial", viewModel);
        }

        private async Task<BlogListIndexViewModel> GetBlogListViewModel(string blogCategoryFilter = null)
        {
            var blogs = await _blogRepository.GetBlogsAsync();

            if (!string.IsNullOrEmpty(blogCategoryFilter))
            {
                if (blogCategoryFilter != "All")
                {
                    blogs = blogs.Where(x => x.Category.Replace(" ", "") == blogCategoryFilter);
                }
            }

            blogs = blogs.OrderByDescending(x => x.Published);

            var compactModels = await GetCompactBlogsList(blogs);

            var viewModel = new BlogListIndexViewModel
            {
                BlogCategories = _blogCategoriesRepository.GetCategories(),
                BlogEntries = compactModels
            };

            return viewModel;
        }

        private async Task<bool> CurrentUserIsAdmin()
        {
            var user = GetAuthenticatedUser();

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;

            return isAdmin;
        }

        private async Task SaveBlogPicture(IFormFile file, string blogId)
        {
            if (file != null)
            {
                var imageUrl = await _blogPictureRepository.InsertBlogPicture(file.OpenReadStream(), blogId);

                var fileInfo = new BlogPictureInfoEntity
                {
                    RowKey = blogId,
                    ContentType = file.ContentType,
                    FileName = file.FileName,
                    ImageUrl = imageUrl
                };

                await _blogPictureInfoRepository.SaveAsync(fileInfo);
            }
        }
    }
}