using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    public sealed class Tag
    {
        public Tag()
        {
            Articles = new List<Article>();
        }

        public int TagId { get; set; }

        public string TagName { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}
