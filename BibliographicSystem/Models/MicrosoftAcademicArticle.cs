using System.Collections.Generic;

namespace BibliographicSystem.Models
{
    public class MicrosoftAcademicArticle
    {
        public string PaperTitle { get; set; }
        public int PaperYear { get; set; }
        public int CitationCount { get; set; }
        public List<Author> Authors { get; set; }
    }

    public class Author
    {
        public string AuthorName { get; set; }
        public long AuthorId { get; set; }
    }
}