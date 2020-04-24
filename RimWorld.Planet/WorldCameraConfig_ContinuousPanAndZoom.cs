namespace RimWorld.Planet
{
	public class WorldCameraConfig_ContinuousPanAndZoom : WorldCameraConfig_ContinuousPan
	{
		public WorldCameraConfig_ContinuousPanAndZoom()
		{
			zoomSpeed = 0.03f;
			zoomPreserveFactor = 1f;
			smoothZoom = true;
		}
	}
}
