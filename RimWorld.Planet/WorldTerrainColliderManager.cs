using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldTerrainColliderManager
	{
		private static GameObject gameObjectInt;

		public static GameObject GameObject => gameObjectInt;

		static WorldTerrainColliderManager()
		{
			gameObjectInt = CreateGameObject();
		}

		private static GameObject CreateGameObject()
		{
			GameObject gameObject = new GameObject("WorldTerrainCollider");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.layer = WorldCameraManager.WorldLayer;
			return gameObject;
		}
	}
}
