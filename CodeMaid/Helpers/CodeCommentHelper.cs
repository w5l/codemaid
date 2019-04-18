using EnvDTE;
using SteveCadwallader.CodeMaid.Model.Comments;
using SteveCadwallader.CodeMaid.Model.Comments.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteveCadwallader.CodeMaid.Helpers
{
    /// <summary>
    /// A set of helper methods focused around code comments.
    /// </summary>
    internal static class CodeCommentHelper
    {
        public const int CopyrightExtraIndent = 4;
        public const char KeepTogetherSpacer = '\a';
        public const char Spacer = ' ';

        /// <summary>
        /// Gets the list of tokens defined in Tools &gt; Options &gt; Environment &gt; Task List.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTaskListTokens(CodeMaidPackage package)
        {
            var settings = package.IDE.Properties["Environment", "TaskList"];
            var tokens = settings.Item("CommentTokens").Value as string[];
            if (tokens == null || tokens.Length < 1)
                return Enumerable.Empty<string>();

            // Tokens values are written like "NAME:PRIORITY". We want only the names, and require
            // that they are followed by a semicolon and a space.
            return tokens.Select(t => t.Substring(0, t.LastIndexOf(':') + 1) + " ");
        }

        internal static string FakeToSpace(string value)
        {
            return value.Replace(KeepTogetherSpacer, Spacer);
        }

        /// <summary>
        /// Helper function to generate the preview in the options menu.
        /// </summary>
        internal static string Format(string text, Action<FormatterOptions> options = null)
        {
            var formatterOptions = FormatterOptions
                .FromSettings(Properties.Settings.Default)
                .Set(o => o.IgnoreTokens = new[] { "TODO: " });

            options?.Invoke(formatterOptions);

            var parser = new CodeCommentParser(formatterOptions, CodeLanguage.CSharp);
            var comment = parser.Parse(text);
            var formatter = new CommentFormatter(comment);
            return formatter.Format(formatterOptions);
        }

        /// <summary>
        /// Get the comment prefix (regex) for the given document's language.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>The comment prefix regex, without trailing spaces.</returns>
        internal static string GetCommentPrefix(TextDocument document)
        {
            return GetCommentPrefixForLanguage(document.GetCodeLanguage());
        }

        /// <summary>
        /// Get the comment prefix (regex) for the specified code language.
        /// </summary>
        /// <param name="codeLanguage">The code language.</param>
        /// <returns>The comment prefix regex, without trailing spaces.</returns>
        internal static string GetCommentPrefixForLanguage(CodeLanguage codeLanguage)
        {
            switch (codeLanguage)
            {
                case CodeLanguage.CPlusPlus:
                case CodeLanguage.CSharp:
                case CodeLanguage.CSS:
                case CodeLanguage.FSharp:
                case CodeLanguage.JavaScript:
                case CodeLanguage.LESS:
                case CodeLanguage.PHP:
                case CodeLanguage.SCSS:
                case CodeLanguage.TypeScript:
                    return "///?";

                case CodeLanguage.PowerShell:
                case CodeLanguage.R:
                    return "#+";

                case CodeLanguage.VisualBasic:
                    return "'+";
            }

            throw new InvalidOperationException($"No comment prefix defined for '{codeLanguage:g}'.");
        }

        internal static Regex GetCommentLineRegex(CodeLanguage codeLanguage)
        {
            var prefix = GetCommentPrefixForLanguage(codeLanguage);

            // Capture groups:
            // indent: White space before comment prefix.
            // prefix: The comment prefix.
            // initialspacer: Single white space between prefix and comment content.
            // line: Everything that follows the initial spacer, to to newline.
            var pattern = $@"^(?<indent>[\t ]*)(?<prefix>{prefix})(?<initialspacer>( |\t|\r|\n))?(?<line>[^\r\n]*)\r*\n?$";
            return new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        }

        /// <summary>
        /// Gets the regex for matching a complete comment line.
        /// </summary>
        internal static Regex GetCommentWordsRegex()
        {
            // Capture groups:
            // indent: All leading whitespace after prefix and initial spacer.
            // line: All comment following the indent, up to newline.
            // listprefix: Listprefix, indication of this line being a list.
            // words: Individual words, broken on whitespace and newline.
            var pattern = @"^(?<indent>[\t ]*)(?<line>(?<listprefix>[-=\*\+]+[ \t]*|\w+[\):][ \t]+|\d+\.[ \t]+)?((?<words>[^\t\r\n ]+)*[\t ]*)*)\r*\n?$";
            return new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        }

        internal static string SpaceToFake(string value)
        {
            return value.Replace(Spacer, KeepTogetherSpacer);
        }
    }
}