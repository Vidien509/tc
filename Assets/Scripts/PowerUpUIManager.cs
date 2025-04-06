using UnityEngine;
using System.Collections.Generic;

public class PowerUpUIManager : MonoBehaviour
{
    public static PowerUpUIManager Instance;

    [System.Serializable]
    public class PowerUpUI
    {
        public int powerUpCode;
        public GameObject uiElement;
    }

    public List<PowerUpUI> activePowerUpUIs = new List<PowerUpUI>();
    public Vector2 startPosition = new Vector2(-120, -500);
    public Vector2 elementSize = new Vector2(160, 10);
    public float spacing = 10f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePowerUpUI(int[] activePowerUps)
    {

        Debug.Log("UPDATE POWER UP UI: " + activePowerUps);
        // Remove UIs de power-ups que não estão mais ativos
        for (int i = activePowerUpUIs.Count - 1; i >= 0; i--)
        {
            bool stillActive = false;
            foreach (int code in activePowerUps)
            {
                if (code == activePowerUpUIs[i].powerUpCode)
                {
                    stillActive = true;
                    break;
                }
            }

            if (!stillActive)
            {
                Destroy(activePowerUpUIs[i].uiElement);
                activePowerUpUIs.RemoveAt(i);
            }
        }

        // Adiciona UIs para novos power-ups
        foreach (int code in activePowerUps)
        {
            bool hasUI = false;
            foreach (PowerUpUI ui in activePowerUpUIs)
            {
                if (ui.powerUpCode == code)
                {
                    hasUI = true;
                    break;
                }
            }

            if (!hasUI)
            {
                GameObject newUI = RetangulosPowerUp.Instance.CriarRetangulo(GetPowerUpColor(code));
                ConfigurePowerUpUI(newUI, code);

                PowerUpUI newPowerUpUI = new PowerUpUI();
                newPowerUpUI.powerUpCode = code;
                newPowerUpUI.uiElement = newUI;

                activePowerUpUIs.Add(newPowerUpUI);
            }
        }

        // Reposiciona todos os elementos
        RepositionAllElements();
    }

    private void ConfigurePowerUpUI(GameObject uiElement, int powerUpCode)
    {
        GameObject textoObj = new GameObject("PowerUpText");
        textoObj.transform.SetParent(uiElement.transform);

        RectTransform textRect = textoObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        TMPro.TextMeshProUGUI textComponent = textoObj.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = GetPowerUpName(powerUpCode);
        textComponent.color = Color.white;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        textComponent.enableWordWrapping = false;
        textComponent.fontSize = 12;
    }

    private string GetPowerUpName(int code)
    {
        switch (code)
        {
            case 0:
                // Todos os inimigos levam dano instantâneo
                return "TORRES TEM 20% A MAIS DE DANO!";

            case 1:
                // Próxima torre com custo zero
                return "PROJÉTEIS MULTIPLICADOS POR 2X!";

            case 2:
                // Todos os inimigos na tela tem sua vida diminuida pela metade
                return "INIMIGOS AGORA TEM 1/2 DA VIDA!";

            case 3:
                // Todos os inimigos na tela são congelados
                return "CONGELAR TODOS OS INIMIGOS!";

            case 4:
                // Inimigos dão 50% mais dinheiro
                return "14X MAIS DINHEIRO RECEBIDOS!";

            case 5:
                // Aumenta a velocidade de ataque das torres por 10 segundos
                return "TORRES ATIRAM 2X MAIS RÁPIDO!";
            case 6:
                // Multiplica o dano crítico das torres por 3x durante 15 segundos
                return "DANO CRÍTICO DAS TORRES É 3X MAIS FORTE!";

            case 7:
                // Dobra o dinheiro recebido ao acertar respostas por 20 segundos
                return "4X DE RECOMPENSA POR RESPOSTA CERTA!";

            default: return "POWER UP " + code;
        }
    }

    private Color GetPowerUpColor(int code)
    {
        switch (code)
        {
            case 0:
                // Todos os inimigos levam dano instantâneo
                return Color.HSVToRGB(0f, 1f, 1f);

            case 1:
                // Próxima torre com custo zero
                return Color.HSVToRGB(0.1f, 1f, 1f);

            case 2:
                // Todos os inimigos na tela tem sua vida diminuida pela metade
                return Color.HSVToRGB(0.75f, 0.8f, 1f);

            case 3:
                // Todos os inimigos na tela são congelados
                return Color.HSVToRGB(0.55f, 0.7f, 1f);

            case 4:
                // Inimigos dão 50% mais dinheiro
                return Color.HSVToRGB(0.15f, 0.8f, 1f);

            case 5:
                // Aumenta a velocidade de ataque das torres por 10 segundos
                return Color.HSVToRGB(0.3f, 1f, 1f);
            case 6:
                // Multiplica o dano crítico das torres por 3x durante 15 segundos
                return Color.HSVToRGB(0.9f, 0.9f, 1f);

            case 7:
                // Dobra o dinheiro recebido ao acertar respostas por 20 segundos
                return Color.HSVToRGB(0.45f, 0.85f, 1f);

            default: return Color.white;
        }
    }

    private void RepositionAllElements()
    {
        float startY = -100; // Começa abaixo da borda superior

        for (int i = 0; i < activePowerUpUIs.Count; i++)
        {
            RectTransform rt = activePowerUpUIs[i].uiElement.GetComponent<RectTransform>();
            float posY = startY - (rt.sizeDelta.y + 20) * i;
            rt.anchoredPosition = new Vector2(-10f, posY); // -10 no X para margem direita
        }
    }
}