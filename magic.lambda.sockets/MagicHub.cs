/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        readonly ISignaler _signaler;
        readonly IArgumentsHandler _argumentsHandler;

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

                // Executing file.
                await _signaler.SignalAsync("eval", lambda);
            }
        }

        #region [ -- Overridden base class methods -- ]

        /*
         * Overridden since we need to add user to all groups according to what roles
         * user belongs to.
         */
        public override async Task OnConnectedAsync()
        {
            /*
             * Retrieving roles user belongs to and associating
             * user with groups resembling role names, allowing us to only signal users
             * belonging to some specific role(s) later.
             */
            var rolesNode = new Node();
            await _signaler.SignalAsync("auth.ticket.get", rolesNode);
            var inRoles = rolesNode.Children.FirstOrDefault(x => x.Name == "roles");
            if (inRoles != null)
            {
                foreach (var idx in inRoles.Children.Select(x => x.Get<string>()))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "role:" + idx);
                }
            }
            await base.OnConnectedAsync();
        }

        #endregion
    }
}
