﻿#region CodeMaid is Copyright 2007-2014 Steve Cadwallader.

// CodeMaid is free software: you can redistribute it and/or modify it under the terms of the GNU
// Lesser General Public License version 3 as published by the Free Software Foundation.
//
// CodeMaid is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details <http://www.gnu.org/licenses/>.

#endregion CodeMaid is Copyright 2007-2014 Steve Cadwallader.

using EnvDTE;
using SteveCadwallader.CodeMaid.Model.CodeItems;
using SteveCadwallader.CodeMaid.Properties;
using System.Linq;
using System.Threading.Tasks;

namespace SteveCadwallader.CodeMaid.Model
{
    /// <summary>
    /// A builder class for generating code models.
    /// </summary>
    internal class CodeModelBuilder
    {
        #region Fields

        private readonly CodeMaidPackage _package;
        private readonly CodeModelHelper _codeModelHelper;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The singleton instance of the <see cref="CodeModelBuilder" /> class.
        /// </summary>
        private static CodeModelBuilder _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeModelBuilder" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private CodeModelBuilder(CodeMaidPackage package)
        {
            _package = package;

            _codeModelHelper = CodeModelHelper.GetInstance(_package);
        }

        /// <summary>
        /// Gets an instance of the <see cref="CodeModelBuilder" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="CodeModelBuilder" /> class.</returns>
        internal static CodeModelBuilder GetInstance(CodeMaidPackage package)
        {
            return _instance ?? (_instance = new CodeModelBuilder(package));
        }

        #endregion Constructors

        #region Internal Methods

        /// <summary>
        /// Walks the given document and constructs a <see cref="SetCodeItems" /> of CodeItems
        /// within it including regions.
        /// </summary>
        /// <param name="document">The document to walk.</param>
        /// <returns>The set of code items within the document, including regions.</returns>
        internal SetCodeItems RetrieveAllCodeItems(Document document)
        {
            var codeItems = new SetCodeItems();

            if (document.ProjectItem != null)
            {
                RetrieveCodeItems(codeItems, document.ProjectItem.FileCodeModel);
            }

            codeItems.AddRange(_codeModelHelper.RetrieveCodeRegions(document));

            return codeItems;
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Walks the given FileCodeModel, turning CodeElements into code items within the specified
        /// code items set.
        /// </summary>
        /// <param name="codeItems">The code items set for accumulation.</param>
        /// <param name="fcm">The FileCodeModel to walk.</param>
        private static void RetrieveCodeItems(SetCodeItems codeItems, FileCodeModel fcm)
        {
            if (fcm != null && fcm.CodeElements != null)
            {
                RetrieveCodeItemsFromElements(codeItems, fcm.CodeElements);
            }
        }

        /// <summary>
        /// Retrieves code items from each specified code element into the specified code items set.
        /// </summary>
        /// <param name="codeItems">The code items set for accumulation.</param>
        /// <param name="codeElements">The CodeElements to walk.</param>
        private static void RetrieveCodeItemsFromElements(SetCodeItems codeItems, CodeElements codeElements)
        {
            if (Settings.Default.General_Multithread)
            {
                Parallel.ForEach(codeElements.OfType<CodeElement>(), child => RetrieveCodeItemsRecursively(codeItems, child));
            }
            else
            {
                foreach (CodeElement child in codeElements)
                {
                    RetrieveCodeItemsRecursively(codeItems, child);
                }
            }
        }

        /// <summary>
        /// Recursive method for creating a code item for the specified code element, adding it to
        /// the specified code items set and recursing into all of the code element's children.
        /// </summary>
        /// <param name="codeItems">The code items set for accumulation.</param>
        /// <param name="codeElement">The CodeElement to walk (add and recurse).</param>
        private static void RetrieveCodeItemsRecursively(SetCodeItems codeItems, CodeElement codeElement)
        {
            var parentCodeItem = FactoryCodeItems.CreateCodeItemElement(codeElement);
            if (parentCodeItem != null)
            {
                codeItems.Add(parentCodeItem);
            }

            if (codeElement.Children != null)
            {
                RetrieveCodeItemsFromElements(codeItems, codeElement.Children);
            }
        }

        #endregion Private Methods
    }
}