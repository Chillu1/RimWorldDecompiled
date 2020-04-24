using UnityEngine;

namespace RimWorld.Planet
{
	public static class WorldCameraManager
	{
		private static Camera worldCameraInt;

		private static Camera worldSkyboxCameraInt;

		private static WorldCameraDriver worldCameraDriverInt;

		public static readonly string WorldLayerName;

		public static int WorldLayerMask;

		public static int WorldLayer;

		public static readonly string WorldSkyboxLayerName;

		public static int WorldSkyboxLayerMask;

		public static int WorldSkyboxLayer;

		private static readonly Color SkyColor;

		public static Camera WorldCamera => worldCameraInt;

		public static Camera WorldSkyboxCamera => worldSkyboxCameraInt;

		public static WorldCameraDriver WorldCameraDriver => worldCameraDriverInt;

		static WorldCameraManager()
		{
			WorldLayerName = "World";
			WorldLayerMask = LayerMask.GetMask(WorldLayerName);
			WorldLayer = LayerMask.NameToLayer(WorldLayerName);
			WorldSkyboxLayerName = "WorldSkybox";
			WorldSkyboxLayerMask = LayerMask.GetMask(WorldSkyboxLayerName);
			WorldSkyboxLayer = LayerMask.NameToLayer(WorldSkyboxLayerName);
			SkyColor = new Color(16f / 255f, 23f / 255f, 0.117647059f);
			worldCameraInt = CreateWorldCamera();
			worldSkyboxCameraInt = CreateWorldSkyboxCamera(worldCameraInt);
			worldCameraDriverInt = worldCameraInt.GetComponent<WorldCameraDriver>();
		}

		private static Camera CreateWorldCamera()
		{
			GameObject gameObject = new GameObject("WorldCamera", typeof(Camera));
			gameObject.SetActive(value: false);
			gameObject.AddComponent<WorldCameraDriver>();
			Object.DontDestroyOnLoad(gameObject);
			Camera component = gameObject.GetComponent<Camera>();
			component.orthographic = false;
			component.cullingMask = WorldLayerMask;
			component.clearFlags = CameraClearFlags.Depth;
			component.useOcclusionCulling = true;
			component.renderingPath = RenderingPath.Forward;
			component.nearClipPlane = 2f;
			component.farClipPlane = 1200f;
			component.fieldOfView = 20f;
			component.depth = 1f;
			return component;
		}

		private static Camera CreateWorldSkyboxCamera(Camera parent)
		{
			GameObject gameObject = new GameObject("WorldSkyboxCamera", typeof(Camera));
			gameObject.SetActive(value: true);
			Object.DontDestroyOnLoad(gameObject);
			Camera component = gameObject.GetComponent<Camera>();
			component.transform.SetParent(parent.transform);
			component.orthographic = false;
			component.cullingMask = WorldSkyboxLayerMask;
			component.clearFlags = CameraClearFlags.Color;
			component.backgroundColor = SkyColor;
			component.useOcclusionCulling = false;
			component.renderingPath = RenderingPath.Forward;
			component.nearClipPlane = 2f;
			component.farClipPlane = 1200f;
			component.fieldOfView = 60f;
			component.depth = 0f;
			return component;
		}
	}
}
