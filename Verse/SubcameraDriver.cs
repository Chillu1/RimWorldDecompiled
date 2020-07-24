using UnityEngine;

namespace Verse
{
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
				GameObject gameObject = new GameObject();
				gameObject.name = item.defName;
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				Camera camera2 = gameObject.AddComponent<Camera>();
				camera2.orthographic = camera.orthographic;
				camera2.orthographicSize = camera.orthographicSize;
				if (item.layer.NullOrEmpty())
				{
					camera2.cullingMask = 0;
				}
				else
				{
					camera2.cullingMask = LayerMask.GetMask(item.layer);
				}
				camera2.nearClipPlane = camera.nearClipPlane;
				camera2.farClipPlane = camera.farClipPlane;
				camera2.useOcclusionCulling = camera.useOcclusionCulling;
				camera2.allowHDR = camera.allowHDR;
				camera2.renderingPath = camera.renderingPath;
				camera2.clearFlags = CameraClearFlags.Color;
				camera2.backgroundColor = new Color(0f, 0f, 0f, 0f);
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
				subcameras[i].orthographicSize = camera.orthographicSize;
				RenderTexture renderTexture = subcameras[i].targetTexture;
				if (renderTexture != null && (renderTexture.width != Screen.width || renderTexture.height != Screen.height))
				{
					Object.Destroy(renderTexture);
					renderTexture = null;
				}
				if (renderTexture == null)
				{
					renderTexture = new RenderTexture(Screen.width, Screen.height, 0, DefDatabase<SubcameraDef>.AllDefsListForReading[i].BestFormat);
				}
				if (!renderTexture.IsCreated())
				{
					renderTexture.Create();
				}
				subcameras[i].targetTexture = renderTexture;
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
}
