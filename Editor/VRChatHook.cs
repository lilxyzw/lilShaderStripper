#if LIL_VRC_SDK
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.lilxyzw.shaderstripper
{
    public class VRChatHook : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 100000;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType == VRCSDKRequestedBuildType.Scene) ShaderStripper.Reset();
            return true;
        }
    }

    #if !LIL_NDMF
    public class VRChatAvatarHook : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => 100000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            GameObjectScanner.Scan(avatarGameObject);
            return true;
        }

        public void OnPostprocessAvatar()
        {
            ShaderStripper.Reset();
        }
    }
    #endif
}
#endif
