# **[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net)**
[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) provides an easy-to-use and customizable Tcp Server and Tcp Client. The server is created using a `TcpListener`. The server and client can be used for non-SSL or SSL connections and authentication (including client and server ssl certification validation) is provided for identifying the clients connected to your server. Both client and server are created in [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) and use async await functionality. If you are not familiar with async await functionality, you can learn more by reviewing the information found at the [Microsoft Async Programming Concepts page](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/). All [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) packages referenced in this documentation are available on the [NuGet package manager](https://www.nuget.org/) in 1 aggregate package - [Tcp.NET](https://www.nuget.org/packages/Tcp.NET/).

![Image of Tcp.NET Logo](https://pixelhorrorstudios.s3-us-west-2.amazonaws.com/Packages/Tcp.NETLogo.png)

## **Table of Contents**<!-- omit in toc -->
- [**Tcp.NET**](#tcpnet)
  - [**`IPacket`**](#ipacket)
  - [**Client**](#client)
    - [**`ITcpNETClient`**](#itcpnetclient)
      - [**Parameters**](#parameters)
      - [**`OAuth Token`**](#oauth-token)
      - [**Events**](#events)
      - [**Connect to a Tcp Server**](#connect-to-a-tcp-server)
      - [**SSL**](#ssl)
      - [**Send a Message to the Server**](#send-a-message-to-the-server)
      - [**Extending `IPacket`**](#extending-ipacket)
      - [**Receiving an Extended `IPacket`**](#receiving-an-extended-ipacket)
      - [**`Ping`**](#ping)
      - [**Disconnect from the Server**](#disconnect-from-the-server)
      - [**Disposal**](#disposal)
  - [**Server**](#server)
    - [**`ITcpNETServer`**](#itcpnetserver)
      - [**Parameters**](#parameters-1)
      - [**Events**](#events-1)
      - [**Starting the Tcp Server**](#starting-the-tcp-server)
      - [**SSL**](#ssl-1)
      - [**Send a Message to a Connection**](#send-a-message-to-a-connection)
      - [**Receiving an Extended `IPacket`**](#receiving-an-extended-ipacket-1)
      - [**`Ping`**](#ping-1)
      - [**Disconnect a Connection**](#disconnect-a-connection)
      - [**Stop the Server and Disposal**](#stop-the-server-and-disposal)
    - [**`ITcpNETServerAuth<T>`**](#itcpnetserverautht)
      - [**Parameters**](#parameters-2)
      - [**`IUserService<T>`**](#iuserservicet)
      - [**Events**](#events-2)
      - [**Start the Tcp Authentication Server**](#start-the-tcp-authentication-server)
      - [**SSL**](#ssl-2)
      - [**Send a Message to a Connection**](#send-a-message-to-a-connection-1)
      - [**Receiving an Extended `IPacket`**](#receiving-an-extended-ipacket-2)
      - [**`Ping`**](#ping-2)
      - [**Disconnect a Connection**](#disconnect-a-connection-1)
      - [**Stop the Server and Disposal**](#stop-the-server-and-disposal-1)
    - [**Additional Information**](#additional-information)
***

## **`IPacket`**

**`IPacket`** is an interface contained in [PHS.Networking](https://www.nuget.org/packages/PHS.Networking/) that represents an abstract payload to be sent across Tcp. **`IPacket`** also includes a default implementation, **`Packet`**, which contains the following fields:
* **`Data`** - *string* - Property representing the payload of the packet, this for example could be JSON or XML that can be deserialized back into an object by the server or other Tcp clients.
* **`Timestamp`** - *datetime* - Property containing the `UTC DateTime` when the **`Packet`** was created / sent to the server.

---

## **Client**

A Tcp Client module is included which can be used for non-SSL or SSL connections. To get started, first install the [NuGet package](https://www.nuget.org/packages/Tcp.NET/) using the [NuGet package manager](https://www.nuget.org/):

> install-package Tcp.NET.Client

This will add the most-recent version of the [Tcp.NET Client](https://www.nuget.org/packages/Tcp.NET.Client/) package to your specified project. 

### **`ITcpNETClient`**
Once installed, we can create an instance of **`ITcpNETClient`** with the included implementation **`TcpNETClient`**. 
* `TcpNETClient(IParamsTcpClient parameters, string oauthToken = "")`

    ``` c#
        ITcpNETClient client = new TcpNETClient(new ParamsTcpClient
        {
            Uri = "connect.tcp.net",
            Port = 8989,
            EndOfLineCharacters = "\r\n",
            IsSSL = false
        });
    ```

* There is an optional parameter on the constructor called **`oauthToken`** used by the [Tcp.NET Auth Server](https://www.nuget.org/packages/Tcp.NET.Server/) for authenticating a user.

    ``` c#
        ITcpNETClient client = new TcpNETClient(new ParamsTcpClient
        {
                Uri = "connect.tcp.net",
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                IsSSL = false
        }, "oauthToken");
    ```

    * Generating and persisting identity **`OAuth tokens`** is outside the scope of this document, but for more information, check out [IdentityServer4](https://github.com/IdentityServer/IdentityServer4) for a robust and easy-to-use .NET identity server.
    
#### **Parameters**
* **`IParamsTcpClient`** - **Required** - [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) includes a default implementation called **`ParamsTcpClient`** which contains the following connection detail data:
    * **`Port`** - *int* - **Required** - The port of the Tcp Server instance to connect (e.g. 6660, 7210, 6483).
    * **`EndOfLineCharacters`** - *string* - **Required** - Tcp does not automatically include line termination symbols, however it has become common practice in many applications that the end-of-line symbol is **`\r\n`** which represents an Enter key for many operating systems. We recommend you use **`\r\n`** as the line termination symbol. Defaults to **`\r\n`**.
    * **`IsSSL`** - *bool* - **Required** - Flag specifying if the connection should be made using SSL encryption for the connection to the server.
* **`OAuth Token`** - **Optional** - Optional parameter used by the [Tcp.NET Auth Server](https://www.nuget.org/packages/Tcp.NET.Server/) for authenticating a user.

#### **`OAuth Token`**
If you are using **`TcpNETClient`**, an optional parameter is included in the constructor for your `OAuth Token` - for more information, see **[`ITcpNETClient`](#itcpnetclient)**. Upon a successful connection to the server, the **`OAuth Token`** you specify will automatically be sent to the server properly formatted. However, if you are creating a manual Tcp connection to an instance of **`TcpNETServerAuth<T>`**, you must send as the first message to the server a raw message containing your **`OAuth Token`**. You must format this message with the prefix **`oauth:`** followed by your token. This could look similar to the following:

> oauth:yourOAuthTokenGoesHere

#### **Events**
3 events are exposed on the **`ITcpNETClient`** interface: `MessageEvent`, `ConnectionEvent`, and `ErrorEvent`. These event signatures are below:

``` c#
    client.MessageEvent += OMessageEvent;
    client.ConnectionEvent += OnConnectionEvent;
    client.ErrorEvent += OnErrorEvent
```

* `Task OnMessageEvent(object sender, TcpMessageClientEventArgs args);`
    * Invoked when a message is sent or received.
* `Task OnConnectionEvent(object sender, TcpConnectionClientEventArgs args);`
    * Invoked when the [Tcp.NET Client](https://www.nuget.org/packages/Tcp.NET.Client/) is connecting, connects, or disconnects from the server.
* `Task OnErrorEvent(object sender, TcpErrorClientEventArgs args);`
    * Wraps all internal logic with try catch statements and outputs the specific error(s).

#### **Connect to a Tcp Server**
To connect to a Tcp Server, invoke the function `ConnectAsync()`.

``` c#
    await client.ConnectAsync());
```
        
> Note: Connection parameters are provided in the constructors for **`ITcpNETClient`**.

#### **SSL**
To enable SSL for [Tcp.NET.Client](https://www.nuget.org/packages/Tcp.NET.Client/), set the **`IsSSL`** flag in **`IParamsTcpClient`** to true. In order to connect successfully, the server must have a valid, non-expired SSL certificate where the certificate's issued hostname must match the Uri specified in **`IParamsTcpClient`**. For example, the Uri in the above examples is [connect.tcp.net](#), and the SSL certificate on the server must be issued to [connect.tcp.net](#).

> ***Please note that a self-signed certificate or one from a non-trusted Certified Authority (CA) is not considered a valid SSL certificate.***

#### **Send a Message to the Server**
3 functions are exposed to send messages to the server:
* `SendToServerAsync<T>(T packet) where T : IPacket`
    * Send the designated packet to the server.
* `SendToServerAsync(string message)`
    * Transform the message into a **`Packet`** and send to the server.
* `SendToServerRawAsync(string message)`
	* Send the message directly to the server without transforming into a **`Packet`**. 

An example call to send a message to the server could be:

``` c#
    await client.SendToServerAsync(new Packet 
    {
        Data = JsonConvert.SerializeObject(new { header: "value" }),
        DateTime = DateTime.UtcNow
    });
```
> **More information about **`IPacket`** is available [here](#ipacket).**

#### **Extending `IPacket`**
**`IPacket`** can be extended with additional datatypes into a new struct / class and passed into the generic `SendToServerAsync<T>(T packet) where T : IPacket` function. Please note that **`Packet`** is a struct and cannot be inherited - please instead implement the interface **`IPacket`**.

``` c#
    enum PacketExtendedType
    {
        PacketType1,
        PacketType2
    }

    interface IPacketExtended : IPacket 
    {
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public class PacketExtended : IPacket 
    {
        string Data { get; set; }
        DateTime Timestamp { get; set; }
        PacketExtendedType PacketExtendedType {get; set; }
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    await SendToServerAsync(new PacketExtended 
    {
        Data = "YourDataPayload",
        DateTime = DateTime.UtcNow,
        FirstName = "FakeFirstName",
        LastName = "FakeLastName"
    });
```

#### **Receiving an Extended `IPacket`**
If you want to extend **`IPacket`** to include additional fields, you will need to extend and override the **`TcpNETClient`** implementation to support the extended type(s). First define a new class that inherits **`TcpNETClient`**, override the protected method `MessageReceivedAsync(string message)`, and deserialize into the extended **`IPacket`** of your choice. An example of this logic is below:

``` c#
    public class TcpNETClientExtended : TcpNETClient
    {
        public TcpNETClientExtended(IParamsTcpClient parameters, string oauthToken = "")
        {
        }

        protected override async Task MessageReceivedAsync(string message)
        {
            IPacket packet;

            try
            {
                packet = JsonConvert.DeserializeObject<PacketExtended>(message);

                if (string.IsNullOrWhiteSpace(packet.Data))
                {
                    packet = new PacketExtended
                    {
                        Data = message,
                        Timestamp = DateTime.UtcNow,
                        PacketExtendedType = PacketExtendedType.PacketType1,
                        FirstName = "FakeFirstName",
                        LastName = "FakeLastName",
                    };
                }
            }
            catch
            {
                packet = new PacketExtended
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow,
                    PacketExtendedType = PacketExtendedType.PacketType1,
                    FirstName = "FakeFirstName",
                    LastName = "FakeLastName",
                };
            }

            await FireEventAsync(this, new TcpMessageClientEventArgs
            {
                MessageEventType = MessageEventType.Receive,
                Message = packet.Data,
                Packet = packet,
                Connection = _connection
            });
        }
    }
    
    enum PacketExtendedType
    {
        PacketType1,
        PacketType2
    }

    interface IPacketExtended : IPacket 
    {
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public class PacketExtended : IPacket 
    {
        string Data { get; set; }
        DateTime Timestamp { get; set; }
        PacketExtendedType PacketExtendedType {get; set; }
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }
```

If you are sending polymorphic objects, first deserialize the initial message into a class or struct that contains “common” fields, such as `PacketExtended` with a `PacketExtendedType` enum field. Then use the value of `PacketExtendedType` and deserialize a second time into the type the enum represents. Repeat until the your polymorphic object is completely deserialized.

#### **`Ping`**
A **`TcpNETServer`** will send a raw message containing **`ping`** to every client every 120 seconds to verify which connections are still alive. If a client fails to respond with a raw message containing **`pong`**, during the the next ping cycle, the connection will be severed and disposed. However, if you are using **`TcpNETClient`**, the ping / pong messages are digested and handled before reaching `MessageReceivedAsync(string message)`. This means you do not need to worry about ping and pong messages if you are using **`TcpNETClient`**. However, if you are creating your own Tcp connection, you should incorporate logic to listen for raw messages containing **`ping`**, and if received, immediately respond with a raw message containing **`pong`**. 

> ***Note: Failure to implement this logic will result in a connection being severed in up to approximately 240 seconds.***

#### **Disconnect from the Server**
To disconnect from the server, invoke the function `DisconnectAsync()`.

``` c#
    await client.DisconnectAsync();
```

#### **Disposal**
At the end of usage, be sure to call `Dispose()` on the **`ITcpNETClient`** to free all allocated memory and resources.

``` c#
    await client.Dispose();
```

---
## **Server**
A Tcp Server module is included which can be used for non-SSL or SSL connections. To get started, create a new console application and install the [NuGet package](https://www.nuget.org/packages/Tcp.NET.Server/) using the [NuGet package manager](https://www.nuget.org/):
> install-package Tcp.NET.Server

This will add the most-recent version of the [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/) package to your specified project. 

Once installed, we can create 2 different classes of Tcp Servers. 

* **[`ITcpNETServer`](#itcpnetserver)**
* **[`ITcpNETServerAuth<T>`](#itcpnetserverauth<T>)**
***
### **`ITcpNETServer`**
We will now create an instance of **`ITcpNETServer`** with the included implementation **`TcpNETServer`**. The included implementation includes the following constructors (for SSL or non-SSL servers):

* `TcpNETServer(IParamsTcpServer parameters, TcpHandler handler = null, TcpConnectionManager connectionManager = null)`

    ``` c#
        ITcpNETServer server = new TcpNETServer(new ParamsTcpServer 
        {
            Port = 8989,
            EndOfLineCharacters = "\r\n",
            ConnectionSuccessString = "Connected Successfully",
        });
    ```

* `TcpNETServer(IParamsTcpServer parameters, byte[] certificate, string certificatePassword, TcpHandler handler = null, TcpConnectionManager connectionManager = null)`
  
    ``` c#
        byte[] certificate = File.ReadAllBytes("cert.pfx");
        string certificatePassword = "yourCertificatePassword";

        ITcpNETServer server = new TcpNETServer(new ParamsTcpServer 
        {
            Port = 8989,
            EndOfLineCharacters = "\r\n",
            ConnectionSuccessString = "Connected Successfully",
        }, certificate, certificatePassword);
    ```

The [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/) does not specify a listening uri / host. Instead, the server is configured to automatically listen on all available interfaces (including 127.0.0.1, localhost, and the server's exposed IPs).

#### **Parameters**
* **`IParamsTcpServer`** - **Required** - [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) includes a default implementation called **`ParamsTcpServer`** which contains the following connection detail data:
    * **`Port`** - *int* - **Required** - The port where the Tcp Server will listen (e.g. 6660, 7210, 6483).
    * **`EndOfLineCharacters`** - *string* - **Required** - Tcp does not automatically include line termination ends, however in many applications the end-of-line symbol is **`\r\n`** which represents an Enter key for many operating systems. We recommend you use **`\r\n`** as the line termination symbol. Defaults to **`\r\n`**.
    * **`ConnectionSuccessString`** - *string* - **Required** - The string that will be sent to a newly successful connected client.
* **`Certificate`** - *byte[]* - **Optional** - A byte array containing the exported SSL certificate with private key if the server will be hosted on Https.
* **`CertificatePassword`** - *string* - **Optional** - The private key of the exported SSL certificate if the server will be hosted on Https.
* **`TcpHandler`** - **Optional** - If you want to deserialize an extended **`IPacket`**, you can extend **`TcpHandler`** and override `MessageReceivedAsync(string message, IConnectionTcpServer connection)` to deserialize the object into the class / struct of your choice. For more information, please see **[Receiving an Extended IPacket](#receiving-an-extended-ipacket)** below.
* **`TcpConnectionManager`** - **Optional** - If you want to customize the connection manager, you can extend and use your own ConnectionManager inherited class here.

#### **Events**
4 events are exposed on the **`ITcpNETServer`** interface: `MessageEvent`, `ConnectionEvent`, `ErrorEvent`, and `ServerEvent`. These event signatures are below:

``` c#
    server.MessageEvent += OMessageEvent;
    server.ConnectionEvent += OnConnectionEvent;
    server.ErrorEvent += OnErrorEvent;
    server.ServerEvent += OnServerEvent;
```

* `Task OnMessageEvent(object sender, TcpMessageServerEventArgs args);`
    * Invoked when a message is sent or received.
* `Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args);`
    * Invoked when a Tcp client is connecting, connects, or disconnects from the server.
* `Task OnErrorEvent(object sender, TcpErrorServerEventArgs args);`
    * Wraps all internal logic with try catch statements and outputs the specific error(s).
* `Task OnServerEvent(object sender, ServerEventArgs args);`
    * Invoked when the Tcp server starts or stops.

#### **Starting the Tcp Server**
To start the `ITcpNETServer`, call the `StartAsync()` method to instruct the server to begin listening for messages. Likewise, you can stop the server by calling the `StopAsync()` method.

``` c#
    await server.StartAsync();
    ...
    await server.StopAsync();
    server.Dispose();
```

#### **SSL**
To enable SSL for [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/), use one of the provided SSL server constructors and manually specify your exported SSL certificate with private key as a byte[] and your certificate's private key as parameters. 

> **The SSL Certificate MUST match the domain where the Tcp Server is hosted / can be accessed or clients will not able to connect to the Tcp Server.** 
 
In order to allow successful SSL connections, you must have a valid, non-expired SSL certificate. There are many sources for SSL certificates and some of them are open-source - we recommend [Let's Encrypt](https://letsencrypt.org/).

> ***Note: A self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate.***

#### **Send a Message to a Connection**
3 functions are exposed to send messages to connections: 
* `SendToConnectionAsync<T>(T packet, IConnectionTcpServer connection) where T : IPacket`
    * Send the designated **`IPacket`** to the specified connection.
* `SendToConnectionAsync(string message, IConnectionTcpServer connection)`
    * Transform the message into a **`Packet`** and send to the specified connection.
* `SendToConnectionRawAsync(string message, IConnectionTcpServer connection)`
    * Send the message to the specified connection directly without transforming it into a **`Packet`**.

> **More information about **`IPacket`** is available [here](#ipacket).**

**`IConnectionTcpServer`** represents a connected client to the server. These are exposed in `ConnectionEvent` or can be retrieved from the **`Connections`** inside of **`ITcpNETServer`**.

An example call to send a message to a connection could be:

``` c#
    IConnectionTcpServer[] connections = server.Connections;

    await server.SendToConnectionAsync(new Packet 
    {
        Data = "YourDataPayload",
        DateTime = DateTime.UtcNow
    }, connections[0]);
```

#### **Receiving an Extended `IPacket`**
If you want to extend **`IPacket`** to include additional fields, you will need to add the optional parameter **`TcpHandler`** that can be included with each constructor. The default **`TcpHandler`** has logic which is specific to deserialize messages of type **`Packet`**, but to receive your own extended **`IPacket`**, we will need to inherit / extend **`TcpHandler`** with our your class. Once **`TcpHandler`** has been extended, override the protected method `MessageReceivedAsync(string message, IConnectionTcpServer connection)` and deserialize into an extended **`IPacket`** of your choice. An example of this logic is below:

``` c#
    public class TcpHandlerExtended : TcpHandler
    {
        public TcpHandlerExtended(IParamsTcpServer parameters) : base(parameters)
        {
        }

        public TcpHandlerExtended(IParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override async Task MessageReceivedAsync(string message, IConnectionTcpServer connection)
        {
            IPacket packet;

            try
            {
                packet = JsonConvert.DeserializeObject<PacketExtended>(message);

                if (string.IsNullOrWhiteSpace(packet.Data))
                {
                    packet = new PacketExtended
                    {
                        Data = message,
                        Timestamp = DateTime.UtcNow,
                        PacketExtendedType = PacketExtendedType.PacketType1,
                        FirstName = "FakeFirstName",
                        LastName = "FakeLastName",
                    };
                }
            }
            catch
            {
                packet = new PacketExtended
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow,
                    PacketExtendedType = PacketExtendedType.PacketType1,
                    FirstName = "FakeFirstName",
                        LastName = "FakeLastName",
                };
            }

            await FireEventAsync(this, new TcpMessageServerEventArgs
            {
                MessageEventType = MessageEventType.Receive,
                Message = packet.Data,
                Packet = packet,
                Connection = connection
            });
        }
    }

    enum PacketExtendedType
    {
        PacketType1,
        PacketType2
    }

    interface IPacketExtended : IPacket 
    {
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public class PacketExtended : IPacket 
    {
        string Data { get; set; }
        DateTime Timestamp { get; set; }
        PacketExtendedType PacketExtendedType {get; set; }
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }
```

If you are sending polymorphic objects, first deserialize the initial message into a class or struct that contains “common” fields, such as `PacketExtended` with a `PacketExtendedType` enum field. Then use the value of `PacketExtendedType` and deserialize a second time into the type the enum represents. Repeat until the your polymorphic object is completely deserialized.

Finally, when constructing your **`TcpNETServer`**, pass in your new **`TcpHandlerExtended`** extended class you created. An example is below:

``` c#
    IParamsTcpServer parameters = new ParamsTcpServer 
    {
        Port = 8989,
        EndOfLineCharacters = "\r\n",
        ConnectionSuccessString = "Connected Successfully",
    };

    ITcpNETServer server = new TcpNETServer(parameters, handler: new TcpHandlerExtended(parameters));
```

#### **`Ping`**
A raw message containing **`ping`** is sent automatically every 120 seconds to each client connected to a **`TcpNETServer`**. Each client is expected to immediately return a raw message containing **`pong`**. If a raw message containing **`pong`** is not received by the server before the next ping interval, the connection will be severed, disconnected, and removed from the **`TcpNETServer`**. This interval time is hard-coded to 120 seconds.

#### **Disconnect a Connection**
To disconnect a connection from the server, invoke the function `DisconnectConnectionAsync(IConnectionTcpServer connection)`. 

``` c#
    await DisconnectConnectionAsync(connection);
```

**`IConnectionTcpServer`** represents a connected client to the server. These are exposed in `ConnectionEvent` or can be retrieved from **`Connections`** inside of **`ITcpNETServer`**.

#### **Stop the Server and Disposal**
To stop the server, call the `StartAsync()` method. If you are not going to start the server again, call the `Dispose()` method to free all allocated memory and resources.

``` c#
    await server.StopAsync();
    server.Dispose();
```

---
### **`ITcpNETServerAuth<T>`**
The second Tcp Server includes authentication for identifying your connections / users. We will create an instance of **`ITcpNETServerAuth<T>`** with the included implementation **`TcpNETServerAuth<T>`**. This object includes a generic, T, which represents the datatype of your user unique Id. For example, T could be an int, a string, a long, or a guid - this depends on the datatype of the unique Id you have set for your user. This generic allows the **`ITcpNETServerAuth<T>`** implementation to allow authentication and identification of users within many different user systems. The included implementation includes the following constructors (for SSL or non-SSL servers):

* `TcpNETServerAuth<T>(IParamsTcpServerAuth parameters, IUserService<T> userService, TcpHandler handler = null, TcpConnectionManagerAuth<T> connectionManager = null)`

``` c#
    public class MockUserService : IUserService<long> 
    { }

    ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(new ParamsTcpServerAuth 
    {
        Port = 8989,
        EndOfLineCharacters = "\r\n",
        ConnectionSuccessString = "Connected Successfully",
        ConnectionUnauthorizedString = "Connection not authorized",
    }, new MockUserService());
```

* `TcpNETServerAuth<T>(IParamsTcpServerAuth parameters, IUserService<T> userService, byte[] certificate, string certificatePassword, TcpHandler handler = null, TcpConnectionManagerAuth<T> connectionManager = null)`

``` c#
    public class MockUserService : IUserService<long> 
    { }

    byte[] certificate = File.ReadAllBytes("yourCert.pfx");
    string certificatePassword = "yourCertificatePassword";

    ITcpNETServerAuth<long> server = new ITcpNETServerAuth<long>(new ParamsTcpServerAuth 
    {
        Port = 8989,
        EndOfLineCharacters = "\r\n",
        ConnectionSuccessString = "Connected Successfully",
        ConnectionUnauthorizedString = "Connection not authorized",
    }, new MockUserService(), certificate, certificatePassword);
```

The [Tcp.NET Authentication Server](https://www.nuget.org/packages/Tcp.NET.Server/) does not specify a listening uri / host. Instead, the server is configured to automatically listen on all available interfaces (including 127.0.0.1, localhost, and the server's IP).

#### **Parameters**
* **`IParamsTcpServerAuth`** - **Required** - [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) includes a default implementation called **`ParamsTcpServerAuth`** which contains the following connection detail data:
  * **`Port`** - *int* - **Required** - The port where the Tcp Server will listen (e.g. 6660, 7210, 6483).
  * **`EndOfLineCharacters`** - *string* - **Required** - Tcp does not automatically include line termination symbols, however many applications set the end-of-line symbol to **`\r\n`** which represents an Enter key for many operating systems. We recommend you use **`\r\n`** as the line termination symbol. Defaults to **`\r\n`**.
  * **`ConnectionSuccessString`** - *string* - **Required** - The string that will be sent to a newly successful connected client.
  * **`ConnectionUnauthorizedString`** - *string* - **Required** - The string that will be sent to a client if they fail authentication.
* **`IUserService<T>`** - **Required** - This interface for a User Service class will need to be implemented. This interface specifies 1 function, `GetIdAsync(string token)`, which will be invoked when the server receives an **`OAuth Token`** from a new connection. For more information regarding the User Service class, please see **[`IUserService<T>`](#userservice<T>)** below.
* **`Certificate`** - *byte[]* - **Optional** - A byte array containing the exported SSL certificate with private key if the server will be hosted on Https.
* **`CertificatePassword`** - *string* - **Optional** - The private key of the exported SSL certificate if the server will be hosted on Https.
* **`TcpHandler`** - **Optional**. This object is optional. If you want to deserialize an extended **`IPacket`**, you could extend **`TcpHandler`** and override `MessageReceivedAsync(string message, IConnectionTcpServer connection)` to deserialize the object into the class / struct of your choice. For more information, please see **[Receiving an Extended IPacket](#receiving-an-extended-ipacket)** below.
* **`TcpConnectionManagerAuth<T>`** - **Optional** - If you want to customize the connection manager, you can extend and use your own connection manager auth instance here.

#### **`IUserService<T>`**
This is an interface contained in [PHS.Networking.Server](https://www.nuget.org/packages/PHS.Networking.Server/). When creating a **`TcpNETServerAuth<T>`**, the interface **`IUserService<T>`** will need to be implemented into a concrete class.

> **A default implementation is *not* included with [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net). You will need to implement this interface and add logic here.**

An example implementation using [Entity Framework](https://docs.microsoft.com/en-us/ef/) is shown below:

``` c#
    public class UserServiceTcp : IUserService<long>
    {
        protected readonly ApplicationDbContext _ctx;

        public UserServiceTcp(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public virtual async Task<long> GetIdAsync(string token)
        {
            // Obfuscate the token in the database
            token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var user = await _ctx.Users.FirstOrDefaultAsync(s => s.OAuthToken == token);
            return user != null ? user.Id : (default);
        }

        public void Dispose()
        {
        }
    }
```

Because you are responsible for creating the logic in `GetIdAsync(string oauthToken)`, the data could reside in many stores including (but not limited to) in memory, a database, or an identity server. In our implementation, we are checking the **`OAuth Token`** using [Entity Framework](https://docs.microsoft.com/en-us/ef/) and validating it against a quick User table in [SQL Server](https://hub.docker.com/_/microsoft-mssql-server). If the **`OAuth Token`** is found, then the appropriate UserId will be returned as type T, and if not, the default of type T will be returned (e.g. 0, "", Guid.Empty).

#### **Events**
4 events are exposed on the **`ITcpNETServerAuth<T>`** interface: `MessageEvent`, `ConnectionEvent`, `ErrorEvent`, and `ServerEvent`. These event signatures are below:

``` c#
    server.MessageEvent += OMessageEvent;
    server.ConnectionEvent += OnConnectionEvent;
    server.ErrorEvent += OnErrorEvent;
    server.ServerEvent += OnServerEvent;
```

* `Task OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<T> args);`
    * Invoked when a message is sent or received.
* `Task OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<T> args);`
    * Invoked when a Tcp client is connecting, connects, or disconnects from the server.
* `Task OnErrorEvent(object sender, TcpErrorServerAuthEventArgs<T> args);`
    * Wraps all internal logic with try catch statements and outputs the specific error(s).
* `Task OnServerEvent(object sender, ServerEventArgs args);`
    * Invoked when the Tcp server starts or stops.

#### **Start the Tcp Authentication Server**
To start the [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/), call the `StartAsync()` method to instruct the server to begin listening for messages. Likewise, you can stop the server by calling the `StopAsync()` method.

``` c#
    await server.StartAsync();
    ...
    await server.StopAsync();
    server.Dispose();
```

#### **SSL**
To enable SSL for [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/), use one of the two provided SSL server constructors and manually specify your exported SSL certificate with private key as a byte[] and your certificate's private key as parameters. 

> **The SSL Certificate MUST match the domain where the Websocket Server is hosted / can be accessed or clients will not able to connect to the Websocket Server.** 
 
In order to allow successful SSL connections, you must have a valid, non-expired SSL certificate. There are many sources for SSL certificates and some of them are open-source - we recommend [Let's Encrypt](https://letsencrypt.org/).

> ***Note: A self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate.***

#### **Send a Message to a Connection**
To send messages to connections, 11 functions are exposed:
* `BroadcastToAllAuthorizedUsersAsync<S>(S packet) where S : IPacket`
    * Send the designated packet to all Users and their connections currently logged into the server.
* `BroadcastToAllAuthorizedUsersAsync(string message)`
    * Transform the message into a **`Packet`** and send to all Users and their connections currently logged into the server.
* `BroadcastToAllAuthorizedUsersAsync(S packet, IConnectionTcpServer connectionSending) where S : IPacket`
    * Send the designated packet to all Users and their connections currently logged into the server except for the connection matching connectionSending.
* `BroadcastToAllAuthorizedUsersAsync(string message, IConnectionTcpServer connectionSending)`
    * Transform the message into a **`Packet`** and send to all Users and their connections currently logged into the server except for the connection matching the connectionSending.
* `BroadcastToAllAuthorizedUsersRawAsync(string message)`
    * Send the message directly to all Users and their connections currently logged into the server without transforming the message into a **`Packet`**.
* `SendToUserAsync<S>(S packet, T userId) where S : IPacket`
    * Send the designated packet to the specified User and their connections currently logged into the server.
* `SendToUserAsync(string message, T userId)`
    * Transform the message into a **`Packet`** and send the to the specified User and their connections currently logged into the server.
* `SendToUserRawAsync(string message, T userId)`
    * Send the message directly to the designated User and their connections without transforming the message into a **`Packet`**.
* `SendToConnectionAsync<S>(S packet, IConnectionTcpServer connection) where S : IPacket`
    * Send the designated packet to the designated User's connection currently logged into the server.
* `SendToConnectionAsync(string message, IConnectionTcpServer connection)`
    * Transform the message into a **`Packet`** and send to the designated User's connection currently logged into the server.
* `SendToConnectionRawAsync(string message, IConnectionTcpServer connection)`
    * Send the message directly to the designated User's connection currently logged into the server without transforming the message into a **`Packet`**.

> **More information about **`IPacket`** is available [here](#ipacket).**

**`IConnectionTcpServer`** represents a connected client to the server. These are exposed in the `ConnectionEvent` or can be retrieved from **`Connections`** or **`Identities`** inside of **`ITcpNETServerAuth<T>`**.

An example call to send a message to a connection could be:

``` c#
    IIdentityTcp<Guid>[] identities = server.Identities;

    await server.SendToConnectionAsync(new Packet 
    {
        Data = "YourDataPayload",
        DateTime = DateTime.UtcNow
    }, identities[0].Connections[0]);
```

#### **Receiving an Extended `IPacket`**
If you want to extend **`IPacket`** to include additional fields, you will need to add the optional parameter **`TcpHandler`** that can be included with each constructor. The included **`TcpHandler`** has logic which is specific to deserialize messages of type **`Packet`**, but to receive your own extended **`IPacket`**, we will need to inherit / extend **`TcpHandler`**. Once **`TcpHandler`** has been extended, override the protected method `MessageReceivedAsync(string message, IConnectionTcpServer connection)` and deserialize into the extended **`IPacket`** of your choice. An example of this logic is below:

``` c#
    public class TcpHandlerExtended : TcpHandler
    {
        public TcpHandlerExtended(IParamsTcpServerAuth parameters) : base(parameters)
        {
        }
       
        public TcpHandlerExtended(IParamsTcpServerAuth parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }


        protected override async Task MessageReceivedAsync(string message, IConnectionTcpServer connection)
        {
            IPacket packet;

            try
            {
                packet = JsonConvert.DeserializeObject<PacketExtended>(message);

                if (string.IsNullOrWhiteSpace(packet.Data))
                {
                    packet = new PacketExtended
                    {
                        Data = message,
                        Timestamp = DateTime.UtcNow,
                        PacketExtendedType = PacketExtendedType.PacketType1,
                        FirstName = "FakeFirstName",
                        LastName = "FakeLastName"
                    };
                }
            }
            catch
            {
                packet = new PacketExtended
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow,
                    PacketExtendedType = PacketExtendedType.PacketType1,
                    FirstName = "FakeFirstName",
                    LastName = "FakeLastName"
                };
            }

            await FireEventAsync(this, new TcpMessageServerEventArgs
            {
                MessageEventType = MessageEventType.Receive,
                Message = packet.Data,
                Packet = packet,
                Connection = connection
            });
        }
    }
    
    enum PacketExtendedType
    {
        PacketType1,
        PacketType2
    }

    interface IPacketExtended : IPacket 
    {
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public class PacketExtended : IPacket 
    {
        string Data { get; set; }
        DateTime Timestamp { get; set; }
        PacketExtendedType PacketExtendedType {get; set; }
        string Username { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }
```

If you are sending polymorphic objects, first deserialize the initial message into a class or struct that contains “common” fields, such as `PacketExtended` with a `PacketExtendedType` enum field. Then use the value of `PacketExtendedType` and deserialize a second time into the type the enum represents. Repeat until the your polymorphic object is completely deserialized.

Finally, when constructing **`TcpNETServerAuth<T>`**, pass in your new **`TcpHandlerExtended`** class you created. An example is as follows:

``` c#
    IParamsTcpServerAuth parameters = new ParamsTcpServerAuth 
    {
        Port = 8989,
        EndOfLineCharacters = "\r\n",
        ConnectionSuccessString = "Connected Successfully",
        ConnectionUnauthorizedString = "Connection Not Authorized"
    };

    ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(parameters, new MockUserService(), cert, handler: new TcpHandlerExtended(parameters));
```

#### **`Ping`**
A raw message containing **`ping`** is sent automatically every 120 seconds to each client connected to a **`TcpNETServerAuth<T>`**. Each client is expected to return a raw message containing a **`pong`**. If a **`pong`** is not received before the next ping interval, the connection will be severed, disconnected, and removed from the **`TcpNETServerAuth<T>`**. This interval time is hard-coded to 120 seconds. If you are using the provided **`TcpNETClient`**, Ping / Pong logic is already handled for you.

#### **Disconnect a Connection**
To disconnect a connection from the server, invoke the function `DisconnectConnection(IConnectionTcpServer connection)`. 

``` c#
    await DisconnectConnectionAsync(connection);
```

**`IConnectionServer`** represents a connected client to the server. These are exposed in the `ConnectionEvent` or can be retrieved from **`Connections`** or **`Identities`** inside of **`ITcpNETServerAuth<T>`**. If a logged-in user disconnects from all connections, that user is automatically removed from **`Identities`**.

#### **Stop the Server and Disposal**
To stop the server, call the `StopAsync()` method. If you are not going to start the server again, call the `Dispose()` method to free all allocated memory and resources.

``` c#
    await server.StopAsync();
    server.Dispose();
```

---
### **Additional Information**
[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) was created by [LiveOrDevTrying](https://www.liveordevtrying.com) and is maintained by [Pixel Horror Studios](https://www.pixelhorrorstudios.com). [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) is currently implemented in (but not limited to) the following projects: [Allie.Chat](https://allie.chat), [NTier.NET](https://github.com/LiveOrDevTrying/NTier.NET), [The Monitaur](https://www.themonitaur.com) and [Gem Wars](https://www.gemwarsgame.com) *(currently in development)*.  
![Pixel Horror Studios Logo](https://pixelhorrorstudios.s3-us-west-2.amazonaws.com/Packages/PHS.png)
