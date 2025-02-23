using TMPro;
using UnityEngine;

public class CampoMultiplicacao : MonoBehaviour
{
    public float multiplicationFactor = 2f;
    public Color fieldColor = new Color(0.5f, 0.5f, 1f, 0.5f);

    private SpriteRenderer spriteRenderer;
    private TextMeshPro factorText;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.color = fieldColor;
        spriteRenderer.sprite = CreateForceFieldSprite();

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        CreateFactorText();
        UpdateFactorText();
    }

    private void CreateFactorText()
    {
        GameObject textObj = new GameObject("FactorText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero;

        factorText = textObj.AddComponent<TextMeshPro>();
        factorText.alignment = TextAlignmentOptions.Center;
        factorText.fontSize = 14;
        factorText.color = Color.cyan;
    }

    private void UpdateFactorText()
    {
        if (factorText != null)
        {
            factorText.text = "x" + multiplicationFactor.ToString("F1");
        }
    }

    private Sprite CreateForceFieldSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                float alpha = Mathf.Clamp01(1 - distance / 16f);
                colors[y * 32 + x] = new Color(fieldColor.r, fieldColor.g, fieldColor.b, alpha * fieldColor.a);
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    public void MultiplyProjectile(Projetil projectile)
    {
        projectile.Multiply(multiplicationFactor);
    }
}

