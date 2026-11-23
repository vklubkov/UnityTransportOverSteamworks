#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || UNITY_ANDROID || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE
#endif

#if !TRANSPORT_OVER_STEAMWORKS_INTERNAL_DISABLE

#if !TRANSPORT_OVER_STEAMWORKS_DISABLE_LOGGING
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_ENABLE_LOGGING
#endif

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_6000_6_OR_NEWER
#define TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
#endif

#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
using System;
using System.Runtime.InteropServices;
using TransportOverSteamworks.Debugging;
using Unity.Burst;
#if UNITY_EDITOR
using System.IO;
using static System.StringComparison;
#endif
#endif

namespace TransportOverSteamworks {
    internal static unsafe partial class SteamAPI {
#pragma warning disable UAC0008
#if TRANSPORT_OVER_STEAMWORKS_INTERNAL_LOAD_STEAMWORKS_DLL_IN_RUNTIME
        // "UAC0008: The UNITY_64 scripting symbol has been
        // removed and is no longer defined, so code guarded
        // by it won't behave as intended. Please use either
        // a runtime check (for example, a condition based on
        // IntPtr.Size) or bitness independent code instead."

        struct NativeFunctions {
            // ReSharper disable InconsistentNaming
            public delegate* unmanaged[Cdecl]<
                int> // HSteamUser (int32)
                SteamAPI_GetHSteamUser;

            public delegate* unmanaged[Cdecl]<
                int> // HSteamUser (int32)
                SteamGameServer_GetHSteamUser;

            public delegate* unmanaged[Cdecl]<
                IntPtr, // SteamNetworkingMessage* self
                void> // void
                SteamAPI_SteamNetworkingMessage_t_Release;

            public delegate* unmanaged[Cdecl]<
                int, // HSteamUser hSteamUser (int32)
                void*, // const char *pszVersion
                IntPtr> // void*
                SteamInternal_FindOrCreateUserInterface;

            public delegate* unmanaged[Cdecl]<
                int, // HSteamUser hSteamUser (int32)
                void*, // const char *pszVersion
                IntPtr> // void*
                SteamInternal_FindOrCreateGameServerInterface;

            public delegate* unmanaged[Cdecl]<
                void*, // ISteamNetworkingSockets* self
                uint, // HSteamNetConnection hConn (uint32)
                byte*, // const void *pData
                uint, // uint32 cbData
                int, // int nSendFlags
                long*, // int64 *pOutMessageNumber
                int> // EResult (enum)
                SteamAPI_ISteamNetworkingSockets_SendMessageToConnection;

            public delegate* unmanaged[Cdecl]<
                void*, // ISteamNetworkingSockets* self
                uint, // HSteamNetConnection hConn (uint32)
                SteamNetworkingMessage**, // SteamNetworkingMessage_t **ppOutMessages
                int, // int nMaxMessages
                int> // int
                SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection;

            public delegate* unmanaged[Cdecl]<
                void*, // ISteamNetworkingSockets* self
                uint, // HSteamNetPollGroup hPollGroup (uint32)
                SteamNetworkingMessage**, // SteamNetworkingMessage_t **ppOutMessages
                int, // int nMaxMessages
                int> // int
                SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup;
            // ReSharper restore InconsistentNaming
        }

        static class SteamworksDll {
            struct TableKey { } // Unique type for SharedStatic

            const string _nativeLib = "kernel32";

            static readonly SharedStatic<NativeFunctions> _table =
                SharedStatic<NativeFunctions>.GetOrCreate<TableKey>();

            public static ref readonly NativeFunctions Functions => ref _table.Data;

            [DllImport(_nativeLib, CharSet = CharSet.Unicode)]
            static extern IntPtr GetModuleHandleW(string lpModuleName);

            [DllImport(_nativeLib, CharSet = CharSet.Unicode, SetLastError = true)]
            static extern IntPtr LoadLibraryW(string lpLibFileName);

