using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.BuildPipeline;

namespace FuckOscConfig
{
    public class FuckOscConfigProcessor : IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder { get; }

        public void OnPostprocessAvatar()
        {
            var avatarId = EditorPrefs.GetString("lastBuiltAssetBundleBlueprintID");
            TryDeleteOscConfigFile(avatarId);
        }
        
        [MenuItem("Tools/Remove OSC config file")]
        private static void RemoveOSCConfig()
        {
            if (!APIUser.IsLoggedIn)
            {
                Debug.LogError("(FTOCF) Cannot remove OSC config file, you are not logged in. User ID is required for removal");
                return;
            }

            var activeObject = Selection.activeGameObject;
            if (activeObject == null) return;

            var pipeline = activeObject.transform.GetComponentInParent<PipelineManager>();
            if (pipeline == null) return;
            
            Debug.Log($"(FTOCF) Trying to delete OSC config file of {pipeline.blueprintId}");
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

            var theFuckingOscConfigFile =
                $"{VRC_SdkBuilder.GetLocalLowPath()}/VRChat/VRChat/OSC/{userId}/Avatars/{avatarId}.json";
            if (!File.Exists(theFuckingOscConfigFile)) return;

            var fileAttributes = File.GetAttributes(theFuckingOscConfigFile);
            if (fileAttributes.HasFlag(FileAttributes.Directory)) return;

            try
            {
                File.Delete(theFuckingOscConfigFile);
                Debug.Log($"(FTOCF) Obliterated the OSC config file located at {theFuckingOscConfigFile}");
            }
            catch (Exception e)
            {
                Debug.LogError($"(FTOCF) Attempt to delete the OSC config file has failed (at {theFuckingOscConfigFile})");
                throw;
            }
        }

        private static bool ContainsPathTraversalElements(string susStr)
        {
            return susStr.Contains("/") || susStr.Contains("\\") || susStr.Contains(".") || susStr.Contains("*");
        }
    }
}