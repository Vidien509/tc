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

    public Button botaoEscudo;
    private bool escudoSelecionado = false;

    private CellState stateMenuSelecionado = CellState.torre;
    private int precoTorre = 10;
    private int precoUpgrade = 5;

    public GameObject textoPopup;
    private int _recurso;
    public int recurso
    {
        get { return _recurso; }
        set
        {
            _recurso = value;
            atualizaTextRecursos();
        }
    }
    public TextMeshProUGUI textRecurso;
    public SpawnerInimigos spawnerInimigos;

    private void Start()
    {
        recurso = 0;
        spawnerInimigos = transform.GetComponent<SpawnerInimigos>();
        gameLoop = transform.GetComponent<GameLoop>();
        gameGrid = new Grid(14, 10, 4f, new Vector3(-30, -20, 0), cellPrefab, this.transform);
        gameGrid.SetRecursosAleatorios();

        if (!ColorUtility.TryParseHtmlString(corSelecionadaHex, out corSelecionada))
        {
            Debug.LogError("Cor hexadecimal inv�lida: " + corSelecionadaHex);
        }

        botaoEscudo.onClick.AddListener(() => {
            onClickBotao();
        });

        corOriginal = botaoEscudo.colors;
    }

    public void onClickBotao()
    {
        escudoSelecionado = !escudoSelecionado;
        if (escudoSelecionado)
        {
            ColorBlock cb = botaoEscudo.colors;
            cb.normalColor = corSelecionada;
            botaoEscudo.colors = cb;
        }
        else
        {
            botaoEscudo.colors = corOriginal;
        }
    }

    private void Update()
    {
        if (!gameLoop.faseRespondendo)
        {
            gameGrid.HighlightCelula(UtilsClass.GetMouseWorldPosition());

            if (Input.GetMouseButtonDown(0))
            {
                Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
                if (cel)
                {
                    if (escudoSelecionado)
                    {
                        if (cel.GetCellState() == CellState.nucleo)
                        {
                            Nucleo nucleo = cel.GetComponent<Nucleo>();
                            if (nucleo != null)
                            {
                                nucleo.AdicionarVida(10);
                                GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                                TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                                text.color = Color.green;
                                text.text = "+10 Vida";
                                escudoSelecionado = false;
                                botaoEscudo.colors = corOriginal;
                            }
                        }
                    }
                    else if (cel.GetCellState() == CellState.vazia)
                    {
                        if (recurso >= precoTorre)
                        {
                            ComprarTorre(cel);
                        }
                        else
                        {
                            MostrarMensagemRecursosInsuficientes(precoTorre);
                        }
                    }
                    else if (cel.GetCellState() == CellState.torre)
                    {
                        UpgradeTorre(cel);
                    }

                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (escudoSelecionado)
                {
                    escudoSelecionado = false;
                    botaoEscudo.colors = corOriginal;
                }
                else
                {
                    Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
                    if (cel)
                    {
                        if (cel.getValue() == "0" && cel.GetCellState() != CellState.vazia && cel.GetCellState() != CellState.nucleo)
                        {
                            GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                            TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                            text.color = Color.green;
                            text.text = "+ $ " + precoTorre;
                            recurso += precoTorre;
                            cel.SetCellState(CellState.vazia);
                        }
                    }
                }
            }
        }
    }

    private void ComprarTorre(Celula cel)
    {
        GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
        TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
        text.color = Color.red;
        text.text = "- $ " + precoTorre;
        recurso -= precoTorre;
        precoTorre += gameLoop.fase;
        cel.SetCellState(stateMenuSelecionado);
    }

    private void UpgradeTorre(Celula cel)
    {
        Torre torre = cel.GetComponent<Torre>();
        if (torre != null && torre.nivel < 3)
        {
            int custoUpgrade = precoUpgrade * (torre.nivel + 1);
            if (recurso >= custoUpgrade)
            {
                recurso -= custoUpgrade;
                torre.Upgrade();
                GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
                TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
                text.color = Color.yellow;
                text.text = "Upgrade! - $ " + custoUpgrade;
            }
            else
            {
                MostrarMensagemRecursosInsuficientes(custoUpgrade);
            }
        }
    }

    private void MostrarMensagemRecursosInsuficientes(int custo)
    {
        GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
        TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
        text.color = Color.red;
        text.text = custo + " $$ Recursos insuficientes!";
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

