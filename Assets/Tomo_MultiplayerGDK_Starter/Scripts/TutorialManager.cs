using UnityEngine.UI;
using UnityEngine;
using TomoClub.Util;
using TomoClub.Core;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [Tooltip("Time after which the tutorial automatically shifts")]
    [SerializeField] int transitionTime;
    [SerializeField] AudioClip confirmationSound;

    [Header("Tutorial UI")]
    [SerializeField] Image Image_Tutorial;
    [SerializeField] Sprite[] Sprites_Tutorial;
    [SerializeField] GameObject[] GO_TutorialPageNavig;
    [SerializeField] GameObject[] GO_Tutorial;
    [SerializeField] Button tutorialConfirmation;

    private int currentTutorialPage = 0;

    private bool hasDoneTutorial;
    private int tutorialsSeen = 1;

    private void Awake()
    {
        //Player side initializations 
        Tutorial_Init();
    }

    private void Start()
    {
        //tutorialTimer.StartTimer();
        hasDoneTutorial = HasSeenTutorialBefore();
        if (!hasDoneTutorial) PlayerPrefs.SetInt(Constants.Player.TutorialCompleted, 1);
        tutorialConfirmation.interactable = false;
        tutorialConfirmation.gameObject.SetActive(!hasDoneTutorial);
        
    }

    private bool HasSeenTutorialBefore()
	{
        return PlayerPrefs.GetInt(Constants.Player.TutorialCompleted, 0) == 1;
	}

    private void Update()
    {

    }


    private void Tutorial_Init()
    {
        //Image_Tutorial.sprite = Sprites_Tutorial[0];
        GO_Tutorial[0].SetActive(true);

        for (int i = 1; i < GO_Tutorial.Length; i++)
        {
            GO_Tutorial[i].SetActive(false);
        }


        //Init setup for the tutorial menu
        for (int i = 0; i < GO_TutorialPageNavig.Length; i++)
        {
            if (i < GO_Tutorial.Length)
            {
                GO_TutorialPageNavig[i].SetActive(true);
            }

            else
            {
                GO_TutorialPageNavig[i].SetActive(false);
                
            }
        }

    }


    #region TUTORIAL UI

    //Update tutorial to previous
    public void Tutorial_PrevButton()
    {
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(false);
        GO_Tutorial[currentTutorialPage].gameObject.SetActive(false);   

        currentTutorialPage--;
        if (currentTutorialPage == -1) currentTutorialPage = 0;

        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(true);
        GO_Tutorial[currentTutorialPage].gameObject.SetActive(true);

    }

    //Update tutorial to next
    public void Tutorial_NextButton()
    {
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(false);
        GO_Tutorial[currentTutorialPage].gameObject.SetActive(false);


        currentTutorialPage++;
        tutorialsSeen++;
        if(tutorialsSeen == GO_Tutorial.Length && !hasDoneTutorial)
		{
            hasDoneTutorial = true;
            tutorialConfirmation.interactable = true;
            tutorialConfirmation.transform.DOScale(Vector3.one * 1.2f, 1.2f).SetLoops(-1);
		}

        if (currentTutorialPage == GO_Tutorial.Length) currentTutorialPage = 0;

        //Image_Tutorial.sprite = Sprites_Tutorial[currentTutorialPage];
        GO_TutorialPageNavig[currentTutorialPage].transform.GetChild(0).gameObject.SetActive(true);
        GO_Tutorial[currentTutorialPage].gameObject.SetActive(true);
    }

    public void TutorialButtonClicked()
	{
        SoundMessages.PlaySFX?.Invoke(confirmationSound);
        tutorialConfirmation.gameObject.SetActive(false);
	}

    #endregion
}
