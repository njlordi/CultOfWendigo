using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wendigo : MonoBehaviour
{
    // Animation and sound references
    private Animator m_Animator;
    private AudioSource m_audioSource;
    public AudioClip[] footSteps;
    public AudioSource voice;
    public AudioSource voiceHurt;
    public AudioSource combat;

    public bool _walking = false;
    public float walkSpeed = 1.8f; // 1.8 seems to work well with Wendigo walk animations
    public float turnSpeed;
    public float attackReactionDelay = 0.7f;

    public FrontDoor frontDoor;
    public Barricade barricade;

    public GameObject[] player;

    public GameObject location1; // outside of room
    public GameObject location2; // inside, infront of door
    public GameObject location3; // offset for exiting when close to door (on left side)

    private bool footStep = false;

    // Use this for initialization
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_audioSource = GetComponent<AudioSource>();

        barricade = frontDoor.GetComponentInParent<Barricade>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_walking)
        {
            m_Animator.SetBool("isWalking", true);
            if(!m_audioSource.isPlaying)
            {
                if (footStep)
                    m_audioSource.clip = footSteps[0]; // Play foot sound
                else
                    m_audioSource.clip = footSteps[1]; // Play "other" foot sound
                m_audioSource.Play();

                footStep = !footStep;
            }
        }
        else
        {
            m_Animator.SetBool("isWalking", false);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(AttackSingleTargetCR(1, "left", 0.3f));
        }
    }


    /******************PUBLIC FUNCTIONS**************************************/

    public void AttackSingleTarget(int indexOfPlayer, float stoppingDistance)
    {
        // Attack with right hand unless it's the first player (for visual reasons)
        string action = "right";

        if (indexOfPlayer == 0)
            action = "left";

        StartCoroutine(AttackSingleTargetCR(indexOfPlayer, action, stoppingDistance));
    }

    /// <summary>
    /// Cancels all coroutines and starts a coroutine for taking damage
    /// E.G. Dynamite explosive damage
    /// </summary>
    public void GetHurt()
    {
        StopAllCoroutines();
        _walking = false;
        StartCoroutine(GetHurtCR());
    }

    /******************PRIVATE FUNCTIONS*************************************/
    private void MoveTowardsTarget(GameObject target)
    {
        _walking = true;

        /************ Gradually face target *************/
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        Quaternion toRotation =
            Quaternion.LookRotation(direction);
        transform.rotation =
            Quaternion.Slerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
        /************************************************/

        // Move forward
        transform.position = Vector3.MoveTowards(transform.position,
            target.transform.position,
            walkSpeed * Time.deltaTime);
    }

    private void Shout()
    {
        m_Animator.SetTrigger("shout");
        voice.PlayDelayed(0.7f);
    }

    // Handles the animation for attacking
    private void AttackAnimation(int playerIndex, string action)
    {
        switch (action)
        {

            case "left":
                m_Animator.SetTrigger("leftHandAttack");
                break;

            case "right":
                m_Animator.SetTrigger("rightHandAttack");
                break;

            default:
                Debug.Log("Error invalid attack command called.");
                break;
        }
    }

    IEnumerator GetHurtCR()
    {
        m_Animator.SetTrigger("getDamaged");
        voiceHurt.PlayDelayed(0.5f);

        yield return new WaitForSeconds(2f);
        StartCoroutine(ExitThroughDoor(0.5f, false));
        yield break;
    }

    // Chained coroutines for entering + attacking specific player, then leaving
    IEnumerator AttackSingleTargetCR(int indexOfPlayer, string action, float stoppingDistance)
    {
        yield return StartCoroutine(EnterThroughDoor(stoppingDistance));
        yield return new WaitForSeconds(3.0f);
        yield return StartCoroutine(ApproachPlayer(indexOfPlayer, action, stoppingDistance));
        yield return new WaitForSeconds(2.0f);

        if (indexOfPlayer == 1)
        {
            yield return StartCoroutine(ExitThroughDoor(stoppingDistance, true));
        }
        else
        {
            yield return StartCoroutine(ExitThroughDoor(stoppingDistance, false));
        }
    }

    // Coroutine for approaching a SINGLE target
    IEnumerator ApproachTarget(GameObject target, string action, float stoppingDistance)
    {

        while (Vector3.Distance(transform.position, target.transform.position) > stoppingDistance)
        {
            _walking = true;
            MoveTowardsTarget(target);
            yield return null;
        }

        _walking = false;
    }

    // Coroutine for approaching a PLAYER only (With option to attack!)
    IEnumerator ApproachPlayer(int indexOfPlayer, string action, float stoppingDistance)
    {

        while (Vector3.Distance(transform.position, player[indexOfPlayer].transform.position) > stoppingDistance)
        {
            _walking = true;
            MoveTowardsTarget(player[indexOfPlayer]);
            yield return null;
        }

        _walking = false;

        if (action == "left" || action == "right")
        {
            AttackAnimation(indexOfPlayer, action);

            // Play SFX for attack (and impact)
            combat.PlayDelayed(0.7f);

            yield return new WaitForSeconds(attackReactionDelay);

            GameManager.Instance.DamagePlayer(indexOfPlayer, "wendigo");
        }

        yield break;
    }

    // Opens door, Wendigo moves to center of room and shouts
    IEnumerator EnterThroughDoor(float stoppingDistance)
    {
        barricade.BreakBarricade();
        frontDoor.OpenDoor();

        // make sure monster is facing the correct way
        this.transform.LookAt(location2.transform.position);

        while (Vector3.Distance(transform.position, location2.transform.position) > stoppingDistance)
        {
            _walking = true;
            MoveTowardsTarget(location2);
            yield return null;
        }

        _walking = false;
        Shout();

        yield break;
    }

    // Moves towards two track objects, exits, then door closes
    IEnumerator ExitThroughDoor(float stoppingDistance, bool ExitDirectly)
    {
        GameObject[] target = new GameObject[] { location2, location1, location3 };

        // Exit-room navigation is different for attacking some players
        if (ExitDirectly)
        {
            while (Vector3.Distance(transform.position, target[2].transform.position) > stoppingDistance)
            {
                _walking = true;
                MoveTowardsTarget(target[2]);
                yield return null;
            }

            while (Vector3.Distance(transform.position, target[1].transform.position) > stoppingDistance)
            {
                _walking = true;
                MoveTowardsTarget(target[1]);
                yield return null;
            }
        }
        else
        {
            while (Vector3.Distance(transform.position, target[0].transform.position) > stoppingDistance)
            {
                _walking = true;
                MoveTowardsTarget(target[0]);
                yield return null;
            }

            while (Vector3.Distance(transform.position, target[1].transform.position) > stoppingDistance)
            {
                _walking = true;
                MoveTowardsTarget(target[1]);
                yield return null;
            }
        }

        _walking = false;
        frontDoor.CloseDoor();

        yield break;
    }
}
