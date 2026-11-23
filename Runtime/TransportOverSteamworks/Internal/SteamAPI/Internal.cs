#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

using System;

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
using System.Runtime.InteropServices;
#endif

namespace TransportOverSteamworks {
    internal static partial class SteamAPI {
        internal static unsafe class Internal {
#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
            public static IntPtr FindOrCreateUserInterface(int user, void* version) =>
                SteamworksDll.Functions.SteamInternal_FindOrCreateUserInterface(user, version);

            public static IntPtr FindOrCreateGameServerInterface(int steamUser, void* version) =>
                SteamworksDll.Functions.SteamInternal_FindOrCreateGameServerInterface(steamUser, version);
#else
            [DllImport(_nativeLib,
                EntryPoint = "SteamInternal_FindOrCreateUserInterface",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr // void*
                FindOrCreateUserInterface(
                    int user, // HSteamUser hSteamUser (int)
                    void* version); // const char *pszVersion

            [DllImport(_nativeLib,
                EntryPoint = "SteamInternal_FindOrCreateGameServerInterface",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr // void*
                FindOrCreateGameServerInterface(
                    int steamUser, // HSteamUser hSteamUser (int)
                    void* version); // const char *pszVersion
#endif
        }
    }
}

#endif