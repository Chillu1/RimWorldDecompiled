using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class IncidentWorker_AnimalInsanityMass : IncidentWorker
{
	public static bool AnimalUsable(Pawn p)
	{
		if (p.Spawned && !p.Position.Fogged(p.Map) && (!p.InMentalState || !p.MentalStateDef.IsAggro) && !p.Downed)
		{
			return p.Faction == null;
		}
		return false;
	}

	public static void DriveInsane(Pawn p)
	{
		p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, forced: false, forceWake: true);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (parms.points <= 0f)
		{
			Log.Error("AnimalInsanity running without points.");
			parms.points = (int)(map.strengthWatcher.StrengthRating * 50f);
		}
		float adjustedPoints = parms.points;
		if (adjustedPoints > 250f)
		{
			adjustedPoints -= 250f;
			adjustedPoints *= 0.5f;
			adjustedPoints += 250f;
		}
		if (!DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef def) => def.RaceProps.Animal && def.combatPower <= adjustedPoints && map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.kindDef == def && AnimalUsable(p)).Count() >= 3).TryRandomElement(out var animalDef))
		{
			return false;
		}
		List<Pawn> list = map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.kindDef == animalDef && AnimalUsable(p)).ToList();
		float combatPower = animalDef.combatPower;
		float num = 0f;
		List<Pawn> list2 = new List<Pawn>();
		list.Shuffle();
		foreach (Pawn item in list)
		{
			if (num + combatPower > adjustedPoints)
			{
				break;
			}
			DriveInsane(item);
			num += combatPower;
			list2.Add(item);
		}
		if (num == 0f)
		{
			return false;
		}
		string text;
		string text2;
		LetterDef baseLetterDef;
		if (list2.Count == 1)
		{
			Pawn pawn = list2.First();
			text = "LetterLabelAnimalInsanitySingle".Translate(pawn.LabelShort, pawn.Named("ANIMAL")).CapitalizeFirst();
			text2 = "AnimalInsanitySingle".Translate(pawn.LabelShort, pawn.Named("ANIMAL")).CapitalizeFirst();
			baseLetterDef = LetterDefOf.ThreatSmall;
		}
		else
		{
			text = "LetterLabelAnimalInsanityMultiple".Translate(animalDef.GetLabelPlural()).CapitalizeFirst();
			text2 = "AnimalInsanityMultiple".Translate(animalDef.GetLabelPlural()).CapitalizeFirst();
			baseLetterDef = LetterDefOf.ThreatBig;
		}
		SendStandardLetter(text, text2, baseLetterDef, parms, list2);
		SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(map);
		if (map == Find.CurrentMap)
		{
			Find.CameraDriver.shaker.DoShake(1f);
		}
		return true;
	}
}
