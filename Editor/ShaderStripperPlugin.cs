#if LIL_NDMF
using jp.lilxyzw.shaderstripper;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(ShaderStripperPlugin))]

namespace jp.lilxyzw.shaderstripper
{
    [RunsOnAllPlatforms]
    internal class ShaderStripperPlugin : Plugin<ShaderStripperPlugin>
    {
        public override string QualifiedName => "jp.lilxyzw.shaderstripper";
        public override string DisplayName => "lilShaderStripper";
        protected override void Configure() =>
            InPhase(BuildPhase.PlatformFinish).Run("lilShaderStripper", ctx => GameObjectScanner.Scan(ctx.AvatarRootObject));
    }
}
#endif
