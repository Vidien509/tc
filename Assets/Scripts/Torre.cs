using System.Collections;
using UnityEngine;
using TMPro;
using System;

public enum TorreType { Basic, Gelo, Fogo, Plasma }

public enum PoderEspecial
{
    Nenhum,
    DanoArea,
    CongelamentoTotal,
    ExplosaoMassiva,
    CampoEnergia
}

public class Torre : MonoBehaviour
{
    public float alcance;
    public float intervaloAtaque;
    public int dano;
    public GameObject prefabProjetil;
    public int nivel = 0;
    public TorreType tipo = TorreType.Basic;
    public PoderEspecial poderEspecial = PoderEspecial.Nenhum;

    private Inimigo alvoAtual;
    private Celula celula;

    private float shootAnimationDuration = 0.1f;
    private float bloomIntensity = 1.5f;
    private Material bloomMaterial;
    Color originalColor;

    private void Awake()
    {
        if (prefabProjetil == null)
        {
            prefabProjetil = Resources.Load<GameObject>("Prefabs/Projetil");
            if (prefabProjetil == null)
            {
                Debug.LogError("Prefab do projétil não encontrado na pasta Resources/Prefabs/Projetil!");
            }
        }
        celula = GetComponent<Celula>();
    }

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        AtualizarAtributos();
        StartCoroutine(AtaqueContinuo());

        AtualizarTextoNivel();

