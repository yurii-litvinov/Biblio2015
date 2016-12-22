using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    public sealed class Article
    {
        public Article()
        {
            Tags = new List<Tag>();
        }

        public int ArticleId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Journal { get; set; }

        public string Publisher { get; set; }

        // [StringLength(4, MinimumLength = 4, ErrorMessage = "Неверный формат")]
        public string Year { get; set; }

        public string Note { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public string UserName { get; set; }

        public int GroupId { get; set; }

        public ICollection<Tag> Tags { get; set; }
    }
}