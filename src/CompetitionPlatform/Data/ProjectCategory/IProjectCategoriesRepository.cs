using System.Collections.Generic;

namespace CompetitionPlatform.Data.ProjectCategory
{
    public interface IProjectCategoriesRepository
    {
        List<string> GetCategories();
    }
}
