using UnityEngine;
using System.Collections;

public class Popup : MonoBehaviour
{
    public float duracaoTotal = 1f;
    public float velocidadeSubida = 1f;
    public float intensidadeBrilho = 1.5f;
    public float velocidadePiscada = 5f;
    public float escalaPulsacao = 0.2f;
    public float velocidadePulsacao = 3f;

    private SpriteRenderer spriteRenderer;
    private Vector3 posicaoInicial;
    private Vector3 escalaInicial;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        posicaoInicial = transform.position;
        escalaInicial = transform.localScale;
    }

    private void OnEnable()
    {
        StartCoroutine(AnimarPopup());
    }

    private IEnumerator AnimarPopup()
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoTotal)
        {
            // Movimento para cima
            transform.Translate(Vector3.up * velocidadeSubida * Time.deltaTime);

            // Piscada (brilho)
            float intensidadeAtual = Mathf.PingPong(Time.time * velocidadePiscada, intensidadeBrilho);
            spriteRenderer.material.SetFloat("_Brightness", intensidadeAtual);

            // Pulsação (escala)
            transform.localScale = escalaInicial * 1;

            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        // Fade out
        float alfa = 1f;
        Color corInicial = spriteRenderer.color;

        while (alfa > 0f)
        {
            alfa -= Time.deltaTime * 2f; // 0.5 segundos para desaparecer
            spriteRenderer.color = new Color(corInicial.r, corInicial.g, corInicial.b, alfa);
            yield return null;
        }

        // Resetar e desativar
        transform.position = posicaoInicial;
        transform.localScale = escalaInicial;
        spriteRenderer.color = corInicial;
        gameObject.SetActive(false);
    }
}

