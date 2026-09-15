using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.shaderstripper
{
    internal static class GameObjectScanner
    {
        public static void Scan(GameObject obj)
        {
            ShaderStripper.Reset();
            // Local keywords will likely be automatically stripped.
            //var scanned = new HashSet<Object>();
            //foreach (var component in obj.GetComponentsInChildren<Component>(true))
            //{
            //    Gather(component, scanned);
            //}

            ShaderStripper.strip_LOD_FADE_CROSSFADE = !obj.GetComponentInChildren<LODGroup>(true);
            // DOTS instancing isn't used with avatars, is it?
            ShaderStripper.strip_DOTS_INSTANCING_ON = true;
            ShaderStripper.strip_LightMapVariants = true;
        }

        private static void Gather(Object obj, HashSet<Object> scanned)
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
                    Gather(iter.objectReferenceValue, scanned);
                }
            }
        }
    }
}
