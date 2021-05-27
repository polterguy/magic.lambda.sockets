/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.signalr
{
    /// <summary>
    /// Main SignalR hub.
    /// </summary>
    public class MagicHub : Hub
    {
        // Signaler to use when executing files.
        readonly ISignaler _signaler;

        /// <summary>
        /// Constructs an instance of your class.
        /// </summary>
        public MagicHub(ISignaler signaler)
        {
            _signaler = signaler;            
        }

        /// <summary>
        /// Signals the specified message with the specified arguments.
        /// </summary>
        /// <param name="file">Hyperlambda file to execute</param>
        /// <param name="json">Argument for invocation</param>
        /// <returns>Awaitable task</returns>
        public async Task execute(string file, string json)
        {
            // Appending the correct file extension(s) to invocation.
            file += ".signalr.hl";

            // Transforming from JSON to lambda node structure.
            var node = new Node("", json);
            if (!string.IsNullOrEmpty(json))
                _signaler.Signal("json2lambda", node);

            // Executing file.
            node.Value = file;
            await _signaler.SignalAsync("io.file.execute", node);
        }
    }
}
