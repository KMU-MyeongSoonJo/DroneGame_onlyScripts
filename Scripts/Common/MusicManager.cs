using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*  음악 재생에 관여하는 스크립트
* 
************************************/

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public List<LocalMusic> playableLocalMusics; // 재생가능한 지역 음악 목록 ( 이 목록 중 우선순위가 가장 높은 지역 음악이 재생됩니다. )
    public AudioSource bgm;
    public AudioSource bgmFadeOut;

    private bool day; // 낮 밤 구분

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (LightManager.instance.timer < 2400)
            day = true;
        else if (LightManager.instance.timer >= 2400)
            day = false;
    }

    private void Update()
    {
        if (day && LightManager.instance.timer >= 2400) // 낮 -> 밤
        {
            UpdateLocalMusic(); // 음악 변경
            day = false; // 밤
        }

        else if (!day && LightManager.instance.timer < 2400) // 밤 -> 낮
        {
            UpdateLocalMusic();
            day = true; // 낮
        }
    }

    public void UpdateLocalMusic() // 지역 음악 업데이트
    {
        AudioClip _music = null;
        int _priority = 0;

        foreach (LocalMusic localMusic in playableLocalMusics) // 지역 음악 목록에서 가장 우선 순위가 높은 음악 찾기
        {
            if (localMusic.priority > _priority)
            {
                if (LightManager.instance.timer < 2400) // 낮
                    _music = localMusic.dayMusic;
                else if (LightManager.instance.timer >= 2400) // 밤
                    _music = localMusic.nightMusic;
            }
        }

        if (_music != null && _music != bgm.clip) // 최종적으로 선택된 지역 음악이 존재하고 이미 재생중인 BGM과 다르다면
            UpdateBGM(_music); // 업데이트된 지역 음악으로 BGM 변경
    }

    private void UpdateBGM(AudioClip _music) // BGM 변경
    {
        // 기존 BGM을 페이드아웃 BGM으로 이동
        bgmFadeOut.clip = bgm.clip;
        bgmFadeOut.volume = bgm.volume;
        bgmFadeOut.Play();
        bgmFadeOut.time = bgm.time;

        // BGM 새로 재생
        bgm.clip = _music;
        bgm.volume = 0;
        bgm.Play();

        FadeInBGM(); // BGM 페이드 인
    }

    private void FadeInBGM() // BGM 페이드 인
    {
        bgm.volume += Time.deltaTime;
        bgmFadeOut.volume -= Time.deltaTime;

        if (bgm.volume < 1.0f) // 메인 BGM의 볼륨이 충분이 커지지 않았다면,
            Invoke("FadeInBGM", Time.deltaTime); // 다음 프레임에 볼륨 증가
    }
}
