using CodeMonkey.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

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

    public GameObject painelGameOver;
    public TextMeshProUGUI gameOverPontos;
    public TextMeshProUGUI gameOverTitulo;
    public bool gameOver;

    public GameObject textoPopup;
    public GameController gameController;
    public bool respostaCorreta;
    public bool powerUpAtivado;
    public bool powerUpConcedido;
    public List<PowerUpAtivo> powerUpsAtivos = new List<PowerUpAtivo>();
    public float tempoPowerUp;

    public TextMeshProUGUI textFase;
    public TextMeshProUGUI textPontos;
    public TextMeshProUGUI textTempoJogo;

    public TextMeshProUGUI textRespCorreta;
    public static bool tutorialMostrado = false;

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

    private float questionTime;
    private float preparationTime;
    private bool inPreparationPhase = false;
    private bool respostaProcessada = false;

    public int multInimigos;

    private void Start()
    {
        painelTut1.SetActive(true);
        painelGameOver.SetActive(false);
    }

    void StartGameLoop()
    {
        powerUpConcedido = false;
        gameOver = false;
        powerUpAtivado = false;
        tempoPowerUp = 0f;
        multInimigos = 5;
        jogoIniciado = true;
        periodoCiclo = 30;
        questionTime = 10f;
        preparationTime = 10f;
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
        if (!gameOver && !powerUpAtivado)
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

        if (powerUpConcedido && powerUpsAtivos.Count > 0)
        {
            // Atualiza a UI
            Debug.Log("POWER UP CONCEDIDO: " + string.Join(", ", powerUpsAtivos.Select(p => p.codigo)));

            PowerUpUIManager.Instance.UpdatePowerUpUI(powerUpsAtivos.Select(p => p.codigo).ToArray());

            // Atualiza temporizadores e remove power-ups expirados
            bool powerUpExpirado = false;

            for (int i = powerUpsAtivos.Count - 1; i >= 0; i--)
            {
                powerUpsAtivos[i].tempoRestante -= Time.deltaTime;

                if (powerUpsAtivos[i].tempoRestante <= 0)
                {
                    int codigoExpirado = powerUpsAtivos[i].codigo;
                    RemoveEfeitoPowerUp(codigoExpirado);
                    powerUpsAtivos.RemoveAt(i);
                    powerUpExpirado = true;
                }
            }

            if (powerUpExpirado)
            {
                PowerUpUIManager.Instance.UpdatePowerUpUI(powerUpsAtivos.Select(p => p.codigo).ToArray());
            }

            powerUpConcedido = false;
        }
    }

    private void RemoveEfeitoPowerUp(int codigo)
    {
        switch (codigo)
        {
            case 3: // Caso específico do power-up 3 (congelar)
                Inimigo[] inimigos = FindObjectsOfType<Inimigo>();
                foreach (Inimigo inimigo in inimigos)
                {
                    inimigo.velocidade = inimigo.velocidadeBase;
                }
                break;

                // Adicione outros casos conforme necessário
        }
    }

    public void processaGameOver(bool win)
    {
        gameOver = true;
        painelGameOver.SetActive(true);
        gameOverPontos.text = "Pontos: " + pontos.ToString();
        btnProximo.SetActive(true);
        btnProximo.GetComponentInChildren<TextMeshProUGUI>().text = "Jogar Novamente";
        if (win)
        {
            gameOverTitulo.text = "VITÓRIA!!";
        }
        else
        {
            gameOverTitulo.text = "GAME OVER";
        }
    }

    public void iniciarNovaQuestao()
    {
        respostaCorreta = false;
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
            if (powerUpsAtivos.Select(p => p.codigo).ToArray().Contains(7) && powerUpConcedido)
            {
                text.text = "+ $ 80";
                gameController.recurso += 80;
            }
            else
            {
                text.text = "+ $ 20";
                gameController.recurso += 20;
            }
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
            if (powerUpsAtivos.Select(p => p.codigo).ToArray().Contains(7) && powerUpConcedido)
            {
                text.text = "+ $ 60";
                gameController.recurso += 60;
            }
            else
            {
                text.text = "+ $ 15";
                gameController.recurso += 15;
            }
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
            if (powerUpsAtivos.Select(p => p.codigo).ToArray().Contains(7) && powerUpConcedido)
            {
                text.text = "+ $ 40";
                gameController.recurso += 40;
            }
            else
            {
                text.text = "+ $ 10";
                gameController.recurso += 10;
            }
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
        //StartCoroutine(gameController.AnimarTextoRecurso(textFase));
    }

    public void atualizaTextPontos()
    {
        textPontos.text = pontos.ToString();
        StartCoroutine(gameController.AnimarTextoRecurso(textPontos));
    }

    public void atualizaTextTempoJogo()
    {
        textTempoJogo.text = tempoCiclo.ToString("F2");
    }

    [System.Obsolete]
    public void proximoTutorial()
    {
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
            tutorialMostrado = true;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            btnProximo.SetActive(false);
        }
    }
    
    private void IniciarFaseCombate()
    {
        if(fase >= 6)
        {
            multInimigos = 1;
        }
        else
        {
            multInimigos = 5;
        }
        spawner.StartSpawner(fase * multInimigos, 0.2f);
        menuQuestao.SetActive(false);
    }

    public void ProcessarResposta() {
        ProcessarResposta(true);
    }

    public void ProcessarResposta(bool novaQuestao)
    {
        Debug.Log("PROCESSANDO RESPOSTA: " + novaQuestao);
        respostaProcessada = true;
        if (verificarResposta(inputField.text))
        {
            respostaCorreta = true;
            processarRespostaCorreta(tempoQuestao);
        }
        if (novaQuestao)
        {
            iniciarNovaQuestao();
            inputField.ActivateInputField();
        }
        else
        {
            menuQuestao.SetActive(false);
        }
    }

    private void FinalizarFaseRespondendo()
    {
        faseRespondendo = false;
        inPreparationPhase = true;
        tempoCiclo = 0f;
        menuQuestao.SetActive(false);
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
    }

    public void NotificarBossDerrotado()
    {
        spawner.bossVivos--;
        gameController.recursosEspeciais++;
    }
    //public bool powerUpAtivado;
    //public bool powerUpConcedido;
    //public int powerUpsAtivos.Select(p => p.codigo).ToArray();
    //public float tempoPowerUp;

    public bool getPowerUpAtivado()
    {
        return powerUpAtivado;
    }
    public bool getPowerUpConcedido()
    {
        return powerUpConcedido;
    }

}

