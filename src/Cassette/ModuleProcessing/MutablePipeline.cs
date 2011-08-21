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

        public void Prepend(IModuleProcessor<T> step)
        {
            pipelineModifiers.Add(
                steps => (new[] { step }).Concat(steps)
            );
        }

        public void Append(IModuleProcessor<T> step)
        {
            pipelineModifiers.Add(
                steps => steps.Concat(new[] { step })
            );
        }

        public void Remove<TStep>()
            where TStep : IModuleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.Where(step => (step is TStep) == false)
            );
        }

        public void Replace<TStep>(IModuleProcessor<T> newStep)
            where TStep : IModuleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.Select(
                    step => step is TStep ? newStep : step
                )
            );
        }

        public void InsertAfter<TStep>(IModuleProcessor<T> newStep)
            where TStep : IModuleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.SelectMany(
                    step => step is TStep 
                        ? new[] { step, newStep } 
                        : new[] { step }
                )
            );
        }

        public void InsertBefore<TStep>(IModuleProcessor<T> newStep)
            where TStep : IModuleProcessor<T>
        {
            pipelineModifiers.Add(
                steps => steps.SelectMany(
                    step => step is TStep
                        ? new[] { newStep, step }
                        : new[] { step }
                )
            );
        }

        public void Update<TStep>(Action<TStep> update)
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
        }

        protected abstract IEnumerable<IModuleProcessor<T>> CreatePipeline(T module, ICassetteApplication application);
    }
}
