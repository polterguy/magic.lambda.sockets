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
    /// [sockets.user.remove-from-group] slot that allows you to explicitly remove a user from a group.
    /// </summary>
    [Slot(Name = "sockets.user.remove-from-group")]
    public class RemoveUserFromGroup : ISlot, ISlotAsync
    {
        readonly IHubContext<MagicHub> _context;

        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="context">Dependency injected SignalR HUB references.</param>
        public RemoveUserFromGroup(IHubContext<MagicHub> context)
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
            var args = AddUserToGroup.GetArgs(input, "sockets.user.remove-from-group");

            // Iterating through each existing connection for user, associating user with specified group.
            foreach (var idx in MagicHub.GetConnections(args.Username))
            {
                await _context.Groups.RemoveFromGroupAsync(idx, "group:" + args.Group);
            }
        }
    }
}
