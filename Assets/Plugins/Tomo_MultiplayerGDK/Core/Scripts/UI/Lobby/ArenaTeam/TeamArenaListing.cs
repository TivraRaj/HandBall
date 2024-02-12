using UnityEngine;

public class TeamArenaListing : MonoBehaviour
{
    [Header("Arena UI")]
    [SerializeField] GameObject teamListingHolder;
    public TeamPlayerListing[] teamPlayerListings_Red;
    public TeamPlayerListing[] teamPlayerListings_Blue;

    public void UpdateTeamListingHolder(bool isActive)
    {
        teamListingHolder.SetActive(isActive);
    }

    public void UpdateTeamPlayerListingObjects(int activeRedPlayers, int activeBluePlayers)
    {
        for (int i = 0; i < teamPlayerListings_Red.Length; i++)
        {
            teamPlayerListings_Red[i].UpdateListingObject(i < activeRedPlayers);
        }

        for (int i = 0; i < teamPlayerListings_Blue.Length; i++)
        {
            teamPlayerListings_Blue[i].UpdateListingObject(i < activeBluePlayers);
        }
    }

    public void TeamPlayerListingsInit()
    {
        for (int i = 0; i < teamPlayerListings_Red.Length; i++)
        {
            teamPlayerListings_Red[i].Init();
        }

        for (int i = 0; i < teamPlayerListings_Blue.Length; i++)
        {
            teamPlayerListings_Blue[i].Init();
        }
    }

}
