using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CodeMonkey;
public class GameLoop : MonoBehaviour
{
    public float tempo = 0.0f;
    public int contadorQuestao = 0;
    public string sinal = "";
    public List<string> listaSinal;
    public int valorSinal;
    public int valor1 = 0;
    public int valor2 = 0;
    public string questao = "Quanto é ";

    public int valorResposta = 0;
    public TMP_InputField inputField;

    public int statusQuestao = 0;
    public GameObject menuQuestao;
    public GameObject tituloQuestao;
    public GameObject textoQuestao;

    public GameObject textoPopup;
    public GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        listaSinal = new List<string> { " + ", " - ", " x ", " / " };
        menuQuestao.SetActive(false);
        gameController = transform.GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        tempo += Time.deltaTime;
        if (statusQuestao == 0)
        {
            if (tempo >= 10)
            {
                contadorQuestao++;
                menuQuestao.SetActive(true);
                inputField.Select();
                TextMeshProUGUI textoQ = textoQuestao.transform.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI tituloQ = tituloQuestao.transform.GetComponent<TextMeshProUGUI>();

                gerarQuestao();
                textoQ.text = questao;
                tituloQ.text = "Questão nº " + contadorQuestao;

                calcularResposta();
                statusQuestao = 1;
                tempo = 0.0f;
            }
        }else
        {
            if (verificarResposta(inputField.text)) {
                menuQuestao.SetActive(false);
                statusQuestao = 0;
                tempo = 0.0f;
                questao = "Quanto é ";
                GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                text.color = Color.green;
                text.text = "+ $ 10";
                gameController.recurso += 10;
                gameController.atualizaTextRecursos();
            }
            if (tempo >= 10)
            {
                menuQuestao.SetActive(false);
                statusQuestao = 0;
                tempo = 0.0f;
                questao = "Quanto é ";
            }
        }
    }

    public void gerarQuestao()
    {
        valor1 = Random.Range(1, 10);
        valor2 = Random.Range(1, 10);
        valorSinal = Random.Range(0, 4);
        sinal = listaSinal[2];
        Debug.Log("Gerar Questao: " + valorSinal + " > " + listaSinal[valorSinal] + " > " + sinal);
        if (sinal == " / ")
        {
            int multiplicador = Random.Range(1, 10);
            valor1 = valor2 * multiplicador;
        }
        questao += valor1 + sinal + valor2 + "?";
    }

    public bool verificarResposta(string respostaJogador)
    {
        int respostaDigitada;
        if (int.TryParse(respostaJogador, out respostaDigitada))
        {
            if (respostaDigitada == valorResposta)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void calcularResposta() {
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
}
