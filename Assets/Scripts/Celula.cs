using UnityEngine;
using TMPro;
using System;
using System.Collections;
using CodeMonkey.Utils;

public enum CellState { vazia, torre, nucleo, campoMultiplicacao}

public class Celula : MonoBehaviour
{
    public CellState state;
    public string value = "";
    private LineRenderer borderRenderer;
    private GameObject torreVisual;

    [SerializeField] private float borderWidth = 0.1f;
    [SerializeField] private float borderInset = 0.05f;
    [SerializeField] private float torreSize = 0.4f;

    private Color[] coresTorre = new Color[]
    {
        new Color(0.5f, 0.5f, 0.5f), // Basic
        new Color(0.0f, 0.7f, 1.0f), // Gelo
        new Color(1.0f, 0.4f, 0.0f), // Fogo
        new Color(0.8f, 0.0f, 1.0f)  // Plasma
    };

    private Material materialEspecial;
    private bool efeitoEspecialAtivo = false;
    private Vector3 escalaOriginal;
    private Material materialOriginal;
    private GameObject upgradePopup;

    private SpriteRenderer spriteRenderer;
    private Material glowMaterial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        glowMaterial = new Material(Shader.Find("Sprites/Default"));
        SetupBorderRenderer();
        SetupTorreVisual();
    }

    void Start()
    {
        // Crie o material para o efeito especial
        escalaOriginal = transform.localScale;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        materialOriginal = spriteRenderer.material;
        materialEspecial = new Material(Shader.Find("Sprites/Default"));
        upgradePopup = Resources.Load<GameObject>("Prefabs/Upgrade");
    }

    private void SetupBorderRenderer()
    {
        borderRenderer = gameObject.AddComponent<LineRenderer>();
        borderRenderer.positionCount = 5;
        borderRenderer.loop = true;
        borderRenderer.useWorldSpace = false;
        borderRenderer.material = new Material(Shader.Find("Sprites/Default"));
        UpdateBorderWidth();
    }

    private void SetupTorreVisual()
    {
        torreVisual = new GameObject("TorreVisual");
        torreVisual.transform.SetParent(transform);
        torreVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer spriteRenderer = torreVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateBasicTowerSprite();
        torreVisual.SetActive(false);
    }

    private Sprite CreateBasicTowerSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                if (y > x * 0.8f && y < 32 - x * 0.8f && x < 24)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (x >= 24 && x < 28 && y > 12 && y < 20)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.25f, 0.5f));
    }

    private void UpdateBorderWidth()
    {
        borderRenderer.startWidth = borderWidth;
        borderRenderer.endWidth = borderWidth;
    }

    public void SetComponent()
    {
        switch (state)
        {
            case CellState.torre:
                if (GetComponent<Torre>() == null)
                    gameObject.AddComponent<Torre>();
                break;
            case CellState.nucleo:
                if (GetComponent<Nucleo>() == null)
                    gameObject.AddComponent<Nucleo>();
                break;
            case CellState.campoMultiplicacao:
                if (GetComponent<CampoMultiplicacao>() == null)
                    gameObject.AddComponent<CampoMultiplicacao>();
                break;
        }
    }

    public void setValue(string newValue)
    {
        value = newValue;
    }

    public string getValue()
    {
        return value;
    }

    public void SetCellState(CellState newState)
    {
        state = newState;
        SetComponent();
        UpdateCellVisuals();
    }

    public CellState GetCellState()
    {
        return state;
    }

    public void UpdateCellVisuals()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        TextMeshPro texto = transform.GetChild(0).GetComponent<TextMeshPro>();
        Color corBorda;

        renderer.color = new Color(1f, 1f, 1f, 0f);

        if (value != "0")
        {
            if (state == CellState.nucleo)
            {
                Nucleo nucleo = GetComponent<Nucleo>();
                if (nucleo != null)
                {
                    texto.text = $"{nucleo.vida}/{nucleo.vidaMaxima}";
                }
            }
            else
            {
                texto.text = value;
            }
            texto.alignment = TextAlignmentOptions.Center; // Centraliza o texto
        }
        else
        {
            texto.text = "";
        }

        switch (state)
        {
            case CellState.vazia:
                corBorda = Color.white;
                torreVisual.SetActive(false);
                break;
            case CellState.torre:
                Torre torre = GetComponent<Torre>();
                if (torre != null)
                {
                    corBorda = coresTorre[(int)torre.tipo];
                    torreVisual.SetActive(true);
                    AtualizarVisualTorre(torre.tipo, torre.nivel);
                }
                else
                {
                    corBorda = coresTorre[0];
                    torreVisual.SetActive(false);
                }
                break;
            case CellState.nucleo:
                if (!UnityEngine.ColorUtility.TryParseHtmlString("#2EA951", out corBorda))
                {
                    corBorda = Color.green;
                    Debug.LogError("Erro ao converter a cor hexadecimal para núcleo!");
                }
                renderer.color = Color.green;
                torreVisual.SetActive(false);
                break;
            case CellState.campoMultiplicacao:
                corBorda = Color.cyan;
                torreVisual.SetActive(false);
                break;
            default:
                corBorda = Color.white;
                torreVisual.SetActive(false);
                break;
        }

        borderRenderer.startColor = corBorda;
        borderRenderer.endColor = corBorda;

        float halfWidth = 0.5f - borderInset;
        borderRenderer.SetPositions(new Vector3[]
        {
            new Vector3(-halfWidth, -halfWidth, 0),
            new Vector3(halfWidth, -halfWidth, 0),
            new Vector3(halfWidth, halfWidth, 0),
            new Vector3(-halfWidth, halfWidth, 0),
            new Vector3(-halfWidth, -halfWidth, 0)
        });

        UpdateBorderWidth();
    }

    public void AnimateCreation()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.3f).setEaseOutBack();
    }

    public void SetBorderWidth(float width)
    {
        borderWidth = width;
        UpdateBorderWidth();
    }

    public void SetBorderInset(float inset)
    {
        borderInset = inset;
        UpdateCellVisuals();
    }

    public void UpdateTorreDirection(Vector3 direction)
    {
        if (state == CellState.torre && torreVisual != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            torreVisual.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void AtualizarVisualTorre(TorreType tipo, int nivel)
    {
        if (state == CellState.torre && torreVisual != null)
        {
            SpriteRenderer spriteRenderer = torreVisual.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateTowerSprite(tipo);
            spriteRenderer.color = coresTorre[(int)tipo];

            // Aumentar o tamanho da torre com base no nível
            float escala = 1f + (nivel * 0.1f);
            torreVisual.transform.localScale = new Vector3(escala, escala, 1f);

            // Atualizar a borda
            borderRenderer.startColor = coresTorre[(int)tipo];
            borderRenderer.endColor = coresTorre[(int)tipo];

            // Adiciona um indicador visual de nível
            TextMeshPro textoNivel = transform.Find("TextoNivel")?.GetComponent<TextMeshPro>();
            if (textoNivel == null)
            {
                GameObject textoNivelObj = new GameObject("TextoNivel");
                textoNivelObj.transform.SetParent(transform);
                textoNivelObj.transform.localPosition = new Vector3(0.35f, 0.35f, -0.1f);
                textoNivel = textoNivelObj.AddComponent<TextMeshPro>();
                textoNivel.alignment = TextAlignmentOptions.Center;
                textoNivel.fontSize = 8;
            }

            // Define o texto do nível em numeração romana
            if (nivel < 5)
            {
                transform.GetComponent<Torre>().AtualizarTextoNivel();
                StopCoroutine("EfeitoArcoIrisTexto");
            }
            else
            {
                textoNivel.text = "S";
                StartCoroutine(EfeitoArcoIrisTexto(textoNivel));
            }
        }
    }

    private Sprite CreateTowerSprite(TorreType tipo)
    {
        Texture2D texture = new Texture2D(32, 32);
        Color cor = Color.white;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                switch (tipo)
                {
                    case TorreType.Basic:
                        if (y > x * 0.8f && y < 32 - x * 0.8f && x < 24)
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                    case TorreType.Gelo:
                        if ((x - 16) * (x - 16) + (y - 16) * (y - 16) <= 12 * 12)
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                    case TorreType.Fogo:
                        if (x >= 8 && x < 24 && y >= 16 - Math.Abs(x - 16) && y <= 16 + Math.Abs(x - 16))
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                    case TorreType.Plasma:
                        if ((x - 16) * (x - 16) + (y - 16) * (y - 16) <= 12 * 12 &&
                            ((x - 16) * (x - 16) + (y - 16) * (y - 16) > 8 * 8 ||
                             (x >= 14 && x <= 18) || (y >= 14 && y <= 18)))
                        {
                            texture.SetPixel(x, y, cor);
                        }
                        break;
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    public void AtivarEfeitoEspecialCor(Color cor)
    {
        StartCoroutine(EfeitoArcoIris(cor));
    }

    public void DesativarEfeitoEspecialCor()
    {
        StopCoroutine("EfeitoArcoIris");
    }

    public void AtivarEfeitoEspecial(TorreType tipo)
    {
        efeitoEspecialAtivo = true;
        Color corPredominante = GetCorPredominante(tipo);
        StartCoroutine(EfeitoArcoIris(corPredominante));
    }

    private Color GetCorPredominante(TorreType tipo)
    {
        switch (tipo)
        {
            case TorreType.Gelo:
                return Color.cyan;
            case TorreType.Fogo:
                return new Color(1f, 0.5f, 0f); // Laranja
            case TorreType.Plasma:
                return new Color(0.5f, 0f, 0.5f); // Roxo
            default:
                return Color.white;
        }
    }

    public void AnimarUpgrade()
    {
        StartCoroutine(AnimacaoUpgrade());
    }

    private IEnumerator AnimacaoUpgrade()
    {
        float duracao = 0.5f;
        float tempoDecorrido = 0f;
        Vector3 escalaOriginal = transform.localScale;
        Vector3 escalaAlvo = escalaOriginal * 1.2f;
        Instantiate(upgradePopup, transform.position, Quaternion.identity);

        // Criar um material de brilho
        Material glowMaterial = new Material(Shader.Find("Sprites/Default"));
        glowMaterial.SetColor("_Color", Color.white);
        glowMaterial.SetFloat("_EmissionPower", 2f);

        while (tempoDecorrido < duracao)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = tempoDecorrido / duracao;

            // Aumenta a escala
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaAlvo, progresso);

            // Altera a cor e o brilho
            Color corAtual = Color.Lerp(Color.white, Color.yellow, progresso);
            glowMaterial.SetColor("_Color", corAtual);
            spriteRenderer.material = glowMaterial;

            if (progresso >= 0.5f)
            {
                // Começa a diminuir a escala de volta ao normal
                transform.localScale = Vector3.Lerp(escalaAlvo, escalaOriginal, (progresso - 0.5f) * 2f);
            }

            yield return null;
        }

        // Restaura o material original
        spriteRenderer.material = materialOriginal;
        transform.localScale = escalaOriginal;
    }


    private IEnumerator EfeitoArcoIris(Color corPredominante)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = materialEspecial;

        float h, s, v;
        Color.RGBToHSV(corPredominante, out h, out s, out v);

        while (efeitoEspecialAtivo)
        {
            float hueShift = Mathf.PingPong(Time.time * 0.5f, 0.2f) - 0.1f;
            Color corAtual = Color.HSVToRGB((h + hueShift) % 1f, s, v);
            materialEspecial.color = corAtual;
            yield return null;
        }

        spriteRenderer.material = materialOriginal;
    }

    public void DesativarEfeitoEspecial()
    {
        efeitoEspecialAtivo = false;
    }

    private IEnumerator EfeitoArcoIrisTexto(TextMeshPro texto)
    {
        float h = 0;
        while (true)
        {
            h = (h + Time.deltaTime * 1.5f) % 1f;
            texto.color = Color.HSVToRGB(h, 1, 1);
            yield return null;
        }
    }
}

