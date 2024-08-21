using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recurso : MonoBehaviour
{
    public double valorBase;
    ResourceType tipoRecurso;

    public void SetValorBase(double valor)
    {
        valorBase = valor;
    }

    public void SetTipoRecurso(ResourceType valor)
    {
        tipoRecurso = valor;
    }

    public ResourceType getTipoRecurso()
    {
        return tipoRecurso;
    }

    public double GetValorBase()
    {
        return valorBase;
    }
}
