/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.endpoint.contracts;
using magic.node.extensions.hyperlambda;

namespace magic.lambda.sockets
{
    /// <summary>
    /// Socket hub allowing users to invoke SignalR methods over web socket connections, to
    /// signal subscribers of specific messages.
    /// </summary>
    public class MagicHub : Hub
    {
        readonly static Dictionary<string, List<string>> _userConnections = new Dictionary<string, List<string>>();
        readonly static object _locker = new object();
        readonly IArgumentsHandler _argumentsHandler;
        readonly ISignaler _signaler;

        /// <summary>
        /// Constructs an instance of your class.
        /// </summary>
        public MagicHub(ISignaler signaler, IArgumentsHandler argumentsHandler)
        {
            _signaler = signaler;  
            _argumentsHandler = argumentsHandler;          
        }

        /// <summary>
        /// Executes the specified Hyperlambda file with the specified arguments.
        /// </summary>
        /// <param name="file">Hyperlambda file to execute</param>
        /// <param name="json">JSON arguments for invocation in string format</param>
        /// <returns>Awaitable task</returns>
        public async Task execute(string file, string json)
        {
            // Appending the correct file extension(s) to invocation.
            file += ".socket.hl";
            file = "modules" + file;

            // Retrieving root folder from where to resolve files.
            var rootFolder = new Node();
            _signaler.Signal(".io.folder.root", rootFolder);
            file = rootFolder.Get<string>() + file;

            // Transforming from JSON to lambda node structure.
            var payload = new Node("", json);
            if (!string.IsNullOrEmpty(json))
                _signaler.Signal("json2lambda", payload);

            // Reading and parsing file as Hyperlambda.
            using (var stream = File.OpenRead(file))
            {
                // Creating our lambda object and attaching arguments specified as query parameters, and/or payload.
                var lambda = new Parser(stream).Lambda();
                _argumentsHandler.Attach(lambda, null, payload);

                // Making sure we push the current connection information into our stack.
                await _signaler.ScopeAsync("dynamic.sockets.connection", Context.ConnectionId, async () =>
                {
                    // Executing file.
                    await _signaler.SignalAsync("eval", lambda);
                });
            }
        }

        #region [ -- Internal static helper methods -- ]

        internal static string[] GetConnections(string username)
        {
            lock (_locker)
            {
                if (_userConnections.TryGetValue(username, out var result))
                    return result.ToArray();
                return Array.Empty<string>();
            }
        }

        #endregion

        #region [ -- Overridden base class methods -- ]

        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            /*
             * Retrieving roles user belongs to and associating
             * user with groups resembling role names, allowing us to only signal users
             * belonging to some specific role(s) later.
             */
            var userNode = new Node();
            await _signaler.SignalAsync("auth.ticket.get", userNode);
            var inRoles = userNode.Children.FirstOrDefault(x => x.Name == "roles");
            if (inRoles != null)
            {
                foreach (var idx in inRoles.Children.Select(x => x.Get<string>()))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "role:" + idx);
                }
            }

            /*
             * Creating an association between a user and all connections in our shared static
             * dictionary.
             */
            var username = userNode.Get<string>();
            if (username != null)
            {
                // Client is authenticated a a user, associating connection with user.
                lock (_locker)
                {
                    if (!_userConnections.TryGetValue(username, out var connections))
                        connections = new List<string>();
                    connections.Add(Context.ConnectionId);
                }
            }
            await base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name;
            if (username != null)
            {
                lock (_locker)
                {
                    if (_userConnections.TryGetValue(username, out var connections) &&
                        connections.Remove(Context.ConnectionId) &&
                        connections.Count == 0)
                        _userConnections.Remove(username);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
