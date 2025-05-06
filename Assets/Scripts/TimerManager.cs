using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[Serializable]
public class PizzaTimerSettings
{
    public Vector2 screenPosition = new Vector2(0, 200); // Posição superior central
    public float _totalTime = 5f;
    public float size = 100f;
    public Color timerColor = new Color(1f, 0.92f, 0.016f, 0.7f); // Amarelo pálido
    public Color endColor = Color.red;
    public Color textColor = Color.white;
    public string timerName = "PizzaTimer";

    public float totalTime
    {
        get => _totalTime;
        set
        {
            if (value > 0)
                _totalTime = value;
            else
                Debug.LogWarning("O tempo do timer deve ser maior que zero");
        }
    }

    // Método para alterar o tempo (alternativa à propriedade)
    public void SetTime(float newTime)
    {
        totalTime = newTime;
    }
}

public class TimerManager : MonoBehaviour
{
    private static GameObject timerPrefab;
    private static Transform timersParent;
    private static bool isInitialized = false;

    private Image backgroundCircle;
    private Image fillCircle;
    private Text timerText;

    private PizzaTimerSettings settings;
    private float currentTime;
    public bool isActive = false;
    private Action onCompleteCallback;
    private Coroutine flashCoroutine;
    private Text nameLabel;

    // Método estático para inicialização segura
    private static void EnsureInitialized()
    {
        if (isInitialized) return;

        // Encontra ou cria o canvas
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Cria o parent object para os timers
        var parentGO = new GameObject("PizzaTimers");
        parentGO.transform.SetParent(canvas.transform);
        timersParent = parentGO.transform;

        // Cria o prefab básico em tempo de execução
        timerPrefab = new GameObject("PizzaTimerPrefab", typeof(RectTransform));
        timerPrefab.SetActive(false);

        // Background - agora com sprite circular
        var bgImage = timerPrefab.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        // Cria um sprite circular simples
        bgImage.sprite = CreateCircleSprite((int)(bgImage.color.a * 255));

        // Fill (parte que diminui)
        var fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(timerPrefab.transform);
        var fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.green;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = false;
        fillImage.sprite = CreateCircleSprite((int)(fillImage.color.a * 255));

        // Ajusta o RectTransform do fill para cobrir completamente o background
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Text
        var textGO = new GameObject("TimerText", typeof(RectTransform));
        textGO.transform.SetParent(fillGO.transform);
        var textComp = textGO.AddComponent<Text>();
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 24;

        // Ajusta o RectTransform do texto
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        isInitialized = true;
    }

    // Método para criar um sprite circular simples
    private static Sprite CreateCircleSprite(int alpha)
    {
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);

        Color transparent = new Color(0, 0, 0, 0);
        Color white = new Color(1, 1, 1, alpha / 255f);

        float radius = textureSize / 2f;
        float radiusSqr = radius * radius;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dx = x - radius;
                float dy = y - radius;
                float distSqr = dx * dx + dy * dy;

