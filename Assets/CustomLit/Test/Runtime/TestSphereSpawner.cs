using System.Collections.Generic;
using UnityEngine;

namespace Test.Runtime
{
    public class TestSphereSpawner : MonoBehaviour
    {
        [SerializeField] private int numberOfSpheres = 25;
        [SerializeField] private float sphereRadius = 0.5f;
        [SerializeField] private float spawnAreaRadius = 5f;
    
        [SerializeField] private List<Shader> shaders;
    
        private List<List<Material>> _allMaterials;
        private List<GameObject> _spheres;
        private int _currentShaderIndex = 0;
    
        private string _currentShaderInfo;

        private void Start()
        {
            Application.targetFrameRate = 120;
            if (shaders == null || shaders.Count == 0)
            {
                Debug.LogError("‼️!");
                this.enabled = false;
                return;
            }

            GenerateAllMaterials();
            GenerateSpheres();
        
            UpdateShaderInfo();
        }

        private void Update()
        {
           HandleInput();
        }

        private void OnGUI()
        {
            var textRect = new Rect(10, 10, 500, 30);
            var style = new GUIStyle
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            };
        
            GUI.Label(textRect, _currentShaderInfo, style);
        }
    
        private void GenerateAllMaterials()
        {
            _allMaterials = new List<List<Material>>();

            foreach (Shader shader in shaders)
            {
                if (shader == null) continue;

                var materialSet = new List<Material>();
                for (int i = 0; i < numberOfSpheres; i++)
                {
                    Material mat = new Material(shader);
                    mat.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
                    materialSet.Add(mat);
                }
                _allMaterials.Add(materialSet);
            }
        }
    
        private void GenerateSpheres()
        {
            _spheres = new List<GameObject>();
            var initialMaterials = _allMaterials[0]; 

            for (int i = 0; i < numberOfSpheres; i++)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(this.transform);
                sphere.transform.position = this.transform.position + (Random.insideUnitSphere * spawnAreaRadius);
                sphere.transform.localScale = Vector3.one * sphereRadius * 2;

                var component = sphere.GetComponent<Renderer>();
                if (component) component.material = initialMaterials[i];
                _spheres.Add(sphere);
            }
        }
    
        private void HandleInput()
        {
            var screenPressed = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0);
            if (screenPressed) CycleToNextMaterialSet();
        }
    
        private void CycleToNextMaterialSet()
        {
            _currentShaderIndex = (_currentShaderIndex + 1) % _allMaterials.Count;
        
            UpdateShaderInfo();

            var nextMaterialSet = _allMaterials[_currentShaderIndex];

            for (var i = 0; i < _spheres.Count; i++)
            {
                var component = _spheres[i].GetComponent<Renderer>();
                if (component) component.material = nextMaterialSet[i];
            }
        }
    
        private void UpdateShaderInfo()
        {
            var shaderName = shaders[_currentShaderIndex].name;
            _currentShaderInfo = $"Текущий шейдер: {shaderName}";
            Debug.Log($"🎨 {_currentShaderInfo}");
        }
    }
}