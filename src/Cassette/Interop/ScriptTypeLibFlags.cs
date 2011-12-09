// Modified from the COM interop code used in https://github.com/xpaulbettsx/SassAndCoffee by @kogir

using System;

namespace Cassette.Interop
{
    [Flags]
    enum ScriptTypeLibFlags : uint
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The type library describes an ActiveX control used by the host.
        /// </summary>
        IsControl = 0x00000010,

        /// <summary>
        /// Not documented.
        /// </summary>
        IsPersistent = 0x00000040,
    }
}