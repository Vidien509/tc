using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum ResourceType { naturais, inteiros, racionais, irracionais, reais, complexos, algebricos, transcendentes, imaginarios, complexospuros }

public class ResourceController : MonoBehaviour
{

    public int naturais = 0;
    public int inteiros = 0;
    public double racionais = 0;
    public double irracionais = 0;
    public double reais = 0;
    public double complexos = 0;
    public double algebricos = 0;
    public double transcendentes = 0; 
    public double imaginarios = 0;
    public double complexospuros = 0;

    private List<Celula> arrayExtratores = new List<Celula>();
    private Dictionary<Celula, Coroutine> coroutineDictionary = new Dictionary<Celula, Coroutine>();

    void Update()
    {
        
    }
    public void adicionaExtrator(Celula extrator)
    {
        if (extrator == null)
        {
            Debug.Log("Erro: extrator é null.");
            return;
        }

        arrayExtratores.Add(extrator);
        StartCoroutine(ColetaRecursos(extrator));
    }

    private IEnumerator ColetaRecursos(Celula extrator)
    {
        if (extrator == null)
        {
            Debug.Log("Erro: extrator é null.");
            yield break;
        }

        Extrator ext = extrator.GetComponent<Extrator>();

        if (ext == null)
        {
            Debug.Log("Erro: Extrator componente não encontrado no GameObject " + extrator.name);
            yield break;
        }

        while (true)
        {
            AdicionaValor(ext.GetTipoRecurso(), ext.GetValorBase());
            yield return new WaitForSeconds(ext.GetTempoExtracaoBase());
        }
    }

    public void ParaColeta(Celula extrator)
    {
        if (coroutineDictionary.ContainsKey(extrator))
        {
            StopCoroutine(coroutineDictionary[extrator]);
            coroutineDictionary.Remove(extrator);
        }
    }

    public List<Celula> getExtratores()
    {
        return arrayExtratores;
    }

    public void AdicionaValor(ResourceType tipo, double valor)
    {
        switch (tipo)
        {
            case ResourceType.naturais:
                naturais += int.Parse(valor.ToString());
                break;
            case ResourceType.inteiros:
                inteiros += int.Parse(valor.ToString());
                break;
            case ResourceType.racionais:
                racionais += valor;
                break;
            case ResourceType.irracionais:
                irracionais += valor;
                break;
            case ResourceType.reais:
                reais += valor;
                break;
            case ResourceType.complexos:
                complexos += valor;
                break;
            case ResourceType.algebricos:
                algebricos += valor;
                break;
            case ResourceType.transcendentes:
                transcendentes += valor;
                break;
            case ResourceType.imaginarios:
                imaginarios += valor;
                break;
            case ResourceType.complexospuros:
                complexospuros += valor;
                break;
        }
    }
}
