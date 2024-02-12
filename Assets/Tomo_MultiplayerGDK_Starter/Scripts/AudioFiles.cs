using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Containers/Audio Files")]
public class AudioFiles : ScriptableObject
{
	[SerializeField]
	private AudioClip[] audioFiles;

	public AudioClip GetAudioFileForSlide(int slideNo)
	{
		if (slideNo - 1 < 0 || slideNo - 1 > audioFiles.Length - 1)
			return null;

		return audioFiles[slideNo - 1];
	}
	
}
