using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DigitalRuby.PyroParticles;

public class Cultist : MonoBehaviour
{
    // All cultists
    public float invisibility;
    public float disappearSpeed = 0.5f;
    public float disappearDelay;
    public bool performingAction = false;

    // Audio
    public AudioSource voice;
    public AudioSource combat;
    // voice clips
    public AudioClip[] line;

    // next two for melee cultists only
    public float attackDamageDelay = 0.6f;
    public int assignedPlayerIndex;

    // Variables for Summoner cultist only
    public GameObject wallOfFire;
    public Transform wallOfFireLocation;
    public Transform wallOfFireSingleTargetLocation;
    public bool attackSingleTarget;

    // Variables for crow cultists only
    private bool crowsAreDispatched;
    public float summonCrowAttackDelay = 2.0f;
    public float crowAttackCooldownTime;
    public Creature[] crow;

    public Material mat; // Link to material in project window for now!

    public enum Status { attack, summon, floatSummon, appear };
    public Status cultistStatus;

    private Animator _animator;

    // Use this for initialization
    void Start()
    {
        // start invisible
        invisibility = 1;

        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("_DissolveIntensity", invisibility);

        // manually clamp invisibility level
        if (invisibility < 0)
            invisibility = 0;
        if (invisibility > 1)
            invisibility = 1;

        // Determines if cultist is physicaly present
        if (invisibility < 1f)
        {
            this.performingAction = true;
        }
        else
        {
            this.performingAction = false;
        }
    }

    public void DoAction()
    {
        StartCoroutine(AppearAndDoAnimation());
    }

    public void GetHurt()
    {
        StopAllCoroutines();
        StartCoroutine(GetHurtCR());
    }

    IEnumerator GetHurtCR()
    {
        _animator.SetTrigger("getHurt"); // Really just an Idle animation!
        while (invisibility < 1)
        {
            invisibility += disappearSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator AppearAndDoAnimation()
    {
        GameObject wof = new GameObject(); // Create new gameobject for wall of fire

        // Play spooky voice sfx (Randomized)
        int lineToPlay = Random.Range(0, 10);
        if (lineToPlay < 6 && cultistStatus != Status.summon) // Summoner cultists dont say random lines
        {
            voice.clip = line[lineToPlay];
            voice.PlayDelayed(0.5f);
        }

        while (invisibility > 0)
        {
            invisibility -= disappearSpeed * Time.deltaTime;
            yield return null;
        }

        // Slight delay before attack
        yield return new WaitForSeconds(2f);

        // Switch statement determines animation based on enum
        switch (cultistStatus)
        {
            case (Status.attack):
                _animator.SetTrigger("attack");
                yield return new WaitForSeconds(attackDamageDelay);
                // Play SFX for attack
                combat.PlayDelayed(0.0f);
                GameManager.Instance.DamagePlayer(assignedPlayerIndex, "cultist");
                break;

            case (Status.summon):
                _animator.SetTrigger("summon");
                yield return new WaitForSeconds(3);
                wof = Instantiate(wallOfFire);

                if (attackSingleTarget)
                {
                    wof.GetComponent<FireConstantBaseScript>().Duration = 1;
                    wof.transform.position = wallOfFireSingleTargetLocation.position;
                    GameManager.Instance.DamagePlayer(assignedPlayerIndex, "fireSingle");
                    attackSingleTarget = false; // reset bool
                }
                else
                {
                    wof.GetComponent<FireConstantBaseScript>().Duration = 1;
                    wof.transform.position = wallOfFireLocation.position;
                    GameManager.Instance.DamagePlayer(assignedPlayerIndex, "fire");
                }
                
                break;

            case (Status.floatSummon):
                _animator.SetTrigger("floatSummon");
                StartCoroutine(SummonsCrows(assignedPlayerIndex));
                break;

            default:
                break;
        }

        yield return new WaitForSeconds(disappearDelay);

        while (invisibility < 1)
        {
            invisibility += disappearSpeed * Time.deltaTime;
            yield return null;
        }

        if (wallOfFire != null)
        {
            Destroy(wof);
        }
    }

    IEnumerator SummonsCrows(int playerIndex)
    {

        crowsAreDispatched = true;

        yield return new WaitForSeconds(summonCrowAttackDelay);

        foreach (Creature cre in crow)
        {
            cre.gameObject.SetActive(true);
            cre.Attack(playerIndex);
        }

        // We subtract summonCrowAttackDelay to make the overall cooldown time precise
        yield return new WaitForSeconds(crowAttackCooldownTime - summonCrowAttackDelay);

    }
}
