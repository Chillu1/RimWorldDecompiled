using UnityEngine;

namespace Verse;

public class SubcameraDriver : MonoBehaviour
{
	private Camera[] subcameras;

	public void Init()
	{
		if (subcameras != null || !PlayDataLoader.Loaded)
		{
			return;
		}
		Camera camera = Find.Camera;
		subcameras = new Camera[DefDatabase<SubcameraDef>.DefCount];
		foreach (SubcameraDef item in DefDatabase<SubcameraDef>.AllDefsListForReading)
		{
			GameObject obj = new GameObject();
			obj.name = item.defName;
			obj.transform.parent = base.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.transform.localRotation = Quaternion.identity;
			Camera camera2 = obj.AddComponent<Camera>();
			camera2.orthographic = camera.orthographic;
			camera2.orthographicSize = camera.orthographicSize;
			camera2.cullingMask = ((!item.layer.NullOrEmpty()) ? LayerMask.GetMask(item.layer) : 0);
			camera2.nearClipPlane = camera.nearClipPlane;
			camera2.farClipPlane = camera.farClipPlane;
			camera2.useOcclusionCulling = camera.useOcclusionCulling;
			camera2.allowHDR = camera.allowHDR;
			camera2.renderingPath = camera.renderingPath;
			camera2.enabled = item.startEnabled;
			camera2.clearFlags = CameraClearFlags.Color;
			camera2.backgroundColor = item.backgroundColor;
			camera2.depth = item.depth;
			subcameras[item.index] = camera2;
		}
	}

	public void UpdatePositions(Camera camera)
	{
		if (subcameras == null)
		{
			return;
		}
		for (int i = 0; i < subcameras.Length; i++)
		{
			SubcameraDef subcameraDef = DefDatabase<SubcameraDef>.AllDefsListForReading[i];
			if (!subcameraDef.doNotUpdate)
			{
				Camera obj = subcameras[i];
				obj.orthographicSize = camera.orthographicSize;
				RenderTexture renderTexture = obj.targetTexture;
				if (renderTexture != null && (renderTexture.width != Screen.width || renderTexture.height != Screen.height))
				{
					Object.Destroy(renderTexture);
					renderTexture = null;
				}
				if (renderTexture == null)
				{
					renderTexture = new RenderTexture(Screen.width, Screen.height, 0, subcameraDef.BestFormat);
				}
				if (!renderTexture.IsCreated())
				{
					renderTexture.Create();
				}
				obj.targetTexture = renderTexture;
			}
		}
	}

	public Camera GetSubcamera(SubcameraDef def)
	{
		if (subcameras == null || def == null || subcameras.Length <= def.index)
		{
			return null;
		}
		return subcameras[def.index];
	}
}
