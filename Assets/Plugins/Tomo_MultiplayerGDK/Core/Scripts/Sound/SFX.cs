using UnityEngine;

public class SFX : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;

	protected virtual void PlaySFX()
	{
		if (audioClip == null)
		{
			Debug.LogError("Give an audio clip to play.");
			return;
		}
		SoundMessages.PlaySFX?.Invoke(audioClip);
	}



}
