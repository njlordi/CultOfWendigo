using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusWendigo : MonoBehaviour {

    public Transform wendigo;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.LookAt(wendigo);
	}
}
