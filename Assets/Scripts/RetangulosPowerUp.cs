using UnityEngine;
using System.Collections.Generic;

public class RetangulosPowerUp : MonoBehaviour
{
    private static RetangulosPowerUp _instance;
    public static RetangulosPowerUp Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RetangulosPowerUp>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RetangulosPowerUpManager");
                    _instance = obj.AddComponent<RetangulosPowerUp>();
                }
            }
            return _instance;
        }
    }

    private class RetanguloData
    {
        public GameObject gameObject;
        public Material material;
        public float hueOffset;
        public float waveOffset;
        public float tempoRestante;
        public float tempoTotal;
        public System.Action onExpirar;
    }

    private List<RetanguloData> retangulos = new List<RetanguloData>();
    private float tempo = 0f;
    public GameLoop gameLoop;

    public Vector2 tamanhoRetangulo = new Vector2(200f, 15f);
    public float espessuraBorda = 4f;
    public float velocidadeCintilacao = 1f;
    public float velocidadeOnda = 2f;
    public Color corBase = new Color(0.2f, 0.6f, 1f, 0.8f);
    [Range(0, 1)] public float translucidoIntensidade = 0.5f;
    public float ondaLargura = 0.3f;
    public Color corTempoEsgotando = Color.red;

    void Update()
    {
        tempo += Time.deltaTime * velocidadeCintilacao;

        for (int i = retangulos.Count - 1; i >= 0; i--)
        {
            var retangulo = retangulos[i];

            // Atualiza o tempo restante
            retangulo.tempoRestante -= Time.deltaTime;

            // Verifica se o tempo acabou
            if (retangulo.tempoRestante <= 0f)
            {
                if (retangulo.onExpirar != null)
                    retangulo.onExpirar.Invoke();

                Destroy(retangulo.gameObject);
                Destroy(retangulo.material);
                retangulos.RemoveAt(i);
                ReorganizarRetangulos();
                continue;
            }

            // Atualiza o efeito de degradê cintilante
            float hue = (Mathf.Sin(tempo + retangulo.hueOffset) + 1f) * 0.5f;
            Color corBorda = Color.HSVToRGB(hue, 0.8f, 1f);
            retangulo.material.SetColor("_BorderColor", corBorda);

            // Atualiza o efeito de onda translúcida
            retangulo.waveOffset += Time.deltaTime * velocidadeOnda;
            float wavePos = Mathf.PingPong(retangulo.waveOffset, 1f);
            retangulo.material.SetFloat("_WavePos", wavePos);
            retangulo.material.SetFloat("_WaveWidth", ondaLargura);
            retangulo.material.SetFloat("_TranslucidoIntensidade", translucidoIntensidade);

            // Atualiza a barra de tempo (preenchimento)
            float progresso = retangulo.tempoRestante / retangulo.tempoTotal;
            retangulo.material.SetFloat("_FillAmount", progresso);

            // Muda a cor quando estiver perto de acabar
            if (retangulo.tempoRestante < retangulo.tempoTotal * 0.2f)
            {
                float piscar = Mathf.PingPong(Time.time * 3f, 1f);
                Color corFinal = Color.Lerp(retangulo.material.GetColor("_Color"), corTempoEsgotando, piscar);
                retangulo.material.SetColor("_Color", corFinal);
            }
        }
    }

    public GameObject CriarRetangulo(Color cor, float duracao, System.Action onExpirar = null)
    {
        // Cria um novo objeto para o retângulo
        gameLoop = FindObjectOfType<GameLoop>();
        GameObject novoRetangulo = new GameObject("RetangulosPowerUp");

        // Adiciona componentes necessários
        var rectTransform = novoRetangulo.AddComponent<RectTransform>();
        var canvasRenderer = novoRetangulo.AddComponent<CanvasRenderer>();
        var image = novoRetangulo.AddComponent<UnityEngine.UI.Image>();

        // Configura o retângulo
        rectTransform.SetParent(GetComponentInParent<Canvas>().transform, false);
        rectTransform.pivot = new Vector2(1f, 0f); // Canto superior direito
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.sizeDelta = tamanhoRetangulo;

        // Posiciona o retângulo (empilhado verticalmente)
        float posY = -(tamanhoRetangulo.y + 10f) * retangulos.Count;
        rectTransform.anchoredPosition = new Vector2(-10f, posY);

        // Cria um material com shader para borda com degradê e efeito de onda
        Material material = new Material(Shader.Find("UI/TranslucentWave"));
        material.EnableKeyword("UNITY_UI_ALPHACLIP");

        // Configura o material
        material.SetColor("_Color", cor);
        material.SetColor("_BorderColor", cor);
        material.SetFloat("_BorderWidth", espessuraBorda);

        // Adiciona propriedades para o efeito de onda
        material.SetFloat("_WavePos", 0f);
        material.SetFloat("_WaveWidth", ondaLargura);
        material.SetFloat("_TranslucidoIntensidade", translucidoIntensidade);
        material.SetFloat("_FillAmount", 1f); // Inicia totalmente preenchido

        // Cria uma textura simples para o efeito de onda
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        material.SetTexture("_WaveTex", texture);

        image.material = material;

        // Armazena os dados do retângulo
        RetanguloData data = new RetanguloData
        {
            gameObject = novoRetangulo,
            material = material,
            hueOffset = Random.Range(0f, Mathf.PI * 2f),
            waveOffset = Random.Range(0f, 2f),
            tempoRestante = duracao,
            tempoTotal = duracao,
            onExpirar = onExpirar
        };

        retangulos.Add(data);

        return novoRetangulo;
    }

    public void RemoverRetangulo(GameObject retangulo)
    {
        // Encontra e remove o retângulo da lista
        RetanguloData data = retangulos.Find(r => r.gameObject == retangulo);
        if (data != null)
        {
            if (data.onExpirar != null)
                data.onExpirar.Invoke();

            Destroy(data.gameObject);
            Destroy(data.material);
            retangulos.Remove(data);

            // Reorganiza os retângulos restantes
            ReorganizarRetangulos();
        }
    }

    public void RemoverTodosRetangulos()
    {
        foreach (var retangulo in retangulos)
        {
            if (retangulo.onExpirar != null)
                retangulo.onExpirar.Invoke();

            Destroy(retangulo.gameObject);
            Destroy(retangulo.material);
        }
        retangulos.Clear();
    }

    private void ReorganizarRetangulos()
    {
        for (int i = 0; i < retangulos.Count; i++)
        {
            RectTransform rt = retangulos[i].gameObject.GetComponent<RectTransform>();
            float posY = -(tamanhoRetangulo.y + 10f) * i;
            rt.anchoredPosition = new Vector2(-10f, posY);
        }
    }
}