using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamPlayerListing : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image playerReadyStatus;
    [SerializeField] GameObject listingObject;


    public void Init()
    {
        UpdateListingReadyStatus(false);
        UpdateListingObject(false); 
    }

    public void UpdateListing(string name)
    {
        playerName.text = name;
    }

    public void UpdateListingObject(bool selfActive) => listingObject.SetActive(selfActive);  

    public void UpdateListingReadyStatus(bool isReady)
    {     
        playerReadyStatus.sprite = isReady? ArenaTeamUI.playerReadyUpSprites[1] : ArenaTeamUI.playerReadyUpSprites[0];
    }
}

