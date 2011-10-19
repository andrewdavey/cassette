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
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class BundleDescriptorReader
    {
        public BundleDescriptorReader(IFile sourceFile)
        {
            this.sourceFile = sourceFile;
            sectionLineParsers = new Dictionary<string, Action<string>>
            {
                { "assets", ParseAsset },
                { "references", ParseReference },
                { "external", ParseExternal }
            };
        }
            
        readonly IFile sourceFile;
        readonly List<string> assetFilenames = new List<string>();
        readonly HashSet<string> references = new HashSet<string>(); 
        readonly Dictionary<string, Action<string>> sectionLineParsers;
        string currentSection = "assets";
        string externalUrl;
        string fallbackCondition;

        public BundleDescriptor Read()
        {
            using (var stream = sourceFile.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ProcessLine(line);
                }
            }
            return new BundleDescriptor(sourceFile, assetFilenames, true, references, externalUrl, fallbackCondition);
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
                    throw new Exception(string.Format("Unexpected bundle descriptor section \"{0}\".", line));
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
            assetFilenames.Add(line);
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
                        if (externalUrl != null) throw new Exception("The [external] section of bundle descriptor can only contain one \"url\".");
                        if (value.IsUrl() == false) throw new Exception("The value \"url\" in bundle descriptor [external] section must be a URL.");
                        externalUrl = value;
                        break;

                    case "fallbackCondition":
                        if (externalUrl==null) throw new Exception("The [external] section of bundle descriptor must contain a \"url\" property before the \"fallbackCondition\" property.");
                        if (fallbackCondition != null) throw new Exception("The [external] section of bundle descriptor can only contain one \"fallbackCondition\".");
                        fallbackCondition = value;
                        break;

                    default:
                        throw new Exception("Unexpected property in bundle descriptor [external] section: " + line);
                }
            }
            else
            {
                throw new Exception("The [external] section of bundle descriptor must contain key value pairs.");
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
    }
}