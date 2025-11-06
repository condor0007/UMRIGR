using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;
//using UnityEditor.Callbacks;

public class PlayerManager : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float powerUpDuration = 5f;
    public float powerUpSpeedMultiplier = 2f;
    private float powerUpMultiplier = 1f;
    private bool hasPowerUp = true;
    private bool isPowerUpActive = false;
    private float powerUpEndTime = 0f;
    private float normalSpeed = 5f;
    private NetworkManager networkManager;
    private GameManager gameManager;
    private Vector3 spawnPoint1 = new Vector3(11f, 1.5f, 0f);
    private Vector3 spawnPoint2 = new Vector3(0f, 1.5f, 11f);
    private bool canMove = false;
    private MeshRenderer playerMeshRenderer;
    //TODO: this network variable needs to be readable by everyone and can be written only by owner
    private NetworkVariable<FixedString512Bytes> playerName = new NetworkVariable<FixedString512Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); //uncomment this

    private void FixedUpdate()
    {
        if (IsOwner && canMove)
        {
            Movement();
        }
        if (IsOwner && Input.GetKeyDown(KeyCode.Space))
        {
            ActivatePowerUpServerRpc();
        }
        if (isPowerUpActive)
        {
            powerUpEndTime -= Time.deltaTime;
            if (powerUpEndTime <= 0f)
            {
                isPowerUpActive = false;
                // Notify all clients to revert speed boost
                DeactivatePowerUpClientRpc();
            }
        }
    }

    // TODO
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkManager = FindObjectOfType<NetworkManager>();
        gameManager = FindObjectOfType<GameManager>();

        // uncomment this block
        // if you are the owner and the host, set the player to spawnPoint1 and rename the player to "Host"
        if (IsHost && IsOwner)
        {
            transform.SetPositionAndRotation(spawnPoint1, new Quaternion());
            playerName.Value = "Host";
        }
        //if you are the owner and the client, set the player to spawnPoint2 and rename the player to "Client"
        else if (IsClient && IsOwner)
        {
            transform.SetPositionAndRotation(spawnPoint2, new Quaternion());
            playerName.Value = "Client";
        }

        
    }

    private void Start()
    {
        playerMeshRenderer = this.GetComponent<MeshRenderer>();
    }

    void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        MovePlayer(moveDirection);
    }

    void MovePlayer(Vector3 moveDirection)
    {
        RaycastHit hit;
        Vector3 move = moveDirection * moveSpeed * Time.deltaTime * powerUpMultiplier;
        var rb = this.GetComponent<Rigidbody>();
        if (!Physics.Raycast(transform.position, moveDirection, out hit, move.magnitude))
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime * powerUpMultiplier);
        }
        else
        {
            rb.MovePosition(hit.point - move.normalized * 0.1f);
        }
        // transform.Translate(moveDirection * moveSpeed * Time.deltaTime * powerUpMultiplier);
    }

    void updateSpeed(string speed = "normal")
    {
        if (speed == "fast")
        {
            moveSpeed = 7.5f;
        }
        else if (speed == "normal")
        {
            moveSpeed = 5f;
        }
    }

    [ClientRpc]
    public void ApplySpeedClientRpc(bool enable = false, ClientRpcParams rpcParams = default)
    {
        if (enable)
            updateSpeed("fast");
        else
            updateSpeed("normal");
    }

    public void ResetPosition()
    {
        if (IsOwner && networkManager.IsHost)
        {
            transform.SetPositionAndRotation(spawnPoint1, new Quaternion());
        }
        else if (IsOwner && networkManager.IsClient)
        {
            transform.SetPositionAndRotation(spawnPoint2, new Quaternion());
        }
        hasPowerUp = true;
        isPowerUpActive = false;
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    public string GetPlayerName()
    {
        return playerName.Value.ToString(); //uncomment this
    }

    public void SetColor(Color color)
    {
        ChangeColorRpc(color);
        ResetPosition();
    }

    //TODO: All entities should activate this function
    [Rpc(SendTo.Everyone)]
    private void ChangeColorRpc(Color color)
    {
        // change the color of the player's material
        playerMeshRenderer.material.color = color;

        // find playerChildManager of the child object
        var playerChildManager = GetComponentInChildren<PlayerChildManager>();

        if (playerChildManager != null)
        {
            // set the color of the child object
            playerChildManager.SetColor(color);
        }
        else
        {
            Debug.LogWarning("playerChildManager not found");
        }

        // tell the game manager that the player is ready
        gameManager.PlayerReady();
    }
    [ServerRpc(RequireOwnership = true)]
    public void ActivatePowerUpServerRpc(ServerRpcParams rpcParams = default)
    {
        ActivatePowerUpClientRpc();
    }

    [ClientRpc]
    public void ActivatePowerUpClientRpc(ClientRpcParams rpcParams = default)
    {
        if (hasPowerUp && !isPowerUpActive)
        {
            isPowerUpActive = true;
            hasPowerUp = false;
            powerUpEndTime = Time.time + powerUpDuration;
            powerUpMultiplier = powerUpSpeedMultiplier;
        }
    }
    [ClientRpc]
    public void DeactivatePowerUpClientRpc(ClientRpcParams rpcParams = default)
    {
        isPowerUpActive = false;
        powerUpMultiplier = 1f;
    }

}
