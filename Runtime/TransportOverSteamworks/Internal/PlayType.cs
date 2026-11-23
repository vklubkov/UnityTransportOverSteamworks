#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

namespace TransportOverSteamworks {
    internal enum PlayType {
        ClientAndServer = 0,
        Client = 1,
        Server = 2,
    }
}

#endif