using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


/* Centralized gamemanager class
 * References are linked via scene file
 * NOTE Currently set to Destroy on scene load to preserve references */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool debugMode;

    public float AttackReactionDelay = 0.6f; // 0.6f recommended
    public int wendigoDamageAmount;
    public int cultistDamageAmount;
    public int crowDamageAmount;
    public int summonFireDamage;

    // Game Data
    public int timeInSecondsToJoinGame = 30;
    public int timeUntilNextGame = 30;
    public int totalDeadPlayers = 0;
    public bool isGameRunning;
    public bool gameOver;
    public List<string> twitchPlayers;
    public int numberOfPlayersJoined = 0;
    public string lastAlive;
    public string hero = "None";
    public bool enemiesProvokedByMechanic; // whether or not enemies were provoked by mechanic

    // Audio
    public GameObject bangOnDoor;
    public GameObject bangOnDoorTwice;

    [Space(20)] // 10 pixels of space in inspector
    // Other Managers
    public LightManager lightManager;
    public WinManager winManager;

    [Space(20)] // 10 pixels of space in inspector
    // Characters
    public Wendigo wendigo;
    public HorrorCharacterController[] player; // 0 - 3 (Four players)
    public Cultist[] attackCultist;
    public Cultist[] summonCultist;
    public Cultist[] floatingCrowCultist;

    [Space(20)] // 10 pixels of space in inspector
    // Level pieces / Props
    public GameObject curtains;
    private Animator _curtainAnim;

    [Space(20)] // 10 pixels of space in inspector
    // GUI Objects and variables
    public Text joinGame;
    public Text countDownTimer;
    public float textScrollSpeed;
    public Text gameOverText;
    public GameObject[] taskBarHolder; // Task progression UI (for disabling before start of game!)
    public GameObject[] healthBar;
    public GameObject[] nameLabel;
    public Image resultUI;
    public TextMeshProUGUI resultTMP;
    public GameObject resultWinText;
    public GameObject resultLoseText;
    public float curtainDelay = 5f;
    public Text[] titleComponent;
    public GameObject awardWindow;
    public Text awardText;
    public Text awardRecipient;
    public GameObject[] AllProgressBars;
    public GameObject[] AllStatusText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        curtains.gameObject.SetActive(true);
        _curtainAnim = curtains.GetComponent<Animator>();

        // Hide all task bars until start of game!
        foreach (GameObject go in taskBarHolder)
        {
            go.gameObject.SetActive(false);
        }
    }

    // Use this for initialization
    void Start()
    {
        // Clear the countdown timer
        countDownTimer.text = "";

        // Initiate count down for start of actual game
        StartCoroutine(TimedPlayerSetup());

        // Clear the title
        foreach (Text t in titleComponent)
        {
            Color textColor = t.color;
            textColor.a = 0.0f;
            t.color = textColor;
        }

        // Fade in the title
        StartCoroutine(FadeTitleText(true, 1f));
    }

    // Update is called once per frame
    void Update()
    {
        // Check for Win
        if (player[0].taskProgress == 100 && !gameOver)
        {
            gameOver = true;
            winManager.ActivateWinScene("Spell");
            hero = (player[0].humanPlayer) ? player[0].playerName : "Player 1";
        }
        else if (player[2].taskProgress == 100 && !gameOver)
        {
            gameOver = true;
            winManager.ActivateWinScene("Cop");
            hero = (player[2].humanPlayer) ? player[2].playerName : "Player 3";
        }
        else if (player[3].taskProgress == 100 && !gameOver)
        {
            gameOver = true;
            winManager.ActivateWinScene("Car");
            hero = (player[3].humanPlayer) ? player[3].playerName : "Player 4";
        }

        // Check if enough players are dead for a Game Over Status
        if (totalDeadPlayers > 3 && !gameOver)
        {
            gameOver = true;
            StartCoroutine(GameOver());
        }


        /**********FOR TESTING ONLY*****************/
        //CULTIST ATTACKS
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            attackCultist[0].DoAction();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            attackCultist[1].DoAction();
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            attackCultist[2].DoAction();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            attackCultist[3].DoAction();
        }

        // CROW ATTACKS
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            floatingCrowCultist[0].DoAction();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            floatingCrowCultist[1].DoAction();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            floatingCrowCultist[2].DoAction();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            floatingCrowCultist[3].DoAction();
        }
        
        /*******END OF TESTING CODE********************/

    }

    // NOTE: Command currently has a 10 char limit (including '!')
    public void HandleCommand(string username, string command)
    {
        command = command.ToLower();

        switch (command)
        {
            case "!play":
                AddPlayer(username, 999);
                break;

            case "!join":
                AddPlayer(username, 999);
                break;

            case "!player1":
                AddPlayer(username, 0);
                break;

            case "!player2":
                AddPlayer(username, 1);
                break;

            case "!player3":
                AddPlayer(username, 2);
                break;

            case "!player4":
                AddPlayer(username, 3);
                break;

            case "!task":
                DoTask(username);
                break;

            case "!rest":
                Rest(username);
                break;

            case "!heal":
                Rest(username);
                break;

            case "!ability":
                Ability(username, 999);
                break;

            case "!rez":
                Ability(username, 0);
                break;

            case "!tnt":
                Ability(username, 1);
                break;

            case "!medkit":
                Ability(username, 2);
                break;

            case "!provoke":
                Ability(username, 3);
                break;

            default:
                break;
        }
    }

    public void AddPlayer(string chatName, int playerSelection)
    {
        // if player already in game, then cancel the join
        if (twitchPlayers.Contains(chatName)) { return; }

        // 999 = Next availible slot
        if (playerSelection.Equals(999))
        {
            foreach (HorrorCharacterController hcc in player)
            {
                if (!hcc.humanPlayer)
                { // Found a free player slot
                    twitchPlayers.Add(chatName);
                    hcc.playerName = chatName;
                    hcc.humanPlayer = true;
                    numberOfPlayersJoined++;
                    return;
                }
            }
        }
        else
        {
            // Check if player is already taken
            if (player[playerSelection].humanPlayer) { return; }

            twitchPlayers.Add(chatName);
            player[playerSelection].playerName = chatName;
            player[playerSelection].humanPlayer = true;
            numberOfPlayersJoined++;
        }
    }

    public void DoTask(string chatName)
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (string.Equals(player[i].playerName, chatName) && !player[i].dead)
            {
                player[i].playerStatus = HorrorCharacterController.Status.workingOnTask;
            }
        }
    }

    public void Rest(string chatName)
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (string.Equals(player[i].playerName, chatName) && !player[i].dead)
            {
                player[i].playerStatus = HorrorCharacterController.Status.healing;
            }
        }
    }

    public void Ability(string chatName, int abilityIndex)
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (string.Equals(player[i].playerName, chatName) 
                && (abilityIndex == i || abilityIndex == 999)
                    && !player[i].dead)
            {
                player[i].AttemptToPerformAbility();
            }
        }
    }

    // Player index: 0=man, 1=worker, 2=woman, 3=cop
    public void DamagePlayer(int playerIndex, string typeOfAttacker)
    {
        switch (typeOfAttacker)
        {
            case "creature":
                player[playerIndex].HurtByEnemy(crowDamageAmount, true);
                break;

            case "wendigo":
                player[playerIndex].HurtByEnemy(wendigoDamageAmount, true);
                break;

            case "cultist":
                player[playerIndex].HurtByEnemy(cultistDamageAmount, true);
                break;

            case "fire":
                foreach (HorrorCharacterController pl in player)
                {
                    pl.GetDOT(summonFireDamage, 4); // parameters are damage amount, and number of hits
                }
                break;

            case "fireSingle":
                player[3].GetDOT(summonFireDamage, 4); // parameters are damage amount, and number of hits
                break;

            default:
                break;
        }
    }

    public void HandleAfterWin()
    {
        StartCoroutine(AfterWin());
    }

    /// <summary>
    /// Sets the results of the game into the results window
    /// </summary>
    public void SetResults(bool won)
    {
        string totalResult = "";
        int numberOfHumanPlayers = twitchPlayers.Count;
        //int numberOfBots = 4 - twitchPlayers.Count;

        resultWinText.SetActive(won);
        resultLoseText.SetActive(!won);

        // Calculate results
        for (int i = 0; i < 4; i++)
        {
            // Determine if player name was a screenname, otherwise "Player 1" etc.
            if (player[i].humanPlayer)
            {
                totalResult += "\n\n<color=white>" + player[i].playerName + " - ";
            }
            else
            {
                // Do (i + 1) so that player[0] appears as "Player1" and so on...
                totalResult += "\n\n<color=white>Player" + (i + 1) + " - ";
            }

            totalResult += (player[i].dead ? "<color=red>DEAD" : "<color=green>SURVIVED")
                    + "<color=white>   | Task Progress: "
                    + (int)player[i].taskProgress + "%";
        }
        // Print to the results UI panel
        resultTMP.text = totalResult;

        if (won)
        { // Won Game
            awardText.text = "Hero";
            awardRecipient.text = hero;
        }
        else
        { // Lost Game
            awardText.text = "Last to Die";
            awardRecipient.text = lastAlive;
        }
    }

    /// <summary>
    /// Made to fade in/out the main title of the game (based on boolean value)
    /// </summary>
    IEnumerator FadeTitleText(bool fadeIn, float delay)
    {

        // Delay Fade
        yield return new WaitForSeconds(delay);

        if (fadeIn)
        {
            float fadeLevel = 0;

            while (fadeLevel < 1)
            {
                fadeLevel += Time.deltaTime * 0.5f;
                foreach (Text t in titleComponent)
                {
                    Color textColor = t.color;
                    textColor.a = fadeLevel;
                    t.color = textColor;
                }
                yield return null;
            }
        }
        else // Fade out!
        {
            float fadeLevel = 1;

            while (fadeLevel > 0)
            {
                fadeLevel -= Time.deltaTime * 0.5f;
                foreach (Text t in titleComponent)
                {
                    Color textColor = t.color;
                    textColor.a = fadeLevel;
                    t.color = textColor;
                }
                yield return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator AfterWin()
    {
        yield return StartCoroutine(ShowResultsAndReset(true));
        yield break;
    }

    // Disables status UI, and scrolls the gameover text
    IEnumerator GameOver()
    {
        // Disable status UI for each player
        foreach (HorrorCharacterController deadPlayer in player)
        {
            deadPlayer.nameLabel.gameObject.SetActive(false);
            deadPlayer.taskBar.gameObject.SetActive(false);
        }

        RectTransform rt = gameOverText.GetComponent<RectTransform>();

        float newY = rt.anchoredPosition.y;

        while (newY < 70.5f)
        {
            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);
            newY += textScrollSpeed * Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(ShowResultsAndReset(false));
    }

    /// <summary>
    /// Sets the target based on whether or not the Wendigo was provoked.
    /// </summary>
    /// <returns>The Target Index</returns>
    private int CheckForMechanicProvoke(int randomTargetNumber, out string debugMsg)
    {
        if (enemiesProvokedByMechanic)
        {
            debugMsg = "Enemy was PROVOKED to attack: Player";
            player[3].reducedDamage = true;
            enemiesProvokedByMechanic = false;
            summonCultist[0].attackSingleTarget = true;
            return 3;
        }

        summonCultist[0].attackSingleTarget = false;
        debugMsg = "Enemy's designated target is: Player";
        return randomTargetNumber;
    }

    /// <summary>
    /// Shows the results of the match (who survived) draws curtains, starts reset counter
    /// Then resets the game using scene manager
    /// </summary>
    /// <returns>void</returns>
    IEnumerator ShowResultsAndReset(bool won)
    {
        yield return new WaitForSeconds(curtainDelay); // Delay after Win-cinematic/GameOver

        // Lower the curtain
        _curtainAnim.SetBool("ActualGameIsPlaying", false);

        yield return new WaitForSeconds(8f); // Delay between close curtains and raise results UI

        /******Raise Results UI **************************************/
        SetResults(won); // Set values to display as result (including win/lose header)
        RectTransform rt = resultUI.GetComponent<RectTransform>();
        float newY = rt.anchoredPosition.y;
        while (newY < 70.5f)
        {
            rt.anchoredPosition = new Vector3(rt.anchoredPosition.x, newY);
            newY += (textScrollSpeed * 4) * Time.deltaTime;
            yield return null;
        } /*************************************************************/

        // After a delay display a timer until the next game starts
        float timeLeft = timeUntilNextGame;

        yield return new WaitForSeconds(3);

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 10)
                countDownTimer.text = "Next game starts in: 00:0" + (int)timeLeft; // Set correct time formatting based on secs
            else
                countDownTimer.text = "Next game starts in: 00:" + (int)timeLeft; // Set correct time formatting based on secs
            yield return null;
        }

        countDownTimer.text = "Starting next game!";

        yield return new WaitForSeconds(3);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator TimedPlayerSetup()
    {
        float timeLeft = timeInSecondsToJoinGame;

        if (!debugMode)
        { // Need at least one human player to play the game, (wait for at least one human)
            yield return new WaitUntil(() => numberOfPlayersJoined > 0);
        }
        else
        {
            timeLeft = 3;
        }

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 10)
                countDownTimer.text = "Game starts in: 00:0" + (int)timeLeft; // Set correct time formatting based on secs
            else
                countDownTimer.text = "Game starts in: 00:" + (int)timeLeft; // Set correct time formatting based on secs
            yield return null;
        }

        // Fade out title text
        StartCoroutine(FadeTitleText(false, 1f));

        // activate curtain to start game!
        _curtainAnim.SetBool("ActualGameIsPlaying", true);

        countDownTimer.text = "Game starting!";
        joinGame.text = "";

        yield return new WaitForSeconds(5);

        countDownTimer.text = "";

        // Official start of game
        isGameRunning = true;

        // Have bots automatically start performing task
        foreach (HorrorCharacterController hcc in player)
        {
            if (!hcc.humanPlayer)
            {
                hcc.playerStatus = HorrorCharacterController.Status.workingOnTask;
            }
        }

        // Game has started show all task bars!
        foreach (GameObject go in taskBarHolder)
        {
            go.gameObject.SetActive(true);
        }

        StartCoroutine(MainScript());
    }

    /// <summary>
    /// Camera shake controller
    /// </summary>
    /// <returns></returns>
    IEnumerator ShakeyCameraCR(float durationOfShake, float shakeLevel)
    {

        while (durationOfShake > 0 && !gameOver)
        {
            durationOfShake -= Time.deltaTime * 2;
            StressReceiver sr = Camera.main.GetComponent<StressReceiver>();
            if (sr != null)
            {
                sr.InduceStress(shakeLevel);
            }
            yield return new WaitForSeconds(0.2f);
            yield return null;
        }
    }

    IEnumerator BangOnDoorCR(bool blocked)
    {
        if (blocked)
        {
            bangOnDoor.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ShakeyCameraCR(0.02f, 0.1f));
            yield return new WaitForSeconds(1.15f);
            StartCoroutine(ShakeyCameraCR(0.02f, 0.1f));
            yield return new WaitForSeconds(1.05f);
            StartCoroutine(ShakeyCameraCR(0.02f, 0.1f));
        }
        else
        {
            bangOnDoorTwice.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ShakeyCameraCR(0.02f, 0.1f));
            yield return new WaitForSeconds(1.15f);
            StartCoroutine(ShakeyCameraCR(0.02f, 0.1f));
        }

        yield return new WaitForSeconds(5f);
        bangOnDoorTwice.SetActive(false);
        bangOnDoor.SetActive(false);
    }

    // Main script for actually gameplay, which is mostly random attacking
    IEnumerator MainScript()
    {
        // Delay before start of while-loop
        yield return new WaitForSeconds(10);

        while (!gameOver)
        {
            // Firstly, if everyone is dead except for worker, invoke "super" Wendigo! (to finish game)
            if (player[0].dead && !player[1].dead && player[2].dead && player[3].dead)
            {
                int worker = 1;
                wendigoDamageAmount = 200;

                lightManager.disableHouseLights(21);
                yield return new WaitForSeconds(4);

                lightManager.activateSpotLight(worker, 12, 0);
                lightManager.activateSpotLight(4, 18, 2);


                // Delay wendigo attack slightly for dramatic purposes
                yield return new WaitForSeconds(3);

                // If barricade exists then change FX for breaking door
                {
                    if (player[1].taskProgress >= 25)
                    {
                        StartCoroutine(BangOnDoorCR(false));
                        yield return new WaitForSeconds(2.5f);
                    }
                }
                wendigo.AttackSingleTarget(worker, 0.3f);

                yield return new WaitForSeconds(20); // Prevents Wendigo "walking through wall" bug
                continue;
            }

            string debugAttack = "";
            int methodOfAttackNumber = UnityEngine.Random.Range(1, 120);
            int target = UnityEngine.Random.Range(0, 4); // Randomize victim

            // If target is already dead, fetch another number
            if (player[target].dead) { continue; }

            // Determine the chances of wendigo breaking down door
            int wendigoChancePercentage = 18;
            if (player[1].taskProgress >= 100)
                wendigoChancePercentage = 4;
            else if (player[1].taskProgress >= 75)
                wendigoChancePercentage = 6;
            else if (player[1].taskProgress >= 50)
                wendigoChancePercentage = 10;
            else if (player[1].taskProgress >= 25)
                wendigoChancePercentage = 14;

            // LOG TO CONSOLE // DEBUG
            Debug.Log(debugAttack + (target + 1)
                + " with method: " + methodOfAttackNumber + "\nWendigo chance % was " + wendigoChancePercentage);

            if (methodOfAttackNumber > wendigoChancePercentage && methodOfAttackNumber <= 16)
            {// Wendigo moved to attack but was blocked by the barricade!
                lightManager.disableHouseLights(12);
                yield return new WaitForSeconds(4);
                target = CheckForMechanicProvoke(target, out debugAttack);
                lightManager.activateSpotLight(target, 8, 0);
                lightManager.activateSpotLight(4, 6, 2);

                // Delay wendigo attack slightly for dramatic purposes
                yield return new WaitForSeconds(3);

                Debug.Log("Wendigo couldn't break through door!");
                StartCoroutine(BangOnDoorCR(true));
            }
            else if (methodOfAttackNumber <= wendigoChancePercentage)
            { // Summon Wendigo chance % = wendigoChance
                lightManager.disableHouseLights(21);
                yield return new WaitForSeconds(4);
                target = CheckForMechanicProvoke(target, out debugAttack);
                lightManager.activateSpotLight(target, 12, 0);
                lightManager.activateSpotLight(4, 18, 2);


                // Delay wendigo attack slightly for dramatic purposes
                yield return new WaitForSeconds(3);

                // If barricade exists then change FX for breaking door
                {
                    if (player[1].taskProgress >= 25)
                    {
                        StartCoroutine(BangOnDoorCR(false));
                        yield return new WaitForSeconds(2.5f);
                    }
                }
                wendigo.AttackSingleTarget(target, 0.3f);

                yield return new WaitForSeconds(20); // Prevents Wendigo "walking through wall" bug
            }
            else if (methodOfAttackNumber >= 29 && methodOfAttackNumber <= 49)
            { // Summon Cultist = 20% chance
                lightManager.disableHouseLights(16);
                yield return new WaitForSeconds(4);
                target = CheckForMechanicProvoke(target, out debugAttack);
                lightManager.activateSpotLight(5, 14, 0);

                // Delay attack slightly for dramatic purposes
                yield return new WaitForSeconds(4);
                summonCultist[0].DoAction();
            }
            else if (methodOfAttackNumber >= 50 && methodOfAttackNumber <= 70)
            { // Attack Cultist = 20% chance
                lightManager.disableHouseLights(9);
                yield return new WaitForSeconds(4);
                target = CheckForMechanicProvoke(target, out debugAttack);
                lightManager.activateSpotLight(target, 8, 0);

                // Delay attack slightly for dramatic purposes
                yield return new WaitForSeconds(4);
                attackCultist[target].DoAction();
            }
            else if (methodOfAttackNumber >= 71 && methodOfAttackNumber <= 96)
            { // Summon Crow Cultist = 25% chance
                lightManager.disableHouseLights(16);
                yield return new WaitForSeconds(4);
                target = CheckForMechanicProvoke(target, out debugAttack);
                lightManager.activateSpotLight(target, 10, 0);

                // Delay attack slightly for dramatic purposes
                yield return new WaitForSeconds(4);
                floatingCrowCultist[target].DoAction();
            }

            yield return new WaitForSeconds(10);
            yield return null;
        }

        yield break;
    }
}
