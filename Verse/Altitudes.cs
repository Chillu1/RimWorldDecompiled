using UnityEngine;

namespace Verse
{
	public static class Altitudes
	{
		private const int NumAltitudeLayers = 32;

		private static readonly float[] Alts;

		private const float LayerSpacing = 0.454545468f;

		public const float AltInc = 0.0454545468f;

		public static readonly Vector3 AltIncVect;

		static Altitudes()
		{
			Alts = new float[32];
			AltIncVect = new Vector3(0f, 0.0454545468f, 0f);
			for (int i = 0; i < 32; i++)
			{
				Alts[i] = (float)i * 0.454545468f;
			}
		}

		public static float AltitudeFor(this AltitudeLayer alt)
		{
			return Alts[(uint)alt];
		}
	}
}
