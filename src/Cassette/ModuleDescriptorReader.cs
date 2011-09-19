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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleDescriptorReader
    {
        public ModuleDescriptorReader(IFile sourceFile, IEnumerable<string> allAssetfilenames)
        {
            this.sourceFile = sourceFile;
            this.allAssetfilenames = new HashSet<string>(allAssetfilenames, StringComparer.OrdinalIgnoreCase);
            sectionLineParsers = new Dictionary<string, Action<string>>
            {
                { "assets", ParseAsset },
                { "references", ParseReference },
                { "external", ParseExternal }
            };
        }
            
        readonly IFile sourceFile;
        readonly HashSet<string> allAssetfilenames;
        readonly List<string> assetFilenames = new List<string>();
        readonly HashSet<string> references = new HashSet<string>(); 
        readonly Dictionary<string, Action<string>> sectionLineParsers;
        string currentSection = "assets";
        string externalUrl;
        string fallbackCondition;

        public ModuleDescriptor Read()
        {
            using (var stream = sourceFile.Open(FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ProcessLine(line);
                }
            }
            return new ModuleDescriptor(sourceFile, assetFilenames, true, references, externalUrl, fallbackCondition);
        }

        void ProcessLine(string line)
        {
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line)) return;
            if (IsComment(line)) return;
            line = RemoveTrailingComment(line);
            if (DetermineSection(line)) return;
            sectionLineParsers[currentSection](line);
        }

        bool DetermineSection(string line)
        {
            if (line.StartsWith("["))
            {
                currentSection = line.Substring(1).TrimEnd(']');
                if (sectionLineParsers.ContainsKey(currentSection))
                {
                    return true;
                }
                else
                {
                    throw new Exception(string.Format("Unexpected module descriptor section \"{0}\".", line));
                }
            }
            return false;
        }

        bool IsComment(string line)
        {
            return line.StartsWith("#");
        }

        void ParseAsset(string line)
        {
            if (line == "*")
            {
                foreach (var filename in allAssetfilenames.Except(assetFilenames))
                {
                    assetFilenames.Add(filename);
                }
            }
            else if (FileExists(line))
            {
                if (assetFilenames.Contains(line))
                {
                    throw new Exception(string.Format(
                        "The file \"{0}\" cannot appear twice in module descriptor.",
                        line
                    ));
                }
                assetFilenames.Add(line);
            }
            else
            {
                throw new FileNotFoundException(string.Format(
                    "Cannot find the file \"{0}\", referenced by module descriptor.",
                    line
                ));
            }
        }

        void ParseReference(string line)
        {
            references.Add(line);
        }

        void ParseExternal(string line)
        {
            var keyValueRegex = new Regex(
                @"^\s* (?<key>[a-z]+) \s* = \s* (?<value>.*)$",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            );
            var match = keyValueRegex.Match(line);
            if (match.Success)
            {
                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;
                switch (key)
                {
                    case "url":
                        if (externalUrl != null) throw new Exception("The [external] section of module descriptor can only contain one \"url\".");
                        if (value.IsUrl() == false) throw new Exception("The value \"url\" in module descriptor [external] section must be a URL.");
                        externalUrl = value;
                        break;

                    case "fallbackCondition":
                        if (externalUrl==null) throw new Exception("The [external] section of module descriptor must contain a \"url\" property before the \"fallbackCondition\" property.");
                        if (fallbackCondition != null) throw new Exception("The [external] section of module descriptor can only contain one \"fallbackCondition\".");
                        fallbackCondition = value;
                        break;

                    default:
                        throw new Exception("Unexpected property in module descriptor [external] section: " + line);
                }
            }
            else
            {
                throw new Exception("The [external] section of module descriptor must contain key value pairs.");
            }
        }

        string RemoveTrailingComment(string line)
        {
            var commentStart = line.IndexOf('#');
            if (commentStart >= 0)
            {
                line = line.Substring(0, commentStart).TrimEnd();
            }
            return line;
        }

        bool FileExists(string filename)
        {
            return allAssetfilenames.Contains(filename);
        }
    }
}
