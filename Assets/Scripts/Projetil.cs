using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEngine.GraphicsBuffer;

public enum ProjetilTipo { Basic, Gelo, Fogo, Plasma, Inimigo, Duplicado}

public class Projetil : MonoBehaviour
{
    public float velocidade;
    private Transform alvo;
    private int dano;
    private bool semAlvo = false;
    private Vector3 direcaoAtual;
    public ProjetilTipo tipo;
    private Vector3 direction;

    public int multiplier = 1;
    private bool isOriginal = true;
    private bool jaMultiplicado = false;
    private Projetil originalProjectile;
    private List<Projetil> multipliedProjectiles = new List<Projetil>();
    public float orbitRadius = 1f;
    public float orbitSpeed = 2f;
    public float offsetDistance = 0.5f;
    private Vector3 offset;

    public void Initialize(Vector3 direction, Transform target, bool isOriginal = true, Vector3? offset = null)
    {
        this.direction = direction.normalized;
        this.alvo = target;
        this.isOriginal = isOriginal;
        if (isOriginal)
        {
            originalProjectile = this;
        }
        else
        {
            this.offset = offset ?? UnityEngine.Random.insideUnitSphere * offsetDistance;
            this.offset.z = 0; // Garante que o offset seja apenas em 2D
        }
    }

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
                case ProjetilTipo.Duplicado:
                    rend.material.color = Color.blue;
                    transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    trail.startColor = Color.blue;
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

        if (isOriginal)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, velocidade * Time.deltaTime);
            if (hit.collider != null)
            {
                CampoMultiplicacao mff = hit.collider.GetComponent<CampoMultiplicacao>();
                if (mff != null)
                {
                    mff.MultiplyProjectile(this);
                }
            }
            Vector3 direcao = alvo.position - transform.position;
            direcaoAtual = direcao.normalized;
            transform.position += direcaoAtual * velocidade * Time.deltaTime;

            if (Vector3.Distance(transform.position, alvo.position) < 0.2f)
            {
                AlvoAtingido();
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, alvo.position) > 0.5f && originalProjectile != null)
            {
                // Segue o projétil original com offset
                Vector3 desiredPosition = originalProjectile.transform.position + offset;
                transform.position = Vector3.MoveTowards(transform.position, desiredPosition, velocidade * Time.deltaTime);
            }
            else
            {
                Vector3 direcao = alvo.position - transform.position;
                direcaoAtual = direcao.normalized;
                transform.position += direcaoAtual * velocidade * Time.deltaTime;
            }
        }
    }

    public void Multiply(float factor)
    {
        if (!jaMultiplicado && isOriginal)
        {
            jaMultiplicado = true;
            int newProjectiles = Mathf.FloorToInt(factor) - 1;
            multiplier *= Mathf.FloorToInt(factor);

            for (int i = 0; i < newProjectiles; i++)
            {
                Projetil newProjectile = Instantiate(this, transform.position, Quaternion.identity);
                newProjectile.Initialize(direction, alvo, false);
                newProjectile.multiplier = this.multiplier;
                newProjectile.jaMultiplicado = true;
                newProjectile.originalProjectile = this;
                newProjectile.tipo = ProjetilTipo.Duplicado;
                multipliedProjectiles.Add(newProjectile);
            }
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
                        inimigo.ReceberDano(dano, Color.white);
                        break;
                    case ProjetilTipo.Gelo:
                        inimigo.ReceberDano(dano, Color.cyan);
                        inimigo.Congelar(2f);
                        break;
                    case ProjetilTipo.Fogo:
                        inimigo.ReceberDano(dano, Color.HSVToRGB(0.09f, 1f, 1f));
                        inimigo.Queimar(3f, dano / 2);
                        break;
                    case ProjetilTipo.Plasma:
                        inimigo.ReceberDano(dano, Color.magenta);
                        Collider2D[] inimigosProximos = Physics2D.OverlapCircleAll(transform.position, 2f);
                        foreach (Collider2D col in inimigosProximos)
                        {
                            Inimigo inimigoProximo = col.GetComponent<Inimigo>();
                            if (inimigoProximo != null && inimigoProximo != inimigo)
                            {
                                inimigoProximo.ReceberDano(dano / 2, Color.magenta);
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

