using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;

public class SnapImg : MonoBehaviour
{
    public Image m_image;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SnapImgCoroutine());
    }


    IEnumerator SnapImgCoroutine()
    {
        Color myColor = m_image.color;
        myColor.a = 0;
        m_image.color = myColor;

        // add alpha
        while(myColor.a < 0.95f)
        {
            myColor.a += Time.deltaTime;
            m_image.color = myColor;
            yield return null;
        }
        myColor.a = 1;
        m_image.color = myColor;

        yield return new WaitForSeconds(1f);

        // reduce alpha
        while(myColor.a > 0.05f)
        {
            myColor.a -= Time.deltaTime;
            m_image.color = myColor;
            yield return null;
        }
        myColor.a = 0;
        m_image.color = myColor;

        yield return new WaitForSeconds(0.3f);

        // load next scene
        LoadingManager.LoadScene("TitleScene", "SnapScene");
    }
}
