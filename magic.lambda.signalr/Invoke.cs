/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            // Retrieving method name.
            var method = input.GetEx<string>() ??
                throw new ArgumentException("No method name provided to [signalr.invoke]");

            // Retrieving arguments, if any.
            var args = input.Children.FirstOrDefault(x => x.Name == "args")?.Clone();
            string json = null;
            if (args != null)
            {
                var jsonNode = new Node();
                jsonNode.AddRange(args.Children);
                signaler.Signal("lambda2json", jsonNode);
                json = jsonNode.Get<string>();
            }

            // Checking if caller wants to restrict message to only users belonging to a specific role.
            var roles = input.Children.FirstOrDefault(x => x.Name == "roles")?.GetEx<string>();

            // Invoking method.
            if (!string.IsNullOrEmpty(roles))
                await _context.Clients.Groups(roles.Split(',')).SendAsync(method, json);
            else
                await _context.Clients.All.SendAsync(method, json);
        }
    }
}
