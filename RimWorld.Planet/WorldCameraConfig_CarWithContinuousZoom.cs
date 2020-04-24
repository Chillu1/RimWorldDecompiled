namespace RimWorld.Planet
{
	public class WorldCameraConfig_CarWithContinuousZoom : WorldCameraConfig_Car
	{
		public WorldCameraConfig_CarWithContinuousZoom()
		{
			zoomSpeed = 0.03f;
			zoomPreserveFactor = 1f;
			smoothZoom = true;
		}
	}
}
