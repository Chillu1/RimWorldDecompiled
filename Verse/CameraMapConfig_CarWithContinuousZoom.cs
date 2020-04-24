namespace Verse
{
	public class CameraMapConfig_CarWithContinuousZoom : CameraMapConfig_Car
	{
		public CameraMapConfig_CarWithContinuousZoom()
		{
			zoomSpeed = 0.043f;
			zoomPreserveFactor = 1f;
			smoothZoom = true;
		}
	}
}
