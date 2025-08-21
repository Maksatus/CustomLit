using UnityEditor.ShaderGraph;

namespace CustomLit.Editor.ShaderGraph.TargetResources
{
    internal static class CustomLitBlockFields
    {
        [GenerateBlocks]
        public struct Description
        {
            public static string Name = "Description";
            public static BlockFieldDescriptor TestLitParameter = new BlockFieldDescriptor(Name, "TestLitParameter", "DESCRIPTION_CUSTOMLIT",
                new FloatControl(0.0f), ShaderStage.Fragment);
        }
    }
}