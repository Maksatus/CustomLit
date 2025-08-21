using System;
using CustomLit.Editor.ShaderGUI;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEditor.Rendering.Universal.ShaderGraph;
using UnityEditor.ShaderGraph;
using UnityEngine;
using static UnityEditor.Rendering.Universal.ShaderGraph.SubShaderUtils;
using static Unity.Rendering.Universal.ShaderUtils;

namespace CustomLit.Editor.ShaderGraph.Targets
{
    internal class UniversalCustomLitSubTarget : UniversalSubTarget
    {
        private static readonly GUID SourceCodeGuid = new("97c3f7dcb477ec842aa878573640314a");

        public override int latestVersion => 2;

        public UniversalCustomLitSubTarget() => displayName = "Custom Lit";

        protected override ShaderID shaderID => ShaderID.SG_Unlit;

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(SourceCodeGuid, AssetCollection.Flags.SourceDependency);
            base.Setup(ref context);

            var universalRPType = typeof(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset);
            if (!context.HasCustomEditorForRenderPipeline(universalRPType))
            {
                var gui = typeof(ShaderGraphCustomLitGUI);
                context.AddCustomEditorForRenderPipeline(gui.FullName, universalRPType);
            }

            context.AddSubShader(PostProcessSubShader(SubShaders.CustomLit(target, target.renderType,
                target.renderQueue, target.disableBatching)));
        }

        public override void ProcessPreviewMaterial(Material material)
        {
            if (target.allowMaterialOverride)
            {
                material.SetFloat(Property.SurfaceType, (float)target.surfaceType);
                material.SetFloat(Property.BlendMode, (float)target.alphaMode);
                material.SetFloat(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                material.SetFloat(Property.CullMode, (int)target.renderFace);
                material.SetFloat(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                material.SetFloat(Property.ZWriteControl, (float)target.zWriteControl);
                material.SetFloat(Property.ZTest, (float)target.zTestMode);
            }

            material.SetFloat(Property.QueueOffset, 0.0f);
            material.SetFloat(Property.QueueControl, (float)BaseShaderGUI.QueueControl.UserOverride);

            ShaderGraphCustomLitGUI.UpdateMaterial(material);
        }


        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(BlockFields.SurfaceDescription.Alpha,
                (target.surfaceType == SurfaceType.Transparent || target.alphaClip) || target.allowMaterialOverride);
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold,
                target.alphaClip || target.allowMaterialOverride);
        }

        public override void CollectShaderProperties(PropertyCollector collector, GenerationMode generationMode)
        {
            if (target.allowMaterialOverride)
            {
                collector.AddFloatProperty(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SurfaceType, (float)target.surfaceType);
                collector.AddFloatProperty(Property.BlendMode, (float)target.alphaMode);
                collector.AddFloatProperty(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SrcBlend, 1.0f);
                collector.AddFloatProperty(Property.DstBlend, 0.0f);
                collector.AddToggleProperty(Property.ZWrite, (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.ZWriteControl, (float)target.zWriteControl);
                collector.AddFloatProperty(Property.ZTest, (float)target.zTestMode);
                collector.AddFloatProperty(Property.CullMode, (float)target.renderFace);

                var enableAlphaToMask = (target.alphaClip && (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.AlphaToMask, enableAlphaToMask ? 1.0f : 0.0f);
            }

            collector.AddFloatProperty(Property.QueueOffset, 0.0f);
            collector.AddFloatProperty(Property.QueueControl, -1.0f);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<string> registerUndo)
        {
            target.AddDefaultMaterialOverrideGUI(ref context, onChange, registerUndo);
            target.AddDefaultSurfacePropertiesGUI(ref context, onChange, registerUndo, showReceiveShadows: false);
        }


        #region SubShader

        private static class SubShaders
        {
            public static SubShaderDescriptor CustomLit(UniversalTarget target, string renderType, string renderQueue, string disableBatchingTag)
            {
                var result = new SubShaderDescriptor
                {
                    pipelineTag = UniversalTarget.kPipelineTag,
                    customTags = UniversalTarget.kUnlitMaterialTypeTag,
                    renderType = renderType,
                    renderQueue = renderQueue,
                    disableBatchingTag = disableBatchingTag,
                    generatesPreview = true,
                    passes = new PassCollection { CustomLitPasses.Forward(target, CustomLitKeywords.Forward) }
                };

                // if (target.mayWriteDepth)
                //     result.passes.Add(PassVariant(CorePasses.DepthOnly(target), CorePragmas.Instanced));

                //result.passes.Add(PassVariant(CustomLitPasses.DepthNormalOnly(target), CorePragmas.Instanced));

                if (target.castShadows || target.allowMaterialOverride)
                    result.passes.Add(PassVariant(CorePasses.ShadowCaster(target), CorePragmas.Instanced));

                //result.passes.Add(CustomLitPasses.GBuffer(target));

                // result.passes.Add(PassVariant(CorePasses.SceneSelection(target), CorePragmas.Default));
                // result.passes.Add(PassVariant(CorePasses.ScenePicking(target), CorePragmas.Default));

                return result;
            }
        }

        #endregion

        #region Pass

        static class CustomLitPasses
        {
            public static PassDescriptor Forward(UniversalTarget target, KeywordCollection keywords)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "Universal Forward",
                    referenceName = "SHADERPASS_UNLIT",
                    useInPreview = true,

                    // Template
                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentColorAlpha,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = CustomLitRequiredFields.Unlit,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target),
                    pragmas = CorePragmas.Forward,
                    defines = new DefineCollection { CoreDefines.UseFragmentFog },
                    keywords = new KeywordCollection { keywords },
                    includes = new IncludeCollection { CustomLitIncludes.CustomLit },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddAlphaToMaskControlToPass(ref result, target);
                //CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }


            #region RequiredFields

            private static class CustomLitRequiredFields
            {
                public static readonly FieldCollection Unlit = new()
                {
                    StructFields.Varyings.positionWS,
                    StructFields.Varyings.normalWS
                };
            }

            #endregion
        }

        #endregion

        #region Keywords

        private static class CustomLitKeywords
        {
            public static readonly KeywordCollection Forward = new()
            {
                //CoreKeywordDescriptors.StaticLightmap,
                // CoreKeywordDescriptors.DirectionalLightmapCombined,
                // CoreKeywordDescriptors.SampleGI,
                // CoreKeywordDescriptors.DBuffer,
                // CoreKeywordDescriptors.DebugDisplay,
                // CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion,
            };
        }

        #endregion

        #region Includes

        private static class CustomLitIncludes
        {
            const string kUnlitPass = "Assets/CustomLit/Editor/ShaderGraph/Includes/CustomLit.hlsl";

            public static readonly IncludeCollection CustomLit = new()
            {
                // Pre-graph
                //{ CoreIncludes.DOTSPregraph },
                // { CoreIncludes.WriteRenderLayersPregraph },
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                //{ CoreIncludes.DBufferPregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kUnlitPass, IncludeLocation.Postgraph },
            };
        }

        #endregion
    }
}