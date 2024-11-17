using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    public GameObject painelPrincipal;
    public GameObject painelRecursos;

    public Transform naturais;

    void Start()
    {
        painelPrincipal.active = true;
        painelRecursos.active = true;
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
