namespace Cassette.ReduceRequest.Reducer
{
    public interface ICssSelectorAnalyzer
    {
        bool IsInScopeOfTarget(string targetSelector, string comparableSelector);
    }
}