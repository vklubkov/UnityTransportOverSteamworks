#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Networking.Transport;

namespace TransportOverSteamworks {
    [PublicAPI]
    public static unsafe class SteamNetworkEndpoint {
        /// Creates a new NetworkEndpoint containing the provided interface
        /// type, connection and poll group. The NetworkEndpoint.Family is
        /// set to NetworkFamily.Custom. No parameter validation is performed.
        /// Can be called from bursted code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateForConnection(
            out NetworkEndpoint endpoint,
            SteamNetworkInterfaceType type = SteamNetworkInterfaceType.None,
            uint connection = 0, uint pollGroup = 0) {
            endpoint = default;
            endpoint.Family = NetworkFamily.Custom;
            var dataPtr = (NetworkEndpointConnection*)endpoint.GetRawDataPtrRef();
            dataPtr->Connection = connection;
            dataPtr->PollGroup = pollGroup;
            dataPtr->InterfaceType = (byte)type;
        }

        /// Creates a new NetworkEndpoint containing the provided interface
        /// types, listen sockets and poll groups. The NetworkEndpoint.Family is
        /// set to NetworkFamily.Custom. No parameter validation is performed.
        /// Can be called from bursted code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateForListenSockets(
            out NetworkEndpoint endpoint,
            SteamNetworkInterfaceTypeFlags types = SteamNetworkInterfaceTypeFlags.None,
            uint listenSocketP2P = 0, uint pollGroupP2P = 0,
            uint listenSocketIP = 0, uint pollGroupIP = 0,
            uint listenSocketFakeIP = 0, uint pollGroupFakeIP = 0) {
            endpoint = default;
            endpoint.Family = NetworkFamily.Custom;
            var dataPtr = (NetworkEndpointListenSockets*)endpoint.GetRawDataPtrRef();
            dataPtr->ListenSocketP2P = listenSocketP2P;
            dataPtr->PollGroupP2P = pollGroupP2P;
            dataPtr->ListenSocketIP = listenSocketIP;
            dataPtr->PollGroupIP = pollGroupIP;
            dataPtr->ListenSocketFakeIP = listenSocketFakeIP;
            dataPtr->PollGroupFakeIP = pollGroupFakeIP;
            dataPtr->InterfaceTypes = (byte)types;
        }

        /// Fills the specified NetworkEndpoint with the provided interface
        /// type, connection and poll group. The NetworkEndpoint.Family is
        /// set to NetworkFamily.Custom. No parameter validation is performed.
        /// Can be called from bursted code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSteamConnection(
            this ref NetworkEndpoint endpoint,
            SteamNetworkInterfaceType type = SteamNetworkInterfaceType.None,
            uint connection = 0, uint pollGroup = 0) {
            endpoint = default;
            endpoint.Family = NetworkFamily.Custom;
            var dataPtr = (NetworkEndpointConnection*)endpoint.GetRawDataPtrRef();
            dataPtr->Connection = connection;
            dataPtr->PollGroup = pollGroup;
            dataPtr->InterfaceType = (byte)type;
        }

        /// Fills the specified NetworkEndpoint with the provided interface
        /// types, listen sockets and poll groups. The NetworkEndpoint.Family is
        /// set to NetworkFamily.Custom. No parameter validation is performed.
        /// Can be called from bursted code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSteamListenSockets(
            this ref NetworkEndpoint endpoint,
            SteamNetworkInterfaceTypeFlags types = SteamNetworkInterfaceTypeFlags.None,
            uint listenSocketP2P = 0, uint pollGroupP2P = 0,
            uint listenSocketIP = 0, uint pollGroupIP = 0,
            uint listenSocketFakeIP = 0, uint pollGroupFakeIP = 0) {
            endpoint = default;
            endpoint.Family = NetworkFamily.Custom;
            var dataPtr = (NetworkEndpointListenSockets*)endpoint.GetRawDataPtrRef();
            dataPtr->ListenSocketP2P = listenSocketP2P;
            dataPtr->PollGroupP2P = pollGroupP2P;
            dataPtr->ListenSocketIP = listenSocketIP;
            dataPtr->PollGroupIP = pollGroupIP;
            dataPtr->ListenSocketFakeIP = listenSocketFakeIP;
            dataPtr->PollGroupFakeIP = pollGroupFakeIP;
            dataPtr->InterfaceTypes = (byte)types;
        }

        /// Extracts the interface type, connection, and poll
        /// group from the specified NetworkEndpoint. It is on
        /// the caller to ensure that NetworkEndpoint contains the
        /// appropriate connection data before calling this method.
        /// Can be called from bursted code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetSteamConnection(
            this ref NetworkEndpoint endpoint,
            out SteamNetworkInterfaceType type,
            out uint connection, out uint pollGroup) {
            var dataPtr = (NetworkEndpointConnection*)endpoint.GetRawDataPtrRef();
            connection = dataPtr->Connection;
            pollGroup = dataPtr->PollGroup;
            type = (SteamNetworkInterfaceType)dataPtr->InterfaceType;
        }

        /// Extracts the interface types, listen sockets, and poll
        /// groups from the specified NetworkEndpoint. It is on
        /// the caller to ensure that NetworkEndpoint contains the
        /// appropriate connection data before calling this method.
        /// Can be called from bursted code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetSteamListenSockets(
            this in NetworkEndpoint endpoint,
            out SteamNetworkInterfaceTypeFlags types,
            out uint listenSocketP2P, out uint pollGroupP2P,
            out uint listenSocketIP, out uint pollGroupIP,
            out uint listenSocketFakeIP, out uint pollGroupFakeIP) {
            var dataPtr = (NetworkEndpointListenSockets*)endpoint.GetRawDataPtrIn();
            listenSocketP2P = dataPtr->ListenSocketP2P;
            pollGroupP2P = dataPtr->PollGroupP2P;
            listenSocketIP = dataPtr->ListenSocketIP;
            pollGroupIP = dataPtr->PollGroupIP;
            listenSocketFakeIP = dataPtr->ListenSocketFakeIP;
            pollGroupFakeIP = dataPtr->PollGroupFakeIP;
            types = (SteamNetworkInterfaceTypeFlags)dataPtr->InterfaceTypes;
        }
    }
}

#endif