namespace Cassette.BundleProcessing
{
    public static class BundlePipelineHelpers
    {
        /// <summary>
        /// Inserts bundle processors before the first instance of <typeparamref name="TItem"/> in the pipeline.
        /// </summary>
        /// <typeparam name="TItem">The type of step to insert before.</typeparam>
        /// <typeparam name="TBundle">The type of bundle the pipeline processes.</typeparam>
        /// <param name="pipeline">The pipeline to insert into.</param>
        /// <param name="processorsToInsert">The bundle processors to insert.</param>
        public static void InsertBefore<TItem, TBundle>(this IBundlePipeline<TBundle> pipeline, params IBundleProcessor<TBundle>[] processorsToInsert)
            where TItem : IBundleProcessor<TBundle>
            where TBundle : Bundle
        {
            for (var i = 0; i < pipeline.Count; i++)
            {
                if (!(pipeline[i] is TItem)) continue;

                foreach (var processor in processorsToInsert)
                {
                    pipeline.Insert(i++, processor);
                }
                break;
            }
        }

        /// <summary>
        /// Inserts bundle processors after the first instance of <typeparamref name="TItem"/> in the pipeline.
        /// </summary>
        /// <typeparam name="TItem">The type of step to insert after.</typeparam>
        /// <typeparam name="TBundle">The type of bundle the pipeline processes.</typeparam>
        /// <param name="pipeline">The pipeline to insert into.</param>
        /// <param name="processorsToInsert">The bundle processors to insert.</param>
        public static void InsertAfter<TItem, TBundle>(this IBundlePipeline<TBundle> pipeline, params IBundleProcessor<TBundle>[] processorsToInsert)
            where TItem : IBundleProcessor<TBundle>
            where TBundle : Bundle
        {
            for (var i = 0; i < pipeline.Count; i++)
            {
                if (!(pipeline[i] is TItem)) continue;

                foreach (var processor in processorsToInsert)
                {
                    pipeline.Insert(++i, processor);
                }
                break;
            }
        }
    }
}