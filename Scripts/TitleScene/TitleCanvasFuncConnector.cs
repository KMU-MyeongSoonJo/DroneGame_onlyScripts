using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCanvasFuncConnector : MonoBehaviour
{
    public TitleSceneController tsc;
    public TitleBgmController tbc;

    private void Start()
    {
        
    }

    public void IntroBgmOn()
    {
        tbc.IntroBgmOn();
    }

    public void OnClick()
    {
        tbc.MainBgmOn();
        tsc.StartToSelectSpawnPos();
    }
}
