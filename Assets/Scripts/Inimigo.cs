using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum InimigoTipo { Normal, PlasmaResistente, Boss }

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

    public InimigoTipo tipo = InimigoTipo.Normal;

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
        CreateEnemyVisual();
    }

    private void CreateEnemyVisual()
    {
        GameObject enemyVisual = new GameObject("EnemyVisual");
        enemyVisual.transform.SetParent(transform);
        enemyVisual.transform.localPosition = Vector3.zero;

        SpriteRenderer renderer = enemyVisual.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateEnemySprite();
        renderer.sortingOrder = 1;

        // Rotate the enemy to face right
        //enemyVisual.transform.rotation = Quaternion.Euler(0, 0, -90);

        // Set the size (adjust as needed)
        enemyVisual.transform.localScale = Vector3.one * 1f;
    }

    private Sprite CreateEnemySprite()
    {
        Texture2D texture = new Texture2D(128, 128);
        Color[] colors = new Color[128 * 128];

        switch (tipo)
        {
            case InimigoTipo.Normal:
                CreateNormalEnemySprite(colors);
                break;
            case InimigoTipo.PlasmaResistente:
                CreatePlasmaResistantEnemySprite(colors);
                break;
            case InimigoTipo.Boss:
                CreateBossEnemySprite(colors);
                break;
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.2f));
    }

    private void CreateNormalEnemySprite(Color[] colors)
    {
        Color bodyColor = Color.red;
        Color wingColor = new Color(0.8f, 0.2f, 0.2f);
        Color windowColor = new Color(0.9f, 0.9f, 1f);

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                if (x > y / 2 && x < 128 - y / 2 && y < 96)
                {
                    if (y < 32 && x > 32 && x < 96)
                    {
                        colors[y * 128 + x] = wingColor;
                    }
                    else if (y >= 32 && y < 64 && x > 48 && x < 80)
                    {
                        colors[y * 128 + x] = windowColor;
                    }
                    else
                    {
                        colors[y * 128 + x] = bodyColor;
                    }
                }
                else
                {
                    colors[y * 128 + x] = Color.clear;
                }
            }
        }
    }

    private void CreatePlasmaResistantEnemySprite(Color[] colors)
    {
        Color bodyColor = new Color(0.5f, 0, 0.5f);
        Color wingColor = new Color(0.7f, 0, 0.7f);
        Color shieldColor = new Color(0, 0.7f, 1f, 0.5f);

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                if (x > y / 2 && x < 128 - y / 2 && y < 96)
                {
                    if (y < 32 && x > 32 && x < 96)
                    {
                        colors[y * 128 + x] = wingColor;
                    }
                    else
                    {
                        colors[y * 128 + x] = bodyColor;
                    }
                }
                else if (x > (y - 16) / 2 && x < 128 - (y - 16) / 2 && y < 112)
                {
                    colors[y * 128 + x] = shieldColor;
                }
                else
                {
                    colors[y * 128 + x] = Color.clear;
                }
            }
        }
    }

    private void CreateBossEnemySprite(Color[] colors)
    {
        Color bodyColor = Color.red;
        Color accentColor = Color.yellow;

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64));
                if (distanceFromCenter < 60)
                {
                    colors[y * 128 + x] = Color.Lerp(bodyColor, accentColor, Mathf.PingPong(distanceFromCenter * 0.1f + Time.time, 1));
                }
                else
                {
                    colors[y * 128 + x] = Color.clear;
                }
            }
        }
    }

    public void Configurar(Vector3 posicaoNucleo, Nucleo nucleo, int faseAtual, InimigoTipo tipoInimigo)
    {
        nucleoAlvo = nucleo;
        fase = faseAtual;
        tipo = tipoInimigo;
        AtualizarAtributos();
        waypoints = GerarCaminho(transform.position, posicaoNucleo);
        if (waypoints.Count > 0)
        {
            alvoAtual = waypoints.Dequeue();
        }
        CreateEnemyVisual();
    }

    void AtualizarAtributos()
    {
        switch (tipo)
        {
            case InimigoTipo.Normal:
                vidaMaxima = vidaBase + (fase * 10);
                velocidade = velocidadeBase + (fase * 2f);
                break;
            case InimigoTipo.PlasmaResistente:
                vidaMaxima = (vidaBase + (fase * 10)) * 2;
                velocidade = (velocidadeBase + (fase * 2f)) * 0.8f;
                break;
            case InimigoTipo.Boss:
                vidaMaxima = (vidaBase + (fase * 10)) * 10; // Increased from 5 to 10
                velocidade = (velocidadeBase + (fase * 2f)) * 0.3f; // Decreased from 0.5f to 0.3f
                break;
        }
        vida = vidaMaxima;
        temEscudo = fase % 3 == 0; // Adiciona escudo a cada 3 fases
        AtualizarBarraDeVida();
    }

    void Update()
    {
        if (chegouNoNucleo || nucleoAlvo == null) return;

        transform.position = Vector3.MoveTowards(transform.position, alvoAtual, velocidade * Time.deltaTime);

        // Calculate the direction vector
        Vector3 direction = (alvoAtual - transform.position).normalized;

        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotate the enemy to face the movement direction
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

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
            if (tipo == InimigoTipo.Boss)
            {
                dano *= 3;
            }
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
                int danoProjeto = danoAtaqueDistancia;
                if (tipo == InimigoTipo.Boss)
                {
                    danoProjeto *= 2;
                }
                projetil.Configurar(nucleoAlvo.transform, danoProjeto, ProjetilTipo.Inimigo);
                Debug.Log($"Projetil lançado em direção ao núcleo! Dano potencial: {danoProjeto}");
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
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(transform);
        healthBarObject.transform.localPosition = new Vector3(0, 1.2f, 0);
        healthBarObject.transform.localRotation = Quaternion.identity;

        GameObject healthBarBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBarBackground.transform.SetParent(healthBarObject.transform);
        healthBarBackground.transform.localScale = new Vector3(1, 0.05f, 0.1f);
        healthBarBackground.transform.localPosition = Vector3.zero;
        healthBarBackground.GetComponent<Renderer>().material.color = Color.gray;

        GameObject healthBarFillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBarFill = healthBarFillObject.transform;
        healthBarFill.SetParent(healthBarObject.transform);
        healthBarFill.localScale = new Vector3(1, 0.05f, 0.1f);
        healthBarFill.localPosition = Vector3.zero;
        healthBarFillObject.GetComponent<Renderer>().material.color = Color.green;

        Destroy(healthBarBackground.GetComponent<Collider>());
        Destroy(healthBarFillObject.GetComponent<Collider>());

        if (tipo == InimigoTipo.Boss)
        {
            healthBarObject.transform.localScale = new Vector3(2, 0.5f, 1);
        }
    }

    private void AtualizarBarraDeVida()
    {
        if (healthBarFill != null)
        {
            float percentualVida = ((float)vida / vidaMaxima) * 2;
            healthBarFill.localScale = new Vector3(percentualVida, 0.5f, 1f);
            healthBarFill.localPosition = new Vector3((percentualVida - 1) / 2, 0, 0);

            Renderer renderer = healthBarFill.GetComponent<Renderer>();
            renderer.material.color = Color.Lerp(Color.red, Color.green, percentualVida);
        }
    }

    public void ReceberDano(int dano)
    {
        if (tipo == InimigoTipo.PlasmaResistente && dano > 0)
        {
            dano = Mathf.Max(1, dano / 2);
        }

        if (temEscudo)
        {
            dano = Mathf.Max(1, dano / 2);
            temEscudo = false;
        }

        vida -= dano;
        AtualizarBarraDeVida();
        ShowHitEffect();
        if (vida <= 0)
        {
            GameObject textoPopupI = Instantiate(textoPopup, transform.position, Quaternion.identity);
            TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
            text.color = Color.green;
            int recompensa = CalcularRecompensa();
            text.text = $"+ $ {recompensa}";
            gameController.recurso += recompensa;
            onInimigoMorto?.Invoke();
            Destroy(gameObject);
        }
    }

    private int CalcularRecompensa()
    {
        int recompensaBase = 3 + (fase * 2);
        switch (tipo)
        {
            case InimigoTipo.Normal:
                return recompensaBase;
            case InimigoTipo.PlasmaResistente:
                return recompensaBase * 2;
            case InimigoTipo.Boss:
                return recompensaBase * 5;
            default:
                return recompensaBase;
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
        ShowIceEffect();
    }

    public void Queimar(float duracao, int danoPorSegundo)
    {
        tempoQueimando = Mathf.Max(tempoQueimando, duracao);
        danoQueimadura = danoPorSegundo;
        ShowFireEffect();
    }

    private void ShowHitEffect()
    {
        LeanTween.color(gameObject, Color.white, 0.1f).setLoopPingPong(1);
    }

    private void ShowIceEffect()
    {
        LeanTween.color(gameObject, Color.cyan, 0.5f).setLoopPingPong(1);
    }

    private void ShowFireEffect()
    {
        LeanTween.color(gameObject, Color.red, 0.5f).setLoopPingPong(1);
    }
}

