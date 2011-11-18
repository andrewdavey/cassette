#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    class BundleContainerFactory : BundleContainerFactoryBase
    {
        public BundleContainerFactory(IDictionary<Type, IBundleFactory<Bundle>> factories)
            : base(factories)
        {
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, CassetteSettings settings)
        {
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();

            ProcessAllBundles(bundlesArray, settings);

            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray, settings);

            return new BundleContainer(bundlesArray.Concat(externalBundles));
        }
    }
}
