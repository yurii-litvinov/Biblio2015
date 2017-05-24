using System.Collections.Generic;
using BibliographicSystem.Models;

namespace BibliographicSystem.SearchingMethods
{
    /// <summary>
    /// interface for parsers
    /// </summary>
    interface IParser
    {
        /// <summary>
        /// Requests articles from current bibliographic system
        /// </summary>
        void RequestArticles();

        /// <summary>
        /// Verify the success of the request on current bibliographic system
        /// </summary>
        bool IsSuccessful { get; set; }

        /// <summary>
        /// Returns message about problem that occurred during the query process on current bibliographic system
        /// </summary>
        /// <returns></returns>
        Problem GetProblem();

        /// <summary>
        /// Returns articles from current bibliographic system
        /// </summary>
        /// <returns></returns>
        List<OutsideArticle> GetArticles();

        /// <summary>
        /// aggregate information the user entered in the text boxes
        /// </summary>
        UserQuery Query { get; set; }
    }
}
