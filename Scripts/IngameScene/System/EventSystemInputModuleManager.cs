using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class EventSystemInputModuleManager : MonoBehaviour
{
    static public EventSystemInputModuleManager instance;

    public StandaloneInputModule module;

    private void Start()
    {
        instance = this;
    }


}
