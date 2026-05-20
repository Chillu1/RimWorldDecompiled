using Verse;

namespace RimWorld.Planet;

public static class HillinessUtility
{
	public static string GetLabel(this Hilliness h)
	{
		switch (h)
		{
		case Hilliness.Flat:
			return "Hilliness_Flat".Translate();
		case Hilliness.SmallHills:
			return "Hilliness_SmallHills".Translate();
		case Hilliness.LargeHills:
			return "Hilliness_LargeHills".Translate();
		case Hilliness.Mountainous:
			return "Hilliness_Mountainous".Translate();
		case Hilliness.Impassable:
			return "Hilliness_Impassable".Translate();
		default:
			Log.ErrorOnce("Hilliness label unknown: " + h, 694362);
			return h.ToString();
		}
	}

	public static string GetLabelCap(this Hilliness h)
	{
		return h.GetLabel().CapitalizeFirst();
	}
}
