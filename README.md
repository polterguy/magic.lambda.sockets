
# Magic Lambda Sockets

This project provides web sockets hooks for for [Magic](https://github.com/polterguy.magic).
The main idea for the project, is that it allows you to resolve Hyperlambda files, executing these,
passing in a URL and JSON arguments over a socket connection, in addition to subscribing to messages
transmitted over a web socket connection. It contains one socket HUB method with the following signature.

```
execute(string file, string json)
```

To connect to the hub use the relative URL `/sockets`, optionally passing in a JWT token, and then
transmit messages to the hub using something such as for instance the following TypeScript.

```typescript
let builder = new HubConnectionBuilder();
this.connection = builder.withUrl('https://api.your-domain.com/signalr', {
    accessTokenFactory: () => 'pass-in-your-JWT-token-here'
  }).build();
this.connection.invoke('execute', '/foo/some-hyperlambda-file', JSON.stringify({foo:'bar'}))
```

The above will resolve to a Hyperlambda file caller `/modules/foo/some-hyperlambda-file.socket.hl`, passing
in the `foo` argument as lambda nodes. In addition you can invoke SignalR methods by signaling
the **[signalr.invoke]** slot, which will automatically transform the specified children nodes to JSON
and invoke the specified method. Below is an example.

```
signalr.invoke:foo.bar
   roles:root, admin
   args
      howdy:world
```

The above will invoke the `foo.bar` method, passing in `{"howdy":"world"}` as a JSON string, but only users belonging
to the roles of either `admin` or `root` will be notified. Both the **[roles]** and the **[args]** arguments are optionally.

To subscribe to the above invocation, you could use something such as the following in TypeScript.

```typescript
this.connection.on('foo.bar', (args) => {
  console.log(JSON.parse(args));
});
```

