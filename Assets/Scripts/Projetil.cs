using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projetil : MonoBehaviour
{
    public float velocidade;
    private Transform alvo;
    private int dano;
    private bool semAlvo = false;
    private Vector3 direcaoAtual;

    private void Start()
    {
        velocidade = 80f;
    }

    public void Configurar(Transform alvo, int dano)
    {
        this.alvo = alvo;
        this.dano = dano;

        // Se já não houver um alvo, define uma direção padrão
        if (alvo == null)
        {
            direcaoAtual = transform.up; // Direção padrão inicial
        }
    }

    void Update()
    {
        if (alvo == null)
        {
            if (!semAlvo)
            {
                // Primeiro momento sem alvo: define a direção atual baseada no movimento atual
                semAlvo = true;
                direcaoAtual = direcaoAtual != Vector3.zero ? direcaoAtual : transform.up; // Direção já calculada ou padrão
                Invoke(nameof(DestroyAfterTime), 2f); // Destrói após 2 segundos
            }

            // Move o projétil na direção previamente definida
            transform.position += direcaoAtual.normalized * velocidade * Time.deltaTime;
            return;
        }

        // Move o projétil em direção ao alvo
        Vector3 direcao = alvo.position - transform.position;
        direcaoAtual = direcao.normalized; // Atualiza a direção atual para seguir o alvo
        transform.position += direcaoAtual * velocidade * Time.deltaTime;

        // Verifica se o projétil atingiu o alvo
        if (Vector3.Distance(transform.position, alvo.position) < 0.1f)
        {
            AlvoAtingido();
        }
    }

    void AlvoAtingido()
    {
        Inimigo inimigo = alvo.GetComponent<Inimigo>();
        if (inimigo != null)
        {
            inimigo.ReceberDano(dano);
        }
        Destroy(gameObject); // Destrói o projétil
    }

    void DestroyAfterTime()
    {
        Destroy(gameObject); // Destrói o projétil após 2 segundos sem alvo
    }
}
