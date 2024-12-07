using System.Collections;
using UnityEngine;

public class Torre : MonoBehaviour
{
    public float alcance;
    public float intervaloAtaque;
    public int dano;
    public GameObject prefabProjetil;
    public int nivel = 0;

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

    public void Upgrade()
    {
        if (nivel < 3)
        {
            nivel++;
            AtualizarAtributos();
            celula.AtualizarVisualTorre(nivel);
        }
    }

    private void AtualizarAtributos()
    {
        alcance = 20f + (nivel * 5f);
        intervaloAtaque = 1.5f - (nivel * 0.25f);
        dano = 10 + (nivel * 5);
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
            switch (nivel)
            {
                case 0:
                case 1:
                    LancarProjetil(inimigo.transform);
                    break;
                case 2:
                    StartCoroutine(LancarProjetilDuplo(inimigo.transform));
                    break;
                case 3:
                    StartCoroutine(LancarProjetilTriplo(inimigo.transform));
                    break;
            }
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
            scriptProjetil.Configurar(alvo, dano, ProjetilTipo.Torre);
        }
    }

    IEnumerator LancarProjetilDuplo(Transform alvo)
    {
        LancarProjetil(alvo);
        yield return new WaitForSeconds(0.1f);
        LancarProjetil(alvo);
    }

    IEnumerator LancarProjetilTriplo(Transform alvo)
    {
        LancarProjetil(alvo);
        yield return new WaitForSeconds(0.1f);
        LancarProjetil(alvo);
        yield return new WaitForSeconds(0.1f);
        LancarProjetil(alvo);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}

