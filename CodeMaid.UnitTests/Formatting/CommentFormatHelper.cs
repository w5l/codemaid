using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteveCadwallader.CodeMaid.Helpers;
using SteveCadwallader.CodeMaid.Model.Comments.Options;
using System;

namespace SteveCadwallader.CodeMaid.UnitTests.Formatting
{
    internal class CommentFormatHelper
    {
        public static string AssertEqualAfterFormat(
             string text,
             Action<FormatterOptions> options = null)
        {
            return AssertEqualAfterFormat(text, null, options);
        }

        public static string AssertEqualAfterFormat(
            string text,
            string expected,
            Action<FormatterOptions> options = null)
        {
            var result = CodeCommentHelper.Format(text, options);
            Assert.AreEqual(expected ?? text, result);
            return result;
        }
    }
}