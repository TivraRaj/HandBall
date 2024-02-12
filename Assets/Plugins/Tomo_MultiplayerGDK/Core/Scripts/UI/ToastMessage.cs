using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using TomoClub.Util;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI screenLogTextElement;
    [SerializeField] float toastAnimTime = 0.65f;
    [SerializeField] float toastStayTime = 2;
    
    private RectTransform rectTransform;
    private float movementFactor;
    private float safety = 10f;

    private Queue<string> messegeQueue = new Queue<string>();

    private Timer toastTimer;
    private bool toastActivated = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        movementFactor = rectTransform.rect.height + safety;

        //Initially dont show the toast message
        ResetRect();

        toastTimer = new Timer(toastStayTime);
    }

    private void OnEnable()
    {
        toastTimer.OnTimerComplete += ShowNextMessage;
    }

    private void OnDisable()
    {
        toastTimer.OnTimerComplete -= ShowNextMessage;
    }

    private void Update()
    {
        toastTimer.TickTimer();
    }

    private void ResetRect()
    {
        rectTransform.position = new Vector3(rectTransform.position.x, -movementFactor, rectTransform.position.y);
    }

    public void ShowToastMessage(string text)
    {
        messegeQueue.Enqueue(text);

        if (!toastActivated)
        {
            toastActivated = true;
            screenLogTextElement.text = messegeQueue.Peek();

            rectTransform.DOAnchorPosY(0, toastAnimTime).SetEase(Ease.OutCubic).OnComplete(()=> toastTimer.RestartTimer());
        }

    }

    public void ShowNextMessage()
    {
        messegeQueue.Dequeue();

        if(messegeQueue.Count > 0)
        {
            if(messegeQueue.Count > 1)
            {
                for (int i = 0; i < messegeQueue.Count - 1; i++)
                {
                    messegeQueue.Dequeue();
                }

            }

            screenLogTextElement.text = messegeQueue.Peek();
            toastTimer.RestartTimer();
            
        }
        else
        {
            toastActivated = false;
            rectTransform.DOAnchorPosY(-movementFactor, toastAnimTime).SetEase(Ease.OutCubic);
        }

    }

}
