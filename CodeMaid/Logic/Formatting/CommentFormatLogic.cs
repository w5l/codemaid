using EnvDTE;
using SteveCadwallader.CodeMaid.Helpers;
using SteveCadwallader.CodeMaid.Model.Comments;
using SteveCadwallader.CodeMaid.Model.Comments.Options;
using SteveCadwallader.CodeMaid.Properties;
using System.Linq;

namespace SteveCadwallader.CodeMaid.Logic.Formatting
{
    /// <summary>
    /// A class for encapsulating comment formatting logic.
    /// </summary>
    internal class CommentFormatLogic
    {
        #region Fields

        /// <summary>
        /// The singleton instance of the <see cref="CommentFormatLogic" /> class.
        /// </summary>
        private static CommentFormatLogic _instance;

        private readonly CodeMaidPackage _package;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentFormatLogic" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private CommentFormatLogic(CodeMaidPackage package)
        {
            _package = package;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Reformat all comments in the specified document.
        /// </summary>
        /// <param name="textDocument">The text document.</param>
        public void FormatComments(TextDocument textDocument)
        {
            if (!Settings.Default.Formatting_CommentRunDuringCleanup) return;

            FormatComments(textDocument.StartPoint.CreateEditPoint(), textDocument.EndPoint.CreateEditPoint());
        }

        /// <summary>
        /// Reformat all comments between the specified start and end point. Comments that start
        /// within the range, even if they expand beyond the start or end point, are included.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <returns><c>true</c> if comments are found, otherwise <c>false</c>.</returns>
        public bool FormatComments(EditPoint startPoint, EditPoint endPoint)
        {
            bool foundComments = false;

            var options = FormatterOptions
                .FromSettings(Settings.Default)
                .Set(o =>
                {
                    o.TabSize = startPoint.Parent.TabSize;
                    o.IgnoreTokens = CodeCommentHelper
                        .GetTaskListTokens(_package)
                        .Concat(Settings.Default.Formatting_IgnoreLinesStartingWith.Cast<string>())
                        .ToArray();
                });

            var searcher = new CommentSearcher();
            CommentSearcherLocation location;
            while ((location = searcher.Find(startPoint, endPoint)).Valid)
            {
                foundComments = true;
                var originalText = location.StartPoint.GetText(location.EndPoint);

                var parser = new CodeCommentParser(options, location.CodeLanguage);
                var comment = parser.Parse(originalText);

                var formatter = new CommentFormatter(comment);
                var formattedText = formatter.Format(options);

                if (!formattedText.Equals(originalText))
                {
                    var cursor = location.StartPoint.CreateEditPoint();
                    cursor.Delete(location.EndPoint);
                    cursor.Insert(formattedText);

                    startPoint = cursor.CreateEditPoint();
                }
                else
                {
                    startPoint = location.EndPoint.CreateEditPoint();
                }

                startPoint.LineDown();
            }

            return foundComments;
        }

        /// <summary>
        /// Gets an instance of the <see cref="CommentFormatLogic" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="CommentFormatLogic" /> class.</returns>
        internal static CommentFormatLogic GetInstance(CodeMaidPackage package)
        {
            return _instance ?? (_instance = new CommentFormatLogic(package));
        }

        #endregion Methods
    }
}