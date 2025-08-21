using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace CustomLit.Editor.ShaderGUI
{
    internal class ShaderGraphCustomLitGUI : BaseShaderGUI
    {
        private MaterialProperty[] _materialProperties;
        
        public override void FindProperties(MaterialProperty[] properties)
        {
            _materialProperties = properties;

            base.FindProperties(properties);
        }

        public static void UpdateMaterial(Material material)
        {
            var automaticRenderQueue = GetAutomaticQueueControlSetting(material);
            UpdateMaterialSurfaceOptions(material, automaticRenderQueue);
        }

        public override void ValidateMaterial(Material material) => UpdateMaterial(material);
        
        public override void DrawSurfaceInputs(Material material) => DrawShaderGraphProperties(material, _materialProperties);

        public override void DrawAdvancedOptions(Material material)
        {
            DoPopup(Styles.queueControl, queueControlProp, Styles.queueControlNames);
            if (material.HasProperty(Property.QueueControl) && Mathf.Approximately(material.GetFloat(Property.QueueControl), (float)QueueControl.UserOverride))
                materialEditor.RenderQueueField();
            base.DrawAdvancedOptions(material);
            materialEditor.DoubleSidedGIField();
        }
    }
}