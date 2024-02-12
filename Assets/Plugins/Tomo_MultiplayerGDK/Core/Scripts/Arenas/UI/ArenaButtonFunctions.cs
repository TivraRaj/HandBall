using System;
using UnityEngine;
using Photon.Pun;
using TomoClub.Core;

public class ArenaButtonFunctions : MonoBehaviour
{
    public Action OnPlayPauseAll;
    public Action<int> OnPlayPauseArena;
    public Action<int> OnSetCurrentArenaLookAt;
    public Action OnSpectate;

    public void SpectateGame() => OnSpectate?.Invoke();

    public void PlayPauseAllArenas() => OnPlayPauseAll?.Invoke();

    public void PlayPauseArena(int arenaNo) => OnPlayPauseArena?.Invoke(arenaNo);

    public void SetCurrentArenaToLookAt(int arenaNo) => OnSetCurrentArenaLookAt?.Invoke(arenaNo);

    public void InGameSettingsButton()
    {
        if (PersistantUI.Instance != null)
            PersistantUI.Instance.ShowSettingsPopup();
    }

    public void ExitToLobby()
    {
        MultiplayerManager.Instance.ResetRoom();
    }

    public void RestartRound()
    {
        PhotonNetwork.LoadLevel(Constants.TempScene);
    }
}
