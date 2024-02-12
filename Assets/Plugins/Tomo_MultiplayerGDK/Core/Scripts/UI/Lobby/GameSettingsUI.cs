using TMPro;
using UnityEngine.UI;
using UnityEngine;
using TomoClub.Core;

public class GameSettingsUI : MonoBehaviour
{

    [Header("Game Settings UI")]
    [SerializeField] GameObject Popup_GameSettings;
    [SerializeField] GameObject[] Tabs_GameSettings;
    [SerializeField] GameObject[] Panels_GameSettins;

    [Header("Game Timer Base Data")]
    [SerializeField] int minimumGameTime = 1;
    [SerializeField] int maximumGameTime = 20;

    [Header("Session Timer UI")]
    [SerializeField] Slider gameTimeSlider;
    [SerializeField] TextMeshProUGUI gameTimeText;
    [SerializeField] TextMeshProUGUI roundStatusText;

    private void Awake()
    {
        GameSettings_Init();
    }

    private void GameSettings_Init()
    {
        Popup_GameSettings.SetActive(false);

        //Round status init
        roundStatusText.text = "";

        //Game timer init
        gameTimeSlider.minValue = minimumGameTime;
        gameTimeSlider.maxValue = maximumGameTime;
        gameTimeSlider.wholeNumbers = true;
        gameTimeSlider.value = MultiplayerManager.Instance.GameSessionTime / 60; //Convert to mins
        gameTimeText.text = $"{gameTimeSlider.value} mins";

    }

    //Open/Close game settings menu and save settings to player pref while closing
    public void UpdateGameSettingsUI(bool status)
    {
        Popup_GameSettings.SetActive(status);
        if (!status)
        {
            //If the game session time has been changed sync across the server
            int currentGameSessionTime = (int)gameTimeSlider.value * 60;
            if (currentGameSessionTime != MultiplayerManager.Instance.GameSessionTime)
            {
                NetworkEvents.Instance.SyncGameSettingsOnNetwork(currentGameSessionTime);
            }
        }
    }

    //Switch Tabs Based on tabNo in game settings menu
    public void OnClickMenu(int tabNo)
    {
        //Turn off current panel
        Panels_GameSettins[1 - tabNo].SetActive(false);
        Tabs_GameSettings[1 - tabNo].transform.GetChild(0).gameObject.SetActive(false);
        Tabs_GameSettings[1 - tabNo].transform.GetChild(1).gameObject.SetActive(true);

        //Turn on hit panel
        Panels_GameSettins[tabNo].SetActive(true);
        Tabs_GameSettings[tabNo].transform.GetChild(0).gameObject.SetActive(true);
        Tabs_GameSettings[tabNo].transform.GetChild(1).gameObject.SetActive(false);

    }

    //Update UI on moving the game time slider
    public void OnChangeGameTime()
    {
        gameTimeText.text = $"{gameTimeSlider.value} mins";
        roundStatusText.text = $"The round time has been changed to {gameTimeText.text}";
    }
}
