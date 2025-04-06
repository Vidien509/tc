using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerInimigos : MonoBehaviour
{
    public GameObject prefabInimigo; // Prefab do inimigo
    public Transform pontoSpawn; // Pontos onde os inimigos aparecem
    public float raioSpawn = 3f; // Raio ao redor do ponto de spawn para gerar inimigos aleatoriamente
    public TextMeshProUGUI textoContagemInimigos; // Texto UI que mostrar� a contagem de inimigos

    private Nucleo[] nucleos; // Array para armazenar os n�cleos detectados
    private int inimigosRestantes; // Quantidade de inimigos restantes para spawnar
    private int inimigosTotal;

    private int _inimigosVivos;
    public int inimigosVivos
    {
        get { return _inimigosVivos; }
        set
        {
            _inimigosVivos = value;
            AtualizarTextoContagem(); 
        }
    }

    private float intervaloSpawn; // Intervalo entre cada inimigo
    public GameLoop gameLoop;

    public int faseBoss;
    public int bossVivos;

    public void StartSpawner(int quantidadeInimigos, float intervaloSpawn)
    {
        // Detecta todas as células marcadas como n�cleo
        faseBoss = 10;
        bossVivos = 0;
        nucleos = FindObjectsOfType<Nucleo>();

        if (nucleos.Length == 0)
        {
            Debug.LogError("Nenhum n�cleo encontrado no cen�rio!");
            return;
        }

        // Configura o n�mero de inimigos e o intervalo
        if (gameLoop.fase % faseBoss == 0)
        {
            inimigosTotal = 1 + (gameLoop.fase / 2); // Boss + additional enemies
            inimigosRestantes = inimigosTotal;
            inimigosVivos = inimigosTotal;
            this.intervaloSpawn = intervaloSpawn;
        }
        else
        {
            inimigosTotal = quantidadeInimigos;
            inimigosRestantes = quantidadeInimigos;
            inimigosVivos = quantidadeInimigos;
            this.intervaloSpawn = intervaloSpawn;
        }

        // Inicia o spawn de inimigos
        StartCoroutine(SpawnerLoop());
    }

    public void StopSpawner()
    {
        // Cancela o spawn manualmente
        StopAllCoroutines();
    }

    private IEnumerator SpawnerLoop()
    {
        if (gameLoop.fase % faseBoss == 0 && bossVivos <= 0)
        {
            // Spawn boss and additional enemies
            bossVivos++;
            CriarInimigo(InimigoTipo.Boss, 1f);

            // Instancia outros tipos de inimigos
            for (int i = 1; i < inimigosTotal; i++)
            {
                CriarInimigo(DeterminarTipoInimigo(), 1f);
            }
        }
        else
        {
            while (inimigosRestantes > 0)
            {
                CriarInimigo(DeterminarTipoInimigo(), 1f);
                inimigosRestantes--;
                yield return new WaitForSeconds(intervaloSpawn);
            }
        }
    }

    private void CriarInimigo(InimigoTipo tipo, float escala = 1f)
    {
        if (nucleos.Length == 0)
        {
            Debug.LogWarning("Sem núcleos disponíveis para ataque!");
            return;
        }

        Vector3 posicaoAleatoria = GerarPosicaoAleatoria(pontoSpawn.position);

        GameObject inimigoObj = Instantiate(prefabInimigo, posicaoAleatoria, Quaternion.identity);

        if (tipo == InimigoTipo.Boss)
        {
            // Define a escala do boss
            inimigoObj.transform.localScale = new Vector3(2f, 2f, 1f); // Define a escala para 2x o tamanho original
        }
        else
        {
            inimigoObj.transform.localScale *= escala;
        }
        Inimigo inimigo = inimigoObj.GetComponent<Inimigo>();

        if (inimigo != null)
        {
            Nucleo alvo = ObterNucleoAlvo();
            if (alvo != null)
            {
                inimigo.Configurar(alvo.transform.position, alvo, gameLoop.fase, tipo);
                inimigo.onInimigoMorto += InimigoMorto;
            }
        }
    }

    private InimigoTipo DeterminarTipoInimigo()
    {
        if (gameLoop.fase % faseBoss == 0 && bossVivos <= 0)
        {
            return InimigoTipo.Boss;
        }
        else if (gameLoop.fase >= 6)
        {
            float random = Random.value;
            if (random < 0.4f)
                return InimigoTipo.Normal;
            else if (random < 0.7f)
                return InimigoTipo.PlasmaResistente;
            else
                return InimigoTipo.Divisivel;
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
        StartCoroutine(gameLoop.gameController.AnimarTextoRecurso(textoContagemInimigos));
    }

    private Vector3 GerarPosicaoAleatoria(Vector3 pontoBase)
    {
        float distanciaMinimaNucleo = 5f; // Ajuste este valor conforme necessário
        Vector3 posicaoAleatoria;
        bool posicaoValida = false;
        int tentativas = 0;
        int maxTentativas = 100; // Evita loop infinito

        do
        {
            Vector2 deslocamento = Random.insideUnitCircle * raioSpawn;
            posicaoAleatoria = pontoBase + new Vector3(deslocamento.x, deslocamento.y, 0);

            posicaoValida = true;
            foreach (Nucleo nucleo in nucleos)
            {
                if (Vector3.Distance(posicaoAleatoria, nucleo.transform.position) < distanciaMinimaNucleo)
                {
                    posicaoValida = false;
                    break;
                }
            }

            tentativas++;
            if (tentativas >= maxTentativas)
            {
                Debug.LogWarning("Não foi possível encontrar uma posição válida após " + maxTentativas + " tentativas.");
                return pontoBase; // Retorna o ponto de spawn original se não encontrar uma posição válida
            }
        } while (!posicaoValida);

        return posicaoAleatoria;
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

    public void SpawnDividedEnemies(Vector3 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = position + Random.insideUnitSphere * 1f;
            CriarInimigo(spawnPosition, InimigoTipo.Normal, 0.5f);
        }
    }

    private void CriarInimigo(Vector3 posicao, InimigoTipo tipo, float escala = 1f)
    {
        GameObject inimigoObj = Instantiate(prefabInimigo, posicao, Quaternion.identity);
        inimigoObj.transform.localScale *= escala;
        Inimigo inimigo = inimigoObj.GetComponent<Inimigo>();

        if (inimigo != null)
        {
            Nucleo alvo = ObterNucleoAlvo();
            if (alvo != null)
            {
                inimigosVivos++;
                inimigo.Configurar(alvo.transform.position, alvo, gameLoop.fase, tipo);
                inimigo.onInimigoMorto += InimigoMorto;
            }
        }
    }

    public void SpawnEnemiesFromBoss(Vector3 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = position + Random.insideUnitSphere * 2f;
            CriarInimigo(spawnPosition, DeterminarTipoInimigo());
        }
    }

    private void CriarInimigo(InimigoTipo tipo)
    {
        CriarInimigo(Vector3.zero, tipo);
    }
}

