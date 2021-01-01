using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestPointer : MonoBehaviour
{
    public enum matColor { GREEN, YELLOW }

    public Material[] materials;
    public MeshRenderer[] pointers;


    // Start is called before the first frame update
    void Start()
    {
        pointers[0].material = materials[(int)matColor.GREEN];
        pointers[1].material = materials[(int)matColor.GREEN];
    }

    public void SetPointerColor(matColor color)
    {
        pointers[0].material = materials[(int)color];
        pointers[1].material = materials[(int)color];
    }

    
}