            [DllImport(_nativeLib, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            public static void LoadIfNeeded() {
                var dllName = IntPtr.Size == 8 ? "steam_api64.dll" : "steam_api.dll";
                var targetPath = dllName;

                var handle = GetModuleHandleW(dllName); // Maybe it is already loaded?
                if (handle == IntPtr.Zero)
                    handle = LoadLibraryW(dllName); // This should work in standalone builds

#if UNITY_EDITOR
                if (handle == IntPtr.Zero) {
                    // This should work in Editor
                    targetPath = FindPluginPathInEditor(dllName);
                    if (!string.IsNullOrEmpty(targetPath))
                        handle = LoadLibraryW(targetPath);
                }
#endif

                if (handle == IntPtr.Zero) {
                    _table.Data = default;
                    return;
                }

                if (handle == IntPtr.Zero) {
                    Log.SteamworksDll.DllLoadingFailure(targetPath);
                    _table.Data = default;
                    return;
                }

                NativeFunctions functions = default;

                functions.SteamAPI_GetHSteamUser =
                    (delegate* unmanaged[Cdecl]<int>)LoadSymbol(
                        handle, dllName, "SteamAPI_GetHSteamUser");

                functions.SteamGameServer_GetHSteamUser =
                    (delegate* unmanaged[Cdecl]<int>)LoadSymbol(
                        handle, dllName, "SteamGameServer_GetHSteamUser");

                functions.SteamAPI_SteamNetworkingMessage_t_Release =
                    (delegate* unmanaged[Cdecl]<IntPtr, void>)LoadSymbol(
                        handle, dllName, "SteamAPI_SteamNetworkingMessage_t_Release");

                functions.SteamInternal_FindOrCreateUserInterface =
                    (delegate* unmanaged[Cdecl]<int, void*, IntPtr>)LoadSymbol(
                        handle, dllName, "SteamInternal_FindOrCreateUserInterface");

                functions.SteamInternal_FindOrCreateGameServerInterface =
                    (delegate* unmanaged[Cdecl]<int, void*, IntPtr>)LoadSymbol(
                        handle, dllName, "SteamInternal_FindOrCreateGameServerInterface");

                functions.SteamAPI_ISteamNetworkingSockets_SendMessageToConnection =
                    (delegate* unmanaged[Cdecl]<void*, uint, byte*, uint, int, long*, int>)LoadSymbol(
                        handle, dllName, "SteamAPI_ISteamNetworkingSockets_SendMessageToConnection");

                functions.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection =
                    (delegate* unmanaged[Cdecl]<void*, uint, SteamNetworkingMessage**, int, int>)LoadSymbol(
                        handle, dllName, "SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection");

                functions.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup =
                    (delegate* unmanaged[Cdecl]<void*, uint, SteamNetworkingMessage**, int, int>)LoadSymbol(
                        handle, dllName, "SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup");

                _table.Data = functions;
            }

            static IntPtr LoadSymbol(IntPtr moduleHandle, string dllName, string symbolName) {
                var ptr = GetProcAddress(moduleHandle, symbolName);
                if (ptr == IntPtr.Zero)
                    Log.SteamworksDll.SymbolLoadingFailure(dllName, symbolName);

                return ptr;
            }

#if UNITY_EDITOR
            static string FindPluginPathInEditor(string dllName) {
                foreach (var importer in UnityEditor.PluginImporter.GetAllImporters()) {
                    if (importer.isNativePlugin && importer.assetPath.EndsWith(dllName, OrdinalIgnoreCase))
                        return Path.GetFullPath(importer.assetPath);
                }

                return null;
            }
#endif
        }

        public static void LoadDllIfNeeded() => SteamworksDll.LoadIfNeeded();
#else

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && UNITY_64
        const string _nativeLib = "steam_api64";
#else
        const string _nativeLib = "steam_api";
#endif

#endif
#pragma warning restore UAC0008
    }
}

#endif
