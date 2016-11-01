using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.ProjectCategory;

namespace CompetitionPlatform.Data.ProjectCategory
{
    public class ProjectCategoriesRepository : IProjectCategoriesRepository
    {
        public List<string> GetCategories()
        {
            return new List<string>
            {
                "Finance",
                "Technology",
                "Bitcoin",
                "Mobile",
                "Payments",
                "Communications and media"
            };
        }
    }
}
