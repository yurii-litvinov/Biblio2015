using System.Collections.Generic;
using System.Linq;

namespace BibliographicSystem.Models
{
    /// <summary>
    /// Class for adding article to group
    /// </summary>
    public class AddingToGroup
    {
        private readonly AddingToSystem lists = new AddingToSystem();

        public int GroupId { get; set; }

        public string UserName { get; set; }

        public List<Article> ArticlesToAdd() =>
            lists.ArticlesByUser(UserName).Where(l => l.GroupId == 0).ToList();
    }
}