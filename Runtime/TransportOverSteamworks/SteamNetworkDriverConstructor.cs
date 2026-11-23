#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if !TRANSPORT_OVER_STEAMWORKS_DISABLE_LOGGING
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_ENABLE_LOGGING
#endif

#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_ENABLE_LOGGING && TRANSPORT_OVER_STEAMWORKS_ENABLE_TRACING
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_ENABLE_TRACING
#endif

#if HAS_NETCODE_7_OR_NEWER_PACKAGE || (HAS_NETCODE_PACKAGE && HAS_ENTITIES_PACKAGE)

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TransportOverSteamworks.Debugging;
using Unity.Scripting.LifecycleManagement;

#if HAS_NETCODE_PACKAGE
#if HAS_NETCODE_7_OR_NEWER_PACKAGE
using Unity.Netcode;
#else
using Unity.NetCode;
using Unity.Entities;
#endif
#endif

namespace TransportOverSteamworks {
    [PublicAPI]
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public partial class SteamNetworkDriverConstructor : INetworkStreamDriverConstructor {
        static PlayType _playType;
        static IntPtr _sockets;
        static SteamNetworkInterfaceTypeFlags _serverInterfaces;
        static SteamNetworkInterfaceType _clientInterface;
        static SteamSendFlags _sendFlags;
        static int _messagesPerReceive;

        /// Returns the pointer to the Steamworks sockets.
        /// Use this one to get the sockets for a Steam user
        /// (the one logged into the currently running Steam app).
        public static IntPtr GetSteamUserSockets() {
            try {
                Trace.NetworkDriverConstructor.GetSteamUserSocketsStart();

#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
                SteamAPI.LoadDllIfNeeded();
#endif

                var userHandle = SteamAPI.GetHSteamUser();
                if (userHandle <= 0) {
                    Log.NetworkDriverConstructor.GetSteamUserSocketsHUserFailure();
                    return IntPtr.Zero;
                }

                var sockets = userHandle.FindOrCreateUserInterface(SteamAPI.SocketsVersion);
                if (sockets == IntPtr.Zero) {
                    Log.NetworkDriverConstructor.GetSteamUserSocketsFailure(userHandle, SteamAPI.SocketsVersion);
                    return IntPtr.Zero;
                }

                return sockets;
            }
            finally {
                Trace.NetworkDriverConstructor.GetSteamUserSocketsEnd();
            }
        }

        /// Returns the pointer to the Steamworks sockets.
        /// Use this one to get the sockets for the Steamworks
        /// game server (not tied to the Steam app).
        public static IntPtr GetSteamGameServerSockets() {
            try {
                Trace.NetworkDriverConstructor.GetSteamGameServerSocketsStart();

#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
                SteamAPI.LoadDllIfNeeded();
#endif

                var userHandle = SteamAPI.SteamGameServer.GetHSteamUser();
                if (userHandle <= 0) {
                    Log.NetworkDriverConstructor.GetSteamGameServerSocketsHUserFailure();
                    return IntPtr.Zero;
                }

                var sockets = userHandle.FindOrCreateGameServerInterface(SteamAPI.SocketsVersion);
                if (sockets == IntPtr.Zero) {
                    Log.NetworkDriverConstructor.GetSteamGameServerSocketsFailure(userHandle, SteamAPI.SocketsVersion);
                    return IntPtr.Zero;
                }

                return sockets;
            }
            finally {
                Trace.NetworkDriverConstructor.GetSteamGameServerSocketsEnd();
            }
        }

        /// Sets SteamNetworkDriverConstructor as the DriverConstructor in
        /// Unity Netcode, so any new client drivers created after this call
        /// use Steamworks as transport, while creation of any new server
        /// drivers fails. You have to specify the Steamworks sockets pointer
        /// and the client interface type (P2P, IP, FakeIP). Other parameters
        /// are optional and include the Steamworks sockets send flags, the
        /// number of received messages processed at once, and a list of
        /// worlds in which existing drivers should be updated. Returns false
        /// if the driver constructor was not set. Returns true in any other
        /// case, including when updating any of the existing drivers fails.
        public static bool InitializeClient(
            IntPtr sockets,
            SteamNetworkInterfaceType clientInterface,
            SteamSendFlags sendFlags = Defaults.SendFlags,
            int messagesPerReceive = Defaults.MessagesPerReceive,
#if HAS_NETCODE_7_OR_NEWER_PACKAGE
            List<NetcodeWorld> worlds = null) {
#else
            List<World> worlds = null) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.InitializeClientStart();

                var playType = (PlayType)ClientServerBootstrap.RequestedPlayType;
                if (!playType.CheckPlayType(client: true)) {
                    Log.NetworkDriverConstructor.NotInitializedClient(playType);
                    return false;
                }

                if (!clientInterface.CheckInterfaceType()) {
                    Log.NetworkDriverConstructor.NotInitializedClient(clientInterface);
                    return false;
                }

                return Initialize(
                    playType,
                    sockets,
                    clientInterface,
                    serverInterfaces: SteamNetworkInterfaceTypeFlags.None,
                    sendFlags,
                    messagesPerReceive,
                    worlds);
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.InitializeClientEnd();
                }
            }
        }

        /// Sets SteamNetworkDriverConstructor as the DriverConstructor in
        /// Unity Netcode, so any new server drivers created after this call
        /// use Steamworks as transport, while creation of any new client
        /// drivers fails. You have to specify the Steamworks sockets pointer
        /// and the server interface types (P2P, IP, FakeIP). Other parameters
        /// are optional and include the Steamworks sockets send flags, the
        /// number of received messages processed at once, and a list of
        /// worlds in which existing drivers should be updated. Returns false
        /// if the driver constructor was not set. Returns true in any other
        /// case, including when updating any of the existing drivers fails.
        public static bool InitializeServer(
            IntPtr sockets,
            SteamNetworkInterfaceTypeFlags serverInterfaces,
            SteamSendFlags sendFlags = Defaults.SendFlags,
            int messagesPerReceive = Defaults.MessagesPerReceive,
#if HAS_NETCODE_7_OR_NEWER_PACKAGE
            List<NetcodeWorld> worlds = null) {
#else
            List<World> worlds = null) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.InitializeServerStart();

                var playType = (PlayType)ClientServerBootstrap.RequestedPlayType;
                if (!playType.CheckPlayType(server: true)) {
                    Log.NetworkDriverConstructor.NotInitializedServer(playType);
                    return false;
                }

                if (!serverInterfaces.CheckInterfaceFlags()) {
                    Log.NetworkDriverConstructor.NotInitializedServer(serverInterfaces);
                    return false;
                }

                return Initialize(
                    playType,
                    sockets,
                    clientInterface: SteamNetworkInterfaceType.None,
                    serverInterfaces,
                    sendFlags,
                    messagesPerReceive,
                    worlds);
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.InitializeServerEnd();
                }
            }
        }

        /// Sets SteamNetworkDriverConstructor as the DriverConstructor in
        /// Unity Netcode, so any new drivers created after this call
        /// use Steamworks as transport. You have to specify the Steamworks
        /// sockets pointer, the client interface type (P2P, IP, FakeIP),
        /// and the server interface types (P2P, IP, FakeIP). Other parameters
        /// are optional and include the Steamworks sockets send flags, the
        /// number of received messages processed at once, and a list of
        /// worlds in which existing drivers should be updated. Returns false
        /// if the driver constructor was not set. Returns true in any other
        /// case, including when updating any of the existing drivers fails.
        public static bool InitializeClientAndServer(
            IntPtr sockets,
            SteamNetworkInterfaceType clientInterface,
            SteamNetworkInterfaceTypeFlags serverInterfaces,
            SteamSendFlags sendFlags = Defaults.SendFlags,
            int messagesPerReceive = Defaults.MessagesPerReceive,
#if HAS_NETCODE_7_OR_NEWER_PACKAGE
            List<NetcodeWorld> worlds = null) {
#else
            List<World> worlds = null) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.InitializeClientAndServerStart();

                var playType = (PlayType)ClientServerBootstrap.RequestedPlayType;
                if (!playType.CheckPlayType(client: true, server: true)) {
                    Log.NetworkDriverConstructor.NotInitializedClientAndServer(playType);
                    return false;
                }

                if (!clientInterface.CheckInterfaceType()) {
                    Log.NetworkDriverConstructor.NotInitializedClient(_clientInterface);
                    return false;
                }

                if (!serverInterfaces.CheckInterfaceFlags()) {
                    Log.NetworkDriverConstructor.NotInitializedServer(serverInterfaces);
                    return false;
                }

                return Initialize(
                    playType,
                    sockets,
                    clientInterface,
                    serverInterfaces,
                    sendFlags,
                    messagesPerReceive,
                    worlds);
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.InitializeClientAndServerEnd();
                }
            }
        }

        static bool Initialize(
            PlayType playType,
            IntPtr sockets,
            SteamNetworkInterfaceType clientInterface,
            SteamNetworkInterfaceTypeFlags serverInterfaces,
            SteamSendFlags sendFlags = Defaults.SendFlags,
            int messagesPerReceive = Defaults.MessagesPerReceive,
#if HAS_NETCODE_7_OR_NEWER_PACKAGE
            List<NetcodeWorld> worlds = null) {
#else
            List<World> worlds = null) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.InitializeStart();

                if (sockets == IntPtr.Zero) {
                    Log.NetworkDriverConstructor.NotInitializedSockets();
                    return false;
                }

                if (!sendFlags.CheckSendFlags()) {
                    Log.NetworkDriverConstructor.NotInitializedSendFlags(sendFlags);
                    return false;
                }

                if (messagesPerReceive <= 0) {
                    Log.NetworkDriverConstructor.MessagesPerReceive(messagesPerReceive);
                    messagesPerReceive = Defaults.MessagesPerReceive;
                }

                _playType = playType;
                _sockets = sockets;
                _clientInterface = clientInterface;
                _serverInterfaces = serverInterfaces;
                _sendFlags = sendFlags;
                _messagesPerReceive = messagesPerReceive;

                // Set custom drivers
                NetworkStreamReceiveSystem.DriverConstructor = new SteamNetworkDriverConstructor();

                if (worlds is { Count: > 0 }) {
                    var initialized = false;
                    foreach (var world in worlds) {
                        if (!world.IsCreated)
                            continue;

                        var netDebug = world.EntityManager
                            .CreateEntityQuery(typeof(NetDebug))
                            .GetSingleton<NetDebug>();

                        var driverStore = new NetworkDriverStore();

#if HAS_NETCODE_7_OR_NEWER_PACKAGE
                        var isServer = world.IsServer() && _playType != PlayType.Client;
#else
                        var isServer = ClientServerWorldExtensions.IsServer(world) && _playType != PlayType.Client;
#endif
                        if (isServer) {
                            NetworkStreamReceiveSystem.DriverConstructor.CreateServerDriver(
                                world, ref driverStore, netDebug);
                        }

#if HAS_NETCODE_7_OR_NEWER_PACKAGE
                        if (world.IsClient() && _playType != PlayType.Server) {
#else
                        if (ClientServerWorldExtensions.IsClient(world) && _playType != PlayType.Server) {
#endif
                            NetworkStreamReceiveSystem.DriverConstructor.CreateClientDriver(
                                world, ref driverStore, netDebug);
                        }

                        var networkStreamDriver = world.EntityManager
                            .CreateEntityQuery(typeof(NetworkStreamDriver))
                            .GetSingleton<NetworkStreamDriver>();

                        networkStreamDriver.ResetDriverStore(world.Unmanaged, ref driverStore);

                        initialized |= driverStore.IsCreated;

                        Log.NetworkDriverConstructor.UpdateWorld(
                            _playType, isServer, world.Name, in driverStore, in netDebug);
                    }

                    if (!initialized)
                        Log.NetworkDriverConstructor.NoDriversCreated();
                }

                Log.NetworkDriverConstructor.Initialized();
                return true;
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.InitializeEnd();
                }
            }
        }

        /// Resets the DriverConstructor in Unity Netcode to DefaultDriverConstructor,
        /// so any new drivers created after this call use the default transport. You
        /// can specify a list of worlds in which existing drivers should be updated.
