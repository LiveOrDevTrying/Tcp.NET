# **[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net)**
[**Tcp.NET**](https://www.github.com/liveordevtrying/tcp.net) provides a robust, performant, easy-to-use, and extendible Tcp server and Tcp client with included authorization / authentication  to identify clients connected to your server. All [**Tcp.NET**](https://www.github.com/liveordevtrying/tcp.net) packages referenced in this documentation are available on the [**NuGet package manager**](https://www.nuget.org) as separate packages ([**Tcp.NET.Client**](https://www.nuget.org/packages/Tcp.NET.Client) or [**Tcp.NET.Server**](https://www.nuget.org/packages/Tcp.NET.Server)) or in 1 aggregate package ([**Tcp.NET**](https://www.nuget.org/packages/Tcp.NET)). It has a sister-package called [WebsocketsSimple](https://www.nuget.org/packages/WebsocketsSimple) which follows the same patterns but for Websocket Clients and Servers.

![Image of Tcp.NET Logo](https://pixelhorrorstudios.s3-us-west-2.amazonaws.com/Packages/Tcp.NETLogo.png)

## **Table of Contents**<!-- omit in toc -->
- [**TcpNETClient**](#tcpnetclient)
    - [**Parameters**](#parameters)
    - [**Token**](#token)
    - [**Events**](#events)
    - [**SSL**](#ssl)
    - [**Connect to a Tcp Server**](#connect-to-a-tcp-server)
    - [**Send a Message to the Server**](#send-a-message-to-the-server)
    - [**`Ping`**](#ping)
    - [**Disconnect from the Server**](#disconnect-from-the-server)
    - [**Disposal**](#disposal)
- [**TcpNETServer**](#tcpnetserver)
    - [**Parameters**](#parameters-1)
    - [**Events**](#events-1)
    - [**SSL**](#ssl-1)
    - [**Start the Server**](#start-the-server)
    - [**Send a Message**](#send-a-message)
    - [**`Ping`**](#ping-1)
    - [**Disconnect a Connection**](#disconnect-a-connection)
    - [**Stop the Server**](#stop-the-server)
    - [**Disposal**](#disposal-1)
- [**TcpNETServerAuth<T>**](#tcpnetserverautht)
    - [**Parameters**](#parameters-2)
    - [**`IUserService<T>`**](#iuserservicet)
    - [**Events**](#events-2)
    - [**SSL**](#ssl-2)
    - [**Start the Server**](#start-the-server-1)
    - [**Send a Message**](#send-a-message-1)
    - [**`Ping`**](#ping-2)
    - [**Disconnect a Connection**](#disconnect-a-connection-1)
    - [**Stop the Server**](#stop-the-server-1)
    - [**Disposal**](#disposal-2)
- [**Additional Information**](#additional-information)
***

# **Client**
## **`TcpNETClient`**

First install [**Tcp.NET.Client NuGet package**](https://www.nuget.org/packages/Tcp.NET.Client) using the [**NuGet package manager**](https://www.nuget.org):

> install-package Tcp.NET.Client

This will add the most-recent version of [**Tcp.NET.Client**](#tcpnetclient) package to your specified project. 

Create a variable of type **`ITcpNETClient`** with the included implementation **`TcpNETClient`**. 

Signature:
* **`TcpNETClient(ParamsTcpClient parameters) : ITcpNETClient`**

Example:    
``` c#
    ITcpNETClient client = new TcpNETClient(new ParamsTcpClient("connect.tcp.net", 8989, "\r\n", isSSL: false);
```

### **Parameters**
* **`ParamsTcpClient`** - **Required** - Contains the following connection detail data:
    * **`Host`** - *int* - **Required** - The host or URI of the Tcp server instance to connect (e.g. connect.tcp.net, localhost, 127.0.0.1, etc).
    * **`Port`** - *int* - **Required** - The port of the Tcp server instance to connect (e.g. 6660, 7210, 6483).
    * **`EndOfLineCharacters`** - *string* - **Required** - The Tcp protocol does not automatically include line termination symbols, but it has become common practice in many applications that the end-of-line symbol is **`\r\n`** which represents an Enter key for many operating systems. It is recommended you use **`\r\n`** as the line termination symbol.
    * [**`Token`**](#token) - *string* - **Optional** - Optional parameter used by [**TcpNETServerAuth**](#tcpnetserverautht) for authenticating a user. Defaults to **`null`**.
        * Generating and validating a **`token`** is outside the scope of this document, but for more information, check out [**OAuthServer.NET**](https://github.com/LiveOrDevTrying/OAuthServer.NET) or [**IdentityServer4**](https://github.com/IdentityServer/IdentityServer4) for robust, easy-to-implemnt, and easy-to-use .NET identity servers.
        * A token can be any string. If wanting to use certificates, load the certs as a byte array, base64 encode them, and pass them as a string.
    * [**`IsSSL`**](#ssl) - *bool* - **Optional** - Flag specifying if the connection should be made using SSL encryption when connecting to the server. Defaults to **`true`**.
    * **`OnlyEmitBytes`** - *bool* - **Optional** - Flag specifying if [**TcpNETClient**](#tcpnetclient) should decode messages (**`Encoding.UTF8.GetString()`**) and return a string in [**MessageEvent**](#events) or return only the raw byte array received. Defaults to **`false`**.
    * **`UsePingPong`** - *bool* - **Optional** - Flag specifying if the connection will listen for **`PingCharacters`** and return **`PongCharacters`**. If using [**TcpNETServer**](#tcpnetserver) or [**TcpNETServerAuth<T>**](#tcpnetserverautht), ping/pong is enabled by default. If **`UsePingPong`** is set to false, the connection will be servered after the server's next ping cycle. Defaults to **`true`**.
    * **`PingCharacters`** - *string* - **Optional** - String specifying what string [**TcpNETClient**](#tcpnetclient) will be listening for from the server to verify the connection is still alive. When this string is received, **`PongCharacters`** will immediately be returned. An overload is included where **`PingCharacters`** and **`PongCharacters`** are byte arrays and called **`PingBytes`** and **`PongBytes`**. Defaults to **`"ping"`**.
    * **`PongCharacters`** - *string* - **Optional** - String specifying what string [**TcpNETClient**](#tcpnetclient) will send to the server immediately after **`PingCharacters`** is received. An overload is included where **`PingCharacters`** and **`PongCharacters`** are byte arrays and called **`PingBytes`** and **`PongBytes`**. Defaults to **`"pong"`**.
    * **`UseDisconnectBytes`** - *bool* - **Optional** - When [**TcpNETClient**](#tcpnetclient) gracefully [**disconnects from the server (DisconnectAsync())**](#disconnect-from-the-server), this flag specifies if the **`DisconnectBytes`** should be first sent to the server to signal a disconnect event. Defaults to **`true`**.
    * **`DisconnectBytes`** - *byte\[]* - **Optional** - If **`UseDisconnectBytes`** is true, this byte array allows a custom byte array to be sent to the server to signal a client invoked disconnect. This is the default behaviour for [**TcpNETServer**](#tcpnetserver) and [**TcpNETServerAuth<T>**](#tcpnetserverautht). If **`UseDisconnectBytes`** is true and **`DisconnectBytes`** is either null or an empty byte array, defaults to **`byte[] { 3 }`**.  
* **`ParamsTcpClient`** can be overloaded to specify **`EndOfLineCharacters`**, **`PingCharacters`**, and **`PongCharacters`** as byte arrays:
    * **`EndOfLineBytes`** - *byte\[]* - **Required** - Defaults to **`byte[] { 13, 10 }`**.
    * **`PingBytes`** - *byte\[]* - **Optional** - Defaults to **`byte[] { 112, 105, 110, 103 }`**.
    * **`PongBytes`** - *byte\[]* - **Optional** - Defaults to **`byte[] { 112, 111, 110, 103 }`**.

### **`Token`**

An optional parameter called [**`Token`**](#token) is included in the constructor for [**ParamsTcpClient**](#parameters) to authorize your client with [**TcpNETServerAuth<T>**](#tcpnetserverautht) or a custom Tcp server. Upon a successful connection to the server, the **`Token`** you specify and the **`EndOfLineCharacters`** or **`EndOfLineBytes`**will immediately and automatically be sent to the server.

If you are creating a manual Tcp connection to an instance of [**TcpNETServerAuth<T>**](#tcpnetserverautht), the first message you must send to the server is your **`Token`** followed by **`EndOfLineCharacters`** or **`EndOfLineBytes`**. This could look similar to the following:

Example:
```
    yourOAuthTokenGoesHere\r\n
```

### **Events**
3 events are exposed on the **`ITcpNETClient`** interface: **`MessageEvent`**, **`ConnectionEvent`**, and **`ErrorEvent`*. These event signatures are below:

Signatures:
* **`void MessageEvent(object sender, TcpMessageClientEventArgs args);`**
    * Invoked when a message is sent or received.
* **`void ConnectionEvent(object sender, TcpConnectionClientEventArgs args);`**
    * Invoked when [**TcpNETClient**](https://www.nuget.org/packages/Tcp.NET.Client) connects or disconnects from a server.
* **`void ErrorEvent(object sender, TcpErrorClientEventArgs args);`**
    * Wraps all logic in [**TcpNETClient**](https://www.nuget.org/packages/Tcp.NET.Client) with try catch statements and outputs the specific error(s).

Example:

``` c#
    client.MessageEvent += OMessageEvent;
    client.ConnectionEvent += OnConnectionEvent;
    client.ErrorEvent += OnErrorEvent
```

### **SSL**
SSL is enabled by default for [**TcpNETClient**](#tcpnetclient), but if you would like to disable SSL, set the **`IsSSL`** flag in [`ParamsTcpClient`](#parameters) to true. In order to connect successfully to an SSL server, the server must have a valid, non-expired SSL certificate where the certificate's issued hostname matches the **`host`** specified in [**`ParamsTcpClient`**](#parameters).

> ***Please note that a self-signed certificate or one from a non-trusted Certified Authority (CA) is not considered a valid SSL certificate.*** 

#### **Connect to a Tcp Server**

To connect to a Tcp Server, invoke the **`ConnectAsync()`** method.
* **`Task<bool> ConnectAsync(CancellationToken cancellationToken = default)`**
    * Connect to the Tcp Server specified in [**`ParamsTcpClient`**](#parameters)

Signature:

``` c#
    Task<bool> client.ConnectAsync(CancellationToken cancellationToken = default);
```     

#### **Send a Message to the Server**

2 functions are exposed to send messages to the server:
* **`Task<bool> SendAsync(string message, CancellationToken cancellationToken = default)`**
    * Send the specified string to the server.
* **`Task<bool> SendAsync(byte[] message, CancellationToken cancellationTokenn = default)`**
    * Send the specified byte array to the server.

Example:

``` c#
    await client.SendAsync("Hello World");
```

### **Ping**
A [**TcpNETServer**](#tcpnetserver) or [**TcpNETServerAuth<T>**](#tcpnetserverautht) will send a ping message to every client at a specified interval defined to verify which connections are still alive. If a client fails to detect the **`PingCharacters`** or **`PingBytes`** and/or respond with the **`PongCharacters`** or **`PongBytes`**, during the the next ping cycle, the connection will be severed and disposed. However, if you are using [**TcpNETClient**](#tcpnetclient), the ping / pong messages are digested and handled and will not be emit by [**`MessageEvent`**](#events). This means you do not need to worry about ping and pong messages if you are using [**TcpNETClient**](#tcpnetclient). 

If you are creating your own Tcp connection, you should incorporate logic to listen for **`PingCharacters`** or **`PingBytes`**. If received, immediately respond with a message containing **`PongCharacters`** or **`PingBytes`** followed by the **`EndOfLineCharacters`** or **`EndOfLineBytes`**. This could look similar to the following:

Sent by Server:
```
    ping\r\n
```

Response by Client:
```
    pong\r\n
```

> ***Note: Failure to implement this logic will result in a connection being severed in up to approximately 240 seconds.***

### **Disconnect from the Server**
To disconnect from the server, invoke the **`DisconnectAsync()`** method.

Signature:
* **`Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)`**
    * Disconnect from the connect Tcp Server

Example: 

``` c#
    await client.DisconnectAsync();
```

### **Disposal**
At the end of usage, be sure to call the **`Dispose()`** method on [**TcpNETClient**](#tcpnetclient) to free allocated memory and resources.

Signature:
*  **`void Dispose()`**
    * Close the connection and free allocated memory, resources, and event handlers.

Example:

``` c#
    client.Dispose();
```

---
# **Server**
First install the [**Tcp.NET.Server package**](https://www.nuget.org/packages/Tcp.NET.Server) using the [**NuGet package manager**](https://www.nuget.org):
> install-package Tcp.NET.Server

This will add the most-recent version of the [**Tcp.NET.Server**](https://www.nuget.org/packages/Tcp.NET.Server) package to your project. 

There are 2 different types of Tcp Servers. 
* [**TcpNETServer**](#tcpnetserver)
* [**TcpNETServerAuth<T>**](#tcpnetserverautht)

---

## **`TcpNETServer`**
Create a variable of type **`ITcpNETServer`** with the included implementation [**TcpNETServer**](#tcpnetserver). The included implementation includes 2 constructors - one for SSL connections and one for non-SSL connections:

Signatures:
* **`TcpNETServer(ParamsTcpServer parameters) : ITcpNETServer`**
* **`TcpNETServer(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : ITcpNETServer`**

Example non-SSL server:
``` c#
    ITcpNETServer server = new TcpNETServer(new ParamsTcpServer(8989, "\r\n", connectionSuccesString:"Connected Successfully"));
```

Example SSL server:
``` c#
    byte[] certificate = File.ReadAllBytes("cert.pfx");
    string certificatePassword = "yourCertificatePassword";

    ITcpNETServer server = new TcpNETServer(new ParamsTcpServer(8989, "\r\n", connectionSuccessString: "Connected Successfully"), certificate, certificatePassword);
```

### **Parameters**
* **`ParamsTcpServer`** - **Required** - Contains the following connection detail data:
    * **`Port`** - *int* - **Required** - The port that the Tcp server will listen on (e.g. 6660, 7210, 6483).
    * **`EndOfLineCharacters`** - *string* - **Required** - Tcp does not automatically include line termination ends, however in many applications the end-of-line symbol is **`\r\n`** which represents an Enter key for many operating systems. We recommend you use **`\r\n`** as the line termination symbol.
    * **`ConnectionSuccessString`** - *string* - **Optional** - The string that will be sent to a newly successful connected client. Defaults to **`null`**.
    * **`OnlyEmitBytes`** - *bool* - **Optional** - Flag specifying if [**TcpNETServer**](#tcpnetserver) should decode messages (**`Encoding.UTF8.GetString()`**) and return a string in [**MessageEvent**](#events-1) or return only the raw byte array received. Defaults to **`false`**.
    * **`PingIntervalSec`** - *int* - **Optional** - Int representing how often the server will send all connected clients **`PingCharacters`**. Clients need to immediately response with **`PongCharacters`** or the connection will be severed at the next ping cycle. If you would like to disable the ping service on your Tcp server, set this value to 0. Defaults to **`true`**.
    * **`PingCharacters`** - *string* - **Optional** - String specifying what string [**TcpNETServer**](#tcpnetserver) will send to each connected client to verify the connection is still alive. When a client receives this string, they will immediately need to respond with **`PongCharacters`** or the connection will be severed during the next ping cycle.. An overload is included where **`PingCharacters`** and **`PongCharacters`** are byte arrays and called **`PingBytes`** and **`PongBytes`**. Defaults to **`"ping"`**.
    * **`PongCharacters`** - *string* - **Optional** - String specifying what string [**TcpNETServer**](#tcpnetserver) will listen for following a ping cycle to specify that the connection is still alive. Clients need to immediately send **`PongCharacters`** to the server after receiving **`PingCharacters`** or the connection will be severed after the next ping cycle. An overload is included where **`PingCharacters`** and **`PongCharacters`** are byte arrays and called **`PingBytes`** and **`PongBytes`**. Defaults to **`"pong"`**.
    * **`UseDisconnectBytes`** - *bool* - **Optional** - When [**TcpNETServer**](#tcpnetserver) gracefully [**disconnects a connection (DisconnectConnectionAsync())**](#disconnect-a-connection), this flag specifies if the **`DisconnectBytes`** should be first sent to the client to signal a disconnect event. Defaults to **`true`**.
    * **`DisconnectBytes`** - *byte\[]* - **Optional** - If **`UseDisconnectBytes`** is true, this byte array allows a custom byte array to be sent to the client to signal a server invoked disconnect. This is the default behaviour for [**TcpNETServer**](#tcpnetserver) and [**TcpNETServerAuth<T>**](#tcpnetserverautht). If **`UseDisconnectBytes`** is true and **`DisconnectBytes`** is either null or an empty byte array, defaults to **`byte[] { 3 }`**.  
* **`Certificate`** - *byte[]* - **Optional** - A byte array containing a SSL certificate with private key if the server will be hosted on Https.
* **`CertificatePassword`** - *string* - **Optional** - The private key of the SSL certificate if the server will be hosted on Https.

### **Events**
4 events are exposed on the **`ITcpNETServer`** interface: **`MessageEvent`**, **`ConnectionEvent`**, **`ErrorEvent`**, and **`ServerEvent`**. These event signatures are below:

Signatures:
* **`void MessageEvent(object sender, TcpMessageServerEventArgs args);`**
    * Invoked when a message is sent or received.
* **`void ConnectionEvent(object sender, TcpConnectionServerEventArgs args);`**
    * Invoked when a Tcp client is connecting, connects, or disconnects from the server.
* **`void ErrorEvent(object sender, TcpErrorServerEventArgs args);`**
    * Wraps all internal logic with try catch statements and outputs the specific error(s).
* **`void ServerEvent(object sender, ServerEventArgs args);`**
    * Invoked when the Tcp server starts or stops.

Examples:
``` c#
    server.MessageEvent += OnMessageEvent;
    server.ConnectionEvent += OnConnectionEvent;
    server.ErrorEvent += OnErrorEvent;
    server.ServerEvent += OnServerEvent;
```

### **SSL**
To enable SSL, use the provided SSL server constructor and specify your SSL certificate as a byte array and your certificate's private key as a string. 

Signature:
* **`TcpNETServer(ParamsTcpServer parameters, byte[] certificate, string certificatePassword);`**

> **The SSL Certificate MUST match the domain where the Tcp Server is hosted or clients will not able to connect to the Tcp Server.** 
 
Example:
``` c#
    byte[] certificate = File.ReadAllBytes("cert.pfx");
    string certificatePassword = "yourCertificatePassword";

    ITcpNETServer server = new TcpNETServer(new ParamsTcpServer(8989, "\r\n", connectionSuccessString: "Connected Successfully"), certificate, certificatePassword);
```

In order to allow successful SSL connections, you must have a valid, non-expired SSL certificate. There are many sources for SSL certificates and some of them are open-source ([Let's Encrypt](https://letsencrypt.org/)).

> ***Note: A self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate.***

### **Start the Server**
To start the server, call the **`Start()`** method to instruct the server to begin listening for messages. The **`Start()`** method is a synchronous operation:

Signature:
* **`void Start();`**
    * Start the Tcp server and begin listening on the specified port

Example:

``` c#
    server.Start();
```

### **Send a Message**
3 functions are exposed to send messages to connections: 

Signatures:
* **`Task<bool> SendToConnectionAsync(string message, ConnectionTcpServer connection, CancellationToken cancellationToken = default);`**
    * Send the message string to the specified connection.
* **`Task<bool> SendToConnectionAsync(byte[] message, ConnectionTcpServer connection, CancellationToken cancellationToken = default);`**
    * Send the byte array to the specified connection.
* **`Task<bool> BroadcastToAllConnectionsAsync(string message, CancellationToken cancellationToken = default);`**
    * Send the message string to all connectionsn.
* **`Task<bool> BroadcastToAllConnectionsAsync(byte[] message, CancellationToken cancellationToken = default);`**
    * Send the byte array to all connections.

**`ConnectionTcpServer`** represents a connected client to the server. These are exposed in **`ConnectionEvent`**, can be retrieved from  **`Connections`** inside of [**TcpNETServer**](#tcpnetserver), or by extending [**TcpNETServer**](#tcpnetserver) and using the **`this._connectionManager`** object.

An example to send a message to a specific connection could be:

``` c#
    ConnectionTcpServer connection = server.Connections.FirstOrDefault(x => x.ConnectionId = "desiredConnectionId");

    if (connection != null) {
        await server.SendToConnectionAsync("YourDataPayload", connection);
    }
```

### **`Ping`**
[**TcpNETServer**](#tcpnetserver) will send a ping message to every client at a specified interval defined by **`PingIntervalSec`** (defaults to 120 sec, in **`ParamsTcpServer`**) to verify which connections are still alive. If a client fails to detect the **`PingCharacters`** or **`PingBytes`** and/or respond with the **`PongCharacters`** or **`PongBytes`**, during the the next ping cycle, the connection will be severed and disposed. However, if you are using [**TcpNETClient**](#tcpnetclient), the ping / pong messages are digested and handled and will not be emit by [**`MessageEvent`**](#events-1). This means you do not need to worry about ping and pong messages if you are using [**TcpNETClient**](#tcpnetclient). 

If you would like to disable the ping / pong feature, set the **`PingIntervalSec`** defined in **`ParamsTcpServer`** to 0.

If you are creating your own Tcp connection and **`PingIntervalSec`** is greater than 0, you should incorporate logic to listen for **`PingCharacters`** or **`PingBytes`**. If received, immediately respond with a message containing **`PongCharacters`** or **`PingBytes`** followed by the **`EndOfLineCharacters`** or **`EndOfLineBytes`**. This could look similar to the following:

Sent by Server:
```
    ping\r\n
```

Response by Client:
```
    pong\r\n
```

### **Disconnect a Connection**
To disconnect a connection from the server, invoke the function **`DisconnectConnectionAsync(ConnectionTcpServer connection)`**.

Signature:
* **`Task<bool> DisconnectionConnectionAsync(ConnectionTcpServer connection, CancellationToken cancellationToken = default)`**

Example:
``` c#
    await DisconnectConnectionAsync(connection);
```

**`ConnectionTcpServer`** represents a connected client to the server. These are exposed in **`ConnectionEvent`**, can be retrieved from  **`Connections`** inside of [**TcpNETServer**](#tcpnetserver), or by extending [**TcpNETServer**](#tcpnetserver) and using the **`this._connectionManager`** object.

### **Stop the Server**
To stop the server, call the **`Stop()`** method.

Signature:
* **`void Stop();`**

Example: 
``` c#
    server.Stop();
```

### **Disposal**
After stopping the server, if you are not going to start the server again, call the **`Dispose()`** method to free all allocated memory and resources.

Signatures:
* **`void Dispose();`**

Example: 
``` c#
    server.Dispose();
```

---
## **`TcpNETServerAuth<T>`**
[**TcpNETServerAuth<T>**](#tcpnetserverautht) includes authentication for identifying your connections / users.

You will need to define your UserService, so make a new class that implements IUserService<T>. This object includes a generic, T, which represents the datatype of your user unique Id. For example, T could be an int, a string, a long, or a guid - this depends on the datatype of the unique Id you have set for your user. This generic allows the **`ITcpNETServerAuth<T>`** implementation to allow authentication and identification of users for any user systems. 

Signature:
``` c#
    public interface IUserService<UId>
    {
        Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<UId> GetIdAsync(string token, CancellationToken cancellationToken = default);
    }
```

Example:
``` c#
    public class UserService : IUserService<long>
    {
        public Task<long> GetIdAsync(string token, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return token == "testToken" ? Task.FromResult(true) : Task.FromResult(false);
        }
    }
```

Implement the **`GetIdAsync()`** and **`IsValidTokenAsync()`** methods to validate the Token that was passed. Generating and validating a **`token`** is outside the scope of this document, but for more information, check out [**OAuthServer.NET**](https://github.com/LiveOrDevTrying/OAuthServer.NET) or [**IdentityServer4**](https://github.com/IdentityServer/IdentityServer4) for robust, easy-to-implemnt, and easy-to-use .NET identity servers.

Next, create a variable of type **`ITcpNETServerAuth<T>`** with the included implementation **`TcpNETServerAuth<T>`** where T is the same type as you defined in IUserService. The included implementation includes 2 constructors - one for SSL connections and one for non-SSL connections:

Signatures:
* `TcpNETServerAuth<T>(ParamsTcpServerAuth parameters, IUserService<T> userService)`
* `TcpNETServerAuth<T>(IParamsTcpServerAuth parameters, IUserService<T> userService, byte[] certificate, string certificatePassword)`

Example non-SSL:
``` c#
    ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(new ParamsTcpServerAuth(8989, "\r\n", connectionSuccessString: "Connected Successfully", connectionUnauthorizedString: "Connection not authorized"), new UserService());
```

Example SSL:
``` c#
    byte[] certificate = File.ReadAllBytes("yourCert.pfx");
    string certificatePassword = "yourCertificatePassword";

    ITcpNETServerAuth<long> server = new ITcpNETServerAuth<long>(new ParamsTcpServerAuth(8989, "\r\n", connectionSuccessString: "Connected Successfully", connectionUnauthorizedString: "Connection not authorized"), new UserService(), certificate, certificatePassword);
```

### **Parameters**
* **`ParamsTcpServerAuth`** - **Required** - Contains the following connection detail data:
    * **`Port`** - *int* - **Required** - The port that the Tcp server will listen on (e.g. 6660, 7210, 6483).
    * **`EndOfLineCharacters`** - *string* - **Required** - Tcp does not automatically include line termination ends, however in many applications the end-of-line symbol is **`\r\n`** which represents an Enter key for many operating systems. We recommend you use **`\r\n`** as the line termination symbol.
    * **`ConnectionSuccessString`** - *string* - **Optional** - The string that will be sent to a newly successful connected client. Defaults to **`null`**.
    * **`ConnectionUnauthorizedString`** - *string* - **Optional** - The string that will be sent to a client if they fail authentication. Defaults to **`null`**.
    * **`OnlyEmitBytes`** - *bool* - **Optional** - Flag specifying if [**TcpNETServer**](#tcpnetserver) should decode messages (**`Encoding.UTF8.GetString()`**) and return a string in [**MessageEvent**](#events-1) or return only the raw byte array received. Defaults to **`false`**.
    * **`PingIntervalSec`** - *int* - **Optional** - Int representing how often the server will send all connected clients **`PingCharacters`**. Clients need to immediately response with **`PongCharacters`** or the connection will be severed at the next ping cycle. If you would like to disable the ping service on your Tcp server, set this value to 0. Defaults to **`true`**.
    * **`PingCharacters`** - *string* - **Optional** - String specifying what string [**TcpNETServer**](#tcpnetserver) will send to each connected client to verify the connection is still alive. When a client receives this string, they will immediately need to respond with **`PongCharacters`** or the connection will be severed during the next ping cycle.. An overload is included where **`PingCharacters`** and **`PongCharacters`** are byte arrays and called **`PingBytes`** and **`PongBytes`**. Defaults to **`"ping"`**.
    * **`PongCharacters`** - *string* - **Optional** - String specifying what string [**TcpNETServer**](#tcpnetserver) will listen for following a ping cycle to specify that the connection is still alive. Clients need to immediately send **`PongCharacters`** to the server after receiving **`PingCharacters`** or the connection will be severed after the next ping cycle. An overload is included where **`PingCharacters`** and **`PongCharacters`** are byte arrays and called **`PingBytes`** and **`PongBytes`**. Defaults to **`"pong"`**.
    * **`UseDisconnectBytes`** - *bool* - **Optional** - When [**TcpNETServerAuth<T>**](#tcpnetserverautht) gracefully [**disconnects a connection (DisconnectConnectionAsync())**](#disconnect-a-connection-1), this flag specifies if the **`DisconnectBytes`** should be first sent to the client to signal a disconnect event. Defaults to **`true`**.
    * **`DisconnectBytes`** - *byte\[]* - **Optional** - If **`UseDisconnectBytes`** is true, this byte array allows a custom byte array to be sent to the client to signal a server invoked disconnect. This is the default behaviour for [**TcpNETServerAuth<T>**](#tcpnetserverautht) and [**TcpNETServerAuth<T>**](#tcpnetserverautht). If **`UseDisconnectBytes`** is true and **`DisconnectBytes`** is either null or an empty byte array, defaults to **`byte[] { 3 }`**.  
* **`IUserService<T>`** - **Required** - This interface for a User Service class will need to be implemented. This interface specifies 21 functions, `GetIdAsync()` and `IsValidTokenAsync(string token)`, which will be invoked when the server receives an **`Token`** from a new connection. For more information regarding the User Service class, please see [**IUserService<T>**`](#iuserservicet) below.
* **`Certificate`** - *byte[]* - **Optional** - A byte array containing the exported SSL certificate with private key if the server will be hosted on Https.
* **`CertificatePassword`** - *string* - **Optional** - The private key of the exported SSL certificate if the server will be hosted on Https.

#### **`IUserService<T>`**
This is an interface contained in [PHS.Networking.Server](https://www.nuget.org/packages/PHS.Networking.Server). The constructor for **`TcpNETServerAuth<T>`** requires an **`IUserService<T>`**, and this interface will need to be implemented into a concrete class.

> **A default implementation is *not* included with [Tcp.NET.Server](https://www.nuget.org/packages/Tcp.NET.Server). You will need to implement this interface and add logic here.**

An example implementation using [Entity Framework](https://docs.microsoft.com/en-us/ef/) is shown below:

``` c#
    public class UserServiceTcp : IUserService<long>
    {
        protected readonly ApplicationDbContext _ctx;

        public UserServiceTcp(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<long> GetIdAsync(string token, CancellationToken cancellationToken = default)
        {
            // Obfuscate the token in the database
            token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var user = await _ctx.Users.FirstOrDefaultAsync(s => s.OAuthToken == token);
            return user != null ? user.Id : (default);
        }

        public Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            // Obfuscate the token in the database
            token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            return await _ctx.Users.Any(s => s.OAuthToken == token);
        }
    }
```

Because you are responsible for creating the logic in **`GetIdAsync(string token)`** and **`IsValidTokenAsync(string token)`**, the data could reside in many stores including (but not limited to) in-memory, database, identity system, or auth systems.

### **Events**
4 events are exposed on the **`ITcpNETServerAuth<T>`** interface: `MessageEvent`, `ConnectionEvent`, `ErrorEvent`, and `ServerEvent`. These event signatures are below:

Signatures:
* **`void MessageEvent(object sender, TcpMessageServerAuthEventArgs<T> args);`**
    * Invoked when a message is sent or received.
* **`void ConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<T> args);`**
    * Invoked when a Tcp client is connecting, connects, or disconnects from the server.
* **`void ErrorEvent(object sender, TcpErrorServerAuthEventArgs<T> args);`**
    * Wraps all internal logic with try catch statements and outputs the specific error(s).
* **`void ServerEvent(object sender, ServerEventArgs args);`**
    * Invoked when the Tcp server starts or stops.

Examples:
``` c#
    server.MessageEvent += OMessageEvent;
    server.ConnectionEvent += OnConnectionEvent;
    server.ErrorEvent += OnErrorEvent;
    server.ServerEvent += OnServerEvent;
```

### **SSL**
To enable SSL, use the provided SSL server constructor and specify your SSL certificate as a byte array and your certificate's private key as a string. 

Signature:
* **`TcpNETServerAuth<T>(ParamsTcpServerAuth parameters, IUserManager<T> userManager, byte[] certificate, string certificatePassword);`**

> **The SSL Certificate MUST match the domain where the Tcp Server is hosted or clients will not able to connect to the Tcp Server.** 
 
Example:
``` c#
    byte[] certificate = File.ReadAllBytes("cert.pfx");
    string certificatePassword = "yourCertificatePassword";

    ITcpNETServerAuth<long> server = new TcpNETServerAuth<long>(new ParamsTcpServerAuth(8989, "\r\n", connectionSuccessString: "Connected Successfully"), new UserManager(), certificate, certificatePassword);
```

In order to allow successful SSL connections, you must have a valid, non-expired SSL certificate. There are many sources for SSL certificates and some of them are open-source ([Let's Encrypt](https://letsencrypt.org/)).

> ***Note: A self-signed certificate or one from a non-trusted CA is not considered a valid SSL certificate.***

### **Start the Server**
To start the server, call the **`Start()`** method to instruct the server to begin listening for messages. The **`Start()`** method is a synchronous operation:

Signature:
* **`void Start();`**
    * Start the Tcp server and begin listening on the specified port

Example:

``` c#
    server.Start();
```

### **Send a Message**
To send messages to connections, 6 methods are exposed:
* **`Task<bool> SendToConnectionAsync(string message, IdentityTcpServer<T> connection, CancellationToken cancellationToken = default);`**
    * Send the message string to the specified connection.
* **`Task<bool> SendToConnectionAsync(byte[] message, IdentityTcpServer<T> connection, CancellationToken cancellationToken = default);`**
    * Send the byte array to the specified connection.
* **`Task<bool> SendToUserAsync<T>(string message, T userId, CancellationToken cancellationToken = default)`**
    * Send the message string to the specified user and their connections currently logged into the server.
* **`Task<bool> SendToUserAsync(byte[] message, T userId, CancellationToken cancellationToken = default)`**
    * Send the byte array string to the specified user and their connections currently logged into the server.
* **`Task<bool> BroadcastToAllConnectionsAsync(string message, CancellationToken cancellationToken = default);`**
    * Send the message string to all connectionsn.
* **`Task<bool> BroadcastToAllConnectionsAsync(byte[] message, CancellationToken cancellationToken = default);`**
    * Send the byte array to all connections.

**`IdentityTcpServer<T>`** represents a connected client to the server. These are exposed in **`ConnectionEvent`**, can be retrieved from  **`Connections`** inside of [**TcpNETServerAuth<T>**](#tcpnetserverautht), or by extending [**TcpNETServerAuth<T>**](#tcpnetserverautht) and using the **`this._connectionManager`** object.

An example to send a message to a specific connection could be:

``` c#
    IdentityTcpServer<T> connection = server.Connections.FirstOrDefault(x => x.ConnectionId = "desiredConnectionId");

    if (connection != null) {
        await server.SendToConnectionAsync("YourDataPayload", connection);
    }
```

### **`Ping`**
[**TcpNETServerAuth<T>**](#tcpnetserverautht) will send a ping message to every client at a specified interval defined by **`PingIntervalSec`** (defaults to 120 sec, in **`ParamsTcpServerAuth`**) to verify which connections are still alive. If a client fails to detect the **`PingCharacters`** or **`PingBytes`** and/or respond with the **`PongCharacters`** or **`PongBytes`**, during the the next ping cycle, the connection will be severed and disposed. However, if you are using [**TcpNETClient**](#tcpnetclient), the ping / pong messages are digested and handled and will not be emit by [**`MessageEvent`**](#events-1). This means you do not need to worry about ping and pong messages if you are using [**TcpNETClient**](#tcpnetclient). 

If you would like to disable the ping / pong feature, set the **`PingIntervalSec`** defined in [**`ParamsTcpServerAuth`**](#parameters-1) to 0.

If you are creating your own Tcp connection and **`PingIntervalSec`** is greater than 0, you should incorporate logic to listen for **`PingCharacters`** or **`PingBytes`**. If received, immediately respond with a message containing **`PongCharacters`** or **`PingBytes`** followed by the **`EndOfLineCharacters`** or **`EndOfLineBytes`**. This could look similar to the following:

Sent by Server:
```
    ping\r\n
```

Response by Client:
```
    pong\r\n
```

### **Disconnect a Connection**
To disconnect a connection from the server, invoke the function **`DisconnectConnectionAsync(IdentityTcpServer<T> connection)`**.

Signature:
* **`Task<bool> DisconnectionConnectionAsync(IdentityTcpServer<T> connection, CancellationToken cancellationToken = default)`**

Example:
``` c#
    await DisconnectConnectionAsync(connection);
```

**`IdentityTcpServer<T>`** represents a connected client to the server. These are exposed in **`ConnectionEvent`**, can be retrieved from  **`Connections`** inside of [**TcpNETServerAuth<T>**](#tcpnetserverautht), or by extending [**TcpNETServerAuth<T>**](#tcpnetserverautht) and using the **`this._connectionManager`** object.

### **Stop the Server**
To stop the server, call the **`Stop()`** method.

Signature:
* **`void Stop();`**

Example: 
``` c#
    server.Stop();
```

### **Disposal**
After stopping the server, if you are not going to start the server again, call the **`Dispose()`** method to free all allocated memory and resources.

Signatures:
* **`void Dispose();`**

Example: 
``` c#
    server.Dispose();
```

---
### **Additional Information**
[Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) was created by [Rob Engel](https://www.robthegamedev.com) - [LiveOrDevTrying](https://www.liveordevtrying.com) - and is maintained by [Pixel Horror Studios](https://www.pixelhorrorstudios.com). [Tcp.NET](https://www.github.com/liveordevtrying/tcp.net) is currently implemented in (but not limited to) the following projects: [The Monitaur](https://www.themonitaur.com), [Allie.Chat](https://allie.chat), and [Gem Wars](https://www.pixelhorrorstudios.com) *(currently in development)*. It is used in the following packages: [WebsocketsSimple](https://github.com/LiveOrDevTrying/WebsocketsSimple), [NTier.NET](https://github.com/LiveOrDevTrying/NTier.NET), and [The Monitaur](https://www.themonitaur.com).
  
![Pixel Horror Studios Logo](https://pixelhorrorstudios.s3-us-west-2.amazonaws.com/Packages/PHS.png)