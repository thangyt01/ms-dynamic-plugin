using Microsoft.Xrm.Sdk;
using System;

namespace demo
{
    /// <summary>
    /// Demonstrates a simple Delete operation: flags the Account for deletion by creating a task, or optionally deletes a helper record.
    /// Example registration: Message=Create, Primary Entity=account, Stage=PostOperation (40)
    /// Note: Deleting the just-created Account here could surprise users; instead we create a follow-up Task.
    /// </summary>
    public class DeletePlugin : PluginBase
    {
        public DeletePlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(DeletePlugin))
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

            // Instead of deleting the new Account (which could be disruptive), create a Task reminder to review/delete
            var task = new Entity("task");
            task["subject"] = "Review account for deletion (demo)";
            task["description"] = "Created by DeletePlugin demo. If this is a test account, consider deleting it.";
            task["regardingobjectid"] = new EntityReference("account", accountId);
            var taskId = svc.Create(task);

            trace?.Trace($"Created Task {taskId} to review Account {accountId} for deletion");

            // If you truly want to delete the just-created account (not recommended), uncomment:
            // svc.Delete("account", accountId);
            // trace?.Trace($"Deleted Account {accountId}");
        }
    }
}
