using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

public class GameController : MonoBehaviour
{
    public GameObject cellPrefab;
    private Grid gameGrid;
    public GameLoop gameLoop;
    private string corSelecionadaHex = "#0F5080";
    private Color corSelecionada;
    private ColorBlock corOriginal;

    private CellState stateMenuSelecionado = CellState.torre;
    private int precoTorre = 10;
    private int precoUpgrade;

    public GameObject textoPopup;
    Vector3 escalaOriginalTxt;
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

    private GameObject menuUpgrade;
    private Torre torreAtual;
    private bool menuAberto = false;

    private GameObject menuUpgradeNucleo;
    private Nucleo nucleoAtual;
    private bool jogoIniciado = false;

    // Adicione uma nova propriedade para os recursos especiais
    private int _recursosEspeciais;
    public int recursosEspeciais
    {
        get { return _recursosEspeciais; }
        set
        {
            _recursosEspeciais = value;
            atualizaTextRecursosEspeciais();
        }
    }

    // Adicione uma referência ao texto de recursos especiais
    public TextMeshProUGUI textRecursosEspeciais;
    private Dictionary<Image, Coroutine> corrotinasAtivas = new Dictionary<Image, Coroutine>();
    private Dictionary<TextMeshProUGUI, Coroutine> corrotinasAtivasTxt = new Dictionary<TextMeshProUGUI, Coroutine>();

    public void StartGameController()
    {
        jogoIniciado = true;
        precoUpgrade = 15;
        recurso = 1000;
        recursosEspeciais = 0;
        spawnerInimigos = transform.GetComponent<SpawnerInimigos>();
        gameLoop = transform.GetComponent<GameLoop>();
        escalaOriginalTxt = textRecurso.transform.localScale;

        // Create a new GameObject for the grid and add the Grid component
        GameObject gridObject = new GameObject("Grid");
        gameGrid = gridObject.AddComponent<Grid>();
        gameGrid.InitializeGrid(14, 10, 4f, new Vector3(-30, -20, 0), cellPrefab, this.transform);

        gameGrid.SetRecursosAleatorios();

        if (!ColorUtility.TryParseHtmlString(corSelecionadaHex, out corSelecionada))
        {
            Debug.LogError("Cor hexadecimal inválida: " + corSelecionadaHex);
        }

        CriarMenuUpgrade();
        CriarMenuUpgradeNucleo();
    }

    private void CriarMenuUpgrade()
    {
        menuUpgrade = new GameObject("MenuUpgrade");
        menuUpgrade.transform.SetParent(transform);
        Canvas canvas = menuUpgrade.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = menuUpgrade.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = menuUpgrade.AddComponent<GraphicRaycaster>();

        RectTransform rectTransform = menuUpgrade.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image background = menuUpgrade.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.5f);

        GameObject menuContent = new GameObject("MenuContent");
        menuContent.transform.SetParent(menuUpgrade.transform, false);
        RectTransform contentRect = menuContent.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(200, 150);

        Image contentBackground = menuContent.AddComponent<Image>();
        contentBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        float buttonWidth = 160f;
        float buttonHeight = 30f;

        CriarBotaoUpgrade("Gelo", new Vector2(0, 45), TorreType.Gelo, buttonWidth, buttonHeight, new Color(0, 0.7f, 1));
        CriarBotaoUpgrade("Fogo", new Vector2(0, 0), TorreType.Fogo, buttonWidth, buttonHeight, new Color(1, 0.4f, 0));
        CriarBotaoUpgrade("Plasma", new Vector2(0, -45), TorreType.Plasma, buttonWidth, buttonHeight, new Color(0.8f, 0, 1));

        CriarBotaoVender(new Vector2(0, -95), buttonWidth, buttonHeight);
        //CriarBotaoUpgradeEspecial(new Vector2(0, -85), buttonWidth, buttonHeight);

        menuUpgrade.SetActive(false);

