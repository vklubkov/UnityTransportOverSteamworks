#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

using System;
using System.Runtime.InteropServices;

namespace TransportOverSteamworks {
    internal static partial class SteamAPI {
        [StructLayout(LayoutKind.Sequential)] // Default packing (should work fine unless Linux x86 is supported)
        internal unsafe struct SteamNetworkingMessage {
            public void* Data; // void *m_pData
            public int Size; // int m_cbSize
            public uint Connection; // HSteamNetConnection m_conn (uint32)

            // Fields below are not used by Steam Transport:
            public SteamNetworkingIdentity IdentityPeer; // SteamNetworkingIdentity m_identityPeer
            public long ConnectionUserData; // int64 m_nConnUserData
            public long TimeReceivedMicroseconds; // SteamNetworkingMicroseconds m_usecTimeReceived (int64)
            public long MessageNumber; // int64 m_nMessageNumber
            public IntPtr OnFreeData; // void (*m_pfnFreeData)( SteamNetworkingMessage_t *pMsg )
            public IntPtr OnRelease; // void (*m_pfnRelease)( SteamNetworkingMessage_t *pMsg )
            public int Channel; // int m_nChannel
            public int Flags; // int m_nFlags
            public long UserData; // int64 m_nUserData
            public ushort LaneIndex; // uint16 m_idxLane
            public ushort Padding; // uint16 _pad1__

#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
            public static void Release(IntPtr @this) =>
                SteamworksDll.Functions.SteamAPI_SteamNetworkingMessage_t_Release(@this);
#else
            [DllImport(_nativeLib,
                EntryPoint = "SteamAPI_SteamNetworkingMessage_t_Release",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void Release(IntPtr @this); // SteamNetworkingMessage*
#endif
        }
    }
}

#endif