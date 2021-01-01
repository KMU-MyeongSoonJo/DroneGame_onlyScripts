using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleBgmController : MonoBehaviour
{
    public AudioSource audioSource;
    
    public AudioClip intro;
    public AudioClip main;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 타이틀 인트로 bgm on
    /// </summary>
    public void IntroBgmOn()
    {
        print($"Intro Bgm On");

        audioSource.clip = intro;
        audioSource.Play();
    }

    /// <summary>
    /// 타이틀 메인 bgm on
    /// </summary>
    public void MainBgmOn()
    {
        print($"Main Bgm On");

        audioSource.clip = main;
        audioSource.Play();
    }
}
