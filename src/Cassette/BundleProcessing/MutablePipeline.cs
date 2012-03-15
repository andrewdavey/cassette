using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public abstract class MutablePipeline<T> : IBundleProcessor<T>
        where T : Bundle
    {
        readonly List<Func<IEnumerable<IBundleProcessor<T>>, IEnumerable<IBundleProcessor<T>>>> pipelineModifiers = new List<Func<IEnumerable<IBundleProcessor<T>>, IEnumerable<IBundleProcessor<T>>>>();

        public void Process(T bundle, CassetteSettings settings)
        {
            var steps = CreateMutatedPipeline(bundle, settings);
            foreach (var step in steps)
            {
                step.Process(bundle, settings);
            }
        }

        protected virtual IEnumerable<IBundleProcessor<T>> CreateMutatedPipeline(T bundle, CassetteSettings settings)
        {
            return pipelineModifiers.Aggregate(
                CreatePipeline(bundle, settings),
                (pipeline, modify) => modify(pipeline)
            );
        }

        public MutablePipeline<T> Prepend(IBundleProcessor<T> step)
        {
            pipelineModifiers.Add(
                steps => (new[] { step }).Concat(steps)
            );
            return this;
        }

        public MutablePipeline<T> Append(IBundleProcessor<T> step)
        {
            pipelineModifiers.Add(
                steps => steps.Concat(new[] { step })
            );
            return this;
        }

        public MutablePipeline<T> Remove<TStep>()
            where TStep : IBundleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.Where(step => (step is TStep) == false)
            );
            return this;
        }

        public MutablePipeline<T> Replace<TStep>(IBundleProcessor<T> newStep)
            where TStep : IBundleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.Select(
                    step => step is TStep ? newStep : step
                )
            );
            return this;
        }

        public MutablePipeline<T> InsertAfter<TStep>(IBundleProcessor<T> newStep)
            where TStep : IBundleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.SelectMany(
                    step => step is TStep 
                        ? new[] { step, newStep } 
                        : new[] { step }
                )
            );
            return this;
        }

        public MutablePipeline<T> InsertAfter<TStep>(params IBundleProcessor<T>[] newSteps)
            where TStep : IBundleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.SelectMany(
                    step => step is TStep
                        ? new[] { step }.Concat(newSteps)
                        : new[] { step }
                )
            );
            return this;
        }

        public MutablePipeline<T> InsertBefore<TStep>(IBundleProcessor<T> newStep)
            where TStep : IBundleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.SelectMany(
                    step => step is TStep
                        ? new[] { newStep, step }
                        : new[] { step }
                )
            );
            return this;
        }

        public MutablePipeline<T> Update<TStep>(Action<TStep> update)
            where TStep : IBundleProcessor<T>
        {
            pipelineModifiers.Add(
                steps =>
                {
                    foreach (var step in steps.OfType<TStep>())
                    {
                        update(step);
                    }
                    return steps;
                }
            );
            return this;
        }

        protected abstract IEnumerable<IBundleProcessor<T>> CreatePipeline(T bundle, CassetteSettings settings);
    }
}