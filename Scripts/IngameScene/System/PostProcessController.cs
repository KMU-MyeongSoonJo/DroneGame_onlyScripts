using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PostProcessType {
    playerViewEff, // 플레이어 시점 플레이어 조작
    playerDroneViewEff, // 플레이어 시점 드론 조작
    droneTppEff, // 드론 3인칭 시점
    droneFppEff, // 드론 1인칭 시점
}


public class PostProcessController : MonoBehaviour
{
    static public PostProcessController instance;

    [Header("Drone PostProcess Effs")]
    public Transform playerViewEff;
    public Transform playerDroneViewEff;
    public Transform droneTpvEff;
    public Transform droneFpvEff;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        DisableAllPostProcessEff();
    }

    public void EnablePostProcessEff(PostProcessType pp)
    {
        DisableAllPostProcessEff();

        switch (pp)
        {
            case PostProcessType.playerViewEff:
                playerViewEff.gameObject.SetActive(true);
                break;
            case PostProcessType.playerDroneViewEff:
                playerDroneViewEff.gameObject.SetActive(true);
                break;
            case PostProcessType.droneTppEff:
                droneTpvEff.gameObject.SetActive(true);
                break;
            case PostProcessType.droneFppEff:
                droneFpvEff.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void DisableAllPostProcessEff()
    {
        playerViewEff.gameObject.SetActive(false);
        playerDroneViewEff.gameObject.SetActive(false);
        droneTpvEff.gameObject.SetActive(false);
        droneFpvEff.gameObject.SetActive(false);
    }
}
