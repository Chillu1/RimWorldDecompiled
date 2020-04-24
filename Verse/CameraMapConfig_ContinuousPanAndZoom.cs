namespace Verse
{
	public class CameraMapConfig_ContinuousPanAndZoom : CameraMapConfig_ContinuousPan
	{
		public CameraMapConfig_ContinuousPanAndZoom()
		{
			zoomSpeed = 0.043f;
			zoomPreserveFactor = 1f;
			smoothZoom = true;
			minSize = 8.2f;
		}
	}
}
