using System.Collections;
using TMPro;
using UnityEngine;

public class TextoPopup : MonoBehaviour
{
    public float fadeDuration = 1f; // Duração do fade
    public bool enableRainbowEffect = false; // Ativar efeito de arco-íris
    public bool enableBlueEffect = false; // Ativar efeito azul
    public bool enableGreenEffect = false; // Ativar efeito verde
    public float rainbowSpeed = 5f; // Velocidade do arco-íris
    public float spiralSpeed = 2f; // Velocidade da espiral
    public float amplitude = 0.5f; // Amplitude da espiral
    public float moveSpeed = 1f; // Velocidade de movimento para cima

    private float tempoVida = 0f;
    private float fadeTimer;
    private TextMeshPro textMeshPro;
    private string originalText;
    private float[] charOffsets;

    void Start()
    {
        fadeTimer = fadeDuration;
        textMeshPro = GetComponent<TextMeshPro>();
        originalText = textMeshPro.text;

        if (enableRainbowEffect || enableBlueEffect || enableGreenEffect)
        {
            charOffsets = new float[originalText.Length];
        }
    }

    void Update()
    {
        tempoVida += Time.deltaTime;

        // Movimento para cima
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // Fade
        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;

            textMeshPro.ForceMeshUpdate();
            TMP_TextInfo textInfo = textMeshPro.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int vertexIndex = charInfo.vertexIndex;
                Color32[] colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;

                // Ajusta apenas o alfa para o fade
                float fadeAlpha = Mathf.Clamp01(fadeTimer / fadeDuration) * 255f;
                for (int j = 0; j < 4; j++)
                {
                    colors[vertexIndex + j].a = (byte)fadeAlpha;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.colors32 = meshInfo.colors32;
                textMeshPro.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        // Efeito arco-íris
        if (enableRainbowEffect)
        {
            ApplyRainbowEffect();
        }

        // Efeito azul
        if (enableBlueEffect)
        {
            ApplyColorEffect(0.5f, 1f, 1f); // Tons de azul
        }

        // Efeito verde
        if (enableGreenEffect)
        {
            ApplyColorEffect(0.33f, 0.7f, 1f); // Tons de verde
        }

        // Destruir após o fade
        if (fadeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyRainbowEffect()
    {
        textMeshPro.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Movimento em espiral
            float offset = Mathf.Sin(Time.time * spiralSpeed + i) * amplitude;
            charOffsets[i] = offset;
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] += new Vector3(0, offset, 0);
            }

            // Cor arco-íris
            Color32[] colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
            Color rainbowColor = Color.HSVToRGB((Time.time * rainbowSpeed + i * 0.1f) % 1f, 1f, 1f);

            // Mantém o alfa atual
            float currentAlpha = colors[vertexIndex].a / 255f;
            rainbowColor.a = currentAlpha;

            for (int j = 0; j < 4; j++)
            {
                colors[vertexIndex + j] = rainbowColor;
            }
        }

        UpdateMeshColors(textInfo);
    }

    private void ApplyColorEffect(float hue, float saturation, float brightness)
    {
        textMeshPro.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Movimento em espiral
            float offset = Mathf.Sin(Time.time * spiralSpeed + i) * amplitude;
            charOffsets[i] = offset;
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] += new Vector3(0, offset, 0);
            }

            // Cores em tons específicos
            Color32[] colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
            Color color = Color.HSVToRGB((Time.time * rainbowSpeed + i * 0.1f) % 0.1f + hue, saturation, brightness);

            // Mantém o alfa atual
            float currentAlpha = colors[vertexIndex].a / 255f;
            color.a = currentAlpha;

            for (int j = 0; j < 4; j++)
            {
                colors[vertexIndex + j] = color;
            }
        }

        UpdateMeshColors(textInfo);
    }

    private void UpdateMeshColors(TMP_TextInfo textInfo)
    {
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            meshInfo.mesh.colors32 = meshInfo.colors32;
            textMeshPro.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    public void Configure(bool rainbowEffect, bool blueEffect, bool greenEffect, float fadeTime, float moveSpeed)
    {
        enableRainbowEffect = rainbowEffect;
        enableBlueEffect = blueEffect;
        enableGreenEffect = greenEffect;
        fadeDuration = fadeTime;
        this.moveSpeed = moveSpeed;
    }
}