#if HAS_NETCODE_7_OR_NEWER_PACKAGE
        public static void Deinitialize(List<NetcodeWorld> worlds = null) {
#else
        public static void Deinitialize(List<World> worlds = null) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.DeinitializeStart();

                _playType = default;
                _sockets = default;
                _clientInterface = default;
                _serverInterfaces = default;
                _sendFlags = default;
                _messagesPerReceive = 0;

                // Reset the drivers
                NetworkStreamReceiveSystem.DriverConstructor = DefaultDriverBuilder.DefaultDriverConstructor;

                // Update the worlds
                if (worlds is { Count: > 0 }) {
                    var initialized = false;
                    foreach (var world in worlds) {
                        if (!world.IsCreated)
                            continue;

                        var netDebug = world.EntityManager
                            .CreateEntityQuery(typeof(NetDebug))
                            .GetSingleton<NetDebug>();

                        var driverStore = new NetworkDriverStore();

#if HAS_NETCODE_7_OR_NEWER_PACKAGE
                        var isServer = world.IsServer() && _playType != PlayType.Client;
#else
                        var isServer = ClientServerWorldExtensions.IsServer(world) && _playType != PlayType.Client;
#endif
                        if (isServer) {
                            NetworkStreamReceiveSystem.DriverConstructor.CreateServerDriver(
                                world, ref driverStore, netDebug);
                        }

#if HAS_NETCODE_7_OR_NEWER_PACKAGE
                        var isClient = world.IsClient() && _playType != PlayType.Server;
#else
                        var isClient = ClientServerWorldExtensions.IsClient(world) && _playType != PlayType.Server;
#endif
                        if (isClient) {
                            NetworkStreamReceiveSystem.DriverConstructor.CreateClientDriver(
                                world, ref driverStore, netDebug);
                        }

                        var networkStreamDriver = world.EntityManager
                            .CreateEntityQuery(typeof(NetworkStreamDriver))
                            .GetSingleton<NetworkStreamDriver>();

                        networkStreamDriver.ResetDriverStore(world.Unmanaged, ref driverStore);

                        initialized |= driverStore.IsCreated;

                        Log.NetworkDriverConstructor.UpdateWorld(
                            _playType, isServer, world.Name, in driverStore, in netDebug);
                    }

                    if (!initialized)
                        Log.NetworkDriverConstructor.NoDriversCreated();
                }

                Log.NetworkDriverConstructor.Deinitialized();
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.DeinitializeEnd();
                }
            }
        }

