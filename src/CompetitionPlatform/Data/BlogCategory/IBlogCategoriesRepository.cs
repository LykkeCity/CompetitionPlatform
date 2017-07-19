using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.BlogCategory
{
    public interface IBlogCategoriesRepository
    {
        List<string> GetCategories();
    }
}
