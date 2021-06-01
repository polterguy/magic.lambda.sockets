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

namespace magic.lambda.sockets
{
    /// <summary>
    /// [sockets.signal] slot that allows you to publish a message to subscribers
    /// having subscribed to the specified message over a (web) socket connection.
    /// </summary>
    [Slot(Name = "sockets.signal")]
    public class Signaler : ISlot, ISlotAsync
    {
        readonly IHubContext<MagicHub> _context;

        public Signaler(IHubContext<MagicHub> context)
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
            this.SignalAsync(signaler, input).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        /// <returns>Awaitable task</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Retrieving method name.
            var method = input.GetEx<string>() ??
                throw new ArgumentException("No method name provided to [sockets.signal]");

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

            /*
             * Checking if caller wants to restrict message to only users belonging to a specific role,
             * or only a list of named users.
             */
            var roles = input.Children.FirstOrDefault(x => x.Name == "roles")?.GetEx<string>();
            var users = input.Children.FirstOrDefault(x => x.Name == "users")?.GetEx<string>();

            // Invoking method.
            if (!string.IsNullOrEmpty(roles))
            {
                if (users != null)
                    throw new ArgumentException("[sockets.signal] cannot be given both a list of [roles] and a list of [users], choose only one or none");
                await _context
                    .Clients
                    .Groups(roles.Split(',').Select(x => "role:" + x.Trim()).ToArray())
                    .SendAsync(method, json);
            }
            else
            {
                if (users != null)
                    await _context
                        .Clients
                        .Users(users.Split(',').Select(x => x.Trim()).ToArray())
                        .SendAsync(method, json);
                else
                    await _context
                        .Clients
                        .All
                        .SendAsync(method, json);
            }
        }
    }
}
