using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RisingText : MonoBehaviour {

    // Local Variables
    public bool hasFadeEffect;
    public bool autoDestroy;
    public float fadeSpeed = 0.5f;
    public float textScrollSpeed = 20f;
    public float distanceToRise = 50f;

    private RectTransform rt;
    private float newY;
    private float opacityLevel = 1f;
    private float destinationY;
    private Text _text;

    // Use this for initialization
    void Awake () {
        _text = this.GetComponent<Text>();
        rt = _text.GetComponent<RectTransform>();

        newY = rt.anchoredPosition.y;
        destinationY = newY + distanceToRise;
    }
	
	// Update is called once per frame
	void Update () {

            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);

            newY += textScrollSpeed * Time.deltaTime;

        if (hasFadeEffect)
        {
            opacityLevel -= Time.deltaTime * fadeSpeed;

            _text.color = new Color(_text.color.r,
                _text.color.g,
                _text.color.b,
                opacityLevel);
        }

        if (autoDestroy && newY >= destinationY)
        {
            Destroy(this.gameObject);
        }
    }
}
