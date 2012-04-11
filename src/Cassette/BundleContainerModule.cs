using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using TinyIoC;

namespace Cassette
{
    abstract class BundleContainerModule<T>
        where T : Bundle
    {
        protected abstract string FilePattern { get; }

        protected abstract Type BundleFactoryType { get; }

        protected abstract Type BundlePipelineType { get; }

        protected virtual Regex FileSearchExclude
        {
            get { return null; }
        }

        public void Load(TinyIoCContainer container)
        {
            RegisterFileSearchServices(container);
            RegisterBundleFactory(container);
            RegisterBundlePipelineServices(container);
        }

        void RegisterFileSearchServices(TinyIoCContainer container)
        {
            container.Register(
                typeof(IFileSearch),
                (c, p) => CreateFileSearch(c),
                HostBase.FileSearchComponentName(typeof(T))
            );
            container.RegisterMultiple(
                typeof(IFileSearchModifier<T>),
                AppDomainAssemblyTypeScanner.TypesOf<IFileSearchModifier<T>>()
            );
        }

        void RegisterBundlePipelineServices(TinyIoCContainer container)
        {
            container.Register(
                typeof(IBundlePipeline<T>),
                (c, p) => CreateBundlePipeline(c)
            );
            container.RegisterMultiple(
                typeof(IBundlePipelineModifier<T>),
                AppDomainAssemblyTypeScanner.TypesOf<IBundlePipelineModifier<T>>()
            );
        }

        void RegisterBundleFactory(TinyIoCContainer container)
        {
            container.Register(typeof(IBundleFactory<T>), BundleFactoryType);
        }

        IFileSearch CreateFileSearch(TinyIoCContainer container)
        {
            var fileSearch = new FileSearch
            {
                Pattern = FilePattern,
                SearchOption = SearchOption.AllDirectories,
                Exclude = FileSearchExclude
            };
            ApplyFileSearchModifiers(container, fileSearch);
            return fileSearch;
        }

        void ApplyFileSearchModifiers(TinyIoCContainer container, FileSearch fileSearch)
        {
            var modifiers = container.ResolveAll<IFileSearchModifier<T>>();
            foreach (var modifier in modifiers)
            {
                modifier.Modify(fileSearch);
            }
        }

        IBundlePipeline<T> CreateBundlePipeline(TinyIoCContainer container)
        {
            var pipeline = (IBundlePipeline<T>)container.Resolve(BundlePipelineType);
            return ApplyBundlePipelineModifiers(pipeline, container);
        }

        IBundlePipeline<T> ApplyBundlePipelineModifiers(IBundlePipeline<T> pipeline, TinyIoCContainer container)
        {
            var modifiers = container.ResolveAll<IBundlePipelineModifier<T>>();
            return modifiers.Aggregate(
                pipeline,
                (currentPipeline, modifier) => modifier.Modify(currentPipeline)
            );
        }
    }
}