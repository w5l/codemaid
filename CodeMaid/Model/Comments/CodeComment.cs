using SteveCadwallader.CodeMaid.Model.Comments.Options;
using System.Collections.Generic;

namespace SteveCadwallader.CodeMaid.Model.Comments
{
    internal class CodeComment
    {
        public CodeComment(IEnumerable<ICommentLine> lines, CommentOptions options)
        {
            Lines = lines;
            Options = options;
        }

        public IEnumerable<ICommentLine> Lines { get; }

        public CommentOptions Options { get; }
    }
}