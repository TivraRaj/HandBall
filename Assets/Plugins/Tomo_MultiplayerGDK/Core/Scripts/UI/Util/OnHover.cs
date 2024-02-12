
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class OnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("On Hover Events")]
    [SerializeField] UnityEvent pointerEntryEvent;
    [SerializeField] UnityEvent pointerLeaveEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {        
        pointerEntryEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        pointerLeaveEvent?.Invoke();
    }

}
