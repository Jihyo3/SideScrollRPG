using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SideScrollRPG
{
    /// <summary>
    /// UI를 코드로 조립한다. 프리팹·Canvas 에셋을 만들지 않으므로 에디터 셋업이 가벼워지고,
    /// 레이아웃 수정이 전부 코드 diff로 남는다(프로토타입 기준의 선택).
    /// </summary>
    public static class UIFactory
    {
        public static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static readonly Color Ink = new Color(0.96f, 0.96f, 0.92f);
        public static readonly Color PanelBg = new Color(0.07f, 0.08f, 0.11f, 0.94f);
        public static readonly Color ButtonBg = new Color(0.19f, 0.22f, 0.29f, 1f);
        public static readonly Color ButtonDisabled = new Color(0.13f, 0.14f, 0.17f, 1f);
        public static readonly Color Gold = new Color(1f, 0.83f, 0.35f);
        public static readonly Color HpRed = new Color(0.82f, 0.22f, 0.26f);

        public static Canvas CreateCanvas(string name, int sortOrder, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform Group(Transform parent, string name,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            return Place(go, parent, anchor, pivot, pos, size);
        }

        public static Image Panel(Transform parent, string name, Color color,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Place(go, parent, anchor, pivot, pos, size);

            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        /// 화면 전체를 덮는 패널(사망·승리·타이틀 배경).
        public static Image FullScreen(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(Transform parent, string name, string content,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
            int fontSize, TextAnchor align, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Place(go, parent, anchor, pivot, pos, size);

            var text = go.AddComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /// 좌→우로 채워지는 게이지. 반환된 GaugeView.SetRatio로 갱신한다.
        public static GaugeView Gauge(Transform parent, string name, Color fill, Color back,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var container = Group(parent, name, anchor, pivot, pos, size);

            // 배경: 컨테이너 전체를 덮는다.
            var bg = new GameObject("BG", typeof(RectTransform));
            bg.transform.SetParent(container, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = back;

            // 채움: 왼쪽 기준으로 너비만 변한다.
            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(container, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.anchoredPosition = Vector2.zero;
            fillRt.sizeDelta = new Vector2(size.x, 0f);
            fillGo.AddComponent<Image>().color = fill;

            var view = container.gameObject.AddComponent<GaugeView>();
            view.fill = fillRt;
            view.fullWidth = size.x;
            return view;
        }

        public static Button TextButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
            int fontSize, UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Place(go, parent, anchor, pivot, pos, size);

            var image = go.AddComponent<Image>();
            image.color = ButtonBg;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null) button.onClick.AddListener(onClick);

            var text = Label(go.transform, "Label", label,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, size, fontSize, TextAnchor.MiddleCenter, Ink);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(12f, 0f);
            text.rectTransform.offsetMax = new Vector2(-12f, 0f);

            return button;
        }

        /// 버튼의 활성/비활성 상태를 색까지 같이 반영한다.
        public static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button == null) return;

            button.interactable = enabled;

            var image = button.targetGraphic as Image;
            if (image != null) image.color = enabled ? ButtonBg : ButtonDisabled;

            var text = button.GetComponentInChildren<Text>();
            if (text != null) text.color = enabled ? Ink : new Color(0.55f, 0.55f, 0.58f);
        }

        static RectTransform Place(GameObject go, Transform parent,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;
            return rt;
        }

        // 화면 모서리 기준 앵커 상수
        public static Vector2 TopLeft => new Vector2(0f, 1f);
        public static Vector2 TopCenter => new Vector2(0.5f, 1f);
        public static Vector2 Center => new Vector2(0.5f, 0.5f);
        public static Vector2 BottomCenter => new Vector2(0.5f, 0f);
    }
}
