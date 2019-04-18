using SteveCadwallader.CodeMaid.Helpers;

namespace SteveCadwallader.CodeMaid.Model.Comments.Options
{
    /// <summary>
    /// Comment specific options for the formatter.
    /// </summary>
    internal class CommentOptions
    {
        public string Indent { get; internal set; }

        public CodeLanguage Language { get; internal set; }

        public string Prefix { get; internal set; }
    }
}