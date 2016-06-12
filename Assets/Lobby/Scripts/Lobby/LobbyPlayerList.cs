using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Prototype.NetworkLobby
{
    //List of players in the lobby
    public class LobbyPlayerList : MonoBehaviour
    {
        public static LobbyPlayerList _instance = null;

        public RectTransform team1PlayerListContentTransform;
        public RectTransform team2PlayerListContentTransform;
        public GameObject warningDirectPlayServer;

        protected VerticalLayoutGroup _team1Layout;
        protected VerticalLayoutGroup _team2Layout;
        protected List<LobbyPlayer> _team1Players = new List<LobbyPlayer>();
        protected List<LobbyPlayer> _team2Players = new List<LobbyPlayer>();

        public void OnEnable()
        {
            _instance = this;
            _team1Layout = team1PlayerListContentTransform.GetComponent<VerticalLayoutGroup>();
            _team2Layout = team2PlayerListContentTransform.GetComponent<VerticalLayoutGroup>();
        }

        public void DisplayDirectServerWarning(bool enabled)
        {
            if(warningDirectPlayServer != null)
                warningDirectPlayServer.SetActive(enabled);
        }

        void Update()
        {
            //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
            //sometime to child being assigned before layout was enabled/init, leading to broken layouting)

            if (_team1Layout)
                _team1Layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
            if (_team2Layout)
                _team2Layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }

        public void AddPlayer(LobbyPlayer player)
        {
            if(_team1Players.Count <= _team2Players.Count)
            {
                if (_team1Players.Contains(player))
                    return;

                _team1Players.Add(player);

                player.tag = "Team1";

                player.transform.SetParent(team1PlayerListContentTransform, false);
            }
            else
            {
                if (_team2Players.Contains(player))
                    return;

                _team2Players.Add(player);

                player.tag = "Team2";

                player.transform.SetParent(team2PlayerListContentTransform, false);
            }

            PlayerListModified();
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            if (_team1Players.Contains(player))
            {
                _team1Players.Remove(player);
                PlayerListModified();
            }
            else if (_team2Players.Contains(player))
            {
                _team2Players.Remove(player);
                PlayerListModified();
            }
        }

        public void PlayerListModified()
        {
            int i = 0;
            foreach (LobbyPlayer p in _team1Players)
            {
                p.OnPlayerListChanged(i);
                ++i;
            }
            foreach (LobbyPlayer p in _team2Players)
            {
                p.OnPlayerListChanged(i);
                ++i;
            }
        }
    }
}