#if HAS_NETCODE_7_OR_NEWER_PACKAGE
        public void CreateClientDriver(NetcodeWorld world, ref NetworkDriverStore driver, NetDebug netDebug) {
#else
        public void CreateClientDriver(World world, ref NetworkDriverStore driver, NetDebug netDebug) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.CreateClientDriverStart(
                    _playType, world.Name, in driver, in netDebug);

                if (!_playType.CheckPlayType(client: true)) {
                    Log.NetworkDriverConstructor.NotInitializedClient(_playType);
                    return;
                }

                if (_sockets == IntPtr.Zero) {
                    Log.NetworkDriverConstructor.NotInitializedSockets();
                    return;
                }

                if (!_clientInterface.CheckInterfaceType()) {
                    Log.NetworkDriverConstructor.NotInitializedClient(_clientInterface);
                    return;
                }

                if (!_sendFlags.CheckSendFlags()) {
                    Log.NetworkDriverConstructor.NotInitializedSendFlags(_sendFlags);
                    return;
                }

                if (_messagesPerReceive <= 0) {
                    Log.NetworkDriverConstructor.MessagesPerReceive(_messagesPerReceive);
                    _messagesPerReceive = Defaults.MessagesPerReceive;
                }

                var settings = DefaultDriverBuilder.GetNetworkClientSettings();

                var clientDriverInstance = DefaultDriverBuilder.CreateClientNetworkDriver(
                    new SteamNetworkInterface(
                        in netDebug,
                        isServer: false,
                        _sockets,
                        _clientInterface,
                        _sendFlags,
                        _messagesPerReceive),
                    settings);

                driver.RegisterDriver(TransportType.Socket, clientDriverInstance);

                Log.NetworkDriverConstructor.CreateDriverResult(
                    _playType, isServer: false, world.Name, in driver, netDebug);
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.CreateClientDriverEnd(
                        _playType, world.Name, in driver, in netDebug);
                }
            }
        }

