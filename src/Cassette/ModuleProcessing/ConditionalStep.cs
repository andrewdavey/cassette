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

namespace Cassette.BundleProcessing
{
    public class ConditionalStep<T> : IBundleProcessor<T>
        where T : Bundle
    {
        public ConditionalStep(
            Func<Bundle, ICassetteApplication, bool> condition, 
            params IBundleProcessor<T>[] children)
        {
            this.condition = condition;
            this.children = children;
        }

        readonly Func<Bundle, ICassetteApplication, bool> condition;
        readonly IBundleProcessor<T>[] children;

        public void Process(T bundle, ICassetteApplication application)
        {
            if (condition(bundle, application) == false) return;

            ProcessEachChild(bundle, application);
        }

        void ProcessEachChild(T bundle, ICassetteApplication application)
        {
            foreach (var child in children)
            {
                child.Process(bundle, application);
            }
        }
    }
}

