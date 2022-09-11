using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    private bool isGameInProgress = false;

    //Lista dei player, mi serve per avviare la partita
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    //Eventi che chiamiamo per fare le nostre cose quando il client si connette o disconnette
    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    #region Server

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        //Se il game è in corso e joina qualcuno, questo viene kickato
        if (!isGameInProgress) { return; }
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        //Se il server disconnette qualcuno, prendo l'RTSPlayer e lo rimuovo dalla lista di player
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        //Quando stoppo il serve pulisco la lista dei player e il game non è più in progress
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        //Mi servono almeno due player per iniziare la partita
        if (Players.Count < 2) { return; }

        isGameInProgress = true;

        //Di qua carico la scena di gioco. Poichè ho una lobby, non lo faccio più mettendo la online scene da editor,
        //perchè voglio prima aspettare di riempire la lobby appunto
        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        //Assegno un colore randomico al player
        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));

        //Setto il palyer come owner della lobby solo se il numero di player nella lobby è uguale a 1
        player.SetPartyOwner(Players.Count == 1);
    }

    /// <summary>
    /// Callback che viene chiamata subito dopo che una scena è stata caricata
    /// </summary>
    /// <param name="sceneName"></param>
    public override void OnServerSceneChanged(string sceneName)
    {
        //Spawno il gameoverhandler e le basi dei player solo se mi trovo in un livello e non nel menu
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);

                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion




}
