/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.sockets.slots
{
    /// <summary>
    /// [sockets.connection.leave-group] slot that allows you to de-associate the current
    /// SignalR connectionId with a group.
    /// </summary>
    [Slot(Name = "sockets.connection.leave-group")]
    public class LeaveGroup : ISlot, ISlotAsync
    {
        readonly IHubContext<MagicHub> _context;

        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="context">Dependency injected SignalR HUB references.</param>
        public LeaveGroup(IHubContext<MagicHub> context)
        {
            _context = context;
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            SignalAsync(signaler, input).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>Awaitable task</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Retrieving arguments.
            var args = EnterGroup.GetArgs(signaler, input, "sockets.connection.leave-group");

            // Associating user with group.
            await _context.Groups.RemoveFromGroupAsync(args.ConnectionId, "group:" + args.Group);
        }
    }
}
