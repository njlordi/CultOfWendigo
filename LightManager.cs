using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour {

    public Light[] houseLight;
    public Material bulbMat;
    public Color originalBulbColor;
    public Color bulbOffColor;
    public Light[] redSpotLight;

	// Use this for initialization
	void Start () {
        originalBulbColor = new Color(225f, 243f, 160f, 111f);
        originalBulbColor *= 0.005f;
        bulbMat.SetColor("_EmissionColor", originalBulbColor);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void activateSpotLight(int indexOfSpotLight, int duration, int delay)
    {
        StartCoroutine(activateSpotLightCR(indexOfSpotLight, duration, delay));
    }

    public void disableHouseLights(int duration)
    {
        StartCoroutine(disableHouseLightsCR(duration));
    }

    public void toggleHouseLights()
    {
        foreach (Light li in houseLight)
        {
            li.enabled = !li.enabled;

            if (li.enabled)
            {
                bulbMat.SetColor("_EmissionColor", originalBulbColor);
            }
            else
            {
                bulbMat.SetColor("_EmissionColor", bulbOffColor);
            }
        }
    }

    // Disables house lights with flicker
    IEnumerator disableHouseLightsCR(int duration)
    {
        toggleHouseLights(); // OFF
        yield return new WaitForSeconds(0.2f);
        toggleHouseLights(); // ON
        yield return new WaitForSeconds(0.3f);
        toggleHouseLights(); // OFF
        yield return new WaitForSeconds(0.15f);
        toggleHouseLights(); // ON
        yield return new WaitForSeconds(0.05f);
        toggleHouseLights(); // OFF
        yield return new WaitForSeconds(0.1f);
        toggleHouseLights(); // ON
        yield return new WaitForSeconds(0.05f);
        toggleHouseLights(); // Finally OFF!

        yield return new WaitForSeconds(duration);

        toggleHouseLights();
    }

    // Activates a red spot light for a set duration
    IEnumerator activateSpotLightCR(int indexOfSpotLight, int duration, int delay)
    {
        yield return new WaitForSeconds(delay);

        redSpotLight[indexOfSpotLight].intensity = 1;

        yield return new WaitForSeconds(duration);

        redSpotLight[indexOfSpotLight].intensity = 0;
    }
}
