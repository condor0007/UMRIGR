using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;

public class FloorManager : NetworkBehaviour
{
    private MeshRenderer floorRenderer;
    private bool active = false;
    public string playerName;
    private Color defaultColor;

    private void Start()
    {
        floorRenderer = GetComponent<MeshRenderer>();
        defaultColor = floorRenderer.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && active)
        {
            var playerRenderer = other.GetComponent<MeshRenderer>();
            var player = other.GetComponent<PlayerManager>().GetPlayerName();
            var playerNet = other.GetComponent<NetworkObject>();

            if (playerRenderer != null || playerNet != null)
            {
                PlayerStepServerRpc(playerRenderer.material.color, player, playerNet.OwnerClientId, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && active)
        {
            var playerRenderer = other.GetComponent<MeshRenderer>();
            var player = other.GetComponent<PlayerManager>().GetPlayerName();
            var playerNet = other.GetComponent<NetworkObject>();
            if (playerRenderer != null || playerNet != null)
            {
                PlayerStepServerRpc(playerRenderer.material.color, player, playerNet.OwnerClientId, false);
            }
        }
    } 

    public void ActivateFloor()
    {
        active = true;
    }

    public void DeActivateFloor()
    {
        active = false;
    }

    public void ResetColor()
    {
        floorRenderer.material.color = defaultColor;
        playerName = "";
    }

    //TODO: All entities should activate this function
    [ClientRpc]
    void ColorFloorClientRpc(Color color, string player)
    {
        //set floor color to the color of the player
        floorRenderer.material.color = color;
        playerName = player;
        //set playerName to the player that touched the floor
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerStepServerRpc(Color color, string player, ulong clientId, bool isEntering)
    {
        if (isEntering)
        {
            if (floorRenderer.material.color == color)
            {
                var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
                if (playerObj != null)
                {
                    var playerManager = playerObj.GetComponent<PlayerManager>();
                    if (playerManager != null)
                    {
                        var rpcParams = new ClientRpcParams
                        {
                            Send = new ClientRpcSendParams
                            {
                                TargetClientIds = new ulong[] { clientId }
                            }
                        };
                        playerManager.ApplySpeedClientRpc(true, rpcParams);
                    }
                }
            }
            else
            {
                ColorFloorClientRpc(color, player);
            }
        }
        else
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObj != null)
            {
                var playerManager = playerObj.GetComponent<PlayerManager>();
                if (playerManager != null)
                {
                    var rpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { clientId }
                        }
                    };
                    playerManager.ApplySpeedClientRpc(false, rpcParams);
                }
            }
        }
    }
}
