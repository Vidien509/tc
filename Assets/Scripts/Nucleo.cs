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
            _vida = Mathf.Min(value, vidaMaxima);
            atualizaTextVida();
        }
    }

    public int vidaMaxima;
    public float regeneracaoVida;
    public float escudo;
    public int nivel;

    public Celula cel;
    private TextMeshPro texto;

    // Novos atributos para upgrades
    public int nivelVidaMaxima;
    public int nivelRegeneracao;
    public int nivelEscudo;
    public float timerRegen;

    void Start()
    {
        timerRegen = 0f;
        texto = transform.GetChild(0).GetComponent<TextMeshPro>();
        texto.fontSize = 3.5f;
        cel = transform.GetComponent<Celula>();
        int aux;
        vidaMaxima = int.TryParse(cel.getValue(), out aux) ? aux : 100;
        vida = vidaMaxima;
        regeneracaoVida = 0f;
        escudo = 0f;
        nivel = 1;
        nivelVidaMaxima = 0;
        nivelRegeneracao = 0;
        nivelEscudo = 0;

        GameObject textoNivelObj = new GameObject("TextoNivel");
        textoNivelObj.transform.SetParent(transform);
        textoNivelObj.transform.localPosition = new Vector3(0.4f, 0.4f, -0.1f);
        TextMeshPro textoNivel = textoNivelObj.AddComponent<TextMeshPro>();
        textoNivel.alignment = TextAlignmentOptions.TopRight;
        textoNivel.fontSize = 3;
        textoNivel.color = Color.white;

        AtualizarTextoNivel();
    }

    void Update()
    {
        // Regeneração de vida
        if (vida < vidaMaxima)
        {
            timerRegen += Time.deltaTime;
            if(timerRegen >= 1 && regeneracaoVida > 0)
            {
                vida += (int)(regeneracaoVida * 2);
                timerRegen = 0;
                IndicarVida();
            }
        }
    }

    public void ReceberDano(int dano)
    {
        int danoReduzido = Mathf.Max(dano - (int)escudo, 1);
        vida -= danoReduzido;
        if (vida <= 0)
        {
            Debug.Log("O núcleo foi destruído!");
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(IndicarDano());
        }
    }

    private void atualizaTextVida()
    {
        cel.setValue(vida.ToString());
        texto.text = cel.getValue() + "/\n" + vidaMaxima;
    }

    public void AdicionarVida(int quantidade)
    {
        vida += quantidade;
    }

    public void UpgradeVidaMaxima()
    {
        nivel++;
        nivelVidaMaxima++;
        vidaMaxima += 50;
        AtualizarTextoNivel();
    }

    public void UpgradeRegeneracao()
    {
        nivel++;
        nivelRegeneracao++;
        regeneracaoVida += 0.5f;
        AtualizarTextoNivel();
    }

    public void UpgradeEscudo()
    {
        nivel++;
        nivelEscudo++;
        escudo += 5f;
        AtualizarTextoNivel();
    }

    public void AvancarFase()
    {
        nivel++;
        vidaMaxima += 25;
        regeneracaoVida += 0.1f;
        escudo += 1f;
        vida = vidaMaxima;
        AtualizarTextoNivel();
    }

    private IEnumerator IndicarDano()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;
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

        renderer.color = originalColor;
    }

    private IEnumerator IndicarVida()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;
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

    private void AtualizarTextoNivel()
    {
        string nivelTexto = new string('I', nivel);
        TextMeshPro textoNivel = transform.Find("TextoNivel").GetComponent<TextMeshPro>();
        textoNivel.text = nivelTexto;
    }
}

