namespace Cassette.RequireJS
{
    /// <summary>
    /// Run by Cassette at application start-up.
    /// Initializes static helpers.
    /// </summary>
    public class StartUpTask : IStartUpTask
    {
        readonly ViewHelperImplementation viewHelper;
        
        public StartUpTask(ViewHelperImplementation viewHelper)
        {
            this.viewHelper = viewHelper;
        }

        public void Start()
        {
            ViewHelper.Instance = viewHelper;
        }
    }
}