using System.Collections;
using UnityEngine;

public class TextureFlash : MonoBehaviour
{
    public int flashCount;
    public Material boardMat;

    void Start()
    {
    }

    void OnEnable()
    {
        boardMat.SetColor("_EmissionColor", Color.black);
        StartCoroutine(FlashTexCR());   
    }

    IEnumerator FlashTexCR()
    {
        int count = flashCount;

        while (count > 0)
        {
            count--;
            boardMat.SetColor("_EmissionColor", Color.yellow);
            yield return new WaitForSeconds(0.03f);
            boardMat.SetColor("_EmissionColor", Color.black);
            yield return new WaitForSeconds(0.03f);
        }

        boardMat.SetColor("_EmissionColor", Color.black);
    }
}