using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    public float velocidade; // Velocidade de movimento
    public GameObject textoPopup;
    public GameController gameController;
    private Queue<Vector3> waypoints; // Fila de waypoints para o caminho
    private Vector3 alvoAtual; // Próximo ponto no caminho
    private bool chegouNoNucleo = false;
    private Nucleo nucleoAlvo;
    private int vida;

    public delegate void InimigoMortoHandler();
    public event InimigoMortoHandler onInimigoMorto; // Evento que será chamado quando o inimigo morrer

    private void Start() {
        gameController = FindAnyObjectByType<GameController>();
        vida = 30;
        velocidade = 15f;
    }
    public void Configurar(Vector3 posicaoNucleo, Nucleo nucleo)
    {
        nucleoAlvo = nucleo;
        waypoints = GerarCaminho(transform.position, posicaoNucleo); // Gera o caminho
        if (waypoints.Count > 0)
        {
            alvoAtual = waypoints.Dequeue(); // Define o primeiro ponto como o alvo
        }
    }

    void Update()
    {
        if (chegouNoNucleo || nucleoAlvo == null) return;

        // Move em direção ao alvo atual
        transform.position = Vector3.MoveTowards(transform.position, alvoAtual, velocidade * Time.deltaTime);

        // Verifica se chegou no waypoint atual
        if (Vector3.Distance(transform.position, alvoAtual) < 0.1f)
        {
            if (waypoints.Count > 0)
            {
                alvoAtual = waypoints.Dequeue(); // Próximo ponto
            }
            else
            {
                chegouNoNucleo = true;
                AtacarNucleo();
            }
        }
    }

    void AtacarNucleo()
    {
        if (nucleoAlvo != null)
        {
            nucleoAlvo.ReceberDano(10); // Aplica 10 de dano
            Debug.Log($"Núcleo atacado! Vida restante: {nucleoAlvo.vida}");
        }
        Destroy(gameObject); // Remove o inimigo após o ataque
    }

    Queue<Vector3> GerarCaminho(Vector3 inicio, Vector3 destino)
    {
        Queue<Vector3> caminho = new Queue<Vector3>();

        Vector3 posAtual = inicio;
        Vector3 diferenca = destino - inicio;

        // Movimenta no eixo X até o destino X
        if (diferenca.x != 0)
        {
            Vector3 destinoX = new Vector3(destino.x, inicio.y, inicio.z);
            caminho.Enqueue(destinoX);
        }

        // Movimenta no eixo Y até o destino final
        if (diferenca.y != 0)
        {
            Vector3 destinoY = new Vector3(destino.x, destino.y, inicio.z);
            caminho.Enqueue(destinoY);
        }

        return caminho;
    }

    public void ReceberDano(int dano)
    {
        vida -= dano;
        if (vida <= 0)
        {
            GameObject textoPopupI = Instantiate(textoPopup, UtilsClass.GetMouseWorldPosition(), Quaternion.identity);
            TextMeshPro text = textoPopupI.GetComponent<TextMeshPro>();
            text.color = Color.green;
            text.text = "+ $ 5";
            gameController.recurso += 5;
            onInimigoMorto?.Invoke();
            Destroy(gameObject);
        }
    }
}
