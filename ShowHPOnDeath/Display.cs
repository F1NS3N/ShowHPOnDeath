using Modding;
using System;
using UnityEngine;

namespace ShowHPOnDeath
{
    internal class ModDisplay
    {
        internal static ModDisplay Instance;

        private string DisplayText = "";
        private DateTime DisplayExpireTime = DateTime.Now;
        private TimeSpan DisplayDuration = TimeSpan.FromSeconds(6);
        private Vector2 TextSize = new(800, 500);
        private Vector2 TextPosition = new(0.78f, 0.243f);

        private GameObject _canvas;
        private UnityEngine.UI.Text _text;


        public ModDisplay()
        {
            Create();
        }
        private void Create()
        {
            if (_canvas != null) return;

            // Create base canvas
            _canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

            CanvasGroup canvasGroup = _canvas.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            UnityEngine.Object.DontDestroyOnLoad(_canvas);

            _text = CanvasUtil.CreateTextPanel(
                _canvas, "", 24, TextAnchor.LowerRight,
                new CanvasUtil.RectData(TextSize, Vector2.zero, TextPosition, TextPosition),
                CanvasUtil.GetFont("Perpetua")
            ).GetComponent<UnityEngine.UI.Text>();
        }

        public void Destroy()
        {
            if (_canvas != null)
            {
                _canvas.SetActive(false);
                UnityEngine.Object.Destroy(_canvas); // Óíè÷òîæàåì GameObject
            }

            _text = null;
            Instance = null; // <-- Î×ÅÍÜ ÂÀÆÍÎ: ñáðîñèì ñòàòè÷åñêèé ýêçåìïëÿð
        }

        public void Update()
        {
            _text.text = DisplayText;
            _canvas.SetActive(true);
        }
        public void Display(string text)
        {
            DisplayText = text.Trim();
            DisplayExpireTime = DateTime.Now + DisplayDuration;
            Update();
        }
    }
}