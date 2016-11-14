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
                "Blockchain",
                "Development",
                "Design",
                "Testing",
                "Finance",
                "Technology",
                "Bitcoin",
                "Communications and media",
                "Research"
            };
        }
    }
}
