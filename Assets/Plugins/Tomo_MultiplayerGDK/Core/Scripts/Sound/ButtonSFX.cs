using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Button))]
public class ButtonSFX : SFX
{
	private Button button;
	[SerializeField] private bool clickSound = true;

	private void Awake()
	{
		button = GetComponent<Button>();
	}
	private void OnEnable()
	{
		button.onClick.AddListener(PlaySFX);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(PlaySFX);
	}

	protected override void PlaySFX()
	{
		if(clickSound) SoundMessages.PlayClickSFX?.Invoke();
		else base.PlaySFX();

	}
}
