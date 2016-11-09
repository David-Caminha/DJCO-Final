using UnityEngine;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class PlayerLobbyHook : LobbyHook
{
	public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        gamePlayer.GetComponent<PlayerStats>().team = lobbyPlayer.tag;
    }
}
