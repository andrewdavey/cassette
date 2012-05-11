using System;
using System.Collections.Generic;

namespace Cassette.Caching
{
    class Manifest
    {
        public static Manifest Static(IEnumerable<Bundle> bundles)
        {
            return new Manifest(bundles, "")
            {
                IsStatic = true
            };
        }

        public IEnumerable<Bundle> Bundles { get; private set; }
        public DateTime CreationDateTime { get; private set; }
        public string Version { get; private set; }
        /// <summary>
        /// A static manifest never needs validating or rebuilding. For example, a manifest built at compile time is static.
        /// </summary>
        public bool IsStatic { get; private set; }

        public Manifest(IEnumerable<Bundle> bundles, string version, DateTime creationDateTime, bool isStatic)
        {
            Bundles = bundles;
            Version = version;
            CreationDateTime = creationDateTime;
            IsStatic = isStatic;
        }

        public Manifest(IEnumerable<Bundle> bundles, string version)
        {
            Bundles = bundles;
            Version = version;
            CreationDateTime = DateTime.UtcNow;
        }
    }
}