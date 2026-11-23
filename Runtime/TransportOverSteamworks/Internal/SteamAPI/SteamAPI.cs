#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
using System.Runtime.InteropServices;
#endif

namespace TransportOverSteamworks {
    internal static unsafe partial class SteamAPI {
        // C/C++ int is 32 bits on most modern systems
        // (and on all systems supported by Steam),
        // so a C# int is used to handle ints.

        // An enum in C/C++ can be any integral type,
        // but by default it is 32 bits or smaller
        // if the larger member fits into the data type,
        // so a C# int is used to handle enums.
#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
        public static int GetHSteamUser() => SteamworksDll.Functions.SteamAPI_GetHSteamUser();
#else
        [DllImport(_nativeLib,
            EntryPoint = "SteamAPI_GetHSteamUser",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int // HSteamUser (int32)
            GetHSteamUser();
#endif
    }
}

#endif
