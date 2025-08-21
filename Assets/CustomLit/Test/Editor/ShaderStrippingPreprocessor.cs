using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace CustomLit.Test.Editor
{
    public class ShaderStrippingPreprocessor : IPreprocessShaders
    {
        public int callbackOrder => 0;

        private readonly string[] _namesStrippingShaders = { "Universal Render Pipeline/Lit", "UI/Additive" };

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            for (var i = 0; i < _namesStrippingShaders.Length; i++)
            {
                if (shader.name == _namesStrippingShaders[i])
                {
                    Debug.Log($"Stripping Shader = {_namesStrippingShaders[i]}");
                    data.Clear();
                    break;
                }
            }
        }
    }
}