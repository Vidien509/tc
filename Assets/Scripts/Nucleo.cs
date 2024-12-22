using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Nucleo : MonoBehaviour
{
    private int _vida;
    public int vida
    {
        get { return _vida; }
        set
        {
            int oldVida = _vida;
            _vida = Mathf.Min(value, vidaMaxima);
            AtualizarTextoVida();
            if (_vida > oldVida)
            {
                StartCoroutine(IndicarVida());
            }
        }
    }

    public int vidaMaxima;
    public float regeneracaoVida;
    public int nivel;

    Color originalColor;
    public Celula cel;
    private TextMeshPro textoVida;
    private TextMeshPro textoEscudo;
    private TextMeshPro textoNivel;

    // Atributos para upgrades
    public int nivelVidaMaxima;
    public int nivelRegeneracao;
    public int nivelEscudo;
    public int nivelFortificacao;
    public float timerRegen;

    // Atributos para o escudo
    private float escudoAtual;

    // Pre�os base para upgrades
    private int precoBaseVidaMaxima = 35;
    private int precoBaseRegeneracao = 25;
    private int precoBaseEscudo = 15;
    private int precoBaseFortificacao = 30;

    private float bloomIntensity = 1.5f;
    private Material bloomMaterial;

    void Start()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        originalColor = renderer.color;
        timerRegen = 0f;
        textoVida = transform.GetChild(0).GetComponent<TextMeshPro>();
        textoVida.fontSize = 3.5f;
        cel = transform.GetComponent<Celula>();
        int aux;
        vidaMaxima = int.TryParse(cel.getValue(), out aux) ? aux : 100;
        vida = vidaMaxima;
        regeneracaoVida = 0f;
        nivel = 1;
        nivelVidaMaxima = 0;
        nivelRegeneracao = 0;
        nivelEscudo = 0;
        nivelFortificacao = 0;
        escudoAtual = 0f;

        bloomMaterial = new Material(Shader.Find("Hidden/BloomShader"));
        bloomMaterial.SetFloat("_BloomIntensity", bloomIntensity);

        CriarTextoEscudo();
        CriarTextoNivel();
        AtualizarTextoVida();
        AtualizarTextoEscudo();
        AtualizarTextoNivel();
    }

    void Update()
    {
        // Regenera��o de vida
        if (vida < vidaMaxima)
        {
            timerRegen += Time.deltaTime;
            if (timerRegen >= 1 && regeneracaoVida > 0)
            {
                vida += (int)(regeneracaoVida * 2);
                timerRegen = 0;
            }
        }
    }

    public void ReceberDano(int dano)
    {
        int danoReduzido = Mathf.Max(dano - (nivelFortificacao * 2), 1);

        if (escudoAtual > 0)
        {
            float danoAoEscudo = danoReduzido * 2;
            if (danoAoEscudo <= escudoAtual)
            {
                escudoAtual -= danoAoEscudo;
                danoReduzido = 0;
            }
            else
            {
                danoReduzido = Mathf.CeilToInt((danoAoEscudo - escudoAtual) / 2f);
                escudoAtual = 0;
            }
            AtualizarTextoEscudo();
            StartCoroutine(IndicarDano());
        }

        if (danoReduzido > 0)
        {
            vida -= danoReduzido;
            if (vida <= 0)
            {
                Debug.Log("O n�cleo foi destru�do!");
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(IndicarDano());
            }
        }
        AtualizarTextoVida();
    }

    private void AtualizarTextoVida()
    {
        cel.setValue(vida.ToString());
        textoVida.text = $"{vida}/{vidaMaxima}";
    }

    private void AtualizarTextoEscudo()
    {
        textoEscudo.text = $"{Mathf.CeilToInt(escudoAtual)}";
    }

    public void AdicionarVida(int quantidade)
    {
        vida += quantidade;
    }

    public void UpgradeVidaMaxima()
    {
        nivelVidaMaxima++;
        vidaMaxima += 50;
        AtualizarTextoNivel();
        AtualizarTextoVida();
    }

    public void UpgradeRegeneracao()
    {
        nivelRegeneracao++;
        regeneracaoVida += 0.5f;
        AtualizarTextoNivel();
    }

    public void UpgradeEscudo()
    {
        nivelEscudo++;
        escudoAtual += 20 + nivelEscudo*5;
        AtualizarTextoNivel();
        AtualizarTextoEscudo();
        StartCoroutine(IndicarDano());
    }

    public void UpgradeFortificacao()
    {
        nivelFortificacao++;
        AtualizarTextoNivel();
    }

    public void AvancarFase()
    {
        nivel++;
        vidaMaxima += 10 + nivelEscudo * 5;
        regeneracaoVida += 0.1f;
        vida = vidaMaxima;
        AtualizarTextoNivel();
        AtualizarTextoVida();
    }

    private IEnumerator IndicarDano()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (escudoAtual > 0)
        {
            Debug.Log("INDICAR DANO ESCUDO");
            renderer.color = Color.blue;
            yield return new WaitForSeconds(0.3f);
            float fadeDuration = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                renderer.color = Color.Lerp(Color.blue, originalColor, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            Debug.Log("INDICAR DANO VIDA");
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            float fadeDuration = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                renderer.color = Color.Lerp(Color.red, originalColor, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        renderer.color = originalColor;
    }

    private IEnumerator IndicarVida()
    {
        Debug.Log("INDICAR VIDA");
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            renderer.color = Color.Lerp(Color.green, originalColor, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        renderer.color = originalColor;
    }

    private void CriarTextoEscudo()
    {
        GameObject textoEscudoObj = new GameObject("TextoEscudo");
        textoEscudoObj.transform.SetParent(transform);
        textoEscudoObj.transform.localPosition = new Vector3(0, -0.3f, -0.1f);
        textoEscudoObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        textoEscudo = textoEscudoObj.AddComponent<TextMeshPro>();
        textoEscudo.alignment = TextAlignmentOptions.Center;
        textoEscudo.fontSize = 14f;
        textoEscudo.color = Color.cyan;
    }

    private void CriarTextoNivel()
    {
        GameObject textoNivelObj = new GameObject("TextoNivel");
        textoNivelObj.transform.SetParent(transform);
        textoNivelObj.transform.localPosition = new Vector3(0.4f, 0.4f, -0.1f);
        textoNivel = textoNivelObj.AddComponent<TextMeshPro>();
        textoNivel.alignment = TextAlignmentOptions.TopRight;
        textoNivel.fontSize = 3;
        textoNivel.color = Color.white;
    }

    private void AtualizarTextoNivel()
    {
        textoNivel.text = $"N{nivel}";
    }

    public int GetPrecoUpgradeVidaMaxima()
    {
        return Mathf.RoundToInt(precoBaseVidaMaxima * Mathf.Pow(1.5f, nivelVidaMaxima));
    }

    public int GetPrecoUpgradeRegeneracao()
    {
        return Mathf.RoundToInt(precoBaseRegeneracao * Mathf.Pow(1.5f, nivelRegeneracao));
    }

    public int GetPrecoUpgradeEscudo()
    {
        return Mathf.RoundToInt(precoBaseEscudo * Mathf.Pow(1.5f, nivelEscudo));
    }

    public int GetPrecoUpgradeFortificacao()
    {
        return Mathf.RoundToInt(precoBaseFortificacao * Mathf.Pow(1.5f, nivelFortificacao));
    }
}

