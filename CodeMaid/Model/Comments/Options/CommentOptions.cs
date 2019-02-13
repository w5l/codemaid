using SteveCadwallader.CodeMaid.Helpers;

namespace SteveCadwallader.CodeMaid.Model.Comments.Options
{
    /// <summary>
    /// Comment specific options for the formatter.
    /// </summary>
    internal class CommentOptions
    {
        public string Prefix { get; internal set; }

        public CodeLanguage Language { get; internal set; }
    }
}