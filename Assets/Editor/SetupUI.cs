#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static partial class ProjectSetup
{
    static void CreateMainMenuScene()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "MainMenu...", 0.40f);

        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single);

        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        camGO.transform.position = new Vector3(0, 0, -10);

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        CreateTMPText("TitleText", "SOLAR", canvasGO.transform,
            new Vector2(0, 200), 72, Color.white, TextAlignmentOptions.Center);

        CreateTMPText("Subtitle", "Огни в забытых глубинах", canvasGO.transform,
            new Vector2(0, 120), 24, new Color(0.6f, 0.7f, 1f), TextAlignmentOptions.Center);

        var btn1 = CreateButton("Уровень 1", canvasGO.transform, new Vector2(0, 20));
        var btn2 = CreateButton("Уровень 2", canvasGO.transform, new Vector2(0, -50));
        var btn3 = CreateButton("Уровень 3", canvasGO.transform, new Vector2(0, -120));
        var btn4 = CreateButton("Выход",     canvasGO.transform, new Vector2(0, -190));

        var mcGO = new GameObject("MenuController");
        var mc = mcGO.AddComponent<MenuController>();
        var mcSO = new SerializedObject(mc);
        SetRef(mcSO, "level1Button", btn1.GetComponent<Button>());
        SetRef(mcSO, "level2Button", btn2.GetComponent<Button>());
        SetRef(mcSO, "level3Button", btn3.GetComponent<Button>());
        SetRef(mcSO, "quitButton",   btn4.GetComponent<Button>());
        mcSO.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        EditorUtility.ClearProgressBar();
    }

    static void CreateHUDCanvas()
    {
        var canvasGO = new GameObject("HUD Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var heartsGO = CreateTMPText("HeartsText",
            "<color=#FF4444>\u2665</color> <color=#FF4444>\u2665</color> <color=#FF4444>\u2665</color>",
            canvasGO.transform, new Vector2(20, -15), 28, Color.white, TextAlignmentOptions.TopLeft);
        var hRT = heartsGO.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 1);
        hRT.anchorMax = new Vector2(0, 1);
        hRT.pivot = new Vector2(0, 1);
        hRT.sizeDelta = new Vector2(200, 40);
        heartsGO.GetComponent<TMP_Text>().richText = true;

        var sliderGO = new GameObject("EnergyBar");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        var sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0, 1);
        sliderRT.anchorMax = new Vector2(0, 1);
        sliderRT.pivot = new Vector2(0, 1);
        sliderRT.anchoredPosition = new Vector2(20, -50);
        sliderRT.sizeDelta = new Vector2(150, 12);

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        bgGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bgGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bgGO.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillRT = fillAreaGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImage = fillGO.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.85f, 1f);
        fillGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fillGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
        fillGO.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        var slider = sliderGO.AddComponent<Slider>();
        slider.fillRect = fillGO.GetComponent<RectTransform>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        slider.interactable = false;

        var burstGO = new GameObject("BurstCooldown");
        burstGO.transform.SetParent(canvasGO.transform, false);
        var burstRT = burstGO.AddComponent<RectTransform>();
        burstRT.anchorMin = new Vector2(0, 1);
        burstRT.anchorMax = new Vector2(0, 1);
        burstRT.pivot = new Vector2(0, 1);
        burstRT.anchoredPosition = new Vector2(180, -42);
        burstRT.sizeDelta = new Vector2(24, 24);
        var burstImg = burstGO.AddComponent<Image>();
        burstImg.color = new Color(1f, 0.8f, 0.2f, 0.7f);
        burstImg.type = Image.Type.Filled;
        burstImg.fillMethod = Image.FillMethod.Radial360;
        burstImg.fillAmount = 0;

        var artGO = CreateTMPText("ArtifactText", "0 / 3", canvasGO.transform,
            new Vector2(-20, -20), 24, Color.white, TextAlignmentOptions.TopRight);
        var artRT = artGO.GetComponent<RectTransform>();
        artRT.anchorMin = new Vector2(1, 1);
        artRT.anchorMax = new Vector2(1, 1);
        artRT.pivot = new Vector2(1, 1);

        var upgPanel = new GameObject("UpgradesPanel");
        upgPanel.transform.SetParent(canvasGO.transform, false);
        var upgRT = upgPanel.AddComponent<RectTransform>();
        upgRT.anchorMin = new Vector2(0, 1);
        upgRT.anchorMax = new Vector2(0, 1);
        upgRT.pivot = new Vector2(0, 1);
        upgRT.anchoredPosition = new Vector2(20, -75);
        upgRT.sizeDelta = new Vector2(200, 50);
        upgPanel.SetActive(false);

        var rangeIconGO = new GameObject("RangeIcon");
        rangeIconGO.transform.SetParent(upgPanel.transform, false);
        var rangeIconRT = rangeIconGO.AddComponent<RectTransform>();
        rangeIconRT.anchoredPosition = new Vector2(0, -5);
        rangeIconRT.sizeDelta = new Vector2(18, 18);
        var rangeIconImg = rangeIconGO.AddComponent<Image>();
        rangeIconImg.color = new Color(0.4f, 0.8f, 1f, 0.9f);

        var rangeTextGO = CreateTMPText("RangeText", "+0", upgPanel.transform,
            new Vector2(24, -5), 16, new Color(0.4f, 0.8f, 1f), TextAlignmentOptions.Left);
        var rangeTextRT = rangeTextGO.GetComponent<RectTransform>();
        rangeTextRT.anchorMin = new Vector2(0, 0.5f);
        rangeTextRT.anchorMax = new Vector2(0, 0.5f);
        rangeTextRT.pivot = new Vector2(0, 0.5f);
        rangeTextRT.sizeDelta = new Vector2(80, 20);

        var durIconGO = new GameObject("DurationIcon");
        durIconGO.transform.SetParent(upgPanel.transform, false);
        var durIconRT = durIconGO.AddComponent<RectTransform>();
        durIconRT.anchoredPosition = new Vector2(100, -5);
        durIconRT.sizeDelta = new Vector2(18, 18);
        var durIconImg = durIconGO.AddComponent<Image>();
        durIconImg.color = new Color(1f, 0.8f, 0.3f, 0.9f);

        var durTextGO = CreateTMPText("DurationText", "+0с", upgPanel.transform,
            new Vector2(124, -5), 16, new Color(1f, 0.8f, 0.3f), TextAlignmentOptions.Left);
        var durTextRT = durTextGO.GetComponent<RectTransform>();
        durTextRT.anchorMin = new Vector2(0, 0.5f);
        durTextRT.anchorMax = new Vector2(0, 0.5f);
        durTextRT.pivot = new Vector2(0, 0.5f);
        durTextRT.sizeDelta = new Vector2(80, 20);

        var winPanel = CreateOverlayPanel(canvasGO.transform, "WinPanel",
            "Уровень пройден!", "Следующий уровень", "В меню");
        winPanel.SetActive(false);

        var losePanel = CreateOverlayPanel(canvasGO.transform, "LosePanel",
            "Тебя поглотила тьма", "Заново", "В меню");
        losePanel.SetActive(false);

        var uiCtrl = canvasGO.AddComponent<UIController>();
        var uiSO = new SerializedObject(uiCtrl);

        SetRef(uiSO, "heartsText", heartsGO.GetComponent<TMP_Text>());
        SetRef(uiSO, "energyBar", slider);
        SetRef(uiSO, "energyFill", fillImage);
        SetRef(uiSO, "artifactText", artGO.GetComponent<TMP_Text>());
        SetRef(uiSO, "burstCooldownIcon", burstImg);
        SetRef(uiSO, "upgradesPanel", upgPanel);
        SetRef(uiSO, "upgradeRangeText", rangeTextGO.GetComponent<TMP_Text>());
        SetRef(uiSO, "upgradeDurationText", durTextGO.GetComponent<TMP_Text>());
        SetRef(uiSO, "upgradeRangeIcon", rangeIconImg);
        SetRef(uiSO, "upgradeDurationIcon", durIconImg);
        SetRef(uiSO, "winPanel", winPanel);
        SetRef(uiSO, "losePanel", losePanel);

        var winBtns = winPanel.GetComponentsInChildren<Button>(true);
        if (winBtns.Length >= 2)
        {
            SetRef(uiSO, "winNextLevelButton", winBtns[0]);
            SetRef(uiSO, "winMenuButton", winBtns[1]);
        }

        var loseBtns = losePanel.GetComponentsInChildren<Button>(true);
        if (loseBtns.Length >= 2)
        {
            SetRef(uiSO, "loseRestartButton", loseBtns[0]);
            SetRef(uiSO, "loseMenuButton", loseBtns[1]);
        }

        uiSO.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreateOverlayPanel(Transform parent, string name,
        string titleText, string btn1Text, string btn2Text)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);

        CreateTMPText("Title", titleText, panel.transform,
            new Vector2(0, 60), 48, Color.white, TextAlignmentOptions.Center);

        CreateButton(btn1Text, panel.transform, new Vector2(0, -20));
        CreateButton(btn2Text, panel.transform, new Vector2(0, -90));

        return panel;
    }

    static GameObject CreatePausePanel(Transform parent)
    {
        var panel = new GameObject("PausePanel");
        if (parent != null) panel.transform.SetParent(parent, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        var panelCanvas = panel.AddComponent<Canvas>();
        panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        panelCanvas.sortingOrder = 100;
        panel.AddComponent<GraphicRaycaster>();

        var bg = new GameObject("Background");
        bg.transform.SetParent(panel.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);

        CreateTMPText("PauseTitle", "ПАУЗА", panel.transform,
            new Vector2(0, 60), 48, Color.white, TextAlignmentOptions.Center);

        CreateButton("Продолжить", panel.transform, new Vector2(0, -20));
        CreateButton("В меню", panel.transform, new Vector2(0, -90));

        panel.SetActive(false);
        return panel;
    }

    static GameObject CreateButton(string text, Transform parent, Vector2 pos)
    {
        var btnGO = new GameObject(text + "_Button");
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280, 50);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.3f, 0.5f);
        btnGO.AddComponent<Button>();

        CreateTMPText("Text", text, btnGO.transform,
            Vector2.zero, 24, Color.white, TextAlignmentOptions.Center);
        var txtRT = btnGO.transform.GetChild(0).GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;

        return btnGO;
    }

    static GameObject CreateTMPText(string name, string text, Transform parent,
        Vector2 pos, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 80);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;

        return go;
    }
}
#endif
