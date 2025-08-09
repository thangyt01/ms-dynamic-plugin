using Microsoft.Xrm.Sdk;
using System;

namespace demo
{
    /// <summary>
    /// Demonstrates a simple Update operation: sets a description on Account after it's created.
    /// Register on: Message=Create, Primary Entity=account, Stage=PostOperation (40)
    /// </summary>
    public class UpdatePlugin : PluginBase
    {
        public UpdatePlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(UpdatePlugin))
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext local)
        {
            if (local == null) throw new ArgumentNullException(nameof(local));

            var ctx = local.PluginExecutionContext;
            var trace = local.TracingService;

            if (ctx.Depth > 1)
            {
                trace?.Trace("Depth > 1, skipping to avoid recursion");
                return;
            }

            if (!string.Equals(ctx.MessageName, "Create", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(ctx.PrimaryEntityName, "account", StringComparison.OrdinalIgnoreCase)
                || ctx.Stage != 40)
            {
                return;
            }

            var svc = local.PluginUserService;
            var accountId = ctx.PrimaryEntityId;

            var update = new Entity("account", accountId)
            {
                ["description"] = $"Updated by UpdatePlugin at {DateTime.UtcNow:O}"
            };
            svc.Update(update);

            trace?.Trace($"Updated Account {accountId} description");
        }
    }
}
