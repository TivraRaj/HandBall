using UnityEngine;
using TomoClub.Core;
using UnityEngine.SceneManagement;

public class PersistantUI : MonoBehaviour
{
    public static PersistantUI Instance;

    [Header("ToastMessage")]
    [SerializeField] private bool enableToastMessage = true;
    [SerializeField] private bool showMessageInEditor = true;
    [SerializeField] private ToastMessage toastMessage;

    [Header("Settings")]
    [SerializeField] GameObject Popup_Settings;

    [Header("Splash Screen")]
    [SerializeField] GameObject splashScreen;

    [Header("Disconnection Handling")]
    [SerializeField] GameObject Popup_Disconnection;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

	private void Start()
	{
        splashScreen.SetActive(false);
    }

	private void OnEnable()
    {
        UtilEvents.ShowToastMessage += LogOnScreen;
    }

    public void OnDisable()
    {
        UtilEvents.ShowToastMessage -= LogOnScreen;
    }


    public void ShowDisconnectionPopup()
    {
        Popup_Disconnection.SetActive(true);
    }

    public void GoBackToMainMenu()
    {
        Popup_Disconnection.SetActive(false);
    }



    private void LogOnScreen(string logText)
    {
        if (enableToastMessage) toastMessage.ShowToastMessage(logText);

        if (showMessageInEditor) Debug.Log(logText);

    }
    public void UpdateSplashScreen(bool active) => splashScreen.SetActive(active);


    #region SETTINGS MENU

    public void ShowSettingsPopup()
    {
        Popup_Settings.SetActive(true);
    }

    public void HideSettingsPopup()
    {
        Popup_Settings.SetActive(false);
    }

    #endregion

}
