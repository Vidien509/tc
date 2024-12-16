using CodeMonkey.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameLoop : MonoBehaviour
{
    public float tempoQuestao = 0.0f;
    public string sinal = "";
    public List<string> listaSinal;
    public int valorSinal;
    public int valor1 = 0;
    public int valor2 = 0;
    public string questao = "Quanto é ";

    public int valorResposta = 0;
    public TMP_InputField inputField;

    public GameObject menuQuestao;
    public GameObject tituloQuestao;
    public GameObject textoQuestao;
    public GameObject nivelQuestao;

    public GameObject textoPopup;
    public GameController gameController;

    public TextMeshProUGUI textFase;
    public TextMeshProUGUI textPontos;
    public TextMeshProUGUI textTempoJogo;

    public TextMeshProUGUI textRespCorreta;

    private int _fase;
    public int fase
    {
        get { return _fase; }
        set
        {
            _fase = value;
            atualizaTextFase();
        }
    }

    private int _pontos;
    public int pontos
    {
        get { return _pontos; }
        set
        {
            _pontos = value;
            atualizaTextPontos();
        }
    }

    private int periodoCiclo;
    private float tempoCiclo; // Tempo acumulado no ciclo
    public bool faseRespondendo; // Indica se est� no estado de responder quest�es
    public SpawnerInimigos spawner; // Refer�ncia ao spawner

    public GameObject painelTut1;
    public GameObject painelTut2;
    public GameObject painelTut3;
    public GameObject painelTut4;
    public GameObject btnProximo;
    public bool jogoIniciado = false;

    private const float questionTime = 15f;
    private const float preparationTime = 10f;
    private bool inPreparationPhase = false;
    private bool respostaProcessada = false;

    public int multInimigos;

    void StartGameLoop()
    {
        multInimigos = 5;
        jogoIniciado = true;
        periodoCiclo = 30;
        fase = 1;
        listaSinal = new List<string> { " + ", " - ", " x ", " / " };
        menuQuestao.SetActive(false);
        textFase.transform.gameObject.SetActive(true);
        textTempoJogo.transform.gameObject.SetActive(true);
        spawner = transform.GetComponent<SpawnerInimigos>();
        gameController = transform.GetComponent<GameController>();
        faseRespondendo = true;
        tempoCiclo = 0f;
        spawner.StopSpawner(); // Certifique-se de parar o spawner no in�cio
        iniciarNovaQuestao(); // Come�a a primeira quest�o
        gameController.StartGameController();
    }

    void Update()
    {
        if (jogoIniciado)
        {
            tempoCiclo += Time.deltaTime;

            if (faseRespondendo)
            {
                // Fase de responder perguntas
                tempoQuestao += Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.Return) && !respostaProcessada)
                {
                    ProcessarResposta();
                }

                // Adicione esta verificação para manter o foco
                if (!inputField.isFocused)
                {
                    inputField.ActivateInputField();
                }

                if (tempoCiclo >= questionTime)
                {
                    // Finaliza a fase de responder perguntas
                    FinalizarFaseRespondendo();
                }
            }
            else if (inPreparationPhase)
            {
                if (tempoCiclo >= preparationTime)
                {
                    // Finaliza a fase de preparação e inicia o combate
                    FinalizarFasePreparacao();
                }
            }
            else
            {
                // Fase de combate
                if (tempoCiclo >= periodoCiclo || spawner.inimigosVivos <= 0)
                {
                    FinalizarFaseCombate();
                }
            }

            atualizaTextTempoJogo();
        }
    }

    private void iniciarNovaQuestao()
    {
        menuQuestao.SetActive(true);
        inputField.text = ""; // Limpa o campo de entrada
        TextMeshProUGUI textoQ = textoQuestao.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI tituloQ = tituloQuestao.GetComponent<TextMeshProUGUI>();

        gerarQuestao();
        textoQ.text = questao;
        tituloQ.text = "Responda:";
        tempoQuestao = 0.0f;
        respostaProcessada = false;
        inputField.ActivateInputField(); // Substitua a linha Invoke por esta
    }

    void SelectInputField()
    {
        inputField.Select();
    }

    private void processarRespostaCorreta(float tempoQuestao)
    {
        questao = "Quanto é ";
        GameObject textoPopupI = Instantiate(textoPopup, new Vector3(0, -50, 0), Quaternion.identity);
        TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
        text.color = Color.green;
        text.fontSize = 42;

        if (tempoQuestao <= 1.5f)
        {

            GameObject textoPopupRC = Instantiate(textoPopup, new Vector3(0, 0, 0), Quaternion.identity);
            TextMeshPro textRC = textoPopupRC.GetComponent<TextMeshPro>();
            TextoPopup textoPopupScript = textoPopupRC.GetComponent<TextoPopup>();
            textoPopupScript.Configure(true, false, false, 1f, 3f);
            textRC.fontSize = 42;
            textRC.text = "PERFEITO!  2X";

            text.color = Color.green;
            text.text = "+ $ 20";
            gameController.recurso += 20;
        }
        else if (tempoQuestao <= 2.5f)
        {
            GameObject textoPopupRC = Instantiate(textoPopup, new Vector3(0, 0, 0), Quaternion.identity);
            TextMeshPro textRC = textoPopupRC.GetComponent<TextMeshPro>();
            TextoPopup textoPopupScript = textoPopupRC.GetComponent<TextoPopup>();
            textoPopupScript.Configure(false, true, false, 1f, 3f);
            textRC.fontSize = 42;
            textRC.text = "EXCELENTE!  1.5X";

            text.color = Color.green;
            text.text = "+ $ 15";
            gameController.recurso += 15;
        }
        else if (tempoQuestao > 2.5f)
        {
            GameObject textoPopupRC = Instantiate(textoPopup, new Vector3(0, 0, 0), Quaternion.identity);
            TextMeshPro textRC = textoPopupRC.GetComponent<TextMeshPro>();
            TextoPopup textoPopupScript = textoPopupRC.GetComponent<TextoPopup>();
            textoPopupScript.Configure(false, false, true, 1f, 3f);
            textRC.fontSize = 42;
            textRC.text = "CORRETO!";

            text.color = Color.green;
            text.text = "+ $ 10";
            gameController.recurso += 10;
        }
    }

    public void gerarQuestao()
    {
        UnityEngine.UI.Image painelNivelQ = nivelQuestao.GetComponent<UnityEngine.UI.Image>();
        valor1 = UnityEngine.Random.Range(1, 10);
        valor2 = UnityEngine.Random.Range(1, 10);
        valorSinal = UnityEngine.Random.Range(0, 4);
        sinal = listaSinal[valorSinal];

        if (sinal == " / ")
        {
            painelNivelQ.color = Color.yellow;
            int multiplicador = UnityEngine.Random.Range(1, 10);
            valor1 = valor2 * multiplicador;
        }
        else if (sinal == " - ")
        {
            painelNivelQ.color = Color.blue;
            valor1 = UnityEngine.Random.Range(valor2, 10);
        }
        else if (sinal == " + ")
        {
            painelNivelQ.color = Color.red;
        }
        else if (sinal == " x ")
        {
            painelNivelQ.color = Color.green;
        }
        questao = "Quanto é " + valor1 + sinal + valor2 + "?";
        calcularResposta();
    }

    public bool verificarResposta(string respostaJogador)
    {
        if (int.TryParse(respostaJogador, out int respostaDigitada))
        {
            return respostaDigitada == valorResposta;
        }
        return false;
    }

    public void calcularResposta()
    {
        switch (sinal)
        {
            case " + ":
                valorResposta = valor1 + valor2;
                break;
            case " - ":
                valorResposta = valor1 - valor2;
                break;
            case " x ":
                valorResposta = valor1 * valor2;
                break;
            case " / ":
                valorResposta = valor2 != 0 ? valor1 / valor2 : 0;
                break;
            default:
                Debug.LogError("Sinal inválido");
                break;
        }
    }

    public void atualizaTextFase()
    {
        textFase.text = "Fase " + fase.ToString();
    }

    public void atualizaTextPontos()
    {
        textPontos.text = pontos.ToString();
    }

    public void atualizaTextTempoJogo()
    {
        textTempoJogo.text = tempoCiclo.ToString("F2");
    }
    public void proximoTutorial()
    {
        Debug.Log("Clicado " + painelTut1.active);
        if (painelTut1.active)
        {
            painelTut1.SetActive(false);
            painelTut2.SetActive(true);
        }
        else if (painelTut2.active)
        {
            painelTut2.SetActive(false);
            painelTut3.SetActive(true);
        }
        else if (painelTut3.active)
        {
            painelTut3.SetActive(false);
            painelTut4.SetActive(true);
            btnProximo.GetComponentInChildren<TextMeshProUGUI>().text = "Jogar";
        }
        else if (painelTut4.active)
        {
            painelTut4.SetActive(false);
            btnProximo.SetActive(false);
            StartGameLoop();
        }
    }

    private void CriarBossUnico()
    {
        // Create a unique visual for the boss without using textures
        GameObject bossObject = new GameObject("BossEnemy");
        SpriteRenderer spriteRenderer = bossObject.AddComponent<SpriteRenderer>();

        // Create a circular sprite for the boss
        Texture2D texture = new Texture2D(128, 128);
        Color[] colors = new Color[128 * 128];

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64));
                if (distanceFromCenter < 60)
                {
                    colors[y * 128 + x] = Color.Lerp(Color.red, Color.yellow, Mathf.PingPong(distanceFromCenter * 0.1f + fase, 1));
                }
                else
                {
                    colors[y * 128 + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

        // Set the boss object's position, scale, etc. as needed
        // You may want to pass this object to your SpawnerInimigos or set it up directly here
    }

    private void IniciarFasePreparacao()
    {
        Debug.Log("Iniciando fase de preparação. Posicione suas torres!");
        // Adicione aqui qualquer lógica adicional para a fase de preparação
    }

    private void IniciarFaseCombate()
    {
        Debug.Log("Iniciando fase de combate!");
        spawner.StartSpawner(fase * multInimigos, 0.2f);
        menuQuestao.SetActive(false);
    }

    private void ProcessarResposta()
    {
        respostaProcessada = true;
        if (verificarResposta(inputField.text))
        {
            processarRespostaCorreta(tempoQuestao);
        }
        iniciarNovaQuestao();
        inputField.ActivateInputField(); // Adicione esta linha
    }

    private void FinalizarFaseRespondendo()
    {
        faseRespondendo = false;
        inPreparationPhase = true;
        tempoCiclo = 0f;
        menuQuestao.SetActive(false);
        Debug.Log("Iniciando fase de preparação. Você tem 20 segundos para se preparar!");
    }

    private void FinalizarFasePreparacao()
    {
        inPreparationPhase = false;
        tempoCiclo = 0f;
        IniciarFaseCombate();
    }

    private void FinalizarFaseCombate()
    {
        faseRespondendo = true;
        tempoCiclo = 0f;
        fase++;
        spawner.StopSpawner();
        respostaProcessada = false;
        iniciarNovaQuestao();

        if (fase % spawner.faseBoss == 0)
        {
            CriarBossUnico();
            gameController.recursosEspeciais++;
            Debug.Log($"Boss derrotado! Recurso especial adicionado. Total: {gameController.recursosEspeciais}");
        }
    }

    public void NotificarBossDerrotado()
    {
        spawner.bossVivos--;
        gameController.recursosEspeciais++;
        Debug.Log($"Boss derrotado! Recurso especial adicionado. Total: {gameController.recursosEspeciais}");
    }
}

