using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherited from Door, allows manual opening and closing
public class FrontDoor : Door
{
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        // overridden to disable FPS E-key-to-open interaction

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            OpenDoor();
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            CloseDoor();
        }
    }

    public void OpenDoor()
    {
        if (doorStatus) return;

        StartCoroutine(this.moveDoor(doorOpen));

        if (doorOpenSound != null)
            AudioSource.PlayClipAtPoint(doorOpenSound, this.transform.position);
    }

    public void CloseDoor()
    {
        if (!doorStatus) return;

        StartCoroutine(this.moveDoor(doorClosed));

        if (doorCloseSound != null)
            AudioSource.PlayClipAtPoint(doorCloseSound, this.transform.position);
    }
}
