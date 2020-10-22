using UnityEngine;

namespace Verse
{
	public static class Altitudes
	{
		private const int NumAltitudeLayers = 34;

		private static readonly float[] Alts;

		private const float LayerSpacing = 0.428571433f;

		public const float AltInc = 3f / 70f;

		public static readonly Vector3 AltIncVect;

		static Altitudes()
		{
			Alts = new float[34];
			AltIncVect = new Vector3(0f, 3f / 70f, 0f);
			for (int i = 0; i < 34; i++)
			{
				Alts[i] = (float)i * 0.428571433f;
			}
		}

		public static float AltitudeFor(this AltitudeLayer alt)
		{
			return Alts[(uint)alt];
		}
	}
}
