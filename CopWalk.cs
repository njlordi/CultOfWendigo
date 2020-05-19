using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopWalk : MonoBehaviour {

    public float newZ;

	// Use this for initialization
	void Start () {
        newZ = this.transform.position.z;
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void WalkForwardThenStop(float distanceToWalk)
    {
        StartCoroutine(WalkForwardThenStopCR(distanceToWalk));
    }

    public void ApproachHouseAndDrawGun()
    {
        StartCoroutine(ApproachHouseAndDrawGunCR());
    }

    public void HolsterGun()
    {
        Animation anim = this.GetComponent<Animation>();
        anim["extractgun"].speed = -1;
        anim["extractgun"].time = anim ["extractgun"].length;
        anim.Play("extractgun");
    }

    IEnumerator WalkForwardThenStopCR(float distance)
    {
        float destination = newZ - distance;

        while (newZ > destination)
        {
            newZ -= Time.deltaTime * 1f;
            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
            yield return null;
        }

        this.GetComponent<Animation>().Play("idle");
    }

    IEnumerator ApproachHouseAndDrawGunCR()
    {
        while (newZ < -58.2f)
        {
            newZ += Time.deltaTime * 1f;
            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
            yield return null;
        }

        this.GetComponent<Animation>().Play("extractgun");
    }
}
