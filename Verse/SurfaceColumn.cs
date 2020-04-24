namespace Verse
{
	public struct SurfaceColumn
	{
		public float x;

		public SimpleCurve y;

		public SurfaceColumn(float x, SimpleCurve y)
		{
			this.x = x;
			this.y = y;
		}
	}
}
