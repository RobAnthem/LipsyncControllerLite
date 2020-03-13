using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LipsyncTemplate : ScriptableObject
{
	public LipsyncController.SyncType syncType;
	public FFTWindow analysisType = FFTWindow.Rectangular;
	public LipsyncController.Axis mouthAxis = LipsyncController.Axis.X;
	public LipsyncController.Axis mouthRotAxis = LipsyncController.Axis.Z;
	public float volume = 0.25f;
	public float frqLow = 200;
	public float frqHigh = 800;
	public float rotVolume = 0.25f;
	public bool ignoreDistance = true;
	public float minDistance = 5.0f, maxDistance = 30.0f;
	public float volByDistance = 2.0f;
	public float minVol = .25f, maxVol = 10.0f;
}
