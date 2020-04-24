namespace Verse
{
	public class CameraMapConfig_ContinuousPan : CameraMapConfig
	{
		public CameraMapConfig_ContinuousPan()
		{
			dollyRateKeys = 10f;
			dollyRateScreenEdge = 5f;
			camSpeedDecayFactor = 1f;
			moveSpeedScale = 1f;
			minSize = 8.2f;
		}
	}
}
