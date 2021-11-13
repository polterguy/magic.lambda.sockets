/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.sockets
{
    /// <summary>
    /// [sockets.users.count] slot that returns the number of currently connected users.
    /// </summary>
    [Slot(Name = "sockets.users.count")]
    public class CountUsers : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving arguments.
            var filter = input.Children.FirstOrDefault(x => x.Name == "filter")?.GetEx<string>();

            // House cleaning.
            input.Clear();

            // Returning result to caller.
            input.Value = MagicHub.GetUserCount(filter);
        }
    }
}
