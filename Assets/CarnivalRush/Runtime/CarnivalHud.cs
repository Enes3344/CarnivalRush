using UnityEngine;
using UnityEngine.UI;

namespace CarnivalRush
{
    public sealed class CarnivalHud : MonoBehaviour
    {
        [SerializeField] private Text score;
        [SerializeField] private Text record;
        [SerializeField] private Text level;
        [SerializeField] private Text targets;
        [SerializeField] private Text timer;
        [SerializeField] private Text banner;
        [SerializeField] private Text menuTitle;
        [SerializeField] private Text menuDescription;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private GameObject overlay;
        [SerializeField] private GameObject reticle;
        [SerializeField] private Image flash;
        [SerializeField] private Button primary;
        [SerializeField] private Button restart;

        public Button Primary => primary;
        public Button Restart => restart;

        private static readonly Color Ink = new Color32(26, 15, 25, 245);
        private static readonly Color Cream = new Color32(255, 238, 198, 255);
        private static readonly Color Gold = new Color32(255, 195, 91, 255);
        private static readonly Color Red = new Color32(165, 43, 56, 255);

        public void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform top = Rect("Scoreboard", transform, new Vector2(0.5f, 1f),
                new Vector2(0, -47), new Vector2(720, 70));
            Plate(top, Ink);
            Plate(Rect("Gold trim", top, new Vector2(0.5f, 0), Vector2.zero, new Vector2(720, 3)), Gold);
            score = Label("Score", top, new Vector2(-266, 11), new Vector2(176, 24), 20, Cream);
            record = Label("Records", top, new Vector2(-266, -14), new Vector2(176, 22), 13, Gold);
            level = Label("Level", top, new Vector2(-78, 0), new Vector2(180, 46), 24, Cream);
            targets = Label("Targets", top, new Vector2(85, 0), new Vector2(145, 46), 22, Cream);
            timer = Label("Timer", top, new Vector2(258, 0), new Vector2(182, 46), 22, Gold);

            var hint = Rect("Controls", transform, new Vector2(0.5f, 0), new Vector2(0, 28),
                new Vector2(440, 30));
            Plate(hint, Ink);
            Label("Control legend", hint, Vector2.zero, new Vector2(435, 29), 15, Cream).text =
                "MOUSE  Aim     •     LMB  Fire     •     ESC  Pause";

            reticle = Rect("Reticle", transform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(26, 26)).gameObject;
            AddReticleBar(new Vector2(0, 9), new Vector2(2, 6));
            AddReticleBar(new Vector2(0, -9), new Vector2(2, 6));
            AddReticleBar(new Vector2(9, 0), new Vector2(6, 2));
            AddReticleBar(new Vector2(-9, 0), new Vector2(6, 2));
            AddReticleBar(Vector2.zero, new Vector2(2, 2));
            banner = Label("Round status", transform, new Vector2(0, 185), new Vector2(600, 55), 30, Gold);

            flash = Plate(Stretch("Failure flash", transform), Color.clear);
            RectTransform veil = Stretch("Menu overlay", transform);
            Plate(veil, new Color32(19, 10, 23, 181));
            overlay = veil.gameObject;
            RectTransform card = Rect("Menu card", veil, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(530, 300));
            Plate(card, Ink);
            Plate(Rect("Gold trim", card, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(530, 4)), Gold);
            menuTitle = Label("Menu title", card, new Vector2(0, 81), new Vector2(500, 64), 48, Gold);
            menuDescription = Label("Menu description", card, new Vector2(0, 6), new Vector2(490, 73), 19, Cream);
            primary = MenuButton(card, "Primary action", -67, Red, out primaryLabel);
            restart = MenuButton(card, "Restart button", -119, new Color32(81, 40, 57, 255), out Text restartLabel);
            restartLabel.text = "Restart";
            overlay.SetActive(false);
        }

        public void UpdateReadouts(int points, int best, int round, int bestRound, int remaining,
            float seconds, float speed)
        {
            score.text = $"SCORE  {points:N0}";
            record.text = $"BEST  {best:N0}  /  LV {bestRound}";
            level.text = $"LEVEL  {round}";
            targets.text = $"LEFT  {remaining}";
            timer.text = speed > 1.0001f ? $"{seconds:0.0}s   ×{speed:0.00}" : $"{seconds:0.0}s";
            timer.color = seconds <= 4 ? new Color32(255, 106, 99, 255) : Gold;
        }

        public void ShowMenu(string title, string description, string action, bool showRestart)
        {
            menuTitle.text = title;
            menuDescription.text = description;
            primaryLabel.text = action;
            restart.gameObject.SetActive(showRestart);
            overlay.SetActive(true);
            reticle.SetActive(false);
        }

        public void HideMenu()
        {
            overlay.SetActive(false);
            reticle.SetActive(true);
        }

        public void SetBanner(string message) => banner.text = message;
        public void SetFlash(float alpha) => flash.color = new Color(1f, 0.08f, 0.03f, alpha);

        private void AddReticleBar(Vector2 position, Vector2 size)
        {
            RectTransform shade = Rect("Reticle outline", reticle.transform, Vector2.one * 0.5f, position, size + Vector2.one * 2);
            Plate(shade, Color.black);
            Plate(Rect("Reticle mark", shade, Vector2.one * 0.5f, Vector2.zero, size), Cream);
        }

        private static Button MenuButton(Transform parent, string name, int y, Color color, out Text label)
        {
            RectTransform rect = Rect(name, parent, Vector2.one * 0.5f, new Vector2(0, y), new Vector2(225, 42));
            Image background = Plate(rect, color);
            background.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            label = Label("Label", rect, Vector2.zero, new Vector2(215, 40), 20, Cream);
            return button;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static RectTransform Stretch(string name, Transform parent)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero);
            rect.anchorMax = Vector2.one;
            return rect;
        }

        private static Image Plate(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text Label(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var text = Rect(name, parent, Vector2.one * 0.5f, position, size).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 11;
            text.resizeTextMaxSize = fontSize;
            return text;
        }
    }
}
