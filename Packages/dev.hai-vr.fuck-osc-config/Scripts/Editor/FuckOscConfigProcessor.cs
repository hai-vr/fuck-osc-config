using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;

namespace FuckOscConfig
{
    public class FuckOscConfigProcessor
    {
		[InitializeOnLoadMethod]
		public static void RegisterCallback()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += OnSdkPanelEnable;
        }

        private static void OnSdkPanelEnable(object sender, EventArgs args)
        {
            if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
            {
                builder.OnSdkUploadSuccess += OnSdkUploadSuccess;
            }
        }

        private static void OnSdkUploadSuccess(object sender, string avatarId)
        {
            Debug.Log($"(AutoRemoveOscConfig) Upload success, will try to remove OSC config...");
            TryDeleteOscConfigFile(avatarId);
        }
        
        [MenuItem("Tools/Remove OSC config file")]
        private static void RemoveOSCConfig()
        {
            if (!APIUser.IsLoggedIn)
            {
                Debug.LogError("(AutoRemoveOscConfig) Cannot remove OSC config file, you are not logged in. User ID is required for removal");
                return;
            }

            var activeObject = Selection.activeGameObject;
            if (activeObject == null) return;

            var pipeline = activeObject.transform.GetComponentInParent<PipelineManager>();
            if (pipeline == null) return;
            
            Debug.Log($"(AutoRemoveOscConfig) Trying to delete OSC config file of {pipeline.blueprintId}");
            TryDeleteOscConfigFile(pipeline.blueprintId);
        }

        private static void TryDeleteOscConfigFile(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return;
            if (!APIUser.IsLoggedIn) return;

            var userId = APIUser.CurrentUser.id;
            if (ContainsPathTraversalElements(userId) || ContainsPathTraversalElements(avatarId))
            {
                // Prevent the remote chance of a path traversal
                return;
            }

            var endbit = $"/VRChat/VRChat/OSC/{userId}/Avatars/{avatarId}.json";
            var theFuckingOscConfigFile = $"{VRC_SdkBuilder.GetLocalLowPath()}{endbit}";
            var printLocation = $"%LOCALAPPDATA%Low{endbit}"; // Doesn't print the account name to the logs
            if (!File.Exists(theFuckingOscConfigFile)) return;

            var fileAttributes = File.GetAttributes(theFuckingOscConfigFile);
            if (fileAttributes.HasFlag(FileAttributes.Directory)) return;

            try
            {
                File.Delete(theFuckingOscConfigFile);
                Debug.Log($"(AutoRemoveOscConfig) Removed the OSC config file located at {printLocation}");
            }
            catch (Exception e)
            {
                Debug.LogError($"(AutoRemoveOscConfig) Failed to removed the OSC config file at {printLocation}");
                throw;
            }
        }

        private static bool ContainsPathTraversalElements(string susStr)
        {
            return susStr.Contains("/") || susStr.Contains("\\") || susStr.Contains(".") || susStr.Contains("*");
        }
    }
}