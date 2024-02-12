using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Random = System.Random;
using TomoClub.Core;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	[Header("Server")]
	[SerializeField] GameObject Server;

	[Header("Main Menu - Updations")]
	[SerializeField] TextMeshProUGUI gameNameText;
	[SerializeField] TextMeshProUGUI dateText;
	[SerializeField] TextMeshProUGUI playerNameText;
	[SerializeField] TextMeshProUGUI regionNameText;
	[SerializeField] TextMeshProUGUI pingText;

	[Header("Connect To Game")]
	[SerializeField] GameObject Popup_ConnectToGame;
	[SerializeField] GameObject ConnectToGame_CloseButton;
	[SerializeField] TextMeshProUGUI buildTypeText;
	[SerializeField] TextMeshProUGUI regionText;
	[SerializeField] GameObject buildTypeButtonHolder;
	[SerializeField] GameObject regionButtonHolder;

	[Header("Build Data")]
	[SerializeField] TextMeshProUGUI buildText;

	[Header("Player-Name Input")]
	[SerializeField] GameObject Popup_Username;
	[SerializeField] TMP_InputField Username_InputField;
	[SerializeField] TextMeshProUGUI Username_ErrorText;
	[SerializeField] GameObject Username_CloseButton; 

	[Header("Create_Room")]
	[SerializeField] Button Button_CreateRoom;
	[SerializeField] GameObject Popup_CreateRoom;

	[Header("Join_Room")]
	[Tooltip("Sets the type of join room UI: list of available rooms vs entering the room id")]
	[SerializeField] private bool showRoomList;
	[SerializeField] Button Button_JoinRoom;
	[SerializeField] GameObject Popup_JoinRoom_List;
	[SerializeField] GameObject Popup_JoinRoom_Input;

	[Header("Audio")]
	[SerializeField] private AudioClip errorAudioClip;

	public enum Builds {Player, Moderator, Spectator, Mod_Player };
	private List<string> buildOptions = new List<string>();
	private string currentOption;
	private Builds currentBuild;

	private Regions currentRegion;
	private string currentServerRegion;

	private string ServerRegion
	{
		get => PlayerPrefs.GetString(Constants.Game.ServerRegion, "Best");
		set => PlayerPrefs.SetString(Constants.Game.ServerRegion, value);
	}

	private void Awake()
	{
		if (MultiplayerManager.Instance == null)
		{
			Instantiate(Server);
		}
	}

	private void Start()
	{
		MainMenu_Init();
		UpdateMainMenuData();

		if (SessionData.previousGameState == GameStates.MainMenu)
		{
			LoginToGame();
		}
		else
		{

			LocalPlayer.Instance.ResetLocalPlayerData();
			LocalPlayer.Instance.StartKickOutTimer();

			//MakeLobbyButtonsInteractable();
		}

		ServerMesseges.OnDissconnectedFromPhoton += OnDisconnection;
		ServerMesseges.OnConnectedToPhoton += OnConnection;
		UtilEvents.OnKickOutOver += MakeLobbyButtonsInteractable;

		MultiplayerMesseges.OnUpdatePing += UpdatePingText;
	}

	private void OnDestroy()
	{
		ServerMesseges.OnDissconnectedFromPhoton -= OnDisconnection;
		ServerMesseges.OnConnectedToPhoton -= OnConnection;
		UtilEvents.OnKickOutOver -= MakeLobbyButtonsInteractable;

		MultiplayerMesseges.OnUpdatePing -= UpdatePingText;
	}

	private void OnConnection()
	{
		MakeLobbyButtonsInteractable();
		UpdateGameServerText(true);
	}

	private void OnDisconnection()
	{
		MakeLobbyButtonsInteractable();
		UpdateGameServerText(false);
	}

	private void UpdatePingText(int ping)
	{
		var str = ping > 250 ? "\"red\"" : "\"black\"";
		pingText.text = $"Ping:<color={str}> {ping}ms</color>";
	}

	private void UpdateGameServerText(bool update)
	{
		regionNameText.text = update ? $"Region: {ServerRegion}" : "Region:";
		if (update && currentRegion == Regions.Best ) regionNameText.text = $"Region: {PhotonNetwork.CloudRegion}";

		int ping = PhotonNetwork.GetPing();
		var str = ping > 250 ? "\"red\"" : "\"black\"";
		pingText.text = !update ? "Ping: " : $"Ping: <color={str}>{ping}ms</color>";
		

	}

	private void MakeLobbyButtonsInteractable()
	{
		bool canInteract = !LocalPlayer.Instance.timedOut && SessionData.connectionEstablished;

		Button_CreateRoom.gameObject.SetActive(LocalPlayer.Instance.isRoomOwner);
		Button_JoinRoom.gameObject.SetActive(!LocalPlayer.Instance.isRoomOwner);

		Button_CreateRoom.interactable = canInteract;
		Button_JoinRoom.interactable = canInteract;

		Button_CreateRoom.GetComponent<CanvasGroup>().alpha = canInteract ? 1f : 0.1f;
		Button_JoinRoom.GetComponent<CanvasGroup>().alpha = canInteract ? 1f : 0.1f;
	}


	//Set username if it exists else popup username panel
	private void LoginToGame()
	{
		if (string.IsNullOrEmpty(LocalPlayer.Instance.playerName)) Popup_Username.SetActive(true);
		else
		{
			Popup_ConnectToGame.SetActive(true);
			OnUpdateUsername();
		}

	}

	//Initial main menu data updates { date, game name, player name }
	private void UpdateMainMenuData()
	{
		//Update Game Name
		gameNameText.text = SessionData.GameName;

		//Update Current Date
		DateTime currentDate = DateTime.Today;
		dateText.text = currentDate.ToShortDateString();

		//Update User Name        
		playerNameText.text = $"Hey {LocalPlayer.Instance.playerName} !";

	}

	//Initializatoins for the main menu
	private void MainMenu_Init()
	{
		bool isRoomOwner = LocalPlayer.Instance != null && LocalPlayer.Instance.isRoomOwner;

		Button_CreateRoom.gameObject.SetActive(isRoomOwner);
		Button_JoinRoom.gameObject.SetActive(!isRoomOwner);

		Button_CreateRoom.interactable = false;
		Button_JoinRoom.interactable = false;

		Button_CreateRoom.GetComponent<CanvasGroup>().alpha = 0.1f;
		Button_JoinRoom.GetComponent<CanvasGroup>().alpha = 0.1f;

		Username_ErrorText.text = "";

		ConnectToGame_CloseButton.SetActive(false);
		Username_CloseButton.SetActive(false);

		if (SessionData.IsRegionLocked)
			currentRegion = SessionData.BuildRegion;
		else
			Enum.TryParse(ServerRegion, out currentRegion);

		regionButtonHolder.SetActive(!SessionData.IsRegionLocked);
		regionText.text = currentRegion.ToString();

		SetBuildType();

		regionNameText.text = "Region:";
		pingText.text = "Ping:";
	}

	private void SetBuildType()
    {
		buildText.text = $"Build: {SessionData.BuildType}";

        switch (SessionData.BuildType)
        {
			case BuildType.Moderator:
				buildOptions.Add(Builds.Moderator.ToString());
				buildOptions.Add(Builds.Mod_Player.ToString());
				break;

			case BuildType.Player:
				buildOptions.Add(Builds.Player.ToString());
				break;

			case BuildType.Spectator:
				buildOptions.Add(Builds.Spectator.ToString());
				break;

			case BuildType.Common:
                for (int i = 0; i < Enum.GetValues(typeof(Builds)).Length; i++)
                {
					Builds build = (Builds)i;
					buildOptions.Add(build.ToString());
                }
				break;

			default:
                break;
        }

		currentOption = buildOptions[0];
		buildTypeText.text = currentOption;

		buildTypeButtonHolder.SetActive(buildOptions.Count > 1);
	}

	//On click confirm username button
	public void ConfirmUsername()
	{
		Username_ErrorText.text = "";

		if (string.IsNullOrEmpty(Username_InputField.text))
		{
			SoundMessages.PlaySFX?.Invoke(errorAudioClip);
			Username_ErrorText.text = "Please enter a username!";
		}
		else
		{
			LocalPlayer.Instance.playerName = Username_InputField.text;
			OnUpdateUsername();
			//Establish Connection 
			Popup_Username.SetActive(false);

			if(!SessionData.ForceReconnect)
			{
				Popup_ConnectToGame.SetActive(true);
			}
		}

	}

	private void OnUpdateUsername()
	{
		//Set Player Name UI 
		playerNameText.text = $"Hey { LocalPlayer.Instance.playerName } !";
		PhotonNetwork.NickName = LocalPlayer.Instance.playerName;
		//Authenticate Player
		AuthenticatePlayer();

	}

	private void AuthenticatePlayer()
	{
		
		switch (SessionData.AuthenticationType)
		{
			case AuthenticationType.Device_Based:
				if (!string.IsNullOrEmpty(LocalPlayer.Instance.userID)) return;

				LocalPlayer.Instance.userID = GenerateRandomUniqueUserKey();
				PhotonNetwork.AuthValues = new AuthenticationValues(LocalPlayer.Instance.userID);
				break;
			case AuthenticationType.Name_Based:
				PhotonNetwork.AuthValues = new AuthenticationValues(LocalPlayer.Instance.playerName);
				if (SessionData.ForceReconnect) PhotonNetwork.Disconnect();
				break;
			default:
				Debug.LogError("Not possible, Something went terribly wrong");
				break;
		}
	}

	private string GenerateRandomUniqueUserKey()
	{
		Random res = new Random();

		// String that contain both alphabets and numbers
		string str = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		int size = 8;

		// Initializing the empty string
		string randString = "";

		for (int i = 0; i < size; i++)
		{

			// Selecting a index randomly
			int x = res.Next(str.Length);

			// Appending the character at the 
			// index to the random alphanumeric string.
			randString = randString + str[x];
		}

		return randString;
	}


	//On click open/close create room popup
	public void MainMenu_CreateRoom(bool action)
	{
		if (!SessionData.connectionEstablished || LocalPlayer.Instance.timedOut)
		{
			//Error sound 
			return;
		}
		Popup_CreateRoom.SetActive(action);
	}

	//On click open/close join room popup
	public void MainMenu_JoinRoom(bool action)
	{
		if (!SessionData.connectionEstablished || LocalPlayer.Instance.timedOut)
		{
			//Error sound 
			return;
		}

		Popup_JoinRoom_List.SetActive(action && showRoomList);
		Popup_JoinRoom_Input.SetActive(action && !showRoomList);
	}

	public void RedirectToWebsite()
	{
		Application.OpenURL("https://tomoclub.org/");
	}

	public void ToggleUsernamePopup(bool toggle)
	{
		if (toggle) Username_CloseButton.SetActive(true);
		SessionData.ForceReconnect = toggle;
		Popup_Username.SetActive(toggle);
	}

	public void OpenSettings()
	{
		PersistantUI.Instance.ShowSettingsPopup();
	}

	#region Build Type Selection

	public void SwitchBuildType_Left()
	{
        //currentBuild = (int)currentBuild - 1 < 0 ? (Builds)(Enum.GetNames(typeof(Builds)).Length - 1) : (Builds)((int)currentBuild - 1);
        //buildTypeText.text = currentBuild.ToString();

        int currentOptionIndex = buildOptions.IndexOf(currentOption);
        currentOption = buildOptions[currentOptionIndex - 1 < 0 ? buildOptions.Count - 1 : currentOptionIndex - 1];
		buildTypeText.text = currentOption;

	}

	public void SwitchBuildType_Right()
	{
        //currentBuild = (int)currentBuild + 1 > (Enum.GetNames(typeof(Builds)).Length - 1) ? 0 : (Builds)((int)currentBuild + 1);
        //buildTypeText.text = currentBuild.ToString();

        int currentOptionIndex = buildOptions.IndexOf(currentOption);
        currentOption = buildOptions[currentOptionIndex + 1 > buildOptions.Count - 1 ? 0 : currentOptionIndex + 1];
		buildTypeText.text = currentOption;
	}

	private void Select_ModBuild()
	{
		LocalPlayer.Instance.arenaNo = -1;
		Button_JoinRoom.gameObject.SetActive(false);
		Button_CreateRoom.gameObject.SetActive(true);
	}

	private void Select_PlayerBuild()
	{
		LocalPlayer.Instance.arenaNo = -1;
		Button_CreateRoom.gameObject.SetActive(false);
		Button_JoinRoom.gameObject.SetActive(true);
	}

	private void Select_SpecBuild()
	{
		LocalPlayer.Instance.arenaNo = 0;
		Button_CreateRoom.gameObject.SetActive(false);
		Button_JoinRoom.gameObject.SetActive(true);
	}

	private void SetBuild()
	{
		currentBuild = (Builds)Enum.Parse(typeof(Builds), currentOption);
		SessionData.Build = currentBuild;

		switch (currentBuild)
		{
			case Builds.Moderator: 
				Select_ModBuild();
				LocalPlayer.Instance.defaultPlayerType = PlayerType.Master_Spectator;
				break;
			case Builds.Player:
				Select_PlayerBuild();
				LocalPlayer.Instance.defaultPlayerType = PlayerType.Player;
				break;
			case Builds.Spectator: 
				Select_SpecBuild();
				LocalPlayer.Instance.defaultPlayerType = PlayerType.Spectator;
				break;
			case Builds.Mod_Player:
				Select_ModBuild();
				LocalPlayer.Instance.defaultPlayerType = PlayerType.Master_Player;
				break;
			default:
				break;
		}


		
	}

	#endregion

	#region Region Selection

	public void SwitchRegionType_Left()
	{
		currentRegion = (int)currentRegion - 1 < 0 ? (Regions)(Enum.GetNames(typeof(Regions)).Length - 1) : (Regions)((int)currentRegion - 1);
		regionText.text = currentRegion.ToString();
	}

	public void SwitchRegionType_Right()
	{
		currentRegion = (int)currentRegion + 1 > (Enum.GetNames(typeof(Regions)).Length - 1) ? 0 : (Regions)((int)currentRegion + 1);
		regionText.text = currentRegion.ToString();
	}

	private void SetRegion()
	{
		switch (currentRegion)
		{
			case Regions.Best:
				currentServerRegion = "";
				break;
			case Regions.US_West:
				currentServerRegion = "usw";
				break;
			case Regions.US_East:
				currentServerRegion = "us";
				break;
			case Regions.India:
				currentServerRegion = "in";
				break;
			case Regions.Europe:
				currentServerRegion = "eu";
				break;
			case Regions.Turkey:
				currentServerRegion = "tr";
				break;
			case Regions.Singapore:
				currentServerRegion = "asia";
				break;
			case Regions.Australia:
				currentServerRegion = "au";
				break;
			case Regions.Japan:
				currentServerRegion = "jp";
				break;
			case Regions.South_Korea:
				currentServerRegion = "kr";
				break;
			case Regions.Canada:
				currentServerRegion = "cae";
				break;
			case Regions.South_Africa:
				currentServerRegion = "za";
				break;
			case Regions.Brazil:
				currentServerRegion = "sa";
				break;
			default:
				break;
		}

		ServerRegion = currentRegion.ToString();
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = currentServerRegion;
	}

	#endregion


	public void ConnectToGame()
	{
		SetBuild();
		SetRegion();
		Popup_ConnectToGame.SetActive(false);

		if(SessionData.ForceReconnect)
        {
			if (SessionData.connectionEstablished) PhotonNetwork.Disconnect();
			else
			{
				SessionData.ForceReconnect = false;
				ServerMesseges.EstablishConnectionToServer?.Invoke();
			}
		}
		else
			ServerMesseges.EstablishConnectionToServer?.Invoke();

	}
		
	public void ToggleConnectToGame(bool status)
	{
		ConnectToGame_CloseButton.SetActive(true);
		SessionData.ForceReconnect = status;
		Popup_ConnectToGame.SetActive(status);
	}
} 

