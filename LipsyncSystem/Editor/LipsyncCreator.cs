using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class LipsyncCreator : EditorWindow
{
	[MenuItem("Tools/LipsyncSystem/Creator")]
	public static void OpenLipsyncCreator()
	{
		EditorWindow.GetWindow<LipsyncCreator>();
	}
	public LipsyncController.SyncType syncType;
	private LipsyncController controller;
	private MeshRenderer meshRenderer;
	private SkinnedMeshRenderer[] renderers;
	private GameObject MainObject;
	private Transform JawBone;
	private List<Transform> Bones;
	private List<string> objectNames, objectNames1;
		private string[] objectNames2;
	private int selectedIndex, selectedIndex1, selectedIndex2;
	private List<GameObject> objs;
	private string error;
	public const string JawTemplate = "JawPreset", ShapeTemplate = "BlendShapePreset";
	public void fillWithChildren(GameObject masterObj, ref List<GameObject> gos)
	{
		for (int i = 0; i < masterObj.transform.childCount; i++)
		{
			gos.Add(masterObj.transform.GetChild(i).gameObject);
			fillWithChildren(masterObj.transform.GetChild(i).gameObject, ref gos);
		}
	}
	void OnGUI()
	{
		syncType = (LipsyncController.SyncType)EditorGUILayout.EnumPopup("Sync Type", syncType);
		MainObject = (GameObject)EditorGUILayout.ObjectField("Character", MainObject, typeof(GameObject), true);
		if (MainObject != null)
		{
			if (syncType == LipsyncController.SyncType.BlendShape)
			{
				objectNames = new List<string>();
				objectNames.Add("Select Mesh");
				renderers = MainObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer smr in renderers)
				{
					objectNames.Add(smr.name);
				}
				if (selectedIndex >= objectNames.Count)
					selectedIndex = 0;
				selectedIndex = EditorGUILayout.Popup("Shape Mesh", selectedIndex, objectNames.ToArray());
				if (selectedIndex > 0)
				{
					objectNames2 = new string[renderers[selectedIndex -1].sharedMesh.blendShapeCount + 1];
					objectNames2[0] = "Select Blendshape";
					for (int i = 1; i < objectNames2.Length - 1; i++)
					{
						objectNames2[i] = renderers[selectedIndex - 1].sharedMesh.GetBlendShapeName(i-1);
					}
					if (selectedIndex2 >= objectNames2.Length - 1)
						selectedIndex2 = 0;
					selectedIndex2 = EditorGUILayout.Popup("Mouth Shape", selectedIndex2, objectNames2);
					if (selectedIndex2 > 0)
					{
						objectNames1 = new List<string>();
						objs = new List<GameObject>();
						fillWithChildren(MainObject, ref objs);
						objectNames1.Add("Select Head Transform");
						for (int x = 0; x < objs.Count; x++)
						{
							objectNames1.Add(objs[x].name);
						}
						if (selectedIndex1 >= objectNames1.Count - 1)
							selectedIndex1 = 0;
						selectedIndex1 = EditorGUILayout.Popup("Head Transform", selectedIndex1, objectNames1.ToArray());
						if (selectedIndex1 > 0)
						{
							if (GUILayout.Button("Setup Blendshape Rig"))
							{
								SetupBlendshapeRig(renderers[selectedIndex - 1], objectNames2[selectedIndex2], objs[selectedIndex1 - 1].transform);
							}

						}
					}

				}
			}
			else if (syncType == LipsyncController.SyncType.Jaw)
			{
				objectNames = new List<string>();
				objs = new List<GameObject>();
				fillWithChildren(MainObject, ref objs);
				objectNames.Add("Select Jaw Transform");
				for (int x = 0; x < objs.Count; x++)
				{
					objectNames.Add(objs[x].name);
				}
				if (selectedIndex >= objectNames.Count - 1)
					selectedIndex = 0;
				selectedIndex = EditorGUILayout.Popup("Jaw Bone", selectedIndex, objectNames.ToArray());
				if (selectedIndex > 0)
				{
					objectNames[0] = "Select Head Transform";
					if (selectedIndex1 >= objectNames.Count - 1)
						selectedIndex1 = 0;
					selectedIndex1 = EditorGUILayout.Popup("Head Transform", selectedIndex1, objectNames.ToArray());
					if (selectedIndex1 > 0)
					{
						if (GUILayout.Button("Setup Jaw Rig"))
						{
							SetupBoneRig(objs[selectedIndex - 1].transform, objs[selectedIndex1 - 1].transform);
						}
					}

				}
			}
		}
	}
	void SetupBlendshapeRig(SkinnedMeshRenderer smr, string shapeID, Transform head)
	{
		LipsyncTemplate template = Resources.Load<LipsyncTemplate>(ShapeTemplate);
		LipsyncController controller = SetupController(template, MainObject.AddComponent<LipsyncController>());
		AudioSource source = head.GetComponent<AudioSource>();
		if (source == null)
		{
			source = head.gameObject.AddComponent<AudioSource>();
			source.spatialBlend = 1.0f;
		}
		controller.mouthSource = source;
		controller.skinnedMeshRenderer = smr;
		controller.shapeName = shapeID;
	}
	void SetupBoneRig(Transform jawBone, Transform head)
	{
		LipsyncTemplate template = Resources.Load<LipsyncTemplate>(JawTemplate);
		LipsyncController controller = SetupController(template, MainObject.AddComponent<LipsyncController>());
		AudioSource source = head.GetComponent<AudioSource>();
		if (source == null)
		{
			source = head.gameObject.AddComponent<AudioSource>();
			source.spatialBlend = 1.0f;
		}
		controller.mouthSource = source;
		controller.mouth = jawBone.gameObject;

	}
	LipsyncController SetupController(LipsyncTemplate template, LipsyncController controller)
	{
		controller.analysisType = template.analysisType;
		controller.syncType = template.syncType;
		controller.mouthAxis = template.mouthAxis;
		controller.mouthRotAxis = template.mouthRotAxis;
		controller.volByDistance = template.volByDistance;
		controller.volume = template.volume;
		controller.rotVolume = template.rotVolume;
		controller.minVol = template.minVol;
		controller.maxVol = template.maxVol;
		controller.frqHigh = template.frqHigh;
		controller.frqLow = template.frqLow;
		controller.ignoreDistance = template.ignoreDistance;
		return controller;
	}
}
