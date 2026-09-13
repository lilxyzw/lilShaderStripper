#if LIL_BASIS_SDK
using UnityEditor;

namespace jp.lilxyzw.shaderstripper
{
    public static class BasisHook
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            //BasisAssetBundlePipeline.OnBeforeBuildPrefab += (_,_) => ShaderStripper.Reset(); // NDMF
            BasisAssetBundlePipeline.OnBeforeBuildScene += (_,_) => ShaderStripper.Reset();
            BasisAssetBundlePipeline.OnAfterBuildPrefab += (_) => ShaderStripper.Reset();
            BasisAssetBundlePipeline.OnAfterBuildScene += (_) => ShaderStripper.Reset();
        }
    }
}
#endif
