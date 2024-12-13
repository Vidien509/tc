using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerInimigos : MonoBehaviour
{
    public GameObject prefabInimigo; // Prefab do inimigo
    public Transform[] pontosDeSpawn; // Pontos onde os inimigos aparecem
    public float raioSpawn = 3f; // Raio ao redor do ponto de spawn para gerar inimigos aleatoriamente
    public TextMeshProUGUI textoContagemInimigos; // Texto UI que mostrar� a contagem de inimigos

    private Nucleo[] nucleos; // Array para armazenar os n�cleos detectados
    private int inimigosRestantes; // Quantidade de inimigos restantes para spawnar
    private int inimigosTotal;
    public int inimigosVivos; // Quantidade de inimigos vivos na fase
    private float intervaloSpawn; // Intervalo entre cada inimigo
    public GameLoop gameLoop;

    public void StartSpawner(int quantidadeInimigos, float periodo)
    {
        // Detecta todas as c�lulas marcadas como n�cleo
        nucleos = FindObjectsOfType<Nucleo>();

        if (nucleos.Length == 0)
        {
            Debug.LogError("Nenhum n�cleo encontrado no cen�rio!");
            return;
        }

        // Configura o n�mero de inimigos e o intervalo
        if (gameLoop.fase % 10 == 0)
        {
            inimigosTotal = 1;
            inimigosRestantes = 1;
            inimigosVivos = 1;
            intervaloSpawn = periodo;
        }
        else
        {
            inimigosTotal = quantidadeInimigos;
            inimigosRestantes = quantidadeInimigos;
            inimigosVivos = quantidadeInimigos;
            intervaloSpawn = periodo / quantidadeInimigos;
        }
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

        Transform pontoSpawn = pontosDeSpawn[Random.Range(0, pontosDeSpawn.Length)];
        Vector3 posicaoAleatoria = GerarPosicaoAleatoria(pontoSpawn.position);

        GameObject inimigoObj = Instantiate(prefabInimigo, posicaoAleatoria, Quaternion.identity);
        Inimigo inimigo = inimigoObj.GetComponent<Inimigo>();

        if (inimigo != null)
        {
            Nucleo alvo = ObterNucleoAlvo();
            if (alvo != null)
            {
                InimigoTipo tipoInimigo = DeterminarTipoInimigo();
                inimigo.Configurar(alvo.transform.position, alvo, gameLoop.fase, tipoInimigo);
                inimigo.onInimigoMorto += InimigoMorto;
            }
        }
    }

    private InimigoTipo DeterminarTipoInimigo()
    {
        if (gameLoop.fase % 10 == 0)
        {
            return InimigoTipo.Boss;
        }
        else if (gameLoop.fase >= 11)
        {
            return Random.value < 0.5f ? InimigoTipo.Normal : InimigoTipo.PlasmaResistente;
        }
        else
        {
            return InimigoTipo.Normal;
        }
    }

    private void InimigoMorto()
    {
        inimigosVivos--; // Decrementa o n�mero de inimigos vivos
        AtualizarTextoContagem(); // Atualiza o texto de contagem
    }

    private void AtualizarTextoContagem()
    {
        // Exibe o texto com a contagem atual de inimigos
        textoContagemInimigos.text = inimigosVivos + " / " + inimigosTotal;
    }

    private Vector3 GerarPosicaoAleatoria(Vector3 pontoBase)
    {
        // Gera um deslocamento aleat�rio dentro de um c�rculo de raio "raioSpawn"
        Vector2 deslocamento = Random.insideUnitCircle * raioSpawn;
        return pontoBase + new Vector3(deslocamento.x, deslocamento.y, 0);
    }

    // Encontra o n�cleo mais pr�ximo ou com menos vida
    private Nucleo ObterNucleoAlvo()
    {
        Nucleo nucleoAlvo = null;
        float menorDistancia = float.MaxValue;
        int menorVida = int.MaxValue;

        foreach (Nucleo nucleo in nucleos)
        {
            if (nucleo == null) continue; // Ignora n�cleos destru�dos

            float distancia = Vector3.Distance(nucleo.transform.position, transform.position);

            // Prioriza o n�cleo com menos vida; se empate, o mais pr�ximo
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

