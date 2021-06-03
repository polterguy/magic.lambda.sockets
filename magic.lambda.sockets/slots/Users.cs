/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.sockets
{
    /// <summary>
    /// [sockets.users] slot that returns currently connected users.
    /// </summary>
    [Slot(Name = "sockets.users")]
    public class Users : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            input.AddRange(MagicHub.GetUsers().Select(x => new Node("", x)));
        }
    }
}
