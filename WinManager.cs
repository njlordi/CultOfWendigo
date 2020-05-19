using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinManager : MonoBehaviour {

    public GameObject mainCam;
    public GameObject spellCam;
    public GameObject policeCam;
    public GameObject escapeCam;

    // Cinematic objects
    public GameObject car;
    public CopWalk copWalk;
    public Cultist cultist;
    public GameObject wendigo;
    public Material wendigoDissolveMat;
    public GameObject portal;
    public GameObject portalEffect; // Floating portal effect
    public Animator portalSpotLight;
    public GameObject[] cultists;
    public FrontDoor frontDoorCopWin;
    public GameObject WomanCine;
    public StressReceiver spellShakeyCamera;

    // Object's data
    public float portalY;
    public AudioSource portalSFX;
    public AudioSource portalSFXEnd;
    private bool portalClosed = false;
    public float durationOfShake = 0.6f;

    // UI
    public GameObject winText;
    public GameObject winText2;

    // Use this for initialization
    void Start () {
        // Assign Y for portal
        portalY = portal.transform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ActivateWinScene(string type)
    {
        switch(type)
        {
            case "Spell":
                StartCoroutine(DoSpellWinCR());
                break;

            case "Cop":
                StartCoroutine(DoPoliceWinCR());
                break;

            case "Car":
                StartCoroutine(DoEscapeWinCR());
                break;

            default:
                break;
        }
        
    }

    private void DisableGameUI()
    {
        foreach (GameObject go in GameManager.Instance.taskBarHolder)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in GameManager.Instance.healthBar)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in GameManager.Instance.nameLabel)
        {
            go.SetActive(false);
        }

        // Disable Barricade level up bar
        foreach(GameObject go in GameManager.Instance.AllProgressBars)
        {
            go.SetActive(false);
        }

        // All other status UI text objects
        foreach (GameObject go in GameManager.Instance.AllStatusText)
        {
            go.SetActive(false);
        }
    }

    IEnumerator ShakeyCameraCR()
    {
        while (durationOfShake > 0)
        {
            durationOfShake -= Time.deltaTime * 2;
            spellShakeyCamera.InduceStress(0.5f);
            yield return new WaitForSeconds(0.2f);
            yield return null;
        }
    }

    #region SpellWin
    // Handles win by banish spell
    IEnumerator DoSpellWinCR()
    {
        // Initialize invisibility level to 0 (Visible)
        float invisibility = 0;
        // Ensure that Wendigo is visible for cinematic
        wendigoDissolveMat.SetFloat("_DissolveIntensity", invisibility);

        yield return new WaitForSeconds(3);

        // Disable Game UI
        DisableGameUI();

        // Set Camera
        mainCam.SetActive(false);
        spellCam.SetActive(true);

        // Start fade-in of light
        portalSpotLight.SetBool("isOn", true);
        // Start portal sound effect
        portalSFX.PlayDelayed(1.0f);

        // Raise the portal
        while (portalY < -267.8f)
        {
            portalY += Time.deltaTime * 0.2f;
            portal.transform.localPosition = new Vector3(portal.transform.localPosition.x, 
                portalY, portal.transform.localPosition.z);

            yield return null;
        }

        // Trigger shakey camera for portal opening effect
        StartCoroutine(ShakeyCameraCR());

        // Play Wendigo shout animation
        wendigo.GetComponent<Animator>().SetTrigger("shout");
        // Play shout SFX
        wendigo.GetComponent<AudioSource>().PlayDelayed(0.5f);

        // Dissolve the Wendigo texture
        while (invisibility < 1)
        {
            invisibility += 0.2f * Time.deltaTime;
            wendigoDissolveMat.SetFloat("_DissolveIntensity", invisibility);
            // Activate the closing portal visual effect
            if (!portalClosed && invisibility > 0.70f)
            {
                portalClosed = true;
                portalEffect.SetActive(true); // Animate the closing portal effect
                portalSFXEnd.Play(); // Play the second portal sound effect (Ending wind noise)
            }
            yield return null;
        }

        // Cultists look around confused
        cultists[0].GetComponent<Animator>().SetTrigger("lookAround");
        yield return new WaitForSeconds(1);
        cultists[2].GetComponent<Animator>().SetTrigger("lookAround");

        // Start fade-out of light
        portalSpotLight.SetBool("isOn", false);
        // Reassign Y for portal (for lowering!)
        portalY = portal.transform.localPosition.y;
        // Lower the portal
        while (portalY > -268.46f)
        {
            portalY -= Time.deltaTime * 0.4f;
            portal.transform.localPosition = new Vector3(portal.transform.localPosition.x,
                portalY, portal.transform.localPosition.z);

            yield return null;
        }

        //yield return new WaitForSeconds(4); no delay?

        winText.GetComponent<Text>().text = "After reading the words in the book of the dead, a portal opens in the near by woods; banishing the Wendigo to hell. ";
        winText.SetActive(true);

        yield return new WaitForSeconds(4);

        // Cultist looks around confused
        cultists[1].GetComponent<Animator>().SetTrigger("lookAround");

        winText2.GetComponent<Text>().text = "You have survived";
        winText2.SetActive(true);

        GameManager.Instance.HandleAfterWin(); // Tell gamemanager to handle after game stuff
    }
    #endregion

    #region PoliceWin
    /// <summary>
    /// Handles win by calling police
    /// </summary>
    IEnumerator DoPoliceWinCR()
    {
        yield return new WaitForSeconds(3);

        // Disable Game UI
        DisableGameUI();

        // Set Camera
        mainCam.SetActive(false);
        policeCam.SetActive(true);

        copWalk.ApproachHouseAndDrawGun();

        yield return new WaitForSeconds(2.5f);

        cultist.DoAction();

        yield return new WaitForSeconds(4);

        WomanCine.GetComponent<CopWalk>().WalkForwardThenStop(2.5f);
        frontDoorCopWin.OpenDoor();

        yield return new WaitForSeconds(1);

        copWalk.HolsterGun();

        yield return new WaitForSeconds(2);

        winText.GetComponent<Text>().text = "After calling 911, the police finally arrived to the house and escorted the survivors to a safe location. ";
        winText.SetActive(true);

        yield return new WaitForSeconds(4);

        winText2.GetComponent<Text>().text = "You have survived";
        winText2.SetActive(true);

        GameManager.Instance.HandleAfterWin(); // Tell gamemanager to handle after game stuff
    }
    #endregion

    #region CarRepairWin
    // Handles win by car repair
    IEnumerator DoEscapeWinCR()
    {
        yield return new WaitForSeconds(3);

        // Disable Game UI
        DisableGameUI();

        // Set Camera
        mainCam.SetActive(false);
        escapeCam.SetActive(true);
        car.SetActive(true);

        yield return new WaitForSeconds(10);

        winText.GetComponent<Text>().text = "After successfully repairing the vehicle, the survivors hop in quickly and make a daring escape.";
        winText.SetActive(true);

        yield return new WaitForSeconds(4);

        winText2.GetComponent<Text>().text = "You have survived";
        winText2.SetActive(true);

        GameManager.Instance.HandleAfterWin(); // Tell gamemanager to handle after game stuff
    }
    #endregion
}
