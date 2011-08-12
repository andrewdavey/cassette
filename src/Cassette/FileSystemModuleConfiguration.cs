using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.ModuleProcessing;

namespace Cassette
{
    public class FileSystemModuleConfiguration<T> : IModuleContainerFactory<T>
        where T : Module
    {
        public FileSystemModuleConfiguration(ICassetteApplication application)
        {
            this.application = application;
        }

        readonly ICassetteApplication application;
        readonly List<string> moduleDirectories = new List<string>();
        readonly List<string> searchPatterns = new List<string>();
        readonly List<Regex> exclusions = new List<Regex>();
        Pipeline<T> pipeline;

        public FileSystemModuleConfiguration<T> ForSubDirectoriesOf(string relativePath)
        {
            foreach (var subDirectory in application.RootDirectory.GetDirectories(relativePath))
            {
                if (application.RootDirectory.GetAttributes(subDirectory).HasFlag(FileAttributes.Hidden))
                {
                    continue;
                }
                moduleDirectories.Add(subDirectory);
            }

            return this;
        }

        public FileSystemModuleConfiguration<T> Directories(params string[] relativePaths)
        {
            foreach (var relativePath in relativePaths)
            {
                if (application.RootDirectory.DirectoryExists(relativePath))
                {
                    moduleDirectories.Add(relativePath);
                }
                else
                {
                    throw new DirectoryNotFoundException("Directory not found: " + relativePath);
                }
            }

            return this;
        }

        public FileSystemModuleConfiguration<T> IncludeFiles(params string[] searchPatterns)
        {
            this.searchPatterns.AddRange(searchPatterns);

            return this;
        }

        public FileSystemModuleConfiguration<T> ExcludeFiles(Regex filenameRegex)
        {
            this.exclusions.Add(filenameRegex);

            return this;
        }

        public FileSystemModuleConfiguration<T> ProcessWith(params IModuleProcessor<T>[] steps)
        {
            pipeline = new Pipeline<T>(steps);
            return this;
        }

        IModuleContainer<T> IModuleContainerFactory<T>.CreateModuleContainer()
        {
            var moduleFactory = application.GetModuleFactory<T>();
            List<T> modules;
            DateTime lastWriteTimeMax;
            GetModulesAndLastWriteTime(moduleFactory, out modules, out lastWriteTimeMax);

            if (application.IsOutputOptimized)
            {
                var cache = application.GetModuleCache<T>();
                if (cache.IsUpToDate(lastWriteTimeMax, application.Version))
                {
                    return cache.LoadModuleContainer();
                }
                else
                {
                    ProcessAllModules(modules, application);
                    var container = new ModuleContainer<T>(modules);
                    cache.SaveModuleContainer(container, application.Version);
                    return container;
                }
            }
            else
            {
                ProcessAllModules(modules, application);
                var container = new ModuleContainer<T>(modules);
                return container;
            }
        }

        void GetModulesAndLastWriteTime(IModuleFactory<T> moduleFactory, out List<T> modules, out DateTime lastWriteTimeMax)
        {
            modules = new List<T>();
            lastWriteTimeMax = DateTime.MinValue;
            foreach (var directory in moduleDirectories.DefaultIfEmpty(""))
            {
                var module = moduleFactory.CreateModule(directory);
                var filenames = GetAssetFilenames(directory);
                var offset = (directory.Length > 0) ? directory.Length + 1 : 0;
                foreach (var filename in filenames)
                {
                    var moduleRelativeFilename = filename.Substring(offset);
                    module.Assets.Add(new Asset(moduleRelativeFilename, module));

                    var lastWriteTime = application.RootDirectory.GetLastWriteTimeUtc(filename);
                    if (lastWriteTime > lastWriteTimeMax)
                    {
                        lastWriteTimeMax = lastWriteTime;
                    }
                }
                modules.Add(module);
            }
        }

        IEnumerable<string> GetAssetFilenames(string directory)
        {
            IEnumerable<string> filenames;
            if (searchPatterns.Count == 0)
            {
                filenames = application.RootDirectory.GetFiles(directory);
            }
            else
            {
                filenames = searchPatterns.SelectMany(pattern => application.RootDirectory.GetFiles(directory, pattern)).Distinct();
            }
            foreach (var exclusion in exclusions)
            {
                filenames = filenames.Where(f => exclusion.IsMatch(f) == false);
            }
            return filenames;
        }

        void ProcessAllModules(IEnumerable<T> container, ICassetteApplication application)
        {
            foreach (var module in container)
            {
                pipeline.Process(module, application);
            }
        }
    }
}
