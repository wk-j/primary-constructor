using Microsoft.Extensions.Logging;

namespace MyWeb.Services {
    public abstract class MainService {
        protected ILogger<MainService> Logger { get; }
    }

    [PrimaryConstructor]
    public partial class MyService : MainService {

        public void Start() {
            Logger.LogError("Hello, world!");
        }
    }
}