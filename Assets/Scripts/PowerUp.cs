using CodeMonkey.Utils;
using TMPro;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public GameLoop gameLoop;
    public GameController gameController;
    private float tempoQuestao = 0f;
    private float tempoPowerUp = 0f;
    private float rotationSpeed = 50f; // Efeito visual de rotação
    private bool questaoIniciada;
    private string bonusAtivo;
    private int codigoBonusAtivo;
    Inimigo[] inimigos;

    void Start()
    {
        questaoIniciada = false;
        gameLoop = FindObjectOfType<GameLoop>();
        gameController = FindObjectOfType<GameController>();

        gameLoop.powerUpAtivado = true;
    }

    void Update()
    {
        // Efeito visual: gira o power-up
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        if (Input.GetMouseButtonDown(0)) // Botão esquerdo
        {
            float dist = Vector3.Distance(UtilsClass.GetMouseWorldPosition(), transform.position);
            if(dist <= 1.5f)
            {
                if (!questaoIniciada)
                {
                    transform.localScale = new Vector3(0,0,0);
                    ColetarPowerUp();
                    questaoIniciada = true;
                }
            }
        }
        if (questaoIniciada)
        {
            tempoQuestao += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                questaoIniciada = false;
                gameLoop.ProcessarResposta(false);
                Debug.Log("RESPOSTA CORRETA: " + gameLoop.respostaCorreta);
                if (gameLoop.respostaCorreta)
                {
                    int randomBonus = Random.Range(0, 10);
                    GameObject textoPopupRC = Instantiate(gameLoop.textoPopup, new Vector3(0, 0, 0), Quaternion.identity);
                    TextMeshPro textRC = textoPopupRC.GetComponent<TextMeshPro>();
                    TextoPopup textoPopupScript = textoPopupRC.GetComponent<TextoPopup>();
                    textoPopupScript.Configure(true, false, false, 1f, 3f);
                    textRC.fontSize = 28;

                    switch (randomBonus)
                    {
                        case 0:
                            // Todos os inimigos levam dano instantâneo
                            bonusAtivo = "TORRES TEM 20% A MAIS DE DANO!";
                            codigoBonusAtivo = 0;
                            textRC.color = Color.HSVToRGB(0f, 1f, 1f);
                            break;

                        case 1:
                            // Próxima torre com custo zero
                            bonusAtivo = "PROJÉTEIS MULTIPLICADOS POR 2X!";
                            codigoBonusAtivo = 1;
                            textRC.color = Color.HSVToRGB(0.1f, 1f, 1f);
                            break;

                        case 2:
                            inimigos = FindObjectsOfType<Inimigo>();
                            textRC.color = Color.HSVToRGB(0.75f, 0.8f, 1f);
                            // Percorre cada inimigo e reduz sua vida pela metade
                            foreach (Inimigo inimigo in inimigos)
                            {
                                inimigo.vida = Mathf.Max(1, inimigo.vida / 2); // Garante que a vida não seja menor que 1
                            }
                            bonusAtivo = "INIMIGOS AGORA TEM 1/2 DA VIDA!";
                            codigoBonusAtivo = 2;
                            break;

                        case 3:
                            inimigos = FindObjectsOfType<Inimigo>();
                            textRC.color = Color.HSVToRGB(0.55f, 0.7f, 1f);
                            foreach (Inimigo inimigo in inimigos)
                            {
                                inimigo.velocidade = 0;
                            }
                            bonusAtivo = "CONGELAR TODOS OS INIMIGOS!";
                            codigoBonusAtivo = 3;
                            break;

                        case 4:
                            // Inimigos dão 50% mais dinheiro
                            bonusAtivo = "14X MAIS DINHEIRO RECEBIDOS!";
                            codigoBonusAtivo = 4;
                            textRC.color = Color.HSVToRGB(0.15f, 0.8f, 1f);
                            break;

                        case 5:
                            // Aumenta a velocidade de ataque das torres por 10 segundos
                            bonusAtivo = "TORRES ATIRAM 2X MAIS RÁPIDO!";
                            codigoBonusAtivo = 5;
                            textRC.color = Color.HSVToRGB(0.3f, 1f, 1f);
                            break;

                        case 6:
                            // Multiplica o dano crítico das torres por 3x durante 15 segundos
                            bonusAtivo = "DANO CRÍTICO DAS TORRES É 3X MAIS FORTE!";
                            codigoBonusAtivo = 6;
                            textRC.color = Color.HSVToRGB(0.9f, 0.9f, 1f);
                            break;

                        case 7:
                            // Dobra o dinheiro recebido ao acertar respostas por 20 segundos
                            bonusAtivo = "4X DE RECOMPENSA POR RESPOSTA CERTA!";
                            codigoBonusAtivo = 7;
                            textRC.color = Color.HSVToRGB(0.45f, 0.85f, 1f);
                            break;
                    }

                    gameLoop.powerUpConcedido = true;
                    gameLoop.powerUpsAtivos.Add(new PowerUpAtivo(codigoBonusAtivo, 10f));
                    gameLoop.tempoPowerUp = 0f;
                    if (codigoBonusAtivo == 4)
                    {
                        GameObject textoPopupI = Instantiate(gameLoop.textoPopup, new Vector3(0, -50, 0), Quaternion.identity);
                        TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
                        text.color = Color.green;
                        text.fontSize = 28;

                        text.color = Color.green;
                        text.text = "+ $ 280";
                        gameController.recurso += 280;
                    }

                    textRC.text = bonusAtivo;
                }
                gameLoop.powerUpAtivado = false;
                Destroy(gameObject);
            }
            if (tempoQuestao >= 10f)
            {
                questaoIniciada = false;
                gameLoop.powerUpAtivado = false;
                Destroy(gameObject);
            }
        }
        else
        {
            tempoPowerUp += Time.deltaTime;
            if(tempoPowerUp >= 10f)
            {
                gameLoop.powerUpAtivado = false;
                Destroy(gameObject);
            }
        }
    }

    public void ColetarPowerUp()
    {
        gameController.audioSource.PlayOneShot(gameController.powerUpSound);
        gameLoop.iniciarNovaQuestao();

    }
}