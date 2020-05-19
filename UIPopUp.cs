using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUp : MonoBehaviour
{

    RectTransform rt;
    public float OriginalY;
    public float popUpSpeed;
    public float distanceToMove;
    public bool autoHide;
    public float displayTime; // time to display before auto hiding (if enabled)

    // Use this for initialization
    void Start()
    {
        rt = this.GetComponent<RectTransform>();
        OriginalY = rt.anchoredPosition.y;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PopUp()
    {
        StartCoroutine(PopUpCR());
    }

    public void PopBackDown()
    {
        StartCoroutine(PopBackDownCR());
    }

    public void PopDown()
    {
        StartCoroutine(PopDownCR());
    }

    public void PopBackUp()
    {
        StartCoroutine(PopBackUpCR());
    }

    IEnumerator PopUpCR()
    {
        float newY = rt.anchoredPosition.y;
        float target = newY + distanceToMove;

        while (newY < target)
        {
            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);
            newY += popUpSpeed * Time.deltaTime;
            yield return null;
        }

        if (!autoHide) { yield break; }

        yield return new WaitForSeconds(displayTime);

        PopBackDown();
    }

    IEnumerator PopBackDownCR()
    {
        float newY = rt.anchoredPosition.y;
        float target = OriginalY;

        while (newY > target)
        {
            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);
            newY -= popUpSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator PopDownCR()
    {
        float newY = rt.anchoredPosition.y;
        float target = newY - distanceToMove;

        while (newY > target)
        {
            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);
            newY -= popUpSpeed * Time.deltaTime;
            yield return null;
        }

        if (!autoHide) { yield break; }

        yield return new WaitForSeconds(displayTime);

        PopBackUp();
    }

    IEnumerator PopBackUpCR()
    {
        float newY = rt.anchoredPosition.y;
        float target = OriginalY;

        while (newY < target)
        {
            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);
            newY += popUpSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
