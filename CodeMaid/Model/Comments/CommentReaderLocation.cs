using EnvDTE;
using SteveCadwallader.CodeMaid.Helpers;

namespace SteveCadwallader.CodeMaid.Model.Comments
{
    internal struct CommentReaderLocation
    {
        public CommentReaderLocation(EditPoint startPoint, EditPoint endPoint, CodeLanguage codeLanguage)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            CodeLanguage = codeLanguage;
            Valid = (startPoint != null && endPoint != null);
        }

        public static CommentReaderLocation None { get; } = new CommentReaderLocation(null, null, CodeLanguage.Unknown);

        public CodeLanguage CodeLanguage { get; }

        public EditPoint EndPoint { get; }

        public EditPoint StartPoint { get; }

        public bool Valid { get; }
    }
}