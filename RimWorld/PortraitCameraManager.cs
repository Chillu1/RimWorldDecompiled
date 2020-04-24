using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PortraitCameraManager
	{
		private static Camera portraitCameraInt;

		private static PortraitRenderer portraitRendererInt;

		public static Camera PortraitCamera => portraitCameraInt;

		public static PortraitRenderer PortraitRenderer => portraitRendererInt;

		static PortraitCameraManager()
		{
			portraitCameraInt = CreatePortraitCamera();
			portraitRendererInt = portraitCameraInt.GetComponent<PortraitRenderer>();
		}

		private static Camera CreatePortraitCamera()
		{
			GameObject gameObject = new GameObject("PortraitCamera", typeof(Camera));
			gameObject.SetActive(value: false);
			gameObject.AddComponent<PortraitRenderer>();
			Object.DontDestroyOnLoad(gameObject);
			Camera component = gameObject.GetComponent<Camera>();
			component.transform.position = new Vector3(0f, 15f, 0f);
			component.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			component.orthographic = true;
			component.cullingMask = 0;
			component.orthographicSize = 1f;
			component.clearFlags = CameraClearFlags.Color;
			component.backgroundColor = new Color(0f, 0f, 0f, 0f);
			component.useOcclusionCulling = false;
			component.renderingPath = RenderingPath.Forward;
			Camera camera = Current.Camera;
			component.nearClipPlane = camera.nearClipPlane;
			component.farClipPlane = camera.farClipPlane;
			return component;
		}
	}
}
