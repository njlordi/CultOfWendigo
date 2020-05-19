using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public float movementSpeed = 5;
    public float turnSpeed = 4;
    public float stoppingDistance = 0.25f;
    public float retreatAfterAttackDelay = 1.5f;
    public float attackReactionDelay = 0.5f; // delay between attack and player getting damaged
    public GameObject[] crowAttackTarget;
    public GameObject retreatTarget;
    public bool creatureDoesDamage = false; // Used for flocks/groups enable this for at least one creature!
    public Material mat;
    public float disappearSpeed = 0.5f;

    // SFX
    public AudioSource cawSound;
    public AudioSource flySound;
    public AudioSource scratchSound;

    private float invisibility;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Animator _animator;

    void OnEnable()
    {
        // remember original position and rotation
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;

        invisibility = 1;

        _animator = GetComponent<Animator>();

        mat.SetFloat("_DissolveIntensity", invisibility);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("_DissolveIntensity", invisibility);

        // manually clamp invisibility level
        if (invisibility < 0) { invisibility = 0; }
        if (invisibility > 1) { invisibility = 1; }
    }

    private void OnDisable()
    {
        this.transform.position = originalPosition;
        this.transform.rotation = originalRotation;
    }

    public void Attack(int playerIndex)
    {
        //StopAllCoroutines();
        StartCoroutine(AppearAttackTargetThenLeave(playerIndex));
    }

    IEnumerator AppearAttackTargetThenLeave(int playerIndex)
    {
        // NOTE: Crow will advance on target only after becoming completely visible!
        while (invisibility > 0)
        {
            invisibility -= disappearSpeed * Time.deltaTime;
            yield return null;
        }

        _animator.SetTrigger("glide");

        if (creatureDoesDamage) // This is checked so that both crows play the same sound effect
        {
            cawSound.PlayDelayed(0.1f); // Caw sound effect
            flySound.PlayDelayed(0.2f); // Flying flying sound effect
        }

        while (Vector3.Distance(transform.position, crowAttackTarget[playerIndex].transform.position) > stoppingDistance)
        {
            MoveTowardsTarget(crowAttackTarget[playerIndex].transform.position);
            yield return null;
        }

        // ATTACK animation
        _animator.SetTrigger("attack");
        if (creatureDoesDamage) // This is checked so that both crows play the same sound effect
            scratchSound.PlayDelayed(0.3f); // Play attack SFX

        yield return new WaitForSeconds(attackReactionDelay);

        // Tell GameManager that the player got hurt
        if (creatureDoesDamage)
        {
            GameManager.Instance.DamagePlayer(playerIndex, "creature");
        }

        yield return new WaitForSeconds(retreatAfterAttackDelay);

        while (Vector3.Distance(transform.position, retreatTarget.transform.position) > stoppingDistance)
        {
            MoveTowardsTarget(retreatTarget.transform.position);
            yield return null;
        }

        _animator.SetTrigger("resetToIdle");

        this.gameObject.SetActive(false);
    }

    private void MoveTowardsTarget(Vector3 target)
    {

        /************ Gradually face target *************/
        Vector3 direction = target - transform.position;
        direction.y = 0;
        Quaternion toRotation =
            Quaternion.LookRotation(direction);
        transform.rotation =
            Quaternion.Slerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
        /************************************************/

        /************ Move towards target ***************/ // Replace with simple move relative forward?
        transform.position = Vector3.MoveTowards(transform.position,
            target,
            movementSpeed * Time.deltaTime);
        /************************************************/
    }
}
