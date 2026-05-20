using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_AnimalInsanitySingle : IncidentWorker
{
	private const int FixedPoints = 30;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		Pawn animal;
		return TryFindRandomAnimal(map, out animal);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!TryFindRandomAnimal(map, out var animal))
		{
			return false;
		}
		IncidentWorker_AnimalInsanityMass.DriveInsane(animal);
		string text = "AnimalInsanitySingle".Translate(animal.Label, animal.Named("ANIMAL")).CapitalizeFirst();
		SendStandardLetter("LetterLabelAnimalInsanitySingle".Translate(animal.Label, animal.Named("ANIMAL")).CapitalizeFirst(), text, LetterDefOf.ThreatSmall, parms, animal);
		return true;
	}

	private bool TryFindRandomAnimal(Map map, out Pawn animal)
	{
		int maxPoints = 150;
		if (GenDate.DaysPassedSinceSettle < 7)
		{
			maxPoints = 40;
		}
		return map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.IsAnimal && p.kindDef.combatPower <= (float)maxPoints && IncidentWorker_AnimalInsanityMass.AnimalUsable(p)).TryRandomElement(out animal);
	}
}
