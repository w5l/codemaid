using EnvDTE;
using SteveCadwallader.CodeMaid.Helpers;
using System;
using System.Text.RegularExpressions;

namespace SteveCadwallader.CodeMaid.Model.Comments
{
    internal class CommentReader
    {
        private CodeLanguage codeLanguage;
        private Regex commentLineRegex;

        /// <summary>
        /// Find the first comment between the specified start and end point. Comments that start
        /// within the range, even if they expand beyond the start or end point, are included.
        /// </summary>
        public CommentReaderLocation Find(EditPoint startPoint, EditPoint endPoint)
        {
            if (startPoint.Parent != endPoint.Parent)
            {
                throw new InvalidOperationException($"{nameof(startPoint)} and {nameof(endPoint)} must have the same {nameof(startPoint.Parent)}.");
            }

            if (codeLanguage != startPoint.GetCodeLanguage())
            {
                codeLanguage = startPoint.GetCodeLanguage();
                commentLineRegex = CodeCommentHelper.GetCommentLineRegex(codeLanguage);
            }

            while (startPoint.Line <= endPoint.Line)
            {
                var location = Expand(startPoint);
                if (location.Valid)
                {
                    return location;
                }

                startPoint.LineDown();
                if (startPoint.AtEndOfDocument)
                {
                    break;
                }
            }

            return CommentReaderLocation.None;
        }

        /// <summary>
        /// Expands a text point to a full comment.
        /// </summary>
        /// <param name="point">The original point to expand from.</param>
        /// <returns>
        /// The start and endpoint of the full comment block, or <c>null</c> if there is no comment
        /// on the current line.
        /// </returns>
        private CommentReaderLocation Expand(EditPoint point)
        {
            // Look up to find the start of the comment.
            EditPoint
                 startPoint = Expand(point, p => p.LineUp()),
                 endPoint = null;

            // If no startpoint found, there is no valid (formattable) comment on this line.
            if (startPoint == null)
            {
                return CommentReaderLocation.None;
            }

            // TODO: Does this mean we look at the "current line" twice? I think it does. Optimize!
            endPoint = Expand(point, p => p.LineDown());

            // If no endpoint is found, there is no valid (formattable) comment on this line.
            if (endPoint == null)
            {
                return CommentReaderLocation.None;
            }

            startPoint.StartOfLine();
            endPoint.EndOfLine();
            return new CommentReaderLocation(startPoint, endPoint, codeLanguage);
        }

        /// <summary>
        /// Expand a textpoint to the full comment, in the direction specified by the <paramref
        /// name="foundAction" />.
        /// </summary>
        /// <param name="point">The initial starting point for the expansion.</param>
        /// <param name="foundAction">An action which advances the search either up or down.</param>
        /// <returns>
        /// The endpoint of the comment, or <c>null</c> if the expansion did not find a valid comment.
        /// </returns>
        private EditPoint Expand(TextPoint point, Action<EditPoint> foundAction)
        {
            EditPoint current = point.CreateEditPoint();
            EditPoint result = null;
            string prefix = null;

            do
            {
                var line = current.Line;
                var text = current.GetLine();

                var match = commentLineRegex.Match(text);
                if (match.Success)
                {
                    // Cancel the expansion if the prefix does not match. This takes priority over
                    // the initial spacer check to allow formatting comments adjacent to Stylecop
                    // SA1626 style commented code.
                    var currentPrefix = match.Groups["prefix"].Value.TrimStart();
                    if (prefix != null && !string.Equals(prefix, currentPrefix))
                    {
                        break;
                    }
                    else
                    {
                        prefix = currentPrefix;
                    }

                    // The initial spacer is required, otherwise we assume this is commented out code
                    // and do not format.
                    if (match.Groups["initialspacer"].Success)
                    {
                        result = current.CreateEditPoint();
                        foundAction(current);

                        // If result and iterator line are the same, the found action (move line up or
                        // down) did nothing. This means we're at the start or end of the file, and
                        // there is no point to keep searching, it would create an infinite loop.
                        if (result.Line == current.Line)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // Did not succesfully match the intial spacer, we have to assume this is
                        // code and cancel all formatting.
                        result = null;
                        current = null;
                    }
                }
                else
                {
                    current = null;
                }
            } while (current != null);

            return result;
        }
    }
}