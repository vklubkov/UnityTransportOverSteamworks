#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

using System.Runtime.InteropServices;

namespace TransportOverSteamworks {
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct NetworkEndpointConnection {
        [FieldOffset(0)] public uint Connection;
        [FieldOffset(4)] public uint PollGroup;
        [FieldOffset(8)] public byte InterfaceType;
    }
}

#endif