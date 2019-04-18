using SteveCadwallader.CodeMaid.Helpers;
using SteveCadwallader.CodeMaid.Model.Comments.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SteveCadwallader.CodeMaid.Model.Comments
{
    internal class CodeCommentParser
    {
        private readonly CodeLanguage _codeLanguage;
        private readonly Regex _commentLineRegex;
        private readonly FormatterOptions _formatterOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeComment" /> class.
        /// </summary>
        public CodeCommentParser(FormatterOptions formatterOptions, CodeLanguage codeLanguage)
        {
            _codeLanguage = codeLanguage;
            _formatterOptions = formatterOptions;
            _commentLineRegex = CodeCommentHelper.GetCommentLineRegex(_codeLanguage);
        }

        public CodeComment Parse(string text)
        {
            var matches = _commentLineRegex.Matches(text).OfType<Match>().ToArray();

            // TODO: Fail when not able to parse.
            //if (!matches.All(m => m.Success))

            var commentOptions = new CommentOptions
            {
                Indent = matches.FirstOrDefault(m => m.Success).Groups["indent"].Value,
                Prefix = matches.FirstOrDefault(m => m.Success).Groups["prefix"].Value,
                Language = _codeLanguage
            };

            IEnumerable<ICommentLine> lines = null;

            // Concatenate the comment lines without comment prefixes.
            var commentText = string.Join(Environment.NewLine, matches.Select(m => m.Groups["line"].Value));

            // See if the resulting bit can be parsed as XML.
            if (commentText.Contains('<'))
            {
                try
                {
                    // XML needs a single root element, wrap it before parsing.
                    var xml = XElement.Parse($"<doc>{commentText}</doc>");
                    lines = new CommentLineXml(xml, _formatterOptions).Lines;
                }
                catch (System.Xml.XmlException)
                {
                    // If XML cannot be parsed, comment will be handled as a normal text comment.
                }
            }

            if (lines == null)
            {
                lines = new[] { new CommentLine(commentText) };
            }

            return new CodeComment(lines, commentOptions);
        }
    }
}