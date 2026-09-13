using System.Collections.Generic;
using jp.lilxyzw.shaderstripper;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(ShaderStripperPlugin))]

namespace jp.lilxyzw.shaderstripper
{
    [RunsOnAllPlatforms]
    internal class ShaderStripperPlugin : Plugin<ShaderStripperPlugin>
    {
        public override string QualifiedName => "jp.lilxyzw.shaderstripper";
        public override string DisplayName => "lilShaderStripper";

        protected override void Configure()
        {
            var PlatformFinishing = InPhase(BuildPhase.PlatformFinish);
            PlatformFinishing.Run("lilShaderStripper", ctx =>
            {
                // Local keywords will likely be automatically stripped.
                //var scanned = new HashSet<Object>();
                //foreach (var component in ctx.AvatarRootObject.GetComponentsInChildren<Component>(true))
                //{
                //    Scan(component, scanned);
                //}
                ShaderStripper.strip_LOD_FADE_CROSSFADE = !ctx.AvatarRootObject.GetComponentInChildren<LODGroup>(true);
                ShaderStripper.strip_LightMapVariants = true;
            });
        }

        private static void Scan(Object obj, HashSet<Object> scanned)
        {
            if (!obj) return;
            if (!scanned.Add(obj)) return;
            if (obj is GameObject ||
                // Skip - Component
                obj is Transform ||
                // Skip - Asset
                obj is Mesh ||
                obj is Texture ||
                obj is Shader ||
                obj is TextAsset ||
                obj.GetType() == typeof(Object)
            ) return;

            using var so = new SerializedObject(obj);
            using var iter = so.GetIterator();
            var enterChildren = true;
            while (iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if (iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue && iter.name != "m_CorrespondingSourceObject")
                {
                    Scan(iter.objectReferenceValue, scanned);
                }
            }
        }
    }
}
