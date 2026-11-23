#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

using Unity.Collections;

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
using System;
using System.Runtime.InteropServices;
#endif

namespace TransportOverSteamworks {
    internal static partial class SteamAPI {
        // STEAMNETWORKINGSOCKETS_INTERFACE_VERSION
        public static readonly FixedString64Bytes SocketsVersion = "SteamNetworkingSockets013";

        // ReSharper disable once InconsistentNaming
        internal static unsafe class ISteamNetworkingSockets {
#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
            public static int SendMessageToConnection(
                void* @this, uint connection, byte* data, uint dataSize, int sendFlags, long* messageNumber) =>
                    SteamworksDll.Functions.SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(
                        @this, connection, data, dataSize, sendFlags, messageNumber);

            public static int ReceiveMessagesOnConnection(
                void* @this, uint connection, SteamNetworkingMessage** messages, int maxMessages) =>
                    SteamworksDll.Functions.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection(
                        @this, connection, messages, maxMessages);

            public static int ReceiveMessagesOnPollGroup(
                void* @this, uint pollGroup, SteamNetworkingMessage** messages, int maxMessages) =>
                    SteamworksDll.Functions.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup(
                        @this, pollGroup, messages, maxMessages);
#else
            [DllImport(_nativeLib,
                EntryPoint = "SteamAPI_ISteamNetworkingSockets_SendMessageToConnection",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern int // EResult (enum)
                SendMessageToConnection(
                    void* @this, // ISteamNetworkingSockets*
                    uint connection, // HSteamNetConnection hConn (uint32)
                    byte* data, // const void *pData
                    uint dataSize, // uint32 cbData
                    int sendFlags, // int nSendFlags
                    long* messageNumber); // int64 *pOutMessageNumber

            [DllImport(_nativeLib,
                EntryPoint = "SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern int // int
                ReceiveMessagesOnConnection(
                    void* @this, // ISteamNetworkingSockets*
                    uint connection, // HSteamNetConnection hConn (uint32)
                    SteamNetworkingMessage** messages, // SteamNetworkingMessage_t **ppOutMessages
                    int maxMessages); // int nMaxMessages

            [DllImport(_nativeLib,
                EntryPoint = "SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern int // int
                ReceiveMessagesOnPollGroup(
                    void* @this, // ISteamNetworkingSockets*
                    uint pollGroup, // HSteamNetPollGroup hPollGroup (uint32)
                    SteamNetworkingMessage** messages, // SteamNetworkingMessage_t **ppOutMessages
                    int maxMessages); // int nMaxMessages
#endif
        }
    }
}

#endif