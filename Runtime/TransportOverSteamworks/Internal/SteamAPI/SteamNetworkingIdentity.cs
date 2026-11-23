#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

using System.Runtime.InteropServices;

namespace TransportOverSteamworks {
    internal static partial class SteamAPI {
        [StructLayout(LayoutKind.Sequential, Pack = 1)] // #pragma pack(push,1)
        internal unsafe struct SteamNetworkingIdentity {
            public int Type; // ESteamNetworkingIdentityType m_eType (enum)
            public int Size; // int m_cbSize
            public fixed byte Data[128]; // a union with max size of 128 chars
        }
    }
}

#endif