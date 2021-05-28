/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.signalr
{
    /// <summary>
    /// Main Magic SignalR hub.
    /// </summary>
    public class MagicHub : Hub
    {
        readonly ISignaler _signaler;

        /// <summary>
        /// Constructs an instance of your class.
        /// </summary>
        public MagicHub(ISignaler signaler)
        {
            _signaler = signaler;            
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
            file = "/modules" + file;

            // Transforming from JSON to lambda node structure.
            var node = new Node("", json);
            if (!string.IsNullOrEmpty(json))
                _signaler.Signal("json2lambda", node);

            // Executing file.
            node.Value = file;
            await _signaler.SignalAsync("io.file.execute", node);
        }

        #region [ -- Overridden base class methods -- ]

        /*
         * Overridden since we need to add user to all groups according to what roles user belongs to.
         */
        public override async Task OnConnectedAsync()
        {
            // Retrieving roles user belongs to.
            var rolesNode = new Node();
            await _signaler.SignalAsync("auth.ticket.get", rolesNode);
            var inRoles = rolesNode.Children.FirstOrDefault(x => x.Name == "roles");
            if (inRoles != null)
            {
                foreach (var idx in inRoles.Children.Select(x => x.Get<string>()))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, idx);
                }
            }
            await base.OnConnectedAsync();
        }

        #endregion
    }
}
