namespace Cassette.Spriting.Spritastic.Selector
{
    interface ICssSelectorAnalyzer
    {
        bool IsInScopeOfTarget(string targetSelector, string comparableSelector);
    }
}