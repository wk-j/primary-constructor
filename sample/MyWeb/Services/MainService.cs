using Microsoft.Extensions.Logging;

namespace MyWeb.Services {
    public abstract class MainService {
        protected ILogger<MainService> Logger { get; }
    }
}