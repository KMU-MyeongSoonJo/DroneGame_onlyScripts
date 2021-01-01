using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// # Need SoundEffect.cs #
[RequireComponent(typeof(Button))]

public class HighlightSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public AudioClip highlightSound;
    public AudioClip clickSound;

    public void OnPointerEnter(PointerEventData ped)
    {
        SoundEffect.instance.audioSource.PlayOneShot(highlightSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SoundEffect.instance.audioSource.PlayOneShot(highlightSound);
    }

    public void PlayClickSound()
    {
        SoundEffect.instance.audioSource.PlayOneShot(clickSound);
    }

    public void PlayHighlightSound()
    {
        SoundEffect.instance.audioSource.PlayOneShot(highlightSound);
    }
}
