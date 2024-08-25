using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject cellPrefab;
    ResourceController controleRecurso;
    private Grid gameGrid;

    private string corSelecionadaHex = "#0F5080";
    private Color corSelecionada;
    private ColorBlock corOriginal;

    public Button botaoBloco;
    public Button botaoExtrator;
    public Button botaoModudlo;
    public Button botaoTorre;

    private CellState stateMenuSelecionado = CellState.extrator;
    private void Start()
    {
        gameGrid = new Grid(20, 10, 10f, new Vector3(0, 0, 0), cellPrefab, this.transform);
        gameGrid.SetRecursosAleatorios();

        controleRecurso = this.GetComponent<ResourceController>();

        if (!ColorUtility.TryParseHtmlString(corSelecionadaHex, out corSelecionada))
        {
            Debug.LogError("Cor hexadecimal inválida: " + corSelecionadaHex);
        }

        corOriginal = botaoBloco.colors;

        ColorBlock cb = botaoExtrator.colors;
        cb.normalColor = corSelecionada;
        cb.highlightedColor = corSelecionada;
        cb.pressedColor = corSelecionada * 0.8f;
        botaoExtrator.colors = cb;

        botaoBloco.onClick.AddListener(() => {
            ResetCor();
            OnButtonClick(botaoBloco);
            });
        botaoExtrator.onClick.AddListener(() => {
            ResetCor();
            OnButtonClick(botaoExtrator);
        });
        botaoModudlo.onClick.AddListener(() => {
            ResetCor();
            OnButtonClick(botaoModudlo);
        });
        botaoTorre.onClick.AddListener(() => {
            ResetCor();
            OnButtonClick(botaoTorre);
        });
    }

    private void OnButtonClick(Button botao)
    {
        ColorBlock cb = botao.colors;
        cb.normalColor = corSelecionada;
        cb.highlightedColor = corSelecionada;
        cb.pressedColor = corSelecionada * 0.8f;
        botao.colors = cb;
    }

    public void ResetCor()
    {
        botaoBloco.colors = corOriginal;
        botaoExtrator.colors = corOriginal;
        botaoModudlo.colors = corOriginal;
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
                cel.SetCellState(stateMenuSelecionado);
                if (stateMenuSelecionado == CellState.extrator)
                {
                    controleRecurso.adicionaExtrator(cel);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
            if (cel)
            {
                if (cel.getValue() == "0")
                {
                    cel.SetCellState(CellState.vazia);
                }
                else
                {
                    controleRecurso.ParaColeta(cel);
                    cel.SetCellState(CellState.recurso);
                }
            }
        }
    }

    public void teste()
    {

    }

    public void SetStateMenuSelecionado(string opcao)
    {
        CellState cellState = CellState.extrator;
        switch (opcao)
        {
            case "extrator":
                cellState = CellState.extrator;
                break;
            case "modulo":
                cellState = CellState.modulo;
                break;
            case "torre":
                cellState = CellState.torre;
                break;
            case "bloco":
                cellState = CellState.bloco;
                break;
        }

        stateMenuSelecionado = cellState;
    }
}
