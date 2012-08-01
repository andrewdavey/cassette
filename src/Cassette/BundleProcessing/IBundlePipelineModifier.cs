namespace Cassette.BundleProcessing
{
    public interface IBundlePipelineModifier<T> where T : Bundle
    {
        IBundlePipeline<T> Modify(IBundlePipeline<T> pipeline);
    }
}