                texture.SetPixel(x, y, distSqr <= radiusSqr ? white : transparent);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f);
    }

    // Método estático para criar um novo timer com parâmetros opcionais
    public static TimerManager Create(
        float? time = null,
        Vector2? position = null,
        float? size = null,
        Color? timerColor = null,
        Color? endColor = null,
        Color? textColor = null,
        string name = null,
        Action onComplete = null)
    {
        EnsureInitialized();

        var settings = new PizzaTimerSettings();
        if (time.HasValue) settings.totalTime = time.Value;
        if (position.HasValue) settings.screenPosition = position.Value;
        if (size.HasValue) settings.size = size.Value;
        if (timerColor.HasValue) settings.timerColor = timerColor.Value;
        if (endColor.HasValue) settings.endColor = endColor.Value;
        if (textColor.HasValue) settings.textColor = textColor.Value;
        if (!string.IsNullOrEmpty(name)) settings.timerName = name;

        GameObject timerObj = Instantiate(timerPrefab, timersParent);
        timerObj.name = settings.timerName;
        timerObj.SetActive(false);

        var timer = timerObj.AddComponent<TimerManager>();
        timer.backgroundCircle = timerObj.GetComponent<Image>();
        timer.fillCircle = timerObj.transform.Find("Fill").GetComponent<Image>();
        timer.timerText = timerObj.transform.Find("Fill/TimerText").GetComponent<Text>();

        // Adiciona o label do nome
        timer.nameLabel = CreateNameLabel(timerObj, settings);

        timer.settings = settings;
        timer.onCompleteCallback = onComplete;
        timer.ApplySettings();

        return timer;
    }

    private static Text CreateNameLabel(GameObject parent, PizzaTimerSettings settings)
    {
        var labelGO = new GameObject("NameLabel", typeof(RectTransform));
        labelGO.transform.SetParent(parent.transform);

        var label = labelGO.AddComponent<Text>();
        label.text = settings.timerName;
        label.color = settings.textColor;
        label.alignment = TextAnchor.MiddleCenter;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 20;

        RectTransform rt = labelGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(2.2f, 0.5f); // Posição à direita do timer
        rt.anchorMax = new Vector2(1.2f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(450, 30); // Largura e altura do label
        rt.anchoredPosition = Vector2.zero;

        return label;
    }

    private void ApplySettings()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = settings.screenPosition;
        rt.sizeDelta = new Vector2(settings.size, settings.size);

        fillCircle.color = settings.timerColor;
        fillCircle.fillAmount = 1f;

        timerText.color = settings.textColor;
        timerText.rectTransform.anchoredPosition = Vector2.zero;
        timerText.rectTransform.sizeDelta = new Vector2(settings.size, settings.size);

        // Configura o label do nome
        if (nameLabel != null)
        {
            nameLabel.text = settings.timerName;
            nameLabel.color = settings.textColor;
        }

        ResetTimer();
    }

    private void Update()
    {
        if (!isActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isActive = false;
            onCompleteCallback?.Invoke();
            flashCoroutine = StartCoroutine(FlashAndHide());
        }

        UpdateVisuals();
    }

    private IEnumerator FlashAndHide()
    {
        // Pisca 2 vezes
        for (int i = 0; i < 2; i++)
        {
            SetVisibility(false);
            yield return new WaitForSeconds(0.2f);
            SetVisibility(true);
            yield return new WaitForSeconds(0.2f);
        }

        // Esconde o timer ao final
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        backgroundCircle.enabled = visible;
        fillCircle.enabled = visible;
        timerText.enabled = visible;
        if (nameLabel != null) nameLabel.enabled = visible;
    }

    private void UpdateVisuals()
    {
        float progress = currentTime / settings.totalTime;
        fillCircle.fillAmount = progress;
        fillCircle.color = Color.Lerp(settings.endColor, settings.timerColor, progress);
        timerText.text = FormatTime(currentTime);
    }

    private string FormatTime(float time)
    {
        // Formata o tempo com ":" como separador decimal e 2 casas decimais
        return time.ToString("0.00").Replace(".", ":");
    }

    public void StartTimer()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        gameObject.SetActive(true);
        SetVisibility(true);
        isActive = true;
        currentTime = settings.totalTime;
        UpdateVisuals();
    }

    public void SetTime(float newTime, bool keepProgress = false)
    {
        if (newTime <= 0)
        {
            Debug.LogWarning("O tempo do timer deve ser maior que zero");
            return;
        }

        if (keepProgress && isActive)
        {
            // Calcula a progressão atual (0 a 1)
            float currentProgress = currentTime / settings.totalTime;
            // Ajusta o tempo atual para manter a mesma progressão
            currentTime = newTime * currentProgress;
        }
        else if (!isActive)
        {
            // Se não estiver ativo, apenas atualiza o tempo
            currentTime = newTime;
        }

        settings.totalTime = newTime;
        UpdateVisuals();
    }

    public void PauseTimer()
    {
        isActive = false;
    }

    public void ResetTimer()
    {
        currentTime = settings.totalTime;
        UpdateVisuals();
        isActive = false;
    }

    public void DestroyTimer()
    {
        Destroy(gameObject);
    }
}