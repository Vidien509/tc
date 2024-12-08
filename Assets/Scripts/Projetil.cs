using UnityEngine;

public enum ProjetilTipo { Basic, Gelo, Fogo, Plasma, Inimigo }

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
            TrailRenderer trail = GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
            }

            switch (tipo)
            {
                case ProjetilTipo.Basic:
                    rend.material.color = Color.white;
                    transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    trail.startColor = Color.white;
                    trail.endColor = new Color(1, 1, 1, 0);
                    break;
                case ProjetilTipo.Gelo:
                    rend.material.color = Color.cyan;
                    transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    trail.startColor = Color.cyan;
                    trail.endColor = new Color(0, 1, 1, 0);
                    break;
                case ProjetilTipo.Fogo:
                    rend.material.color = Color.red;
                    transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                    trail.startColor = Color.red;
                    trail.endColor = new Color(1, 0.5f, 0, 0);
                    break;
                case ProjetilTipo.Plasma:
                    rend.material.color = Color.magenta;
                    transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    trail.startColor = Color.magenta;
                    trail.endColor = new Color(1, 0, 1, 0);
                    break;
                case ProjetilTipo.Inimigo:
                    rend.material.color = Color.yellow;
                    transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    trail.startColor = Color.yellow;
                    trail.endColor = new Color(1, 1, 0, 0);
                    break;
            }

            trail.startWidth = 0.1f;
            trail.endWidth = 0.05f;
            trail.time = 0.5f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
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
        if (tipo == ProjetilTipo.Inimigo)
        {
            Nucleo nucleo = alvo.GetComponent<Nucleo>();
            if (nucleo != null)
            {
                nucleo.ReceberDano(dano);
            }
        }
        else
        {
            Inimigo inimigo = alvo.GetComponent<Inimigo>();
            if (inimigo != null)
            {
                switch (tipo)
                {
                    case ProjetilTipo.Basic:
                        inimigo.ReceberDano(dano);
                        break;
                    case ProjetilTipo.Gelo:
                        inimigo.ReceberDano(dano);
                        inimigo.Congelar(2f);
                        break;
                    case ProjetilTipo.Fogo:
                        inimigo.ReceberDano(dano);
                        inimigo.Queimar(3f, dano / 2);
                        break;
                    case ProjetilTipo.Plasma:
                        inimigo.ReceberDano(dano);
                        Collider2D[] inimigosProximos = Physics2D.OverlapCircleAll(transform.position, 2f);
                        foreach (Collider2D col in inimigosProximos)
                        {
                            Inimigo inimigoProximo = col.GetComponent<Inimigo>();
                            if (inimigoProximo != null && inimigoProximo != inimigo)
                            {
                                inimigoProximo.ReceberDano(dano / 2);
                            }
                        }
                        break;
                }
            }
        }
        Destroy(gameObject);
    }

    void DestroyAfterTime()
    {
        Destroy(gameObject);
    }
}

