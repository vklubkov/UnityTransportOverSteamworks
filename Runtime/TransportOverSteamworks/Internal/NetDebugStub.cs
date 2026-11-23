#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#if !HAS_NETCODE_PACKAGE

namespace TransportOverSteamworks {
    // NetDebug stub for when Unity Netcode is not available.
    internal struct NetDebug { }
}

#endif
#endif