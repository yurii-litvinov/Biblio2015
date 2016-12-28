using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    public class ScholarArticle
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Citations { get; set; }
        public string Reference { get; set; }
        public int Year { get; set; }
        public int CitationCount { get; set; }
        public string ExtendedMetadata { get; set; }
        public List<string> References { get; set; }
        public List<Author> Authors { get; set; }
    }
}