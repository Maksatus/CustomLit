using System;
using CustomLit.Editor.ShaderGraph.Targets;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGraph;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering;

namespace CustomLit.Editor.ShaderGraph.AssetCallbacks
{
    internal static class CreateSimpleLitShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/URP/Custom Lit Shader Graph", priority = CoreUtils.Priorities.assetsCreateShaderMenuPriority)]
        public static void CreateCustomLitGraph()
        {
            var target = (UniversalTarget)Activator.CreateInstance(typeof(UniversalTarget));
            target.TrySetActiveSubTarget(typeof(UniversalCustomLitSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.SurfaceDescription.BaseColor,
            };

            GraphUtil.CreateNewGraphWithOutputs(new Target[] { target }, blockDescriptors);
        }
    }
}
