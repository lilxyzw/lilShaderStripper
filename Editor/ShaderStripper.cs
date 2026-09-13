#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace jp.lilxyzw.shaderstripper
{
    public class ShaderStripper : IPreprocessShaders
    {
        /// <summary>
        /// Set this to true if you want to force stripping.
        /// </summary>
        public static bool strip_LOD_FADE_CROSSFADE
        {
            get => Settings.instance.strip_LOD_FADE_CROSSFADE;
            set => Settings.instance.strip_LOD_FADE_CROSSFADE = value;
        }

        public static bool strip_LightMapVariants
        {
            get => Settings.instance.strip_LightMapVariants;
            set => Settings.instance.strip_LightMapVariants = value;
        }

        public static void Reset()
        {
            strip_LOD_FADE_CROSSFADE = false;
            strip_LightMapVariants = false;
        }

        /// <summary>
        /// Add any global keywords here that should not be stripped.
        /// </summary>
        public static HashSet<string> customKeywords = new();

        public int callbackOrder => 0;
        private readonly ShaderKeyword m_SHADOWS_DEPTH;
        private readonly ShaderKeyword m_SHADOWS_CUBE;
        private readonly ShaderKeyword m_LOD_FADE_CROSSFADE;

        private readonly ShaderKeyword m_LIGHTMAP_SHADOW_MIXING;
        private readonly ShaderKeyword m_SHADOWS_SHADOWMASK;
        private readonly ShaderKeyword m_DIRLIGHTMAP_COMBINED;
        private readonly ShaderKeyword m_LIGHTMAP_ON;
        private readonly ShaderKeyword m_LIGHTMAP_BICUBIC_SAMPLING;
        private readonly ShaderKeyword m_DYNAMICLIGHTMAP_ON;
        private readonly ShaderKeyword m_USE_LEGACY_LIGHTMAPS;

        public ShaderStripper()
        {
            m_SHADOWS_DEPTH = new("SHADOWS_DEPTH");
            m_SHADOWS_CUBE = new("SHADOWS_CUBE");
            m_LOD_FADE_CROSSFADE = new("LOD_FADE_CROSSFADE");

            m_LIGHTMAP_SHADOW_MIXING = new("LIGHTMAP_SHADOW_MIXING");
            m_SHADOWS_SHADOWMASK = new("SHADOWS_SHADOWMASK");
            m_DIRLIGHTMAP_COMBINED = new("DIRLIGHTMAP_COMBINED");
            m_LIGHTMAP_ON = new("LIGHTMAP_ON");
            m_LIGHTMAP_BICUBIC_SAMPLING = new("LIGHTMAP_BICUBIC_SAMPLING");
            m_DYNAMICLIGHTMAP_ON = new("DYNAMICLIGHTMAP_ON");
            m_USE_LEGACY_LIGHTMAPS = new("USE_LEGACY_LIGHTMAPS");
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            // Do not strip built-in shaders
            if (AssetDatabase.GetAssetPath(shader).StartsWith("Packages/com.unity.render-pipelines.universal")) return;

            // BiRP
            if (!GraphicsSettings.currentRenderPipeline)
            {
                // Strip URP SubShader
                switch (snippet.passType)
                {
                    case PassType.Meta:
                    case PassType.ScriptableRenderPipeline:
                        data.Clear();
                        break;
                    case PassType.ShadowCaster:
                        ExclusionStrip(data, m_SHADOWS_DEPTH, m_SHADOWS_CUBE);
                        break;
                }
            }

            // URP
            else
            {
                // Strip BiRP SubShader
                switch (snippet.passType)
                {
                    case PassType.ForwardBase:
                    case PassType.ForwardAdd:
                    case PassType.Meta:
                        data.Clear();
                        break;
                    case PassType.ShadowCaster:
                        Strip(data, true, m_SHADOWS_DEPTH, m_SHADOWS_CUBE);
                        break;
                }

                // Strip invalid global keywords
                // This does not work correctly with shaders that use global shader keywords inappropriately.
                var litKeywords = Shader.Find("Universal Render Pipeline/Lit").keywordSpace.keywords
                    .Where(k => k.isOverridable)
                    .Select(k => k.name)
                    .ToHashSet();
                var invalidKeywords = shader.keywordSpace.keywords
                    .Where(k => k.isOverridable && !litKeywords.Contains(k.name) && !customKeywords.Contains(k.name))
                    .Select(k => new ShaderKeyword(k.name))
                    .ToArray();
                Strip(data, false, invalidKeywords);
            }

            // Strip lightmap
            if (strip_LightMapVariants)
            {
                Strip(data, false,
                    m_LIGHTMAP_SHADOW_MIXING,
                    m_SHADOWS_SHADOWMASK,
                    m_DIRLIGHTMAP_COMBINED,
                    m_LIGHTMAP_ON,
                    m_LIGHTMAP_BICUBIC_SAMPLING,
                    m_DYNAMICLIGHTMAP_ON,
                    m_USE_LEGACY_LIGHTMAPS
                );
            }

            // Strip LOD_FADE_CROSSFADE
            if (strip_LOD_FADE_CROSSFADE || !Object.FindAnyObjectByType<LODGroup>(FindObjectsInactive.Include))
                Strip(data, false, m_LOD_FADE_CROSSFADE);
        }

        private static void Strip(IList<ShaderCompilerData> data, bool stripAll, params ShaderKeyword[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (!stripAll && data.All(d => d.shaderKeywordSet.IsEnabled(keyword))) continue;
                for (int i = data.Count - 1; i >= 0; i--)
                {
                    if (data[i].shaderKeywordSet.IsEnabled(keyword))
                        data.RemoveAt(i);
                }
            }
        }

        private static void ExclusionStrip(IList<ShaderCompilerData> data, params ShaderKeyword[] keywords)
        {
            for (int i = data.Count - 1; i >= 0; i--)
            {
                if (keywords.All(k => !data[i].shaderKeywordSet.IsEnabled(k)))
                    data.RemoveAt(i);
            }
        }

        private class Settings : ScriptableSingleton<Settings>
        {
            public bool strip_LOD_FADE_CROSSFADE = false;
            public bool strip_LightMapVariants = false;
        }
    }
}
#endif
