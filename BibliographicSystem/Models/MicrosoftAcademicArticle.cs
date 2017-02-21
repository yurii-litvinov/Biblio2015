using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    /// <summary>
    /// class that represents an article from the microsoft academic
    /// </summary>
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

    /// <summary>
    /// class that represents author of article
    /// </summary>
    public class Author
    {
        public string AuthorName { get; set; }
        public long AuthorId { get; set; }
    }
}