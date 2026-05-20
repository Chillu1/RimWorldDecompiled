using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public static class CaravanNameGenerator
{
	public static string GenerateCaravanName(Caravan caravan)
	{
		Pawn pawn = IdeoUtility.FindFirstPawnWithLeaderRole(caravan) ?? BestCaravanPawnUtility.FindBestNegotiator(caravan) ?? BestCaravanPawnUtility.FindBestDiplomat(caravan) ?? caravan.PawnsListForReading.Find((Pawn x) => caravan.IsOwner(x));
		TaggedString taggedString = ((pawn != null) ? "CaravanLeaderCaravanName".Translate(pawn.LabelShort, pawn).CapitalizeFirst() : ((TaggedString)caravan.def.label));
		for (int num = 1; num <= 1000; num++)
		{
			TaggedString taggedString2 = taggedString;
			if (num != 1)
			{
				taggedString2 += " " + num;
			}
			if (!CaravanNameInUse(taggedString2))
			{
				return taggedString2;
			}
		}
		Log.Error("Ran out of caravan names.");
		return caravan.def.label;
	}

	private static bool CaravanNameInUse(string name)
	{
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int i = 0; i < caravans.Count; i++)
		{
			if (caravans[i].Name == name)
			{
				return true;
			}
		}
		return false;
	}
}
