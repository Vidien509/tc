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
    }

    private List<RetanguloData> retangulos = new List<RetanguloData>();
    private float tempo = 0f;

    public Vector2 tamanhoRetangulo = new Vector2(200f, 15f);
    public float espessuraBorda = 4f;
    public float velocidadeCintilacao = 1f;
    public Color corBase = new Color(0.2f, 0.6f, 1f, 0.8f);

    void Update()
    {
        tempo += Time.deltaTime * velocidadeCintilacao;

        foreach (var retangulo in retangulos)
        {
            // Atualiza o efeito de degradê cintilante
            float hue = (Mathf.Sin(tempo + retangulo.hueOffset) + 1f) * 0.5f;
            Color corBorda = Color.HSVToRGB(hue, 0.8f, 1f);
            retangulo.material.SetColor("_BorderColor", corBorda);
        }
    }

    public GameObject CriarRetangulo(Color cor)
    {
        // Cria um novo objeto para o retângulo
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
        float posY = -(tamanhoRetangulo.y - 100f) * retangulos.Count;
        rectTransform.anchoredPosition = new Vector2(-10f, posY);

        // Cria um material com shader para borda com degradê
        Material material = new Material(Shader.Find("UI/Default"));
        material.EnableKeyword("UNITY_UI_ALPHACLIP");

        // Configura o material
        material.SetColor("_Color", cor);
        material.SetFloat("_BorderWidth", espessuraBorda);
        image.material = material;

        // Armazena os dados do retângulo
        RetanguloData data = new RetanguloData
        {
            gameObject = novoRetangulo,
            material = material,
            hueOffset = Random.Range(0f, Mathf.PI * 2f)
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