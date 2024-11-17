using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public GameObject cellPrefab;
    private Grid gameGrid;
    public GameLoop gameLoop;  
    private string corSelecionadaHex = "#0F5080";
    private Color corSelecionada;
    private ColorBlock corOriginal;

    public Button botaoTorre;

    private CellState stateMenuSelecionado = CellState.torre;

    public GameObject textoPopup;
    public int recurso = 0;
    public TextMeshProUGUI textRecurso;
    private void Start()
    {
        gameLoop = transform.GetComponent<GameLoop>();
        gameGrid = new Grid(20, 10, 10f, new Vector3(0, 0, 0), cellPrefab, this.transform);
        gameGrid.SetRecursosAleatorios();

        if (!ColorUtility.TryParseHtmlString(corSelecionadaHex, out corSelecionada))
        {
            Debug.LogError("Cor hexadecimal inválida: " + corSelecionadaHex);
        }

        corOriginal = botaoTorre.colors;

        ColorBlock cb = botaoTorre.colors;
        cb.normalColor = corSelecionada;
        cb.highlightedColor = corSelecionada;
        cb.pressedColor = corSelecionada * 0.8f;
        botaoTorre.colors = cb;

        botaoTorre.onClick.AddListener(() => {
            ResetCor();
            OnButtonClick(botaoTorre);
        });
    }

    private void OnButtonClick(Button botao)
    {
        if (gameLoop.statusQuestao == 0)
        {
            ColorBlock cb = botao.colors;
            cb.normalColor = corSelecionada;
            cb.highlightedColor = corSelecionada;
            cb.pressedColor = corSelecionada * 0.8f;
            botao.colors = cb;
        }
    }

    public void ResetCor()
    {
        botaoTorre.colors = corOriginal;
    }

    private void Update()
    {
        gameGrid.HighlightCelula(UtilsClass.GetMouseWorldPosition());

        if (Input.GetMouseButtonDown(0))
        {
            Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
            if (cel)
            {
                if (recurso >= 10)
                {
                    GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                    TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                    text.color = Color.red;
                    text.text = "- $ 10";
                    recurso -= 10;
                    atualizaTextRecursos();
                    cel.SetCellState(stateMenuSelecionado);                }
                else
                {
                    GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                    TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                    text.color = Color.red;
                    text.text = "$$ Recursos insuficientes!";
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
            if (cel)
            {
                if (cel.getValue() == "0" && cel.GetCellState() != CellState.vazia && cel.GetCellState() != CellState.nucleo)
                {
                    GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                    TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                    text.color = Color.green;
                    text.text = "+ $ 10";
                    recurso += 10;
                    atualizaTextRecursos();
                    cel.SetCellState(CellState.vazia);
                }
            }
        }
    }

    public void SetStateMenuSelecionado(string opcao)
    {
        CellState cellState = CellState.torre;
        switch (opcao)
        {
            case "torre":
                cellState = CellState.torre;
                break;
        }

        stateMenuSelecionado = cellState;
    }

    public void atualizaTextRecursos()
    {
        textRecurso.text = recurso.ToString();
    }
}
