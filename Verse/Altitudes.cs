using UnityEngine;

namespace Verse;

public static class Altitudes
{
	private const int NumAltitudeLayers = 40;

	private static readonly float[] Alts;

	private const float LayerSpacing = 0.36585367f;

	public const float AltInc = 0.03658537f;

	public static readonly Vector3 AltIncVect;

	static Altitudes()
	{
		Alts = new float[40];
		AltIncVect = new Vector3(0f, 0.03658537f, 0f);
		for (int i = 0; i < 40; i++)
		{
			Alts[i] = (float)i * 0.36585367f;
		}
	}

	public static float AltitudeFor(this AltitudeLayer alt)
	{
		return Alts[(uint)alt];
	}

	public static float AltitudeFor(this AltitudeLayer alt, float incOffset)
	{
		return alt.AltitudeFor() + incOffset * 0.03658537f;
	}
}
