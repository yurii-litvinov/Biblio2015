using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    /// <summary>
    /// class that represents an article from the different bibliographic systems
    /// </summary>
    public class OutsideArticle
    {
        public string From { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CitationCount { get; set; }
        public int Year { get; set; }
        public string ExtendedMetadata { get; set; }
        public List<string> References { get; set; }
        public List<Author> Authors { get; set; }

        public string Info { get; set; }
        public string Reference { get; set; }
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