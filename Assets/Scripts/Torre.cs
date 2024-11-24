using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torre : MonoBehaviour
{
    public float alcance; // Alcance da torre para detectar inimigos
    public float intervaloAtaque; // Intervalo entre os ataques
    public int dano; // Dano causado pela torre
    public GameObject prefabProjetil; // Prefab do projétil

    private Inimigo alvoAtual;

    private void Awake()
    {
        // Carrega automaticamente o prefab do projétil a partir da pasta Resources
        if (prefabProjetil == null)
        {
            prefabProjetil = Resources.Load<GameObject>("Prefabs/Projetil");
            if (prefabProjetil == null)
            {
                Debug.LogError("Prefab do projétil não encontrado na pasta Resources/Prefabs/Projetil!");
            }
        }
    }
    void Start()
    {
        alcance = 50f;
        intervaloAtaque = 1f;
        dano = 10;
        StartCoroutine(AtaqueContinuo());
    }

    void Update()
    {
        // Atualiza o alvo para o inimigo mais próximo dentro do alcance
        alvoAtual = DetectarInimigoMaisProximo();
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
        // Opcional: Instanciar projétil
        if (prefabProjetil != null)
        {
            GameObject projetil = Instantiate(prefabProjetil, transform.position, Quaternion.identity);
            Projetil scriptProjetil = projetil.GetComponent<Projetil>();
            if (scriptProjetil != null)
            {
                scriptProjetil.Configurar(inimigo.transform, dano);
            }
        }
        else
        {
            // Caso não tenha projétil, aplicar dano diretamente
            inimigo.ReceberDano(dano);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Desenha o alcance da torre no editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}
