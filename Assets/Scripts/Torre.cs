using System.Collections;
using UnityEngine;

public enum TorreType { Basic, Gelo, Fogo, Plasma }

public class Torre : MonoBehaviour
{
    public float alcance;
    public float intervaloAtaque;
    public int dano;
    public GameObject prefabProjetil;
    public int nivel = 0;
    public TorreType tipo = TorreType.Basic;

    private Inimigo alvoAtual;
    private Celula celula;

    private void Awake()
    {
        if (prefabProjetil == null)
        {
            prefabProjetil = Resources.Load<GameObject>("Prefabs/Projetil");
            if (prefabProjetil == null)
            {
                Debug.LogError("Prefab do projétil não encontrado na pasta Resources/Prefabs/Projetil!");
            }
        }
        celula = GetComponent<Celula>();
    }

    void Start()
    {
        AtualizarAtributos();
        StartCoroutine(AtaqueContinuo());
    }

    void Update()
    {
        alvoAtual = DetectarInimigoMaisProximo();
        if (alvoAtual != null && celula != null)
        {
            Vector3 direcao = alvoAtual.transform.position - transform.position;
            celula.UpdateTorreDirection(direcao);
        }
    }

    public void SetTorreType(TorreType novoTipo)
    {
        tipo = novoTipo;
        nivel = 0;
        AtualizarAtributos();
        celula.AtualizarVisualTorre(tipo, nivel);
    }

    public void Upgrade()
    {
        if (nivel < 3)
        {
            nivel++;
            AtualizarAtributos();
            celula.AtualizarVisualTorre(tipo, nivel);
        }
    }

    private void AtualizarAtributos()
    {
        switch (tipo)
        {
            case TorreType.Basic:
                alcance = 20f + (nivel * 5f);
                intervaloAtaque = 1.5f - (nivel * 0.25f);
                dano = 10 + (nivel * 5);
                break;
            case TorreType.Gelo:
                alcance = 15f + (nivel * 3f);
                intervaloAtaque = 2f - (nivel * 0.3f);
                dano = 5 + (nivel * 3);
                break;
            case TorreType.Fogo:
                alcance = 18f + (nivel * 4f);
                intervaloAtaque = 1.2f - (nivel * 0.2f);
                dano = 8 + (nivel * 4);
                break;
            case TorreType.Plasma:
                alcance = 25f + (nivel * 6f);
                intervaloAtaque = 2.5f - (nivel * 0.4f);
                dano = 15 + (nivel * 7);
                break;
        }
    }

    IEnumerator AtaqueContinuo()
    {
        while (true)
        {
            if (alvoAtual != null)
            {
                Atacar(alvoAtual);
            }
            yield return new WaitForSeconds(intervaloAtaque);
        }
    }

    Inimigo DetectarInimigoMaisProximo()
    {
        Inimigo[] inimigos = FindObjectsOfType<Inimigo>();
        Inimigo inimigoMaisProximo = null;
        float menorDistancia = Mathf.Infinity;

        foreach (Inimigo inimigo in inimigos)
        {
            if (inimigo == null) continue;

            float distancia = Vector3.Distance(transform.position, inimigo.transform.position);
            if (distancia < menorDistancia && distancia <= alcance)
            {
                menorDistancia = distancia;
                inimigoMaisProximo = inimigo;
            }
        }

        return inimigoMaisProximo;
    }

    void Atacar(Inimigo inimigo)
    {
        if (inimigo == null) return;

        if (prefabProjetil != null)
        {
            LancarProjetil(inimigo.transform);
        }
        else
        {
            inimigo.ReceberDano(dano);
        }
    }

    void LancarProjetil(Transform alvo)
    {
        GameObject projetil = Instantiate(prefabProjetil, transform.position, Quaternion.identity);
        Projetil scriptProjetil = projetil.GetComponent<Projetil>();
        if (scriptProjetil != null)
        {
            scriptProjetil.Configurar(alvo, dano, TipoTorreParaProjetil(tipo));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }

    private ProjetilTipo TipoTorreParaProjetil(TorreType tipoTorre)
    {
        switch (tipoTorre)
        {
            case TorreType.Gelo:
                return ProjetilTipo.Gelo;
            case TorreType.Fogo:
                return ProjetilTipo.Fogo;
            case TorreType.Plasma:
                return ProjetilTipo.Plasma;
            default:
                return ProjetilTipo.Basic;
        }
    }
}

