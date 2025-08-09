using Microsoft.Xrm.Sdk;
using System;

namespace demo
{
    /// <summary>
    /// Demonstrates a simple Create operation: adds a Note to the created Account.
    /// Register on: Message=Create, Primary Entity=account, Stage=PostOperation (40)
    /// </summary>
    public class CreatePlugin : PluginBase
    {
        public CreatePlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(CreatePlugin))
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

            // Create a Note (annotation) attached to the new account
            var note = new Entity("annotation");
            note["subject"] = "Demo CreatePlugin note";
            note["notetext"] = $"Created automatically at {DateTime.UtcNow:O}";
            note["objectid"] = new EntityReference("account", accountId);

            var noteId = svc.Create(note);
            trace?.Trace($"Created Note {noteId} for Account {accountId}");
        }
    }
}
