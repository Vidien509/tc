using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Importante para usar o Text UI

public class SpawnerInimigos : MonoBehaviour
{
    public GameObject prefabInimigo; // Prefab do inimigo
    public Transform[] pontosDeSpawn; // Pontos onde os inimigos aparecem
    public float raioSpawn = 3f; // Raio ao redor do ponto de spawn para gerar inimigos aleatoriamente
    public TextMeshProUGUI textoContagemInimigos; // Texto UI que mostrará a contagem de inimigos

    private Nucleo[] nucleos; // Array para armazenar os núcleos detectados
    private int inimigosRestantes; // Quantidade de inimigos restantes para spawnar
    private int inimigosTotal;
    private int inimigosVivos; // Quantidade de inimigos vivos na fase
    private float intervaloSpawn; // Intervalo entre cada inimigo
    public GameLoop gameLoop;

    public void StartSpawner(int quantidadeInimigos, float periodo)
    {
        // Detecta todas as células marcadas como núcleo
        nucleos = FindObjectsOfType<Nucleo>();

        if (nucleos.Length == 0)
        {
            Debug.LogError("Nenhum núcleo encontrado no cenário!");
            return;
        }

        // Configura o número de inimigos e o intervalo
        inimigosTotal = quantidadeInimigos;
        inimigosRestantes = quantidadeInimigos;
        inimigosVivos = quantidadeInimigos;
        intervaloSpawn = periodo / quantidadeInimigos;
        AtualizarTextoContagem();
        // Inicia o spawn de inimigos
        StartCoroutine(SpawnerLoop());
    }

    public void StopSpawner()
    {
        // Cancela o spawn manualmente
        StopAllCoroutines();
        Debug.Log("Spawner parado.");
    }

    private IEnumerator SpawnerLoop()
    {
        while (inimigosRestantes > 0)
        {
            CriarInimigo();
            inimigosRestantes--;
            yield return new WaitForSeconds(intervaloSpawn);
        }

        Debug.Log("Todos os inimigos foram instanciados.");
    }

    private void CriarInimigo()
    {
        if (nucleos.Length == 0)
        {
            Debug.LogWarning("Sem núcleos disponíveis para ataque!");
            return;
        }

        // Seleciona um ponto de spawn aleatório
        Transform pontoSpawn = pontosDeSpawn[Random.Range(0, pontosDeSpawn.Length)];

        // Calcula uma posição aleatória ao redor do ponto de spawn
        Vector3 posicaoAleatoria = GerarPosicaoAleatoria(pontoSpawn.position);

        // Instancia o inimigo
        GameObject inimigoObj = Instantiate(prefabInimigo, posicaoAleatoria, Quaternion.identity);

        // Configura o inimigo para atacar o núcleo mais próximo ou com menos vida
        Inimigo inimigo = inimigoObj.GetComponent<Inimigo>();
        if (inimigo != null)
        {
            Nucleo alvo = ObterNucleoAlvo();
            if (alvo != null)
            {
                inimigo.Configurar(alvo.transform.position, alvo, gameLoop.fase);
                inimigo.onInimigoMorto += InimigoMorto; // Associa o evento de morte do inimigo
            }
        }
    }

    private void InimigoMorto()
    {
        inimigosVivos--; // Decrementa o número de inimigos vivos
        AtualizarTextoContagem(); // Atualiza o texto de contagem
    }

    private void AtualizarTextoContagem()
    {
        // Exibe o texto com a contagem atual de inimigos
        textoContagemInimigos.text = inimigosVivos + " / " + inimigosTotal;
    }

    private Vector3 GerarPosicaoAleatoria(Vector3 pontoBase)
    {
        // Gera um deslocamento aleatório dentro de um círculo de raio "raioSpawn"
        Vector2 deslocamento = Random.insideUnitCircle * raioSpawn;
        return pontoBase + new Vector3(deslocamento.x, deslocamento.y, 0);
    }

    // Encontra o núcleo mais próximo ou com menos vida
    private Nucleo ObterNucleoAlvo()
    {
        Nucleo nucleoAlvo = null;
        float menorDistancia = float.MaxValue;
        int menorVida = int.MaxValue;

        foreach (Nucleo nucleo in nucleos)
        {
            if (nucleo == null) continue; // Ignora núcleos destruídos

            float distancia = Vector3.Distance(nucleo.transform.position, transform.position);

            // Prioriza o núcleo com menos vida; se empate, o mais próximo
            if (nucleo.vida < menorVida || (nucleo.vida == menorVida && distancia < menorDistancia))
            {
                menorVida = nucleo.vida;
                menorDistancia = distancia;
                nucleoAlvo = nucleo;
            }
        }

        return nucleoAlvo;
    }
}
