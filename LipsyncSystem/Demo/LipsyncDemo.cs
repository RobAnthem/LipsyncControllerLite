using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LipsyncDemo : MonoBehaviour
{
	public LipsyncController lipsyncController;
	public AudioClip[] audioClips;
	private int clip;
	public void PlayNextClip()
	{
		if (clip + 1 < audioClips.Length)
		{
			clip++;
		}
		else
		{
			clip = 0;
		}
		lipsyncController.PlaySound(audioClips[clip]);
	}
}
