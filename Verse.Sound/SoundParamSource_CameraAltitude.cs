namespace Verse.Sound
{
	public class SoundParamSource_CameraAltitude : SoundParamSource
	{
		public override string Label => "Camera altitude";

		public override float ValueFor(Sample samp)
		{
			return Find.Camera.transform.position.y;
		}
	}
}
