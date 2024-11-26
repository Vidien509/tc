using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Nucleo : MonoBehaviour
{
    private int _vida;
    public int vida
    {
        get { return _vida; }
        set
        {
            _vida = value;
            atualizaTextVida();
        }
    }

    public Celula cel;

    TextMeshPro texto;
    // Start is called before the first frame update
    void Start()
    {
        texto = transform.GetChild(0).GetComponent<TextMeshPro>();
        cel = transform.GetComponent<Celula>(); 
        int aux;
        vida = int.TryParse(cel.getValue(), out aux) ? aux : 100;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReceberDano(int dano)
    {
        vida -= dano;
        if (vida <= 0)
        {
            Debug.Log("O núcleo foi destruído!");
            Destroy(transform.gameObject);
        }
    }

    private void atualizaTextVida()
    {
        cel.setValue(vida.ToString());
        texto.text = cel.getValue();
    }

    public void AdicionarVida(int quantidade)
    {
        vida += quantidade;
    }
}
