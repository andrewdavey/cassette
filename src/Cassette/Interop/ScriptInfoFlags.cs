// Modified from the COM interop code used in https://github.com/xpaulbettsx/SassAndCoffee by @kogir

using System;

namespace Cassette.Interop
{
    [Flags]
    enum ScriptInfoFlags : uint
    {
        /// <summary>
        /// Not a valid option.
        /// </summary>
        None = 0,

        /// <summary>
        /// Returns the IUnknown interface for this item.
        /// </summary>
        IUnknown = 1,

        /// <summary>
        /// Returns the ITypeInfo interface for this item.
        /// </summary>
        ITypeInfo = 2
    }
}