using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    public float velocidadeBase = 15f;
    public float velocidade;
    public GameObject textoPopup;
    public GameController gameController;
    public GameObject projetilPrefab; // Novo: referência ao prefab do projetil
    private Queue<Vector3> waypoints;
    private Vector3 alvoAtual;
    private bool chegouNoNucleo = false;
    private Nucleo nucleoAlvo;
    private int vidaBase = 30;
    private int vida;
    private int fase;
    private bool temEscudo = false;
    private float tempoUltimoAtaqueDistancia = 0f;
    private float intervaloAtaqueDistancia;
    private int danoAtaqueDistancia = 5;

    public delegate void InimigoMortoHandler();
    public event InimigoMortoHandler onInimigoMorto;

    private void Start()
    {
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
        vida = vidaBase + (fase * 10);
        velocidade = velocidadeBase + (fase * 2f);
        temEscudo = fase % 3 == 0; // Adiciona escudo a cada 3 fases
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
        if (fase >= 5 && Time.time - tempoUltimoAtaqueDistancia > intervaloAtaqueDistancia)
        {
            AtaqueDistancia();
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

    public void ReceberDano(int dano)
    {
        if (temEscudo)
        {
            dano = Mathf.Max(1, dano / 2); // Reduz o dano pela metade se tiver escudo
            temEscudo = false; // Remove o escudo após absorver um ataque
        }

        vida -= dano;
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
}