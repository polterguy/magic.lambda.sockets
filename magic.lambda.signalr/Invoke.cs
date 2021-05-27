/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.signalr
{
    /// <summary>
    /// [signalr.invoke] slot that allows you to invoke a SignalR method.
    /// </summary>
    [Slot(Name = "signalr.invoke")]
    public class Invoke : ISlotAsync
    {
        readonly IHubContext<MagicHub> _context;

        public Invoke(IHubContext<MagicHub> context)
        {
            _context = context;
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var method = input.GetEx<string>();
            input.Value = null;
            signaler.Signal("lambda2json", input);
            await _context.Clients.All.SendAsync(method, input.Get<string>());
        }
    }
}
