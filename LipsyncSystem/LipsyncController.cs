using UnityEngine;

public class LipsyncController : MonoBehaviour
{
	public enum SyncType
	{
		Jaw,
		BlendShape
	}
	public SyncType syncType;
	/// <summary>
	/// Type of X, Y, or Z axis
	/// </summary>
	public enum Axis
	{
		X, Y, Z
	}
	/// <summary>
	/// Type of analysis to retrieve spectrum data.
	/// </summary>
	public FFTWindow analysisType = FFTWindow.Rectangular;
	public AudioSource mouthSource;
	/// <summary>
	/// Movement axis for up/down movement of jaw
	/// </summary>
	public Axis mouthAxis = Axis.X;
	/// <summary>
	/// Rotation axis for jaw movement;
	/// </summary>
	public Axis mouthRotAxis = Axis.Z;
	/// <summary>
	/// Jaw Object to lipsync
	/// </summary>
	public GameObject mouth;
	/// <summary>
	/// Weight of jaw movement by audio spectrum
	/// </summary>
	public float volume = 0.25f;
	/// <summary>
	/// Low audio frequency
	/// </summary>
	public float frqLow = 200;
	/// <summary>
	/// High audio frequency
	/// </summary>
	public float frqHigh = 800;
	/// <summary>
	/// Weight of rotation on jaw bone
	/// </summary>
	public float rotVolume = 0.25f;
	/// <summary>
	/// Ignore automatic listener distance and calculate volume to distance
	/// </summary>
	public bool ignoreDistance;
	/// <summary>
	/// only used if Ignore Distance is checked
	/// </summary>
	public float minDistance = 5.0f, maxDistance = 30.0f;
	public float volByDistance = 2.0f;
	/// <summary>
	/// Only used if Ignore Distance is checked
	/// </summary>
	public float minVol = .25f, maxVol = 10.0f;
	/// <summary>
	/// Mouth Shape ID
	/// </summary>
	public int shapeID;
	/// <summary>
	/// Mouth Shape Name
	/// </summary>
	public string shapeName;
	#region Private Fields
	private float[] freqData;
	private float nSamples = 256;
	private float fMax = 24000;
	private AudioSource audio;
	private const float fFloor = 20;
	private float prevSum;
	private float a0;
	private float a1;
	private int sizeFilter = 5;
	private float[] filter;
	private float filterSum;
	private int posFilter = 0;
	private int qSamples = 0;
	private bool resetState;
	private float[] aSamples;
	private const int aSamplesLength = 4096;
	#endregion
	void Start()
	{
		if (!mouthSource)
			mouthSource = GetComponent<AudioSource>(); // get AudioSource component
		if (syncType == SyncType.Jaw)
		{
			if (mouthAxis == Axis.Y)
				a0 = mouth.transform.localPosition.y;
			else if (mouthAxis == Axis.X)
				a0 = mouth.transform.localPosition.x;
			else
				a0 = mouth.transform.localPosition.z;
			if (mouthRotAxis == Axis.Y)
				a1 = mouth.transform.localRotation.y;
			else if (mouthRotAxis == Axis.X)
				a1 = mouth.transform.localRotation.x;
			else
				a1 = mouth.transform.localRotation.z;
		}
		else
		{
			Debug.Log("init blend shapes");
			InitBlendShapes();
		}
		freqData = new float[(int)nSamples];
		mouthSource.Play();
		//aSamples = new float[aSamplesLength];
	}
	void Update()
	{
		if (!mouthSource.isPlaying)
		{
			if (!resetState)
			{
				ResetMouth();
				resetState = true;
			}
			return;
		}
		else
		{
			resetState = false;
		}
		if (syncType == SyncType.BlendShape)
		{
			UpdateBlendShapes();
		}
		else
		{
			UpdateJawBone();
		}
	}
	void ResetMouth()
	{
		if (syncType == SyncType.Jaw)
		{
			Vector3 pos = mouth.transform.localPosition;
			Quaternion rot = mouth.transform.localRotation;
			if (mouthAxis == Axis.Y)
			{
				pos.y = a0;
			}
			else if (mouthAxis == Axis.X)
			{
				pos.x = a0;
			}
			else
			{
				pos.z = a0;
			}
			if (mouthRotAxis == Axis.Y)
			{
				rot.y = a1;
			}
			else if (mouthRotAxis == Axis.X)
			{
				rot.x = a1;
			}
			else
			{
				rot.z = a1;
			}
			mouth.transform.localPosition = pos;
			mouth.transform.localRotation = rot;
		}
	}
	/// <summary>
	/// Play Audio Clip through lipsync system
	/// </summary>
	/// <param name="clip"></param>
	public void PlaySound(AudioClip clip)
	{
		mouthSource.Stop();
		mouthSource.clip = clip;
		mouthSource.Play();
	}
	/// <summary>
	/// Calculate the average spectrum analysis along the audio clip for smoother movement
	/// </summary>
	/// <param name="sample"></param>
	/// <returns></returns>
	float MovingAverage(float sample)
	{

		if (qSamples == 0) filter = new float[sizeFilter];
		filterSum += sample - filter[posFilter];
		filter[posFilter++] = sample;
		if (posFilter > qSamples) qSamples = posFilter;
		posFilter = posFilter % sizeFilter;
		return filterSum / qSamples;
	}
	/// <summary>
	/// Set mouth to base closed state
	/// </summary>
	float CalcSpectrum(float fLow, float fHigh)
	{
		fLow = Mathf.Clamp(fLow, fFloor, fMax);
		fHigh = Mathf.Clamp(fHigh, fLow, fMax);
		mouthSource.GetSpectrumData(freqData, 1, analysisType);
		int n1 = (int)Mathf.Floor(fLow * nSamples / fMax);
		int n2 = (int)Mathf.Floor(fHigh * nSamples / fMax);
		float sum = 0;
		for (int i = n1; i < freqData.Length; i++)
		{
			sum += freqData[i];
		}
		return sum / (n2 - n1 + 1);
	}
	/// <summary>
	/// **TEST NOT IMPLEMENTED**
	/// Calculate audio volume from source for distance calculations
	/// </summary>
	/// <param name="channel"></param>
	/// <returns></returns>
	float GetRMS(int channel)
	{
		mouthSource.GetOutputData(aSamples, channel);
		float sum = 0;
		foreach (float f in aSamples)
		{
			sum += f * f;
		}
		return Mathf.Sqrt(sum / aSamplesLength);
	}
	/// <summary>
	/// Only required if using Blend Shapes
	/// </summary>
	public SkinnedMeshRenderer skinnedMeshRenderer;
	/// <summary>
	/// Initalizes blend shapes for mouth movement
	/// </summary>
	void InitBlendShapes()
	{
		if (!skinnedMeshRenderer)
			skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		if (!string.IsNullOrEmpty(shapeName))
			shapeID = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(shapeName);
	}
	/// <summary>
	/// Update Blend shapes for mouth movement
	/// </summary>
	void UpdateBlendShapes()
	{
		if (!skinnedMeshRenderer)
			return;
		float movement = MovingAverage(CalcSpectrum(frqLow, frqHigh));
		if (ignoreDistance)
		{
			movement += movement * Vector3.Distance(mouthSource.transform.position, Camera.main.transform.position) * volByDistance;
		}
		skinnedMeshRenderer.SetBlendShapeWeight(shapeID, movement * volume);
	}
	/// <summary>
	/// Update jaw bone for mouth movement
	/// </summary>
	void UpdateJawBone()
	{
		if (!mouth)
			return;
		if (ignoreDistance)
		{
			volume = Mathf.Lerp(minVol, maxVol, Mathf.InverseLerp(minDistance, maxDistance,
				Vector3.Distance(mouthSource.transform.position, Camera.main.transform.position)));
		}
		float v = CalcSpectrum(frqLow, frqHigh);
		float mAV = MovingAverage(v);
		Vector3 pos = mouth.transform.localPosition;
		Quaternion rot = mouth.transform.localRotation;
		if (mouthAxis == Axis.Y)
		{
			pos.y = a0 + mAV * volume;
		}
		else if (mouthAxis == Axis.X)
		{
			pos.x = a0 + mAV * volume;
		}
		else
		{
			pos.z = a0 + mAV * volume;
		}
		if (mouthRotAxis == Axis.Y)
		{
			rot.y = a1 + v * rotVolume;
		}
		else if (mouthRotAxis == Axis.X)
		{
			rot.x = a1 + v * rotVolume;
		}
		else
		{
			rot.z = a1 + v * rotVolume;
		}
		mouth.transform.localPosition = pos;
		mouth.transform.localRotation = rot;
	}
}
