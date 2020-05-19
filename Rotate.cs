using UnityEngine;
using System.Collections;

// item rotates and optionally bobs up and down
public class Rotate : MonoBehaviour {

    public Vector3 rotationVector;
    public bool isBobbing;
    public float speed = 5;
    public float bobDistance = 5; // 5 should be good

    public float startingY;
    public float newY;

	// Use this for initialization
	void Start () {
        startingY = this.transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(rotationVector * Time.deltaTime);

        if (isBobbing)
        {
            newY = (Mathf.Sin(Time.time * speed) / bobDistance);

            transform.position = new Vector3(transform.position.x,
                startingY + newY, transform.position.z);
        }
	}
}
