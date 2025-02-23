using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControlaSonsUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GameObject.Find("Canvas").GetComponent<AudioSource>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
