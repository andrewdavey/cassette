namespace Cassette.RequireJS
{
    /// <summary>
    /// Run by Cassette at application start-up.
    /// Initializes static helpers.
    /// </summary>
    public class StartUpTask : IStartUpTask
    {
        readonly ViewHelperImplementation viewHelper;
        readonly RequireJsConfigUrlProvider requireJsConfigUrlProvider;

        public StartUpTask(ViewHelperImplementation viewHelper, RequireJsConfigUrlProvider requireJsConfigUrlProvider)
        {
            this.viewHelper = viewHelper;
            this.requireJsConfigUrlProvider = requireJsConfigUrlProvider;
        }

        public void Start()
        {
            ViewHelper.Instance = viewHelper;
        }
    }
}