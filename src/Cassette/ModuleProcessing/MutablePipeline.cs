using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.ModuleProcessing
{
    public abstract class MutablePipeline<T> : IModuleProcessor<T>
        where T : Module
    {
        readonly List<Func<IEnumerable<IModuleProcessor<T>>, IEnumerable<IModuleProcessor<T>>>> pipelineModifiers = new List<Func<IEnumerable<IModuleProcessor<T>>, IEnumerable<IModuleProcessor<T>>>>();

        public void Process(T module, ICassetteApplication application)
        {
            var steps = pipelineModifiers.Aggregate(
                CreatePipeline(module, application),
                (pipeline, modify) => modify(pipeline)
            );
            foreach (var step in steps)
            {
                step.Process(module, application);
            }
        }

        public MutablePipeline<T> Prepend(IModuleProcessor<T> step)
        {
            pipelineModifiers.Add(
                steps => (new[] { step }).Concat(steps)
            );
            return this;
        }

        public MutablePipeline<T> Append(IModuleProcessor<T> step)
        {
            pipelineModifiers.Add(
                steps => steps.Concat(new[] { step })
            );
            return this;
        }

        public MutablePipeline<T> Remove<TStep>()
            where TStep : IModuleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.Where(step => (step is TStep) == false)
            );
            return this;
        }

        public MutablePipeline<T> Replace<TStep>(IModuleProcessor<T> newStep)
            where TStep : IModuleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.Select(
                    step => step is TStep ? newStep : step
                )
            );
            return this;
        }

        public MutablePipeline<T> InsertAfter<TStep>(IModuleProcessor<T> newStep)
            where TStep : IModuleProcessor<T>
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

        public MutablePipeline<T> InsertBefore<TStep>(IModuleProcessor<T> newStep)
            where TStep : IModuleProcessor<T>
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
            where TStep : IModuleProcessor<T>
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

        protected abstract IEnumerable<IModuleProcessor<T>> CreatePipeline(T module, ICassetteApplication application);
    }
}
