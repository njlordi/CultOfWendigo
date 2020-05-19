using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

/* Grabs the JSON file from a specified twitch channel
/* Converts the JSON to a string of raw code */
public class ParseJSON : MonoBehaviour {

    public string channelName;
    public Text jsonConsole;

    private string jsonString = "";
    private WWW www;

	// Use this for initialization
	void Start () {
        StartCoroutine(ReadFromJSON());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator ReadFromJSON()
    {
        string url = "tmi.twitch.tv/group/user/" + channelName + "/chatters";
        www = new WWW(url);

        yield return new WaitForSeconds(3);

        jsonString = www.text;

        if (jsonConsole != null)
        {
            jsonConsole.text = jsonString;
        }
    }
}
