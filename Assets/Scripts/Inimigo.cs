using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    public float velocidadeBase = 7f;
    public float velocidade;
    public GameObject textoPopup;
    public GameController gameController;
    public GameObject projetilPrefab;
    private Queue<Vector3> waypoints;
    private Vector3 alvoAtual;
    private bool chegouNoNucleo = false;
    private Nucleo nucleoAlvo;
    private int vidaBase;
    private int vidaMaxima;
    private int vida;
    private int fase;
    private bool temEscudo = false;
    private float tempoUltimoAtaqueDistancia = 0f;
    private float intervaloAtaqueDistancia;
    private int danoAtaqueDistancia = 5;

    private GameObject healthBarObject;
    private Transform healthBarFill;

    public delegate void InimigoMortoHandler();
    public event InimigoMortoHandler onInimigoMorto;

    private float velocidadeOriginal;
    private float tempoCongelado;
    private float tempoQueimando;
    private int danoQueimadura;

    private void Start()
    {
        vidaBase = 20;
        intervaloAtaqueDistancia = 5f;
        if (projetilPrefab == null)
        {
            projetilPrefab = Resources.Load<GameObject>("Prefabs/Projetil");
            if (projetilPrefab == null)
            {
                Debug.LogError("Prefab do projétil não encontrado na pasta Resources/Prefabs/Projetil!");
            }
        }
        gameController = FindAnyObjectByType<GameController>();
        AtualizarAtributos();
        CriarBarraDeVida();
        AtualizarBarraDeVida();
    }

    public void Configurar(Vector3 posicaoNucleo, Nucleo nucleo, int faseAtual)
    {
        nucleoAlvo = nucleo;
        fase = faseAtual;
        AtualizarAtributos();
        waypoints = GerarCaminho(transform.position, posicaoNucleo);
        if (waypoints.Count > 0)
        {
            alvoAtual = waypoints.Dequeue();
        }

    }

    void AtualizarAtributos()
    {
        vidaMaxima = vidaBase + (fase * 10);
        vida = vidaMaxima;
        velocidade = velocidadeBase + (fase * 2f);
        temEscudo = fase % 3 == 0; // Adiciona escudo a cada 3 fases
        AtualizarBarraDeVida();
    }

    void Update()
    {
        if (chegouNoNucleo || nucleoAlvo == null) return;

        transform.position = Vector3.MoveTowards(transform.position, alvoAtual, velocidade * Time.deltaTime);

        if (Vector3.Distance(transform.position, alvoAtual) < 0.1f)
        {
            if (waypoints.Count > 0)
            {
                alvoAtual = waypoints.Dequeue();
            }
            else
            {
                chegouNoNucleo = true;
                AtacarNucleo();
            }
        }

        // Ataque à distância
        if (fase >= 14 && Time.time - tempoUltimoAtaqueDistancia > intervaloAtaqueDistancia)
        {
            AtaqueDistancia();
        }

        if (tempoCongelado > 0)
        {
            tempoCongelado -= Time.deltaTime;
            if (tempoCongelado <= 0)
            {
                velocidade = velocidadeOriginal;
            }
        }

        if (tempoQueimando > 0)
        {
            tempoQueimando -= Time.deltaTime;
            ReceberDano((int)(danoQueimadura * Time.deltaTime));
        }
    }

    void AtacarNucleo()
    {
        if (nucleoAlvo != null)
        {
            int dano = 3 + (fase * 2);
            nucleoAlvo.ReceberDano(dano);
            Debug.Log($"Núcleo atacado! Dano: {dano}, Vida restante: {nucleoAlvo.vida}");
        }
        Destroy(gameObject);
    }

    void AtaqueDistancia()
    {
        if (nucleoAlvo != null && projetilPrefab != null)
        {
            GameObject projetilObj = Instantiate(projetilPrefab, transform.position, Quaternion.identity);
            Projetil projetil = projetilObj.GetComponent<Projetil>();
            if (projetil != null)
            {
                projetil.Configurar(nucleoAlvo.transform, danoAtaqueDistancia, ProjetilTipo.Inimigo);
                Debug.Log($"Projetil lançado em direção ao núcleo! Dano potencial: {danoAtaqueDistancia}");
            }
            else
            {
                Debug.LogError("Prefab do projetil não contém o componente Projetil!");
            }
            tempoUltimoAtaqueDistancia = Time.time;
        }
        else if (projetilPrefab == null)
        {
            Debug.LogError("Prefab do projetil não está configurado no Inimigo!");
        }
    }

    Queue<Vector3> GerarCaminho(Vector3 inicio, Vector3 destino)
    {
        Queue<Vector3> caminho = new Queue<Vector3>();
        Vector3 posAtual = inicio;
        Vector3 diferenca = destino - inicio;

        if (diferenca.x != 0)
        {
            Vector3 destinoX = new Vector3(destino.x, inicio.y, inicio.z);
            caminho.Enqueue(destinoX);
        }

        if (diferenca.y != 0)
        {
            Vector3 destinoY = new Vector3(destino.x, destino.y, inicio.z);
            caminho.Enqueue(destinoY);
        }

        return caminho;
    }

    private void CriarBarraDeVida()
    {
        // Criar o objeto pai da barra de vida
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(transform);
        healthBarObject.transform.localPosition = new Vector3(0, 1.2f, 0); // Ajuste a posi��o conforme necess�rio
        healthBarObject.transform.localRotation = Quaternion.identity;

        // Criar o fundo da barra de vida
        GameObject healthBarBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBarBackground.transform.SetParent(healthBarObject.transform);
        healthBarBackground.transform.localScale = new Vector3(1, 0.1f, 0.1f);
        healthBarBackground.transform.localPosition = Vector3.zero;
        healthBarBackground.GetComponent<Renderer>().material.color = Color.gray;

        // Criar a parte preenchida da barra de vida
        GameObject healthBarFillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBarFill = healthBarFillObject.transform;
        healthBarFill.SetParent(healthBarObject.transform);
        healthBarFill.localScale = new Vector3(1, 0.1f, 0.1f);
        healthBarFill.localPosition = Vector3.zero;
        healthBarFillObject.GetComponent<Renderer>().material.color = Color.green;

        // Desativar os colliders
        Destroy(healthBarBackground.GetComponent<Collider>());
        Destroy(healthBarFillObject.GetComponent<Collider>());
    }
    private void AtualizarBarraDeVida()
    {
        if (healthBarFill != null)
        {
            float percentualVida = ((float)vida / vidaMaxima) * 2;
            healthBarFill.localScale = new Vector3(percentualVida, 1f, 1f);
            healthBarFill.localPosition = new Vector3((percentualVida - 1) / 2, 0, 0);

            // Atualizar a cor da barra de vida
            Renderer renderer = healthBarFill.GetComponent<Renderer>();
            renderer.material.color = Color.Lerp(Color.red, Color.green, percentualVida);
        }
    }
    public void ReceberDano(int dano)
    {
        if (temEscudo)
        {
            dano = Mathf.Max(1, dano / 2); // Reduz o dano pela metade se tiver escudo
            temEscudo = false; // Remove o escudo após absorver um ataque
        }

        vida -= dano;
        AtualizarBarraDeVida();

        if (vida <= 0)
        {
            GameObject textoPopupI = Instantiate(textoPopup, transform.position, Quaternion.identity);
            TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
            text.color = Color.green;
            int recompensa = 3 + (fase * 2);
            text.text = $"+ $ {recompensa}";
            gameController.recurso += recompensa;
            onInimigoMorto?.Invoke();
            Destroy(gameObject);
        }
    }

    public void Congelar(float duracao)
    {
        if (tempoCongelado <= 0)
        {
            velocidadeOriginal = velocidade;
        }
        tempoCongelado = Mathf.Max(tempoCongelado, duracao);
        velocidade = velocidadeOriginal * 0.5f;
    }

    public void Queimar(float duracao, int danoPorSegundo)
    {
        tempoQueimando = Mathf.Max(tempoQueimando, duracao);
        danoQueimadura = danoPorSegundo;
    }
}

