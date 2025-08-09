using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace demo
{
    /// <summary>
    /// Demonstrates a simple Read operation: reads fields from the created Account and traces them.
    /// Register on: Message=Create, Primary Entity=account, Stage=PostOperation (40)
    /// </summary>
    public class ReadPlugin : PluginBase
    {
        public ReadPlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(ReadPlugin))
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

            var cols = new ColumnSet("name", "accountnumber", "telephone1");
            var account = svc.Retrieve("account", accountId, cols);

            var name = account.GetAttributeValue<string>("name");
            var accNo = account.GetAttributeValue<string>("accountnumber");
            var phone = account.GetAttributeValue<string>("telephone1");

            trace?.Trace($"ReadPlugin → name={name}, accountnumber={accNo}, phone={phone}");
        }
    }
}
