#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
using System;
using System.Runtime.InteropServices;
#endif

namespace TransportOverSteamworks {
    internal static partial class SteamAPI {
        internal static unsafe class SteamGameServer {
#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static int GetHSteamUser() => SteamworksDll.Functions.SteamGameServer_GetHSteamUser();
#else
            [DllImport(_nativeLib,
                EntryPoint = "SteamGameServer_GetHSteamUser",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern int // HSteamUser (int)
                // ReSharper disable once MemberHidesStaticFromOuterClass
                GetHSteamUser();
#endif
        }
    }
}

#endif