        // Adicionar evento de clique no background para fechar o menu
        Button backgroundButton = background.gameObject.AddComponent<Button>();
        backgroundButton.onClick.AddListener(FecharMenu);
    }

    private void CriarBotaoUpgrade(string texto, Vector2 posicao, TorreType tipo, float largura, float altura, Color cor)
    {
        Color corBack = new Color(0f, 0f, 0f, 140f / 255f);

        GameObject botao = new GameObject("Upgrade" + tipo.ToString());
        botao.transform.SetParent(menuUpgrade.transform.Find("MenuContent"), false);
        RectTransform rectTransform = botao.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicao;
        rectTransform.sizeDelta = new Vector2(largura, altura);

        Image imagem = botao.AddComponent<Image>();
        imagem.color = corBack;

        Button button = botao.AddComponent<Button>();
        button.onClick.AddListener(() => { UpgradeTorre(tipo); });

        ColorBlock cores = button.colors;
        cores.normalColor = corBack;
        cores.highlightedColor = new Color(corBack.r * 1.2f, corBack.g * 1.2f, corBack.b * 1.2f);
        cores.pressedColor = new Color(corBack.r * 0.8f, corBack.g * 0.8f, corBack.b * 0.8f);
        button.colors = cores;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(botao.transform, false);
        RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.offsetMin = new Vector2(5, 0);
        textRectTransform.offsetMax = new Vector2(-25, 0);

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = texto;
        textComponent.color = cor;
        textComponent.fontSize = 14;
        textComponent.alignment = TextAlignmentOptions.Left;

        GameObject priceObj = new GameObject("PriceText");
        priceObj.transform.SetParent(botao.transform, false);
        RectTransform priceRectTransform = priceObj.AddComponent<RectTransform>();
        priceRectTransform.anchorMin = new Vector2(1, 0);
        priceRectTransform.anchorMax = new Vector2(1, 1);
        priceRectTransform.offsetMin = new Vector2(-60, 0);
        priceRectTransform.offsetMax = new Vector2(-5, 0);

        TextMeshProUGUI priceComponent = priceObj.AddComponent<TextMeshProUGUI>();
        priceComponent.color = cor;
        priceComponent.fontSize = 12;
        priceComponent.alignment = TextAlignmentOptions.Right;
    }

    private void CriarBotaoVender(Vector2 posicao, float largura, float altura)
    {
        GameObject botao = new GameObject("Vender");
        botao.transform.SetParent(menuUpgrade.transform.Find("MenuContent"), false);
        RectTransform rectTransform = botao.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicao;
        rectTransform.sizeDelta = new Vector2(largura, altura);

        Image imagem = botao.AddComponent<Image>();
        imagem.color = Color.red;

        Button button = botao.AddComponent<Button>();
        button.onClick.AddListener(VenderTorreAtual);

        // Configurar as cores do botão para diferentes estados
        ColorBlock cores = button.colors;
        cores.normalColor = Color.red;
        cores.highlightedColor = new Color(1, 0.5f, 0.5f);
        cores.pressedColor = new Color(0.7f, 0, 0);
        button.colors = cores;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(botao.transform, false);
        RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "Vender";
        textComponent.color = Color.white;
        textComponent.fontSize = 14;
        textComponent.alignment = TextAlignmentOptions.Center;
    }

    private void Update()
    {
        if (jogoIniciado && !gameLoop.faseRespondendo && !menuAberto)
        {
            gameGrid.HighlightCelula(UtilsClass.GetMouseWorldPosition());

            if (Input.GetMouseButtonDown(0))
            {
                Celula cel = gameGrid.GetCelula(UtilsClass.GetMouseWorldPosition());
                if (cel)
                {
                    if (cel.GetCellState() == CellState.vazia)
                    {
                        ComprarTorre(cel);
                    }
                    else if (cel.GetCellState() == CellState.torre)
                    {
                        AbrirMenuUpgrade(cel);
                    }
                    else if (cel.GetCellState() == CellState.nucleo)
                    {
                        AbrirMenuUpgradeNucleo(cel);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            FecharMenu();
            FecharMenuNucleo();
        }
    }

    private void ComprarTorre(Celula cel)
    {
        if (recurso >= precoTorre)
        {
            GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
            TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
            text.color = Color.red;
            text.text = "- $ " + precoTorre;
            recurso -= precoTorre;
            precoTorre += gameLoop.fase;
            cel.SetCellState(stateMenuSelecionado);
        }
        else
        {
            MostrarMensagemRecursosInsuficientes(precoTorre);
        }
    }

    private void AbrirMenuUpgrade(Celula cel)
    {
        // Primeiro, vamos parar o efeito de arco-íris em todos os botões
        PararEfeitoArcoIrisEmBotoes();

        torreAtual = cel.GetComponent<Torre>();
        if (torreAtual != null)
        {
            menuUpgrade.SetActive(true);
            Vector3 posicaoMundo = cel.transform.position + new Vector3(0, 1f, 0);
            Vector3 posicaoTela = Camera.main.WorldToScreenPoint(posicaoMundo);
            menuUpgrade.transform.Find("MenuContent").position = posicaoTela;
            menuAberto = true;
            AtualizarBotoesUpgrade();
        }
    }

    private void UpgradeTorre(TorreType novoTipo)
    {
        if (torreAtual != null)
        {
            if (torreAtual.nivel == 4 && recursosEspeciais >= 1 && torreAtual.poderEspecial == PoderEspecial.Nenhum)
            {
                // Realiza o upgrade especial
                torreAtual.UpgradeEspecial();
                recursosEspeciais--;
                torreAtual.nivel++; // Incrementa o nível para indicar o nível especial

                // Inicia o efeito de arco-íris na célula da torre
                Celula celulaTorre = torreAtual.GetComponent<Celula>();
                if (celulaTorre != null)
                {
                    celulaTorre.AtivarEfeitoEspecial(torreAtual.tipo);
                    celulaTorre.AnimarUpgrade();
                    AtualizarBotoesUpgrade(); // Atualiza os botões após o upgrade
                    FecharMenu();
                }
            }
            else
            {
                int custoUpgrade = precoUpgrade * (torreAtual.nivel + 2);
                if (recurso >= custoUpgrade)
                {
                    recurso -= custoUpgrade;
                    Celula celulaTorre = torreAtual.GetComponent<Celula>();
                    if (torreAtual.tipo == novoTipo)
                    {
                        {

                            if (celulaTorre != null)
                            {
                                celulaTorre.AnimarUpgrade();
                            }
                            torreAtual.Upgrade();
                        }
                    }
                    else
                    {
                        celulaTorre.AnimarUpgrade();
                        torreAtual.SetTorreType(novoTipo);
                    }
                    GameObject textoPopupI = Instantiate(textoPopup, torreAtual.transform.position, Quaternion.identity);
                    TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
                    text.color = Color.yellow;
                    text.text = "Upgrade! - $ " + custoUpgrade;
                    atualizaTextRecursos();
                    AtualizarBotoesUpgrade(); // Atualiza os botões após o upgrade
                    FecharMenu();
                }
                else
                {
                    MostrarMensagemRecursosInsuficientes(custoUpgrade);
                    FecharMenu();
                }
            }
        }
    }

    private void MostrarMensagemRecursosInsuficientes(int custo)
    {
        MostrarMensagemRecursosInsuficientes(custo, "");
    }

    private void MostrarMensagemRecursosInsuficientes(int custo, string tipoCusto)
    {
        if (tipoCusto.Equals(""))
        {
            GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
            TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
            text.color = Color.red;
            text.text = custo + " $$ Recursos insuficientes!";
        }
        else
        {
            GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
            TextMeshPro text = textoPopupI.transform.GetComponent<TextMeshPro>();
            text.color = Color.magenta;
            text.text = custo + " $$ Recursos Especiais insuficientes!";
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

    private void atualizaTextRecursos()
    {
        textRecurso.text = recurso.ToString(); // Atualiza o texto imediatamente
        StartCoroutine(AnimarTextoRecurso(textRecurso));
    }

    private void atualizaTextRecursosEspeciais()
    {
        textRecursosEspeciais.text = recursosEspeciais.ToString(); // Atualiza o texto imediatamente
        StartCoroutine(AnimarTextoRecurso(textRecursosEspeciais));
    }

    public IEnumerator AnimarTextoRecurso(TextMeshProUGUI texto)
    {
        float duracaoAnimacao = 0.1f;

        LeanTween.scale(texto.gameObject, escalaOriginalTxt * 1.2f, duracaoAnimacao).setEaseOutBack().setOnComplete(() =>
        {
            LeanTween.scale(texto.gameObject, escalaOriginalTxt, duracaoAnimacao).setEaseInBack();
        });

        yield return new WaitForSeconds(duracaoAnimacao * 2f);
    }


    private void FecharMenu()
    {
        if (menuAberto)
        {
            PararEfeitoArcoIrisEmBotoes();
            menuUpgrade.SetActive(false);
            menuAberto = false;
            torreAtual = null;
        }
    }

    private void VenderTorreAtual()
    {
        if (torreAtual != null)
        {
            Celula cel = torreAtual.GetComponent<Celula>();
            if (cel != null)
            {
                int valorVenda = precoTorre / 2; // Vende por metade do preço
                recurso += valorVenda;
                GameObject textoPopupI = Instantiate(textoPopup, cel.transform.position, Quaternion.identity);
                TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
                text.color = Color.green;
                text.text = "+ $ " + valorVenda;
                cel.transform.Find("TextoNivel").GetComponent<TextMeshPro>().text = "";
                // Destruir o componente Torre
                Destroy(torreAtual);

                // Resetar a célula para o estado vazio
                cel.SetCellState(CellState.vazia);
                cel.setValue("0");
                cel.UpdateCellVisuals();

                atualizaTextRecursos();
                FecharMenu();
            }
        }
    }

    private void AtualizarBotoesUpgrade()
    {
        PararEfeitoArcoIrisEmBotoes();
        foreach (Transform child in menuUpgrade.transform.Find("MenuContent"))
        {
            if (child.name.StartsWith("Upgrade"))
            {
                Button button = child.GetComponent<Button>();
                TorreType tipo = (TorreType)System.Enum.Parse(typeof(TorreType), child.name.Substring(7));
                bool isCurrentType = torreAtual.tipo == tipo;
                bool canUpgrade = torreAtual.nivel < 4;

                button.interactable = (isCurrentType && canUpgrade) || (!isCurrentType && torreAtual.tipo == TorreType.Basic);

                // Adicionar ou atualizar a seta
                Transform arrow = child.Find("Arrow");
                if (arrow == null)
                {
                    GameObject arrowObj = new GameObject("Arrow");
                    arrowObj.transform.SetParent(child, false);
                    Image arrowImage = arrowObj.AddComponent<Image>();
                    arrowImage.sprite = Resources.Load<Sprite>("UI/arrow_icon"); // Certifique-se de ter este ícone
                    RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
                    arrowRect.anchorMin = new Vector2(1, 0.5f);
                    arrowRect.anchorMax = new Vector2(1, 0.5f);
                    arrowRect.anchoredPosition = new Vector2(-45, 0);
                    arrowRect.sizeDelta = new Vector2(20, 20);
                    arrow = arrowObj.transform;
                }

                arrow.gameObject.SetActive(isCurrentType && canUpgrade);

                TextMeshProUGUI text = child.Find("Text").GetComponent<TextMeshProUGUI>();

                if (child.name.Equals("UpgradeGelo"))
                {
                    text.color = new Color(0, 0.7f, 1);
                }
                else if (child.name.Equals("UpgradeFogo"))
                {
                    text.color = new Color(1, 0.4f, 0);
                }
                else
                {
                    text.color = new Color(0.8f, 0, 1);
                }

                // Atualizar o texto do preço
                TextMeshProUGUI priceText = child.Find("PriceText").GetComponent<TextMeshProUGUI>();
                int custoUpgrade = precoUpgrade * (torreAtual.nivel + 2);
                if ((isCurrentType && canUpgrade) || (!isCurrentType && torreAtual.tipo == TorreType.Basic))
                {
                    priceText.text = $"$ {custoUpgrade}";
                    priceText.color = recurso >= custoUpgrade ? Color.green : Color.yellow;
                    RectTransform priceRect = priceText.GetComponent<RectTransform>();
                    priceRect.anchoredPosition = new Vector2(-35, 0);
                }
                else
                {
                    priceText.text = "";
                }

                if (isCurrentType && canUpgrade)
                {
                    Image arrowImage = arrow.GetComponent<Image>();
                    arrowImage.color = recurso >= custoUpgrade ? Color.green : Color.yellow;
                }
                else if (isCurrentType && !canUpgrade)
                {
                    if (torreAtual.nivel == 4)
                    {
                        if (recursosEspeciais >= 1 && torreAtual.poderEspecial == PoderEspecial.Nenhum)
                        {
                            priceText.text = $"$ 1";
                            priceText.color = Color.magenta;
                            text.text = "Upgrade Especial"; // Define o texto para Upgrade Especial
                            text.color = Color.white;
                            button.interactable = true;
                            Debug.Log("INICIANDO ARCO IRIS BOTÃO: " + child.name);
                        }
                        else if (torreAtual.poderEspecial != PoderEspecial.Nenhum)
                        {
                            Coroutine novaCorrotina = StartCoroutine(EfeitoArcoIrisTexto(priceText));
                            corrotinasAtivasTxt[priceText] = novaCorrotina;
                            priceText.text = "MAX";
                        }
                        else
                        {
                            priceText.text = "Sem Recursos";
                        }
                    }
                    else
                    {
                        priceText.text = "MAX";
                        priceText.color = Color.white;
                    }
                }
                if (isCurrentType && torreAtual.nivel == 4 && recursosEspeciais >= 1 && torreAtual.poderEspecial == PoderEspecial.Nenhum)
                {
                    Image imagem = child.GetComponent<Image>();
                    if (!corrotinasAtivas.ContainsKey(imagem))
                    {
                        Coroutine novaCorrotina = StartCoroutine(EfeitoArcoIris(imagem));
                        corrotinasAtivas[imagem] = novaCorrotina;
                    }
                }
                else if (isCurrentType && torreAtual.poderEspecial != PoderEspecial.Nenhum)
                {

                    if (!corrotinasAtivasTxt.ContainsKey(priceText))
                    {
                        Coroutine novaCorrotina = StartCoroutine(EfeitoArcoIrisTexto(priceText));
                        corrotinasAtivasTxt[priceText] = novaCorrotina;
                    }
                }
                else
                {
                    // Parar o efeito se não for o caso especial
                    Image imagem = child.GetComponent<Image>();
                    if (corrotinasAtivas.TryGetValue(imagem, out Coroutine corrotina))
                    {
                        StopCoroutine(corrotina);
                        corrotinasAtivas.Remove(imagem);
                    }

                    if (corrotinasAtivasTxt.TryGetValue(priceText, out Coroutine corrotina2))
                    {
                        StopCoroutine(corrotina2);
                        corrotinasAtivasTxt.Remove(priceText);
                    }

                    imagem.color = new Color(0f, 0f, 0f, 140f / 255f);
                }

            }
        }
    }


    private void CriarMenuUpgradeNucleo()
    {
        menuUpgradeNucleo = new GameObject("MenuUpgradeNucleo");
        menuUpgradeNucleo.transform.SetParent(transform);
        Canvas canvas = menuUpgradeNucleo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = menuUpgradeNucleo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = menuUpgradeNucleo.AddComponent<GraphicRaycaster>();

        RectTransform rectTransform = menuUpgradeNucleo.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image background = menuUpgradeNucleo.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.5f);

        GameObject menuContent = new GameObject("MenuContent");
        menuContent.transform.SetParent(menuUpgradeNucleo.transform, false);
        RectTransform contentRect = menuContent.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(200, 220);

        Image contentBackground = menuContent.AddComponent<Image>();
        contentBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        float buttonWidth = 160f;
        float buttonHeight = 30f;

        CriarBotaoUpgradeNucleo("Vida Máxima", new Vector2(0, 60), buttonWidth, buttonHeight, Color.green);
        CriarBotaoUpgradeNucleo("Regeneração", new Vector2(0, 20), buttonWidth, buttonHeight, Color.blue);
        CriarBotaoUpgradeNucleo("Escudo", new Vector2(0, -20), buttonWidth, buttonHeight, Color.yellow);
        CriarBotaoUpgradeNucleo("Fortificação", new Vector2(0, -60), buttonWidth, buttonHeight, Color.cyan);

        menuUpgradeNucleo.SetActive(false);

        Button backgroundButton = background.gameObject.AddComponent<Button>();
        backgroundButton.onClick.AddListener(FecharMenuNucleo);
    }

    private void CriarBotaoUpgradeNucleo(string texto, Vector2 posicao, float largura, float altura, Color cor)
    {
        Color corBack = new Color(0f, 0f, 0f, 140f / 255f);
        GameObject botao = new GameObject("Upgrade" + texto);
        botao.transform.SetParent(menuUpgradeNucleo.transform.Find("MenuContent"), false);
        RectTransform rectTransform = botao.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicao;
        rectTransform.sizeDelta = new Vector2(largura, altura);

        Image imagem = botao.AddComponent<Image>();
        imagem.color = corBack;

        Button button = botao.AddComponent<Button>();
        button.onClick.AddListener(() => { UpgradeNucleo(texto); });

        ColorBlock cores = button.colors;
        cores.normalColor = corBack;
        cores.highlightedColor = new Color(corBack.r * 1.2f, corBack.g * 1.2f, corBack.b * 1.2f);
        cores.pressedColor = new Color(corBack.r * 0.8f, corBack.g * 0.8f, corBack.b * 0.8f);
        button.colors = cores;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(botao.transform, false);
        RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.offsetMin = new Vector2(5, 0);
        textRectTransform.offsetMax = new Vector2(-25, 0);

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = texto;
        textComponent.color = cor;
        textComponent.fontSize = 14;
        textComponent.alignment = TextAlignmentOptions.Left;

        GameObject priceObj = new GameObject("PriceText");
        priceObj.transform.SetParent(botao.transform, false);
        RectTransform priceRectTransform = priceObj.AddComponent<RectTransform>();
        priceRectTransform.anchorMin = new Vector2(1, 0);
        priceRectTransform.anchorMax = new Vector2(1, 1);
        priceRectTransform.offsetMin = new Vector2(-60, 0);
        priceRectTransform.offsetMax = new Vector2(-5, 0);

        TextMeshProUGUI priceComponent = priceObj.AddComponent<TextMeshProUGUI>();
        priceComponent.color = cor;
        priceComponent.fontSize = 12;
        priceComponent.alignment = TextAlignmentOptions.Right;
    }

    private void AbrirMenuUpgradeNucleo(Celula cel)
    {
        nucleoAtual = cel.GetComponent<Nucleo>();
        if (nucleoAtual != null)
        {
            menuUpgradeNucleo.SetActive(true);
            Vector3 posicaoMundo = cel.transform.position + new Vector3(0, 1f, 0);
            Vector3 posicaoTela = Camera.main.WorldToScreenPoint(posicaoMundo);
            menuUpgradeNucleo.transform.Find("MenuContent").position = posicaoTela;
            menuAberto = true;
            AtualizarBotoesUpgradeNucleo();
        }
    }

    private void UpgradeNucleo(string tipoUpgrade)
    {
        if (nucleoAtual != null)
        {
            int custoUpgrade = 0;
            switch (tipoUpgrade)
            {
                case "Vida Máxima":
                    custoUpgrade = nucleoAtual.GetPrecoUpgradeVidaMaxima();
                    break;
                case "Regeneração":
                    custoUpgrade = nucleoAtual.GetPrecoUpgradeRegeneracao();
                    break;
                case "Escudo":
                    custoUpgrade = nucleoAtual.GetPrecoUpgradeEscudo();
                    break;
                case "Fortificação":
                    custoUpgrade = nucleoAtual.GetPrecoUpgradeFortificacao();
                    break;
            }

            if (recurso >= custoUpgrade)
            {
                recurso -= custoUpgrade;
                switch (tipoUpgrade)
                {
                    case "Vida Máxima":
                        nucleoAtual.UpgradeVidaMaxima();
                        break;
                    case "Regeneração":
                        nucleoAtual.UpgradeRegeneracao();
                        break;
                    case "Escudo":
                        nucleoAtual.UpgradeEscudo();
                        break;
                    case "Fortificação":
                        nucleoAtual.UpgradeFortificacao();
                        break;
                }
                GameObject textoPopupI = Instantiate(textoPopup, nucleoAtual.transform.position, Quaternion.identity);
                TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
                text.color = Color.yellow;
                text.text = "Upgrade " + tipoUpgrade + "! - $ " + custoUpgrade;
                atualizaTextRecursos();
                AtualizarBotoesUpgradeNucleo();
            }
            else
            {
                MostrarMensagemRecursosInsuficientes(custoUpgrade);
            }
        }
    }

    private void FecharMenuNucleo()
    {
        menuUpgradeNucleo.SetActive(false);
        menuAberto = false;
        nucleoAtual = null;
    }

    private void AtualizarBotoesUpgradeNucleo()
    {
        foreach (Transform child in menuUpgradeNucleo.transform.Find("MenuContent"))
        {
            if (child.name.StartsWith("Upgrade"))
            {
                Button button = child.GetComponent<Button>();
                string tipoUpgrade = child.name.Substring(7);
                int custoUpgrade = 0;

                switch (tipoUpgrade)
                {
                    case "Vida Máxima":
                        custoUpgrade = nucleoAtual.GetPrecoUpgradeVidaMaxima();
                        break;
                    case "Regeneração":
                        custoUpgrade = nucleoAtual.GetPrecoUpgradeRegeneracao();
                        break;
                    case "Escudo":
                        custoUpgrade = nucleoAtual.GetPrecoUpgradeEscudo();
                        break;
                    case "Fortificação":
                        custoUpgrade = nucleoAtual.GetPrecoUpgradeFortificacao();
                        break;
                }

                TextMeshProUGUI priceText = child.Find("PriceText").GetComponent<TextMeshProUGUI>();
                priceText.text = $"$ {custoUpgrade}";
                priceText.color = recurso >= custoUpgrade ? Color.green : Color.yellow;
                RectTransform priceRect = priceText.GetComponent<RectTransform>();
                priceRect.anchoredPosition = new Vector2(-35, 0);

                Transform arrow = child.Find("Arrow");
                if (arrow == null)
                {
                    GameObject arrowObj = new GameObject("Arrow");
                    arrowObj.transform.SetParent(child, false);
                    Image arrowImage = arrowObj.AddComponent<Image>();
                    arrowImage.sprite = Resources.Load<Sprite>("UI/arrow_icon");
                    RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
                    arrowRect.anchorMin = new Vector2(1, 0.5f);
                    arrowRect.anchorMax = new Vector2(1, 0.5f);
                    arrowRect.anchoredPosition = new Vector2(-45, 0);
                    arrowRect.sizeDelta = new Vector2(20, 20);
                    arrow = arrowObj.transform;

                    arrowImage.color = recurso >= custoUpgrade ? Color.green : Color.yellow;
                }
                else
                {
                    Image arrowImage = arrow.GetComponent<Image>();
                    arrowImage.color = recurso >= custoUpgrade ? Color.green : Color.yellow;
                }
            }
        }
    }

    public void AvancarFase()
    {
        // Chame este método quando a fase avançar
        Nucleo[] nucleos = FindObjectsOfType<Nucleo>();
        foreach (Nucleo nucleo in nucleos)
        {
            nucleo.AvancarFase();
        }
    }

    private IEnumerator EfeitoArcoIrisTexto(TextMeshProUGUI texto)
    {
        float h = 0;
        while (true)
        {
            h = (h + Time.deltaTime * 1.5f) % 1f;
            texto.color = Color.HSVToRGB(h, 1, 1);
            yield return null;
        }
    }

    private IEnumerator EfeitoArcoIris(Image imagem)
    {
        float h, s, v;
        Color corBase = imagem.color;
        Color.RGBToHSV(corBase, out h, out s, out v);

        Button botao = imagem.GetComponent<Button>();
        if (botao == null)
        {
            Debug.LogWarning("O efeito arco-íris requer um botão associado à imagem!");
            yield break;
        }

        ColorBlock colorBlock = botao.colors;

        while (true)
        {
            h = (h + Time.deltaTime * 1.5f) % 1f;
            imagem.color = Color.HSVToRGB(h, 1, 1);

            Color novaCor = Color.HSVToRGB(h, 1, 1);

            // Atualiza as cores do ColorBlock
            colorBlock.normalColor = novaCor * 0.5f;
            colorBlock.highlightedColor = novaCor; // Levemente mais claro no highlight
            colorBlock.pressedColor = novaCor * 3f;    // Levemente mais escuro no pressionado
            colorBlock.selectedColor = novaCor;          // Mantém a mesma cor no selecionado
            colorBlock.disabledColor = novaCor * 0.5f;   // Mais opaco no desativado

            botao.colors = colorBlock;

            yield return null;
        }
    }

    private void PararEfeitoArcoIrisEmBotoes()
    {
        foreach (Transform child in menuUpgrade.transform.Find("MenuContent"))
        {
            if (child.name.StartsWith("Upgrade"))
            {
                Image imagem = child.GetComponent<Image>();

                if (corrotinasAtivas.TryGetValue(imagem, out Coroutine corrotina))
                {
                    StopCoroutine(corrotina);
                    corrotinasAtivas.Remove(imagem);
                }

                // Resetar a cor do botão
                imagem.color = new Color(0f, 0f, 0f, 140f / 255f);

                // Resetar o texto
                TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = child.name.Substring(7);
                }
            }
        }
    }
}

