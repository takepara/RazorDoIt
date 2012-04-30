using System.Collections.Generic;
using System.Linq;

namespace RazorDoIt.Models
{
    public class BuildResult
    {
        public string Response { get; private set; }
        public IList<string> Errors { get; private set; }

        public BuildResult(string response) : this(response, Enumerable.Empty<string>()) { }
        public BuildResult(IEnumerable<string> errors) : this(string.Empty, errors) { }
        public BuildResult(string response, IEnumerable<string> errors)
        {
            Response = response;
            Errors = new List<string>(errors);
        }
    }
}
