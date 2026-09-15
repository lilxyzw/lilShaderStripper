#if LIL_BASIS_SDK
using UnityEditor;

namespace jp.lilxyzw.shaderstripper
{
    public static class BasisHook
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            #if !LIL_NDMF
            BasisAssetBundlePipeline.OnBeforeBuildPrefab += (prefab,_) => GameObjectScanner.Scan(prefab);
            #endif
            BasisAssetBundlePipeline.OnBeforeBuildScene += (_,_) => ShaderStripper.Reset();
            BasisAssetBundlePipeline.OnAfterBuildPrefab += (_) => ShaderStripper.Reset();
            BasisAssetBundlePipeline.OnAfterBuildScene += (_) => ShaderStripper.Reset();
        }
    }
}
#endif
