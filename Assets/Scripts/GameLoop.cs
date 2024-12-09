using CodeMonkey.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

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

    private int periodoCiclo;
    private float tempoCiclo; // Tempo acumulado no ciclo
    public bool faseRespondendo; // Indica se está no estado de responder questões
    public SpawnerInimigos spawner; // Referência ao spawner

    void Start()
    {
        periodoCiclo = 30;
        fase = 1;
        listaSinal = new List<string> { " + ", " - ", " x ", " / " };
        menuQuestao.SetActive(false);
        spawner = transform.GetComponent<SpawnerInimigos>();
        gameController = transform.GetComponent<GameController>();
        faseRespondendo = true;
        tempoCiclo = 0f;
        spawner.StopSpawner(); // Certifique-se de parar o spawner no início
        iniciarNovaQuestao(); // Começa a primeira questão
    }

    void Update()
    {
        tempoCiclo += Time.deltaTime;

        if (faseRespondendo)
        {
            // Fase de responder perguntas
            tempoQuestao += Time.deltaTime;

            if (tempoCiclo >= 10)
            {
                // Finaliza a fase de responder perguntas
                faseRespondendo = false;
                tempoCiclo = 0f;
                spawner.StartSpawner(fase*10, periodoCiclo);
                menuQuestao.SetActive(false);
            }
            else if (verificarResposta(inputField.text))
            {
                // Gera nova questão ao responder corretamente
                processarRespostaCorreta(tempoQuestao);
                iniciarNovaQuestao();
            }
        }
        else
        {
            // Fase de combate
            if (tempoCiclo >= periodoCiclo)
            {
                faseRespondendo = true;
                tempoCiclo = 0f;
                fase++;
                spawner.StopSpawner();
                iniciarNovaQuestao(); // Reinicia o ciclo com nova questão
            }
        }

        atualizaTextTempoJogo();
    }

    private void iniciarNovaQuestao()
    {
        menuQuestao.SetActive(true);
        inputField.text = ""; // Limpa o campo de entrada
        inputField.Select();
        TextMeshProUGUI textoQ = textoQuestao.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI tituloQ = tituloQuestao.GetComponent<TextMeshProUGUI>();

        gerarQuestao();
        textoQ.text = questao;
        tituloQ.text = "Responda:";
        tempoQuestao = 0.0f;
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

            GameObject textoPopupRC = Instantiate(textoPopup, new Vector3(0,0,0), Quaternion.identity);
            TextMeshPro textRC = textoPopupRC.GetComponent<TextMeshPro>();
            TextoPopup textoPopupScript = textoPopupRC.GetComponent<TextoPopup>();
            textoPopupScript.Configure(true, false, false, 1f, 3f);
            textRC.fontSize = 42;
            textRC.text = "PERFEITO!  2X";

            text.color = Color.green;
            text.text = "+ $ 20";
            gameController.recurso += 20;
        }
        else if (tempoQuestao <= 3.5f)
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
        else if (tempoQuestao > 3.5f)
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
        valor1 = Random.Range(1, 10);
        valor2 = Random.Range(1, 10);
        valorSinal = Random.Range(0, 4);
        sinal = listaSinal[valorSinal];
        
        if (sinal == " / ")
        {
            painelNivelQ.color = Color.yellow;
            int multiplicador = Random.Range(1, 10);
            valor1 = valor2 * multiplicador;
        }else if (sinal == " - ")
        {
            painelNivelQ.color = Color.blue;
            valor1 = Random.Range(valor2, 10);
        }else if(sinal ==  " + ")
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

    public void atualizaTextTempoJogo()
    {
        textTempoJogo.text = tempoCiclo.ToString("F2");
    }

}
