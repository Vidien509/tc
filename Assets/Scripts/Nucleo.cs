using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nucleo : MonoBehaviour
{

    public int vida;
    public Celula cel;
    // Start is called before the first frame update
    void Start()
    {
        cel = transform.GetComponent<Celula>(); 
        int aux;
        vida = int.TryParse(cel.getValue(), out aux) ? aux : 100;
    }

    // Update is called once per frame
    void Update()
    {
        if(vida <= 0)
        {
            vida = 0;
            Destroy(transform.gameObject);
        }
    }
}
