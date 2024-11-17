using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextoPopup : MonoBehaviour
{

    private float tempoVida = 0;
    private float fadeTimer;

    void Start()
    {
        fadeTimer = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        tempoVida += Time.deltaTime;
        transform.Translate(0, 3 * Time.deltaTime, 0);

        if (tempoVida > 1)
        {
            Destroy(transform.gameObject);
        }

        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;

            Color color = transform.GetComponent<TextMeshPro>().color;
            color.a = Mathf.Clamp01(fadeTimer / 1);
            transform.GetComponent<TextMeshPro>().color = color;
        }
    }
}
