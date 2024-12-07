using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjetilTipo { Torre, Inimigo }

public class Projetil : MonoBehaviour
{
    public float velocidade;
    private Transform alvo;
    private int dano;
    private bool semAlvo = false;
    private Vector3 direcaoAtual;
    public ProjetilTipo tipo;


    private void Start()
    {
        velocidade = 40f;
        AjustarAparencia();
    }

    public void Configurar(Transform alvo, int dano, ProjetilTipo tipo)
    {
        this.alvo = alvo;
        this.dano = dano;
        this.tipo = tipo;

        if (alvo == null)
        {
            direcaoAtual = transform.up;
        }

        AjustarAparencia();
    }

    void AjustarAparencia()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            TrailRenderer trail;
            if (gameObject.GetComponent<TrailRenderer>() == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
            }
            else
            {
                trail = gameObject.GetComponent<TrailRenderer>();
            }
            switch (tipo)
            {
                case ProjetilTipo.Torre:
                    rend.material.color = Color.blue;
                    transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                    trail.startWidth = 0.1f;
                    trail.endWidth = 0.05f;
                    trail.time = 0.5f;
                    trail.material = new Material(Shader.Find("Sprites/Default"));
                    trail.startColor = Color.blue;
                    trail.endColor = new Color(0, 0, 1, 0); // Vermelho transparente
                    break;
                case ProjetilTipo.Inimigo:
                    rend.material.color = Color.red;
                    transform.localScale = new Vector3(1.2f, 1.5f, 1.5f); // Forma mais alongada
                    trail = gameObject.AddComponent<TrailRenderer>();
                    trail.startWidth = 0.1f;
                    trail.endWidth = 0.05f;
                    trail.time = 0.5f;
                    trail.material = new Material(Shader.Find("Sprites/Default"));
                    trail.startColor = Color.red;
                    trail.endColor = new Color(1, 0, 0, 0); // Vermelho transparente
                    break;
            }
        }
    }

    void Update()
    {
        if (alvo == null)
        {
            if (!semAlvo)
            {
                semAlvo = true;
                direcaoAtual = direcaoAtual != Vector3.zero ? direcaoAtual : transform.up;
                Invoke(nameof(DestroyAfterTime), 2f);
            }

            transform.position += direcaoAtual.normalized * velocidade * Time.deltaTime;
            return;
        }

        Vector3 direcao = alvo.position - transform.position;
        direcaoAtual = direcao.normalized;
        transform.position += direcaoAtual * velocidade * Time.deltaTime;

        if (Vector3.Distance(transform.position, alvo.position) < 0.2f)
        {
            AlvoAtingido();
        }
    }

    void AlvoAtingido()
    {
        switch (tipo)
        {
            case ProjetilTipo.Torre:
                Inimigo inimigo = alvo.GetComponent<Inimigo>();
                if (inimigo != null)
                {
                    inimigo.ReceberDano(dano);
                }
                break;
            case ProjetilTipo.Inimigo:
                Nucleo nucleo = alvo.GetComponent<Nucleo>();
                if (nucleo != null)
                {
                    nucleo.ReceberDano(dano);
                }
                break;
        }
        Destroy(gameObject);
    }

    void DestroyAfterTime()
    {
        Destroy(gameObject);
    }
}