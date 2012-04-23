using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;
using TinyIoC;

namespace Cassette
{
    abstract class ContainerConfiguration<T> : IConfiguration<TinyIoCContainer>
        where T : Bundle
    {
        readonly Func<Type, IEnumerable<Type>> getImplementationTypes;

        protected ContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes)
        {
            this.getImplementationTypes = getImplementationTypes;
        }

        public virtual void Configure(TinyIoCContainer container)
        {
            RegisterFileSearchServices(container);
            RegisterBundleFactory(container);
            RegisterBundlePipelineServices(container);
        }

        protected abstract string FilePattern { get; }

        protected abstract Type BundleFactoryType { get; }

        protected abstract Type BundlePipelineType { get; }

        protected virtual Regex FileSearchExclude
        {
            get { return null; }
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
                getImplementationTypes(typeof(IFileSearchModifier<T>))
            ).AsMultiInstance();
        }

        void RegisterBundlePipelineServices(TinyIoCContainer container)
        {
            container.Register(
                typeof(IBundlePipeline<T>),
                (c, p) => CreateBundlePipeline(c)
            );
            container.RegisterMultiple(
                typeof(IBundlePipelineModifier<T>),
                getImplementationTypes(typeof(IBundlePipelineModifier<T>))
            ).AsMultiInstance();
        }

        void RegisterBundleFactory(TinyIoCContainer container)
        {
            container.Register(typeof(IBundleFactory<T>), BundleFactoryType).AsMultiInstance();
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