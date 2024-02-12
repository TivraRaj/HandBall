using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    public Team team;

    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Outline outline;
    [SerializeField] private Outline outerOutline;
    [SerializeField] private OnHover onHover;
    [SerializeField] private Transform parentGO;

    [Header("Colors")]
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color highlightColor;

    public UnityAction<Team> OnTeamClicked;

    private void Start()
    {
        button.enabled = false;
        onHover.enabled = false;
    }

    public void OnClick()
    {
        OnTeamClicked?.Invoke(team);
    }

    public void Highlight()
    {
        /*
        image.color = Color.green;
        */
        button.enabled = true;
        outline.enabled = true;
        outerOutline.enabled = true;

        onHover.enabled = true;
    }

    public void DisableHighlight()
    {
        /*
        if (team == Team.Red)
            image.color = redColor;
        else if (team == Team.Blue)
            image.color = blueColor;
            */
        button.enabled = false;
        outline.enabled = false;
        outerOutline.enabled = false;

        HoverExit();
        onHover.enabled = false;
    }

    public void HoverEnter()
    {
        parentGO.localScale = new Vector3(1.05f, 1.05f, 1f);
    }

    public void HoverExit()
    {
        parentGO.localScale = new Vector3(1f, 1f, 1f);
    }
}

public enum Team
{
    Red, Blue, None
}
