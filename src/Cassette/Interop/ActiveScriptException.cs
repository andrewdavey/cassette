// Modified from the COM interop code used in https://github.com/xpaulbettsx/SassAndCoffee by @kogir

using System;
using System.Runtime.InteropServices.ComTypes;

namespace Cassette.Interop
{
    public sealed class ActiveScriptException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveScriptException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        ActiveScriptException(string message)
            : base(message)
        {
        }

        internal static ActiveScriptException Create(IActiveScriptError error)
        {
            var source = "";
            uint sourceContext = 0;
            uint lineNumber = 0;
            var characterPosition = 0;

            var message = "";

            try
            {
                error.GetSourceLineText(out source);
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }

            try
            {
                error.GetSourcePosition(out sourceContext, out lineNumber, out characterPosition);
                ++lineNumber;
                ++characterPosition;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }

            try
            {
                EXCEPINFO excepInfo;
                error.GetExceptionInfo(out excepInfo);
                message = excepInfo.bstrDescription;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }

            return new ActiveScriptException(message)
            {
                LineContent = source,
                SourceContext = sourceContext,
                LineNumber = lineNumber,
                Column = characterPosition,
            };
        }

        /// <summary>
        /// Gets or sets the application specific source context.
        /// </summary>
        public uint SourceContext { get; set; }

        /// <summary>
        /// Gets or sets the line number on which the error occurred.
        /// </summary>
        public uint LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the column on which the error occurred..
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the content of the line on which the error occurred..
        /// </summary>
        public string LineContent { get; set; }
    }
}