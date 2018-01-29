using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.BlogCategory
{
    public class BlogCategoriesRepository : IBlogCategoriesRepository
    {
        public List<string> GetCategories()
        {
            return new List<string>
            {
                "News",
                "Results",
                "Winners",
                "Success stories",
                "Videos",
                "About",
                "Tutorials"
            };
        }
    }
}
