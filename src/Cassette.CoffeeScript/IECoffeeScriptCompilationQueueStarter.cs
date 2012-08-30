#if !NET35
namespace Cassette.Scripts
{
    public class IECoffeeScriptCompilationQueueStarter : IStartUpTask
    {
        readonly IECoffeeScriptCompilationQueue queue;

        public IECoffeeScriptCompilationQueueStarter(IECoffeeScriptCompilationQueue queue)
        {
            this.queue = queue;
        }

        public void Start()
        {
            queue.Start();
        }
    }
}
#endif