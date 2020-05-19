using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorrorCharacterController : MonoBehaviour
{
    // Player info & status
    public string playerName = "PLAYER";
    public bool humanPlayer;
    public string botName;
    public enum Status { idle, workingOnTask, healing, dead, returningFromDead };
    public Status playerStatus;
    private string statusString = "";// formatted version of playerStatus
    public string charSpecificTask = ""; // Hint: should match task animation!
    public bool dead = false;
    public bool gettingHurt = false;

    // Health parameters
    public int health = 100;
    public int healIncrement = 4;
    public int maxHealth = 100;
    public bool reducedDamage;
    public float percentageOfOriginalDamage;

    // Effect parameters
    public GameObject bloodSpray; // Enable to animate!
    public AudioSource fallSound; // SFX for falling
    public GameObject resurrectSFX;
    public GameObject resurrectVFX;

    // Task parameters
    public float taskProgress = 0;
    public float taskIncrement = 1.0f;

    // Ability parameters
    public PowerCooldown abilityIcon;
    public int abilityUses = 0;
    public AbilityObject abilityObject;
    
    // UI references
    public ProgressBarPro healthBar;
    public ProgressBarPro taskBar;
    public Text nameLabel;
    public Text statusLabel;
    public Text damageIndicator;
    public Canvas canvas;
    public float indicatorOffsetPos = 1.7f;

    // Animation code
    private Animation anim;
    //private AnimationClip currentAnim;
    public string taskAnimation; // Character specific working task

    // Use this for initialization
    void Start()
    {
        bloodSpray.SetActive(false); // Hide blood on start!

        anim = GetComponent<Animation>();
        playerStatus = Status.idle;

        InvokeRepeating("IncrementLevels", 0.0f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {

        nameLabel.text = playerName;

        if (!this.dead)
        {
            statusLabel.text = statusString;
        }

        // Switch statement to set approriate animation
        if (GameManager.Instance.isGameRunning && !gettingHurt)
        {
            switch (playerStatus)
            {
                case Status.idle:
                    statusLabel.color = Color.gray;
                    statusString = "Type: !task to start working"; // change this later?
                    anim.Play("idle");
                    break;

                case Status.workingOnTask:
                    statusLabel.color = Color.yellow;
                    statusString = charSpecificTask;
                    anim.Play(taskAnimation);
                    break;

                case Status.healing:

                    if (health < maxHealth)
                    {
                        statusLabel.color = Color.green;
                        statusString = "HEALING";
                        anim.Play("sitting idle");
                    }
                    else
                    {
                        // Health is full go back to task
                        playerStatus = Status.workingOnTask;
                    }
                    break;

                case Status.dead:
                    Die();
                    break;

                case Status.returningFromDead:
                    Resurrect(50);
                    break;

                default:
                    break;
            }
        }

        // Custom HEALTH clamping (It's better!)
        if (health < 0) health = 0;
        if (health > maxHealth) health = maxHealth;

        // Check for zero health (zero health = death)
        if (health < 1 && playerStatus != Status.returningFromDead)
            playerStatus = Status.dead;

        // Custom TASKProgress clamping (It's better!)
        if (taskProgress < 0) taskProgress = 0;
        if (taskProgress > 100) taskProgress = 100;
    }

    /// <summary>
    /// Note: Do not use for DOT attacks
    /// </summary>
    public void HurtByEnemy(int damageAmount, bool showBlood)
    {
        if (dead) return;

        if (reducedDamage)
        {
            damageAmount = (int)(damageAmount * (percentageOfOriginalDamage / 100f));
            Debug.Log("Damage amount is: " + damageAmount);
            reducedDamage = false;
        }

        StartCoroutine(GetHurt(damageAmount, showBlood));
    }

    /// <summary>
    /// For damage over time
    /// </summary>
    public void GetDOT(int damageAmount, int damageCount)
    {
        if (dead) return;

        if (reducedDamage)
        {
            damageAmount = (int)(damageAmount * (percentageOfOriginalDamage / 100f));
            reducedDamage = false;
        }

        StartCoroutine(GetDOTCR(damageAmount, damageCount));
    }

    void Die()
    {
        if (dead) return;

        dead = true;

        // Check if this player is the last alive, if so record it!
        if (GameManager.Instance.totalDeadPlayers == 3)
        {
            if (this.humanPlayer)
                GameManager.Instance.lastAlive = playerName;
            else
                GameManager.Instance.lastAlive = botName;
        }

        // Tell GameManager that a player has died
        GameManager.Instance.totalDeadPlayers++;

        // incase Die() is forced, force health to zero
        if (health > 0)
        {
            health = 0;
        }

        // Set status label to indicate death
        statusLabel.color = new Color(0.5f, 0, 0.9f, 1);
        statusString = "DEAD";
        statusLabel.text = statusString;

        // Update health bar regardless of whether death was forced
        UpdateHealthBar();
        anim.Play("death");
        fallSound.PlayDelayed(1.5f);
    }

    /// <summary>
    /// Allows player to be brought back to life with a specified set health
    /// </summary>
    void Resurrect(int rezHealthLevel)
    {
        if (!this.dead || GameManager.Instance.gameOver) { return; }

        this.dead = false;
        GameManager.Instance.totalDeadPlayers--;
        health = rezHealthLevel;
        StartCoroutine(GetRessurectedCR());
    }

    /// <summary>
    /// Allows player get a health boost
    /// </summary>
    public void GetHealthBoost(int healAmount)
    {
        if (this.dead || GameManager.Instance.gameOver) { return; }

        StartCoroutine(GetHealthBoostCR(healAmount));
    }

    /// <summary>
    /// Performs the players special ability
    /// </summary>
    public virtual void AttemptToPerformAbility()
    {
        if (this.dead) { return; }

        if (abilityUses < 1)
        {
            // Alert player that he/she has no abilities to use
            ShowAlertMessage("This character does not do abilities!", Color.red, 14);
            return;
        }
    }

    // Increments the health and taskProgress every second 
    private void IncrementLevels()
    {
        if (dead || GameManager.Instance.gameOver) return;

        if (taskProgress < 100 && playerStatus == Status.workingOnTask)
        {
            taskProgress += taskIncrement;
            UpdateTaskBar();
        }
        else if (taskProgress >= 100 && playerStatus == Status.workingOnTask)
        {
            playerStatus = Status.idle;
        }

            if (health < maxHealth && playerStatus == Status.healing)
        {
            health += healIncrement;
            UpdateHealthBar();
            ShowDamageNumber((int)healIncrement, false, 14);

            // If finished healing (and not dead, and task is not 100%) get back to work!
            if (health >= maxHealth && !dead && !gettingHurt && taskProgress < 100)
                playerStatus = Status.workingOnTask;
            else if (health >= maxHealth && !dead && !gettingHurt) 
                playerStatus = Status.idle; //Full HP and Full taskProgress ->Go idle
        }
    }

    IEnumerator GetHurt(int damage, bool showBlood)
    {
        if (gettingHurt) yield break;

        if (showBlood)
            StartCoroutine(SprayBloodCR(0f));

        statusLabel.color = Color.red;
        statusString = "TAKING DAMAGE";

        gettingHurt = true;
        health -= damage;

        UpdateHealthBar();
        ShowDamageNumber((int)damage, true, 14);
        anim.Play("hurt");

        yield return new WaitForSeconds(1);

        gettingHurt = false;
        statusString = "temp";
    }

    IEnumerator GetHealthBoostCR(int healAmount)
    {
        // Play VFX and SFX

        yield return new WaitForSeconds(1);
        health += healAmount;
        ShowDamageNumber(healAmount, false, 26);
        UpdateHealthBar();

        yield return new WaitForSeconds(5);
        // Disable VFX and SFX
    }

    IEnumerator GetRessurectedCR()
    {
        // Play VFX and SFX
        resurrectSFX.SetActive(true);
        resurrectVFX.SetActive(true);

        yield return new WaitForSeconds(2f);
        playerStatus = Status.idle;
        UpdateHealthBar();

        yield return new WaitForSeconds(4f);
        // Disable SFX and VFX object
        resurrectSFX.SetActive(false);
        resurrectVFX.SetActive(false);

        // If it's a bot automatically heal
        if (!this.dead && !this.humanPlayer)
        {
            playerStatus = Status.healing;
        }
    }

    IEnumerator GetDOTCR(int damage, int damageCounter)
    {
        if (gettingHurt) yield break;

        statusLabel.color = Color.red;
        statusString = "TAKING DAMAGE";
        gettingHurt = true;

        while (damageCounter > 0)
        {
            damageCounter -= 1;
            health -= damage;
            UpdateHealthBar();
            ShowDamageNumber((int)damage, true, 14);
            anim.Play("hurt");
            yield return new WaitForSeconds(1.25f);
            yield return null;
        }

        yield return new WaitForSeconds(1);
        gettingHurt = false;
        statusString = "temp";
    }

    private void UpdateHealthBar()
    {
        healthBar.SetValue(health, maxHealth);
    }

    private void UpdateTaskBar()
    {
        taskBar.Value = taskProgress / 100;
    }

    private void ShowDamageNumber(int damageAmount, bool isDamage, int theFontSize)
    {
        Vector3 location = new Vector3(this.transform.position.x,
            this.transform.position.y + indicatorOffsetPos, this.transform.position.z);

        if (Camera.main != null) // Used to avoid crash at win (Since main camera gets disabled)
        {
            Text latestDamageNumber
                = Instantiate(damageIndicator, Camera.main.WorldToScreenPoint(location),
                    new Quaternion(0, 0, 0, 0), canvas.transform);

            // Set font size after instantiate, this should be OKAY!
            latestDamageNumber.fontSize = theFontSize;

            latestDamageNumber.text = "-" + damageAmount.ToString();
            if (isDamage)
            {
                latestDamageNumber.text = "-" + damageAmount.ToString();
            }
            else
            {
                latestDamageNumber.color = Color.green;
                latestDamageNumber.text = "+" + damageAmount.ToString();
            }
        }
        else
        {
            Debug.Log("Damage number could not be displayed. Game camera was disabled. (Win scenario?)");
        }
    }

    protected void ShowAlertMessage(string textToDisplay, Color c, int theFontSize)
    {
        Vector3 location = new Vector3(this.transform.position.x,
            this.transform.position.y + indicatorOffsetPos, this.transform.position.z);

        if (Camera.main.enabled) // Used to avoid crash at win (Since main camera gets disabled)
        {
            Text alertText
                = Instantiate(damageIndicator, Camera.main.WorldToScreenPoint(location),
                    new Quaternion(0, 0, 0, 0), canvas.transform);

            alertText.fontSize = theFontSize;
            alertText.color = c;
            alertText.text = textToDisplay;
        }
    }

    IEnumerator SprayBloodCR(float delay)
    {
        bloodSpray.SetActive(false); // Reset just incase object was not reset
        yield return new WaitForSeconds(delay);
        bloodSpray.SetActive(true);
        yield return new WaitForSeconds(3f);
        bloodSpray.SetActive(false);
    }
}
