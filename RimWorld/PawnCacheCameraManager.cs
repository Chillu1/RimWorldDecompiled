using UnityEngine;

namespace RimWorld;

public static class PawnCacheCameraManager
{
	private static Camera pawnCacheCameraInt;

	private static PawnCacheRenderer pawnCacheRendererInt;

	public static Camera PawnCacheCamera => pawnCacheCameraInt;

	public static PawnCacheRenderer PawnCacheRenderer => pawnCacheRendererInt;

	static PawnCacheCameraManager()
	{
		pawnCacheCameraInt = CreatePawnCacheCamera();
		pawnCacheRendererInt = pawnCacheCameraInt.GetComponent<PawnCacheRenderer>();
	}

	private static Camera CreatePawnCacheCamera()
	{
		GameObject gameObject = new GameObject("PortraitCamera", typeof(Camera));
		gameObject.SetActive(value: false);
		gameObject.AddComponent<PawnCacheRenderer>();
		Object.DontDestroyOnLoad(gameObject);
		Camera component = gameObject.GetComponent<Camera>();
		component.transform.position = new Vector3(0f, 10f, 0f);
		component.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		component.orthographic = true;
		component.cullingMask = 0;
		component.orthographicSize = 1f;
		component.clearFlags = CameraClearFlags.Color;
		component.backgroundColor = new Color(0f, 0f, 0f, 0f);
		component.useOcclusionCulling = false;
		component.renderingPath = RenderingPath.Forward;
		component.nearClipPlane = 5f;
		component.farClipPlane = 12f;
		return component;
	}
}