        // Create bloom material
        bloomMaterial = new Material(Shader.Find("Hidden/BloomShader"));
        bloomMaterial.SetFloat("_BloomIntensity", bloomIntensity);
    }

    void Update()
    {
        alvoAtual = DetectarInimigoMaisProximo();
        if (alvoAtual != null && celula != null)
        {
            Vector3 direcao = alvoAtual.transform.position - transform.position;
            celula.UpdateTorreDirection(direcao);
        }
    }

    public void SetTorreType(TorreType novoTipo)
    {
        tipo = novoTipo;
        nivel = 1;
        AtualizarAtributos();
        celula.AtualizarVisualTorre(tipo, nivel);
        AtualizarTextoNivel();
    }

    public void Upgrade()
    {
        if (nivel < 4)
        {
            nivel++;
            AtualizarAtributos();
            celula.AtualizarVisualTorre(tipo, nivel);
            AtualizarTextoNivel();
        }
    }

    private void AtualizarAtributos()
    {
        switch (tipo)
        {
            case TorreType.Basic:
                alcance = 20f + (nivel * 5f);
                intervaloAtaque = 1.5f - (nivel * 0.25f);
                dano = 10 + (nivel * 5);
                break;
            case TorreType.Gelo:
                alcance = 15f + (nivel * 3f);
                intervaloAtaque = 2f - (nivel * 0.3f);
                dano = 5 + (nivel * 3);
                break;
            case TorreType.Fogo:
                alcance = 18f + (nivel * 4f);
                intervaloAtaque = 1.2f - (nivel * 0.2f);
                dano = 8 + (nivel * 4);
                break;
            case TorreType.Plasma:
                alcance = 25f + (nivel * 6f);
                intervaloAtaque = 2.5f - (nivel * 0.4f);
                dano = 15 + (nivel * 7);
                break;
        }
    }

    IEnumerator AtaqueContinuo()
    {
        while (true)
        {
            if (alvoAtual != null)
            {
                Atacar(alvoAtual);
            }
            yield return new WaitForSeconds(intervaloAtaque);
        }
    }

    Inimigo DetectarInimigoMaisProximo()
    {
        Inimigo[] inimigos = FindObjectsOfType<Inimigo>();
        Inimigo inimigoMaisProximo = null;
        float menorDistancia = Mathf.Infinity;

        foreach (Inimigo inimigo in inimigos)
        {
            if (inimigo == null || !inimigo.gameObject.activeInHierarchy)
                continue;

            // Inscrever-se no evento de morte para evitar selecionar inimigos mortos
            inimigo.onInimigoMorto += () =>
            {
                if (inimigoMaisProximo == inimigo)
                {
                    inimigoMaisProximo = null; // Resetar o alvo se ele morrer
                }
            };

            float distancia = Vector3.Distance(transform.position, inimigo.transform.position);
            if (distancia < menorDistancia && distancia <= alcance)
            {
                menorDistancia = distancia;
                inimigoMaisProximo = inimigo;
            }
        }

        return inimigoMaisProximo;
    }

    void Atacar(Inimigo inimigo)
    {
        if (inimigo == null) return;

        if (prefabProjetil != null)
        {
            StartCoroutine(ShootingAnimation());
            LancarProjetil(inimigo.transform);

            // Ativar o poder especial
            if (poderEspecial != PoderEspecial.Nenhum)
            {
                AtivarPoderEspecial(inimigo);
            }
        }
        else
        {
            inimigo.ReceberDano(dano);
        }
    }

    void LancarProjetil(Transform alvo)
    {
        GameObject projetil = Instantiate(prefabProjetil, transform.position, Quaternion.identity);
        Projetil scriptProjetil = projetil.GetComponent<Projetil>();
        if (scriptProjetil != null)
        {
            scriptProjetil.Configurar(alvo, dano, TipoTorreParaProjetil(tipo));
            StartCoroutine(ApplyBloomEffect(scriptProjetil.gameObject));
            StartCoroutine(ApplyBloomEffect(gameObject)); // Apply bloom to the tower as well
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }

    private ProjetilTipo TipoTorreParaProjetil(TorreType tipoTorre)
    {
        switch (tipoTorre)
        {
            case TorreType.Gelo:
                return ProjetilTipo.Gelo;
            case TorreType.Fogo:
                return ProjetilTipo.Fogo;
            case TorreType.Plasma:
                return ProjetilTipo.Plasma;
            default:
                return ProjetilTipo.Basic;
        }
    }

    public void AtualizarTextoNivel()
    {
        string nivelTexto = ToRoman(nivel);
        TextMeshPro textoNivel = transform.Find("TextoNivel").GetComponent<TextMeshPro>();
        textoNivel.text = nivelTexto;
    }

    private IEnumerator ShootingAnimation()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Color shootColor = GetTowerColor();
        shootColor.a = 0.7f; // Ajuste a transparência aqui

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f; // Reduzido de 1.2f para 1.1f para um efeito mais suave

        float elapsed = 0f;
        while (elapsed < shootAnimationDuration)
        {
            float t = elapsed / shootAnimationDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            spriteRenderer.color = Color.Lerp(originalColor, shootColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < shootAnimationDuration)
        {
            float t = elapsed / shootAnimationDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            spriteRenderer.color = Color.Lerp(shootColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Garante que a torre volte ao estado original
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ApplyBloomEffect(GameObject target)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Material originalMaterial = renderer.material;
            renderer.material = bloomMaterial;

            yield return new WaitForSeconds(0.1f);

            renderer.material = originalMaterial;
        }
    }

    public Color GetTowerColor()
    {
        switch (tipo)
        {
            case TorreType.Gelo:
                return new Color(0, 0.7f, 1);
            case TorreType.Fogo:
                return new Color(1, 0.4f, 0);
            case TorreType.Plasma:
                return new Color(0.8f, 0, 1);
            default:
                return Color.white;
        }
    }

    private void AtivarPoderEspecial(Inimigo alvo)
    {
        switch (poderEspecial)
        {
            case PoderEspecial.DanoArea:
                DanoEmArea(alvo.transform.position, 3f, dano / 2);
                break;
            case PoderEspecial.CongelamentoTotal:
                CongelamentoTotal(alvo.transform.position, 3f);
                break;
            case PoderEspecial.ExplosaoMassiva:
                ExplosaoMassiva(alvo.transform.position, 5f, dano * 2);
                break;
            case PoderEspecial.CampoEnergia:
                CampoEnergia(5f, 5f);
                break;
        }
    }

    private void DanoEmArea(Vector3 centro, float raio, int dano)
    {
        Collider2D[] inimigosProximos = Physics2D.OverlapCircleAll(centro, raio);
        foreach (Collider2D col in inimigosProximos)
        {
            Inimigo inimigo = col.GetComponent<Inimigo>();
            if (inimigo != null)
            {
                inimigo.ReceberDano(dano);
            }
        }
    }

    private void CongelamentoTotal(Vector3 centro, float raio)
    {
        Collider2D[] inimigosProximos = Physics2D.OverlapCircleAll(centro, raio);
        foreach (Collider2D col in inimigosProximos)
        {
            Inimigo inimigo = col.GetComponent<Inimigo>();
            if (inimigo != null)
            {
                inimigo.Congelar(5f);
            }
        }
    }

    private void ExplosaoMassiva(Vector3 centro, float raio, int dano)
    {
        Collider2D[] inimigosProximos = Physics2D.OverlapCircleAll(centro, raio);
        foreach (Collider2D col in inimigosProximos)
        {
            Inimigo inimigo = col.GetComponent<Inimigo>();
            if (inimigo != null)
            {
                inimigo.ReceberDano(dano);
                inimigo.Queimar(3f, dano / 3);
            }
        }
    }

    private void CampoEnergia(float duracao, float raio)
    {
        StartCoroutine(CampoEnergiaCoroutine(duracao, raio));
    }

    private IEnumerator CampoEnergiaCoroutine(float duracao, float raio)
    {
        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracao)
        {
            Collider2D[] inimigosProximos = Physics2D.OverlapCircleAll(transform.position, raio);
            foreach (Collider2D col in inimigosProximos)
            {
                Inimigo inimigo = col.GetComponent<Inimigo>();
                if (inimigo != null)
                {
                    inimigo.ReceberDano(1);
                }
            }
            tempoDecorrido += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpgradeEspecial()
    {
        switch (tipo)
        {
            case TorreType.Basic:
                poderEspecial = PoderEspecial.DanoArea;
                break;
            case TorreType.Gelo:
                poderEspecial = PoderEspecial.CongelamentoTotal;
                break;
            case TorreType.Fogo:
                poderEspecial = PoderEspecial.ExplosaoMassiva;
                break;
            case TorreType.Plasma:
                poderEspecial = PoderEspecial.CampoEnergia;
                break;
        }
        celula.AtivarEfeitoEspecial(tipo);
    }

    private string ToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
        if (number < 1) return string.Empty;
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        throw new ArgumentOutOfRangeException("something bad happened");
    }

}