#if HAS_NETCODE_7_OR_NEWER_PACKAGE
        public void CreateServerDriver(NetcodeWorld world, ref NetworkDriverStore driver, NetDebug netDebug) {
#else
        public void CreateServerDriver(World world, ref NetworkDriverStore driver, NetDebug netDebug) {
#endif
            var succeeded = true;
            try {
                Trace.NetworkDriverConstructor.CreateServerDriverStart(
                    _playType, world.Name, in driver, in netDebug);

                if (!_playType.CheckPlayType(server: true)) {
                    Log.NetworkDriverConstructor.NotInitializedClient(_playType);
                    return;
                }

                if (_sockets == IntPtr.Zero) {
                    Log.NetworkDriverConstructor.NotInitializedSockets();
                    return;
                }

                if (!_serverInterfaces.CheckInterfaceFlags()) {
                    Log.NetworkDriverConstructor.NotInitializedServer(_serverInterfaces);
                    return;
                }

                if (_messagesPerReceive <= 0) {
                    Log.NetworkDriverConstructor.MessagesPerReceive(_messagesPerReceive);
                    _messagesPerReceive = Defaults.MessagesPerReceive;
                }

                var settings = DefaultDriverBuilder.GetNetworkServerSettings();

                if (_serverInterfaces.HasFlag(SteamNetworkInterfaceTypeFlags.P2P)) {
                    var p2PDriverInstance = DefaultDriverBuilder.CreateServerNetworkDriver(
                        new SteamNetworkInterface(
                            in netDebug,
                            isServer: true,
                            _sockets,
                            SteamNetworkInterfaceType.P2P,
                            _sendFlags,
                            _messagesPerReceive),
                        settings);

                    driver.RegisterDriver(TransportType.Socket, p2PDriverInstance);
                }

                if (_serverInterfaces.HasFlag(SteamNetworkInterfaceTypeFlags.IP)) {
                    var ipDriverInstance = DefaultDriverBuilder.CreateServerNetworkDriver(
                        new SteamNetworkInterface(
                            in netDebug,
                            isServer: true,
                            _sockets,
                            SteamNetworkInterfaceType.IP,
                            _sendFlags,
                            _messagesPerReceive),
                        settings);

                    driver.RegisterDriver(TransportType.Socket, ipDriverInstance);
                }

                if (_serverInterfaces.HasFlag(SteamNetworkInterfaceTypeFlags.FakeIP)) {
                    var fakeIPDriverInstance = DefaultDriverBuilder.CreateServerNetworkDriver(
                        new SteamNetworkInterface(
                            in netDebug,
                            isServer: true,
                            _sockets,
                            SteamNetworkInterfaceType.FakeIP,
                            _sendFlags,
                            _messagesPerReceive),
                        settings);

                    driver.RegisterDriver(TransportType.Socket, fakeIPDriverInstance);
                }

                Log.NetworkDriverConstructor.CreateDriverResult(
                    _playType, isServer: true, world.Name, in driver, netDebug);
            }
            catch {
                succeeded = false;
                throw;
            }
            finally {
                if (succeeded) { // Match Burst behavior where exceptions prevent the execution of `finally` blocks.
                    Trace.NetworkDriverConstructor.CreateServerDriverEnd(
                        _playType, world.Name, in driver, in netDebug);
                }
            }
        }
    }
}

#endif
#endif