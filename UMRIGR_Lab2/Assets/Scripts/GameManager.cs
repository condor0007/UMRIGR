using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour
{
    private const float TIMER_DURATION = 30.0f;
    private const float COUNTDOWN_DURATION = 3.0f;

    private NetworkManager networkManager;
    private int playersReady;
    private GameObject networkSelect;
    private GameObject waitingForPlayer;
    private GameObject colorSelectActive;
    private GameObject colorSelectInactive;
    private GameObject gameCountdown;
    private GameObject gameOverlay;
    private GameObject endScreen;
    private Button redButton;
    private Button blueButton;
    private Button greenButton;
    private Button yellowButton;
    private Button magentaButton;
    private Button cyanButton;
    private Button resetButton;
    private TMP_Text gameStartCountdown;
    private TMP_Text gameTimer;
    private TMP_Text resultText;
    private TMP_Text scoreText;
    private bool gameCountdownState;
    private bool gameRunningState;
    float countdownTimeRemaining;
    float timerTimeRemaining;
    private Dictionary<string, int> playerScores = new Dictionary<string, int>();
    public List<Color> playerColors = new List<Color>();

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        playersReady = 0;
        networkSelect = GameObject.Find("NetworkSelect");
        waitingForPlayer = GameObject.Find("WaitingForPlayer");
        colorSelectActive = GameObject.Find("ColorSelectActive");
        colorSelectInactive = GameObject.Find("ColorSelectInactive");
        gameCountdown = GameObject.Find("GameCountDown");
        gameOverlay = GameObject.Find("GameOverlay");
        endScreen = GameObject.Find("EndScreen");
        redButton = GameObject.Find("RedButton").GetComponent<Button>();
        blueButton = GameObject.Find("BlueButton").GetComponent<Button>();
        greenButton = GameObject.Find("GreenButton").GetComponent<Button>();
        yellowButton = GameObject.Find("YellowButton").GetComponent<Button>();
        magentaButton = GameObject.Find("MagentaButton").GetComponent<Button>();
        cyanButton = GameObject.Find("CyanButton").GetComponent<Button>();
        resetButton = GameObject.Find("RestartButton").GetComponent<Button>();
        redButton.onClick.AddListener(delegate { SetColor(Color.red); });
        blueButton.onClick.AddListener(delegate { SetColor(Color.blue); });
        greenButton.onClick.AddListener(delegate { SetColor(Color.green); });
        yellowButton.onClick.AddListener(delegate { SetColor(Color.yellow); });
        magentaButton.onClick.AddListener(delegate { SetColor(Color.magenta); });
        cyanButton.onClick.AddListener(delegate { SetColor(Color.cyan); });
        resetButton.onClick.AddListener(ResetGameClientRpc);
        gameStartCountdown = GameObject.Find("GameStartCountdown").GetComponent<TMP_Text>();
        gameTimer = GameObject.Find("GameCountdown").GetComponent<TMP_Text>();
        resultText = GameObject.Find("ResultText").GetComponent<TMP_Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
        waitingForPlayer.SetActive(false);
        colorSelectActive.SetActive(false);
        colorSelectInactive.SetActive(false);
        gameCountdown.SetActive(false);
        gameOverlay.SetActive(false);
        endScreen.SetActive(false);
        gameCountdownState = false;
        gameRunningState = false;
        countdownTimeRemaining = COUNTDOWN_DURATION;
        timerTimeRemaining = TIMER_DURATION;
        networkManager.OnClientConnectedCallback += OnPlayerConnected;       
    }

    private void OnPlayerConnected(ulong obj)
    {
        Debug.Log("Player connected: " + obj);

        if (waitingForPlayer)
        {
            networkSelect.SetActive(false);
            waitingForPlayer.SetActive(true);
        }
        if (obj == 1 && networkManager.IsHost)
        {
            StartSetupClientRpc();
        }

    }

    private void SetColor(Color color)
    {
        CheckForColorServerRpc(color, networkManager.LocalClientId);
    }

    private void Update()
    {
        if (gameCountdownState)
        {
            colorSelectInactive.SetActive(false);
            countdownTimeRemaining -= Time.deltaTime;
            if (countdownTimeRemaining <= 0f)
            {
                gameCountdownState = false;
                if (networkManager.IsHost)
                {
                    StartGameClientRpc();
                }
            }
            UpdateText("{0}", countdownTimeRemaining, gameStartCountdown);
        }

        if (!gameRunningState) return;
        timerTimeRemaining -= Time.deltaTime;
        if (timerTimeRemaining <= 0f)
        {
            gameRunningState = false;
            if (networkManager.IsHost)
            {
                GameEndClientRpc();
            }
            GetResult();
            endScreen.SetActive(true);
        }
        UpdateText("{0}", timerTimeRemaining, gameTimer);
    }

    private void GetResult()
    {
        var winner = "";
        var highestScore = -1;

        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            var playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerScores.Add(playerManager.GetPlayerName(), 0);
            }
        }

        var floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (var floor in floors)
        {
            var floorManager = floor.GetComponent<FloorManager>();

            if (floorManager == null) continue;
            var playerName = floorManager.playerName;

            if (playerScores.ContainsKey(playerName))
            {
                playerScores[playerName]++;
            }
        }

        foreach (var entry in playerScores)
        {
            if (entry.Value <= highestScore) continue;
            
            highestScore = entry.Value;
            winner = entry.Key;
        }

        if (winner.Equals("Host"))
        {
            resultText.text = networkManager.IsHost ? "You win" : "You lose";
        }
        else
        {
            resultText.text = networkManager.IsHost ? "You lose" : "You win";
        }

        UpdateScoreText();
        if (!networkManager.IsHost) return;
        
        {
            foreach (var player in players)
            {
                var playerManager = player.GetComponent<PlayerManager>();

                if (playerManager != null && playerManager.GetPlayerName() == winner)
                {
                    player.GetComponent<MeshRenderer>().enabled = false;

                    var playerChildManager = player.GetComponentInChildren<PlayerChildManager>();

                    if (playerChildManager != null)
                    {
                        playerChildManager.AnimationStart();
                    }
                    else
                    {
                        Debug.LogWarning("Child object not found for the winning player.");
                    }

                    break;
                }
            }
        }

    }

    private void UpdateScoreText()
    {
        string scoreString = "Player Scores:\n";

        foreach (var entry in playerScores)
        {
            scoreString += entry.Key + ": " + entry.Value + "\n";
        }

        scoreText.text = scoreString;
    }

    static void UpdateText(string format, float time, TMP_Text text)
    {
        text.text = string.Format(format, Mathf.CeilToInt(time));
    }

    public void PlayerReady()
    {
        Debug.Log($"Player ready");
        playersReady++;
        if (networkManager.IsHost && playersReady == 2)
        {
            Debug.Log($"Starting game");
            StartCountdownClientRpc();
        }
    }

    void ActivateFloor()
    {
        var floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject floor in floors)
        {
            var floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                floorManager.ActivateFloor();
            }
        }
    }

    void DeactivateFloor()
    {
        var floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (var floor in floors)
        {
            var floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                floorManager.DeActivateFloor();
            }
        }
    }

    void EnablePlayerMovement()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            var playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.EnableMovement();
            }
        }
    }

    void DisablePlayerMovement()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            var playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.DisableMovement();
            }
        }
    }

    void ResetPlayerPosition()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            var playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.ResetPosition();
            }

            player.GetComponent<MeshRenderer>().enabled = true;

            var playerChildManager = player.GetComponentInChildren<PlayerChildManager>();

            if (playerChildManager != null)
            {
                playerChildManager.AnimationEnd();
            }
            else
            {
                Debug.LogWarning("Child object not found for the winning player.");
            }
        }
    }

    void ResetFloor()
    {
        var floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject floor in floors)
        {
            var floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                floorManager.ResetColor();
            }
        }
    }

    //TODO: All entities should activate this function
    [ClientRpc]
    private void StartSetupClientRpc()
    {
        //deactivate waitingForPlayer GameObject
        waitingForPlayer.SetActive(false);
        //activate colorSelectActive GameObject
        colorSelectActive.SetActive(true);
    }

    //TODO: Only the Server can run this function
    [ServerRpc(RequireOwnership = false)]
    private void CheckForColorServerRpc(Color color, ulong ClientId)
    {
        //activate only if playerColors does not contain the selected color
        // uncomment this block
        if (!playerColors.Contains(color))
        {
            //add the color to playerColors
            playerColors.Add(color);
            //tell the client that called the function that the selected color has been confirmed
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { ClientId }
                }
            };
            ColorConfirmClientRpc(color, rpcParams);
        }

    }
    

    //TODO: Only the entity specified in params can run this function
    [ClientRpc]
    private void ColorConfirmClientRpc(Color color, ClientRpcParams rpcParams = default)
    {
        //find the player that is owned by the client and set its color to the selected color via the method in PlayerManager
        var player = networkManager.SpawnManager.GetPlayerNetworkObject(networkManager.LocalClientId);

        if (player != null)
        {
            var playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.SetColor(color);
            }
        }

        //deactivate colorSelectActive GameObject
        //activate colorSelectInactive GameObject
        colorSelectActive.SetActive(false);
        colorSelectInactive.SetActive(true);
    
    }

    //TODO: All entities should activate this function
    [ClientRpc]
    private void StartCountdownClientRpc()
    {
        colorSelectInactive.SetActive(false);
        gameCountdown.SetActive(true);
        gameCountdownState = true;
        //deactivate colorSelectInactive GameObject
        //activate gameCountdown GameObject
        //activate the gameCountdownState trigger
    }

    //TODO: All entities should activate this function
    [ClientRpc]
    private void StartGameClientRpc()
    {
        gameCountdown.SetActive(false);
        gameOverlay.SetActive(true);
        ActivateFloor();
        EnablePlayerMovement();
        gameRunningState = true;
        //deactivate gameCountdown GameObject
        //activate gameOverlay GameObject
        //activate on enter triggers for the floor
        //enable players movement
        //activate the gameRunningState trigger
    }

    //TODO: All entities should activate this function
    [ClientRpc]
    private void GameEndClientRpc()
    {
        gameCountdown.SetActive(false);
        DeactivateFloor();
        DisablePlayerMovement();
        //deactivate gameCountdown GameObject
        //deactivate on enter triggers for the floor
        //disable players movement
    }

    //TODO: All entities should activate this function
    [ClientRpc]
    private void ResetGameClientRpc()
    {
        gameOverlay.SetActive(false);
        endScreen.SetActive(false);
        ResetPlayerPosition();
        ResetFloor();
        countdownTimeRemaining = COUNTDOWN_DURATION;
        timerTimeRemaining = TIMER_DURATION;
        playerScores.Clear();
        if (networkManager.IsHost)
        {
            StartCountdownClientRpc();
        }
        //deactivate gameCountdown GameObject
        //deactivate endScreen GameObject
        //reset player position
        //reset the floor 
        //reset countdownTimeRemaining
        //reset timerTimeRemaining
        //clear playerScores
        //if this is the host, start countdown rpc
        /*
        if ()
        {

        }
        */
    }
}
