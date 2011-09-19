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
using System.Linq;

namespace Cassette.ModuleProcessing
{
    public class Pipeline<T> : IModuleProcessor<T>
        where T : Module
    {
        public Pipeline(params IModuleProcessor<T>[] steps)
        {
            if (steps == null)
            {
                throw new ArgumentNullException("steps");
            }
            if (steps.Any(step => step == null))
            {
                throw new ArgumentException("Pipeline steps cannot be null.");
            }

            this.steps = steps;
        }

        readonly IModuleProcessor<T>[] steps;

        public void Process(T module, ICassetteApplication application)
        {
            foreach (var step in steps)
            {
                step.Process(module, application);
            }
        }
    }
}

