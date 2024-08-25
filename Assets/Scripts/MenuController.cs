using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    public GameObject painelPrincipal;
    public GameObject painelRecursos;

    public Transform naturais;
    public Transform inteiros;
    public Transform racionais;
    public Transform irracionais;
    public Transform reais;
    public Transform complexos;
    public Transform algebricos;
    public Transform transcendentes;
    public Transform imaginarios;

    void Start()
    {
        painelPrincipal.active = true;
        painelRecursos.active = false;
    }

    public void TrocaMenu() {
        if (painelPrincipal.active == true) {
            painelPrincipal.active = false;
            painelRecursos.active = true;
        }
        else
        {
            painelPrincipal.active = true;
            painelRecursos.active = false;
        }
    }

}
