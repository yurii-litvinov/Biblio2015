using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    public class MicrosoftAcademicArticle
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public int CitationCount { get; set; }
        public string ExtendedMetadata { get; set; }
        public List<string> References { get; set; }
        public List<Author> Authors { get; set; }
    }

    public class Author
    {
        public string AuthorName { get; set; }
        public long AuthorId { get; set; }
    }
}