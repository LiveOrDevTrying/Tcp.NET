# **[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net)**
[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) provides an easy-to-use and customizable Tcp Server and Tcp Client. The server and client can be used for non-SSL or SSL connections and authentication is provided for identifying the clients connected to your server. Both client and server are created in .NET Core 2.1 and use async await functionality. If you are not familiar with async await functionality, you can learn more by reviewing the information found at the [Microsoft Async Programming Concepts page](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/). All [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) packages referenced in this documentation are available on the NuGet package manager at in 1 aggregate package - [Tcp.NET](https://www.nuget.org/packages/Tcp.NET/).

![Image of Tcp.NET Logo](https://pixelhorrorstudios.s3-us-west-2.amazonaws.com/Packages/Tcp.NETLogo.png)

* [IPacket](#ipacket)
* [Client](#client)
    * [ITcpNETClient](#itcpnetclient)
* [Server](#server)
    * [ITcpNETServer](#itcpnetserver)
    * [ITcpNETServerAuth](#itcpnetserverauth)
* [Additional Information](#additional-information)

***

## **IPacket**

**IPacket** is an interface contained in [PHS.Networking](https://www.nuget.org/packages/PHS.Networking/) that represents an abstract payload to be sent across Tcp. **IPacket** also includes a default implementation struct, **Packet**, which contains the following information:
* **Data** - *string* - Property representing the payload of the packet, many times this could be JSON or XML that can be deserialized back into an object by the server or other Tcp clients.
* **Timestamp** - *datetime* - Property containing the UTC DateTime when the **Packet** was created / sent to the server.

***

## **Client**

A Tcp Client module is included which can be used for non-SSL or SSL connections. To get started, first install the NuGet package using the NuGet package manager:
> install-package Tcp.NET.Client

This will add the most-recent version of the [Tcp.NET Client](https://www.nuget.org/packages/Tcp.NET.Client/) module to your specified project. 
***
### ITcpNETClient
Once installed, we can create an instance of **ITcpNETClient** with the included implementation **TcpNETClient**. 
* `TcpNETClient(IParamsTcpClient parameters, string oauthToken = "")`
    * An example instantiation is below:

            ITcpNETClient client = new TcpNETClient(new ParamsTcpClient
            {
                Uri = "connect.tcp.net",
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                IsSSL = false
            });

* There is an optional parameter on the constructor called **oauthToken** used by the [Tcp.NET Auth Server](https://www.nuget.org/packages/Tcp.NET.Server/) for authenticating a user.
    * An example instantiation is below:

            ITcpNETClient client = new TcpNETClient(new ParamsTcpClient
            {
                 Uri = "connect.tcp.net",
                 Port = 8989,
                 EndOfLineCharacters = "\r\n",
                 IsSSL = false
            }, "oauthToken");
    * Generating and persisting identity **OAuth tokens** is outside the scope of this tutorial, but for more information, check out [IdentityServer4](https://github.com/IdentityServer/IdentityServer4) for a robust and easy-to-use .NET identity server.
    
#### Parameters
* **IParamsTcpClient** - *Required* [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) includes a default struct implementation called **ParamsTcpClient** which contains the following connection detail data:

    * **Uri** - *string* - The endpoint / host / url of the Tcp Server instance to connect (e.g. localhost, 192.168.1.14, connect.tcp.net).
    * **Port** - *int* - The port of the Tcp Server instance to connect (e.g. 6660, 7210, 6483).
    * **EndOfLineCharacters** - *string* - Tcp does not automatically include line termination symbols, however it has become common practice in many applications that the end-of-line symbol is **"\r\n"** which represents an Enter key for many operating systems. We recommend you use **"\r\n"** as the line termination symbol.
    * **IsSSL** - *bool* - Flag specifying if the connection should be made using SSL encryption for the connection to the server.

#### Events
3 events are exposed on the **ITcpNETClient** interface: `MessageEvent`, `ConnectionEvent`, and `ErrorEvent`. These event signatures are below:

        client.MessageEvent += OMessageEvent;
        client.ConnectionEvent += OnConnectionEvent;
        client.ErrorEvent += OnErrorEvent

* `Task OnMessageEvent(object sender, TcpMessageClientEventArgs args);`
    * Invoked when a message is sent or received
* `Task OnConnectionEvent(object sender, TcpConnectionClientEventArgs args);`
    * Invoked when the [Tcp.NET Client](https://www.nuget.org/packages/Tcp.NET.Client/) is connecting, connects, or disconnects from the server
* `Task OnErrorEvent(object sender, TcpErrorClientEventArgs args);`
    * Wraps all internal logic with try catch statements and outputs the specific error(s)

#### Connect to a Tcp Server
To connect to a Tcp Server, invoke the function `ConnectAsync()`.

        await client.ConnectAsync());
        
*Note: Connection parameters were input with the constructors of **ITcpClient**.*

#### SSL
To enable SSL for [Tcp.NET.Client](https://www.nuget.org/packages/Tcp.NET.Client/), set the **IsSSL** flag in **IParamsTcpClient** to true. In order to connect successfully, the server must have a valid, non-expired SSL certificate where the certificate's issued hostname must match the Uri specified in **IParamsTcpClient**. For example, the Uri in the above examples is "connect.tcp.net", and the SSL certificate on the server must be issued to "connect.tcp.net".

*Please note that a self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate.*

#### Send a Message to the Server
3 functions are exposed to send messages to the server:
* `SendToServerAsync<T>(T packet) where T : IPacket`
    * Send the designated packet to the server 
* `SendToServerAsync(string message)`
    * Transform the message into a **Packet** and send to the server 
* `SendToServerRawAsync(string message)`
	* Send the message directly to the server without transforming into a **Packet**. 

An example call to send a message to the server could be:

        await client.SendToServerAsync<IPacket>(new Packet 
        {
            Data = "YourDataPayload",
            DateTime = 2020-02-18 11:54:32.4324
        });
        
More information about **IPacket** is available [here](#ipacket).

#### Extending IPacket
**IPacket** can be extended with additional datatypes into a new struct / class and passed into the generic `SendToServerAsync<T>(T packet) where T : IPacket` function. Please note that **Packet** is a struct and cannot be inherited - please instead implement the interface **IPacket**.

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

        ---

        await SendToServerAsync<IPacketExtended>(new PacketExtended 
        {
            Data = "YourDataPayload",
            DateTime = 2020-02-12 01:29:23.963,
            FirstName = "FakeFirstName",
            LastName = "FakeLastName"
        });

To handle receipt of an extended packet (such as IPacketExtended we created above), see the section below [Receiving an Extended IPacket](#receiving-an-extended-ipacket).

#### Ping
A **ITcpNETServer** will send a raw message containing **'ping'** to every client every 120 seconds to verify which connections are still alive. If a client fails to respond with a raw message containing **'pong'**, during the the next ping cycle, the connection will be severed and disposed. However, if you are using **ITcpNETClient**, the ping / pong messages are digested and handled before reaching `MessageEvent(object sender, TcpMessageServerEventArgs args)`. This means you do not need to worry about ping and pong messages if you are using **ITcpNETClient**. However, if you are creating your own Tcp connection, you should incorporate logic to listen for raw messages containing **'ping'**, and if received, immediately respond with a raw message containing **'pong'** message. 

*Note: Failure to implement this logic will result in a connection being severed in up to approximately 240 seconds.*

#### Disconnect from the Server
To disconnect from the server, invoke the function `Disconnect()`.

#### Disposal
At the end of usage, be sure to call `Dispose()` on the Tcp.NET Client to free all allocated memory and resources.
***
## **Server**
A Tcp Server module is included which can be used for non-SSL or SSL connections. To get started, first install the NuGet package using the NuGet package manager:
> install-package Tcp.NET.Server

This will add the most-recent version of the [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/) module to your specified project. 

Once installed, we can create 2 different classes of Tcp Servers. 

* **[`ITcpNETServer`](#itcpnetserver)**
* **[`ITcpNETServerAuth<T>`](#itcpnetserverauth<T>)**
***
### ITcpNETServer
We will now create an instance of **ITcpNETServer** with the included implementation **TcpNETServer**. The included implementation includes 3 constructors (for SSL or non-SSL servers):

* `TcpNETServer(IParamsTcpServer parameters, TcpHandler handler = null)`
    * The constructor for non-SSL Tcp Servers. Example instantiation is below:
 
            ITcpNETServer server = new TcpNETServer(new ParamsTcpServer 
            {
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                ConnectionSuccessString = "Connected Successfully",
            });

* `TcpNETServer(IParamsTcpServer parameters, X509Certificate certificate, TcpHandler handler = null)`
    * The constructor for a SSL Tcp Server where the SSL certificate is manually specified. Example instantiation is below:

            // Get a path to the SSL certificate
            var filename = Path.Combine(Environment.WebRootPath, "cert.pfx");
        
            // Instantiate the cert and provide the password
            var cert = new X509Certificate2(filename, "myCertPassword");
        
            // Start the server
            ITcpNETServer server = new TcpNETServer(new ParamsTcpServer 
            {
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                ConnectionSuccessString = "Connected Successfully",
            }, cert);

* `TcpNETServer(IParamsTcpServer parameters, string certificateIssuedTo, StoreLocation storeLocation, TcpHandler handler = null))`
    * The constructor for a SSL Tcp Server where the SSL certificate is registered to and obtained from the Windows Certificate Store. Example instantiation is below:

            ITcpNETServer server = new TcpNETServer(new ParamsTcpServer 
            {
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                ConnectionSuccessString = "Connected Successfully",
            }, "connect.tcp.net", StoreLocation.LocalMachine);

The [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/) does not specify a listening uri / host. Instead, the serverr is configured to automatically listen on all available interfaces (including 127.0.0.1, localhost, and the server's exposed IPs).

#### Parameters
* **IParamsTcpServer** - *Required* - [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) includes a default struct implementation called **ParamsTcpServer** which contains the following connection detail data:
    * **Port** - *int* - The port where the Tcp Server will listen (e.g. 6660, 7210, 6483).
    * **EndOfLineCharacters** - *string* - Tcp does not automatically include line termination symbols, however it has become common practice in many applications that the end-of-line symbol is **"\r\n"** which represents an Enter key for many operating systems. We recommend you use **"\r\n"** as the line termination symbol.
    * **ConnectionSuccessString** - *string* - The string that will be sent to a newly connected client.
* **TcpHandler** - *Optional* - If you want to deserialize an extended **IPacket** from a client, you can extend **TcpHandler** in a new class and override `MessageReceived(string message, IConnectionServer connection)` to deserialize the object into the class / struct of your choice. For more information, please see **[Receiving an Extended Packet](#receiving-an-extended-packet)** below.
* **X509Certificate** - *Optional* - This is an object that contains the valid SSL certificate to bind to the server. See above for one implementation.
* **CertificateIssuedTo** - *Optional - string*. The certificate's issued hostname - for example, the Uri in the above examples is "connect.tcp.net", and the SSL certificate must be issued to "connect.tcp.net".
* **StoreLocation** - *Optional - enum* - Location the certificate is in the Windows Certificate Store. Potential options are:
    * **StoreLocation.CurrentUser** - *0* - The certificate is registered to the current user
    * **StoreLocation.LocalMachine** - *1* - The certificate is registered to the local system

#### Events
4 events are exposed on the **ITcpNETServer** interface: `MessageEvent`, `ConnectionEvent`, `ErrorEvent`, and `ServerEvent`. These event signatures are below:

        server.MessageEvent += OMessageEvent;
        server.ConnectionEvent += OnConnectionEvent;
        server.ErrorEvent += OnErrorEvent;
        server.ServerEvent += OnServerEvent;

* `Task OnMessageEvent(object sender, TcpMessageServerEventArgs args);`
    * Invoked when a message is sent or received
* `Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args);`
    * Invoked when a Tcp client is connecting, connects, or disconnects from the server
* `Task OnErrorEvent(object sender, TcpErrorServerEventArgs args);`
    * Wraps all internal logic with try catch statements and outputs the specific error(s)
* `Task OnServerEvent(object sender, ServerEventArgs args);`
    * Invoked when the Tcp server starts or stops

#### Starting the Tcp Server
There is no action to start the [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/) - once instantiated, the server will listen on the specified port until disposed.

#### SSL
To enable SSL for [Tcp.NET Server](https://www.nuget.org/packages/Tcp.NET.Server/), use one of the two provided SSL server constructors and manually specify the SSL certificate or direct the parameters to your Windows Certificate Store. In order to allow successful SSL connections, you must have a valid, non-expired SSL certificate. There are many sources for SSL certificates and some of them are opensource community driven - we recommend [Let's Encrypt](https://letsencrypt.org/).

*Note: A self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate.*

#### Send a Message to a Client
3 functions are exposed to send messages to clients: 
* `SendToConnectionAsync<T>(T packet, IConnectionServer connection) where T : IPacket`
    * Send the designated **IPacket** to the specified connection
* `SendToConnectionAsync(string message, IConnectionServer connection)`
    * Transform the message into a **Packet** and send to the specified connection
* `SendToConnectionRawAsync(string message, IConnectionServer connection)`
    * Send the message to the specified connection directly without transforming it into a **Packet**

More information about **IPacket** is available [here](#ipacket).

**IConnectionServer** is a connncted client to the server. These are exposed in the `ConnectionEvent` or can be retrieved from the **Connections** inside of **ITcpNETServer**.

An example call to send a message to a client could be:

        IConnectionServer[] connections = server.Connections;

        await server.SendToConnectionAsync<IPacket>(new Packet 
        {
            Data = "YourDataPayload",
            DateTime = 2020-02-18 11:54:32.4324
        }, connections[0]);

#### Receiving an Extended IPacket
If you want to extend **IPacket** to include additional fields, you will need to add the optional parameter **TcpHandler** that can be included with each constructor. The included **TcpHandler** has logic which is specific to deserialize messages of type **Packet**, but to receive your own extended **IPacket**, we will need to inherit / extend **TcpHandler** with our your class. Once **TcpHandler** has been extended, override the protected method called `MessageReceived(string message, IConnectionServer connection)` and deserialize into the extended **IPacket** of your choice. An example of this implementation is below:

    public class TcpHandlerExtended : TcpHandler
    {
        public TcpHandlerExtended(IParamsTcpServer parameters) : base(parameters)
        {
        }
        public TcpHandlerExtended(IParamsTcpServer parameters, X509Certificate certificate) : base(parameters, certificate)
        {
        }

        protected override IPacket MessageReceived(string message, IConnectionServer connection)
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
                        PacketExtendedType = PacketType1
                    };
                }
            }
            catch
            {
                packet = new PacketExtended
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow,
                    PacketExtendedType = PacketType1
                };
            }

            return packet;
        }
    }

If you don’t know the type of an object ahead of time, first deserialize the message into a class or struct that contains “common” fields, such as PacketExtended with a PacketExtendedType enum field. Then use the value of PacketExtendedType and deserialize a second time into that type. Repeat until the your custom object is completely deserialized.

Finally, when constructing your **ITcpNETServer**, pass in your new **TcpHandler** extended class you created. An example is as follows:

    IParamsTcpServer parameters = new ParamsTcpServer 
    {
        Port = 8989,
        EndOfLineCharacters = "\r\n",
        ConnectionSuccessString = "Connected Successfully",
    };

    ITcpNETServer server = new TcpNETServer(parameters, cert, handler: new TcpHandlerExtended(parameters, cert));

#### Ping
A raw message containing **'ping'** is sent automatically every 120 seconds to each client connected to a **TcpNETServer**. Each client is expected to immediately return a raw message containing **'pong'**. If a raw message containing **'pong'** is not received by the server before the next ping interval, the connection will be disconnected and removed from the **TcpNETServer**. This interval time is hard-coded to 120 seconds.

#### Disconnect a Client
To disconnect from the server, invoke the function `DisconnectConnection(IConnectionServer connection)`. **IConnectionServer** is a connected client to the server. These are exposed in the `ConnectionEvent` or can be retrieved from the **Connections** inside of **ITcpNETServer**.

#### Stop the Server and Disposal
To stop the server, call the `Dispose()` method to stop listening and free all allocated memory and resources.

***
### ITcpNETServerAuth
The second Tcp Server available is slightly more complex but includes authentication for identifying your connections. We will create an instance of **`ITcpNETServerAuth<T>`**  with the included implementation **`TcpNETServerAuth<T>`**. This object includes a generic, T, which represents the datatype of your user unique Id. For example, T could be an int, a string, a long, or a guid - this depends on the datatype of the unique Id you have set for your user. This generic allows the **`ITcpNETServerAuth<T>`** implementation to allow authentication and identification of users within many different user systems. The included implementation includes 3 constructors (for SSL or non-SSL servers):

* `TcpNETServerAuth<T>(IParamsTcpServer parameters, IUserService<T> userService, TcpHandler handler = null)`
    * The constructor for non-SSL Tcp Authentication Servers. Example instantiation is below:
 
            public class MockUserService : IUserService<long> 
            { }

            ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(new ParamsTcpServerAuth 
            {
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                ConnectionSuccessString = "Connected Successfully",
                ConnectionUnauthorizedString = "Connection not authorized",
            }, new MockUserService());

* `TcpNETServerAuth<T>(IParamsTcpServer parameters, IUserService<T> userService, X509Certificate certificate, TcpHandler handler = null))`
    * The constructor for a SSL Tcp Server where the SSL certificate is manually specified. Example instantiation is below:

            public class MockUserService : IUserService<long> 
            { }

            // Get a path to the SSL certificate
            var filename = Path.Combine(Environment.WebRootPath, "cert.pfx");
        
            // Instantiate the cert and provide the password
            var cert = new X509Certificate2(filename, "myCertPassword");
        
            // Start the server
            ITcpNETServerAuth<long> server = new ITcpNETServerAuth<long>(new ParamsTcpServerAuth 
            {
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                ConnectionSuccessString = "Connected Successfully",
                ConnectionUnauthorizedString = "Connection not authorized"
            }, new MockUserService(), cert);

* `TcpNETServerAuth<T>(IParamsTcpServer parameters, IUserService<T> userService, string certificateIssuedTo, StoreLocation storeLocation, TcpHandler handler = null))`
    * The constructor for a SSL Tcp Server where the SSL certificate is registered to and obtained from the Windows Certificate Store. Example instantiation is below:
            
            public class MockUserService : IUserService<long> 
            { }

            ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(new ParamsTcpServerAuth
            {
                Port = 8989,
                EndOfLineCharacters = "\r\n",
                ConnectionSuccessString = "Connected Successfully",
                ConnectionUnauthorizedString = "Connection not authorized"
            }, new MockUserService(), "connect.tcp.net", StoreLocation.LocalMachine);

The [Tcp.NET Authentication Server](https://www.nuget.org/packages/Tcp.NET.Server/) does not specify a listening uri / host. Instead, the server is configured to automatically listen on all available interfaces (including 127.0.0.1, localhost, and the server's IP).

#### Parameters
* **IParamsTcpServerAuth** - *Required*. [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) includes a default struct implementation called **ParamsTcpServerAuth** which contains the following connection detail data:
    * **Port** - *int* - The port where the Tcp Server will listen (e.g. 6660, 7210, 6483).
    * **EndOfLineCharacters** - *string* - Tcp does not automatically include line termination symbols, however it has become common practice in many applications that the end-of-line symbol is **"\r\n"** which represents an Enter key for many operating systems. We recommend you use **"\r\n"** as the line termination symbol.
    * **ConnectionSuccessString** - *string* - The string that will be sent to a newly connected client.
    * **ConnectionUnauthorizedString** - *string* - The string that will be sent to a connected client when they fail authentication.
* **`IUserService<T>`** - *Required* - This is an interface for a UserService class that will need to be implemented. This interface specifies 1 function, `GetIdAsync(string token)`, which will be invoked when the server receives an **OAuth Token** from a new connection. For more information regarding the User Service class, please see **[`IUserService<T>`](#userservice<T>)** below.
* **TcpHandler** - *Optional*. This is an object that can be passed in optionally. If you want to deserialize an extended **IPacket** from a client, you would extend **TcpHandler** in a new class and override `MessageReceived(string message, IConnectionServer connection)` to deserialize the object into the class / struct of your choice. For more information, please see **[Receiving an Extended Packet](#receiving-an-extended-packet)** below.
* **X509Certificate** - *Optional* - This is an object that contains the valid SSL certificate to bind to the server. See above for one implementation.
* **CertificateIssuedTo** - *Optional - string*. The certificate's issued hostname - for example, the Uri in the above examples is "connect.tcp.net", and the SSL certificate must be issued to "connect.tcp.net".
* **StoreLocation** - *Optional - enum* - Location the certificate is in the Windows Certificate Store. Potential options are:
    * **StoreLocation.CurrentUser** - *0* - The certificate is registered to the current user
    * **StoreLocation.LocalMachine** - *1* - The certificate is registered to the local system

#### `IUserService<T>`
**`IUserService<T>`** is an interface contained in [PHS.Networking.Server](https://www.nuget.org/packages/PHS.Networking.Server/). When creating a [Tcp.NET Authentication Server](https://www.nuget.org/packages/Tcp.NET.Server/), this inteface **`IUserService<T>`** will need to be instantiated into a concrete class. A default implementation is *not* included with Tcp.NET. An example implementation is shown below:

    public class UserServiceImplementation : IUserService<long>
    {
        protected readonly ApplicationDbContext _ctx;

        public UserServiceWS(ApplicationDbContext ctx)
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


Because you are responsible for filling the logic in `GetIdAsync(string oauthToken)`, the data could reside in many stores including (but not limited to) in memory, in a database, or against an identity server. In our implementation, we are checking the **OAuth Token** using [Entity Framework](https://docs.microsoft.com/en-us/ef/) and checking it against a quick User table. If the **OAuth Token** is found, then the appropriate UserId will be returned as type T, and if not, the default of type T will be returned (e.g. 0, "", Guid.Empty).

#### Events
4 events are exposed on the **`ITcpNETServerAuth<T>`** interface: MessageEvent, ConnectionEvent, ErrorEvent, and ServerEvent. These event signatures are below:

        server.MessageEvent += OMessageEvent;
        server.ConnectionEvent += OnConnectionEvent;
        server.ErrorEvent += OnErrorEvent;
        server.ServerEvent += OnServerEvent;

* `Task OnMessageEvent(object sender, TcpMessageServerEventArgs args);`
    * Invoked when a message is sent or received
* `Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args);`
    * Invoked when a Tcp client is connecting, connects, or disconnects from the server
* `Task OnErrorEvent(object sender, TcpErrorServerEventArgs args);`
    * Wraps all internal logic with try catch statements and outputs the specific error(s)
* `Task OnServerEvent(object sender, ServerEventArgs args);`
    * Invoked when the Tcp server starts or stops

#### Start the Tcp Authentication Server
There is no action to start the Tcp Server - once instantiated, the server will listen on the specified port until disposed.

#### SSL
To enable SSL for [Tcp.NET Authentication Server](https://www.nuget.org/packages/Tcp.NET.Server/), use one of the two provided SSL server constructors and manually specify the SSL certificate or direct the parameters to your Windows Certificate store. In order to allow successful connections, the server must have a valid, non-expired SSL certificate. Please note that a self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate. There are many sources for SSL certificates - we recommend [Let's Encrypt](https://letsencrypt.org/).

#### Send a Message to a Client
To send messages to a client, 11 functions are exposed:
* `BroadcastToAllAuthorizedUsersAsync<S>(S packet) where S : IPacket`
    * Send the designated packet to all Users and their connections currently logged into the server
* `BroadcastToAllAuthorizedUsersAsync(string message)`
    * Transform the message into a **Packet** and send to all Users and their connections currently logged into the server
* `BroadcastToAllAuthorizedUsersAsync(S packet, IConnectionServer connectionSending) where S : IPacket`
    * Send the designated packet to all Users and their connections currently logged into the server except for the connection matching connectionSending
* `BroadcastToAllAuthorizedUsersAsync(string message, IConnectionServer connectionSending)`
    * Transform the message into a **Packet** and send to all Users and their connections currently logged into the server except for the connection matching the connectionSending
* `BroadcastToAllAuthorizedUsersRawAsync(string message)`
    * Send the message directly to all Users and their connections currently logged into the server without transforming the message into a **Packet**
* `SendToUserAsync<S>(S packet, T userId) where S : IPacket`
    * Send the designated packet to the specified User and their connections currently logged into the server
* `SendToUserAsync(string message, T userId)`
    * Transform the message into a **Packet** and send the to the specified User and their connections currently logged into the server
* `SendToUserRawAsync(string message, T userId)`
    * Send the message directly to the designated User and their connections without transforming the message into a **Packet**
* `SendToConnectionAsync<S>(S packet, IConnectionServer connection, T userId) where S : IPacket`
    * Send the designated packet to the designated User's connection currently logged into the server
* `SendToConnectionAsync(string message, IConnectionServer connection, T userId)`
    * Transform the message into a **Packet** and send to the designated User's connection currently logged into the server
* `SendToConnectionRawAsync(string message, IConnectionServer connection, T userId)`
    * Send the message directly to the designated User's connection currently logged into the server without transforming the message into a **Packet**

More information about **IPacket** is available [here](#ipacket).

**IConnectionServer** is a connected client to the server. These are exposed in the `ConnectionEvent` or can be retrieved from **UserConnections** and then **Connections** inside of **`ITcpNETServerAuth<T>`**.

An example call to send a message to a client could be:

        IUserConnection<T>[] userConnections = server.UserConnections;

        await server.SendToConnectionAsync<IPacket>(new Packet 
        {
            Data = "YourDataPayload",
            DateTime = 2020-02-18 11:54:32.4324
        }, userConnections[0].Connections[0], userConnections[0]);

#### Receiving an Extended IPacket
If you want to extend **IPacket** to include additional fields, you will need to add the optional parameter **TcpHandler** that can be included with each constructor. The included **TcpHandler** has logic which is specific to deserialize messages of type **Packet**, but to receive your own extended **IPacket**, we will need to inherit / extend **TcpHandler** with your own class. Once **TcpHandler** has been extended, override the protected method called `MessageReceived(string message, IConnectionServer connection)` and deserialize into the extended **IPacket** of your choice. An example of this implementation is below:

    public class TcpHandlerExtended : TcpHandler
    {
        public TcpHandlerExtended(IParamsTcpServer parameters) : base(parameters)
        {
        }
        public TcpHandlerExtended(IParamsTcpServer parameters, X509Certificate certificate) : base(parameters, certificate)
        {
        }

        protected override IPacket MessageReceived(string message, IConnectionServer connection)
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
                        PacketExtendedType = PacketExtendedType.PacketType1
                    };
                }
            }
            catch
            {
                packet = new PacketExtended
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow,
                    PacketExtendedType = PacketExtendedType.PacketType1
                };
            }

            return packet;
        }
    }

If you don’t know the type of an object ahead of time, first deserialize the message into a class or struct that contains “common” fields, such as PacketExtended with a PacketExtendedType enum field. Then use the value of PacketExtendedType and deserialize a second time into that type. Repeat until the your custom object is completely deserialized.

Finally, when constructing **`ITcpNETServerAuth<T>`**, pass in your new **TcpHandler** extended class you created. An example is as follows:

    IParamsTcpServerAuth parameters = new ParamsTcpServerAuth 
    {
        Port = 8989,
        EndOfLineCharacters = "\r\n",
        ConnectionSuccessString = "Connected Successfully",
        ConnectionUnauthorizedString = "Connection Not Authorized"
    };

    ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(parameters, new MockUserService(), cert, handler: new TcpHandlerExtended(parameters, cert));

#### Ping
A raw message containing **'ping'** is sent automatically every 120 seconds to each client connected to a **`TcpNETServerAuth<T>`**. Each client is expected to return a raw message containing a **'pong'**. If a 'pong' is not received before the next ping interval, the connection will be disconnected and removed from the **`TcpNETServerAuth<T>`**. This interval time is hard-coded to 120 seconds.

#### Disconnect a Client
To disconnect from the server, invoke the function `DisconnectConnection(IConnectionServer connection)`. 

**IConnectionServer** is a connected client to the server. These are exposed in the `ConnectionEvent` or can be retrieved from **UserConnnections** then **Connections** inside of **`ITcpNETServeAuth<T>`**. If a logged in User disconnects from all connections, that user is automatically removed from **UserConnections**.

#### Stop the Server and Disposal
To stop the server, call the `Dispose()` method to stop listening and free all allocated memory and resources.

***

### Additional Information
[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) was created by [LiveOrDevTrying](https://www.liveordevtrying.com) and is maintained by [Pixel Horror Studios](https://www.pixelhorrorstudios.com). Tcp.NET is currently implemented in (but not limited to) the following projects: [Allie.Chat](https://allie.chat), [NTier.NET](https://github.com/LiveOrDevTrying/NTier.NET), and [The Monitaur](https://www.themonitaur.com).  
![Pixel Horror Studios Logo](https://pixelhorrorstudios.s3-us-west-2.amazonaws.com/Packages/PHS.png)