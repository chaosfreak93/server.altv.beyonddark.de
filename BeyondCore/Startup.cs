using AltV.Net;
using AltV.Net.Async;

namespace BeyondCore
{
    public class Startup : AsyncResource
    {
        public override void OnStart() {
            var core = new Core();
            Alt.Log("BeyondCore enabled.");
        }

        public override void OnStop() {
            Alt.Log("BeyondCore disabled.");
        }
    }
}