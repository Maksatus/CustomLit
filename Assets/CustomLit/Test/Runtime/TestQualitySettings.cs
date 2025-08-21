using UnityEngine;

namespace Test.Runtime
{
    public class TestQualitySettings : MonoBehaviour
    {
        private string _currentInfo;

        private void Update()
        {
            //HandleInput();
        }

        private void OnGUI()
        {
            var textRect = new Rect(10, 50, 500, 30);
            var style = new GUIStyle
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            };

            GUI.Label(textRect, _currentInfo, style);
        }
        

        private void HandleInput()
        {
            var screenPressed = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began ||
                                Input.GetMouseButtonDown(0);
            if (screenPressed) CycleToNextMaterialSet();
        }

        private void CycleToNextMaterialSet()
        {
            var currentIndex = QualitySettings.GetQualityLevel() + 1;
            if (currentIndex > QualitySettings.names.Length - 1) currentIndex = 0;
            QualitySettings.SetQualityLevel(currentIndex);

            UpdateShaderInfo();
        }

        private void UpdateShaderInfo()
        {
            var nameSettings = QualitySettings.names[QualitySettings.GetQualityLevel()];
            _currentInfo = $"Текущие настройки: {nameSettings}";
            Debug.Log($"🎨 {_currentInfo}");
        }
    }
}