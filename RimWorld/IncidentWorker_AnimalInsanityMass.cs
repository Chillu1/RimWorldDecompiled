using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld
{
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
			p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, forceWake: true);
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
			int num2 = 0;
			Pawn pawn = null;
			list.Shuffle();
			foreach (Pawn item in list)
			{
				if (num + combatPower > adjustedPoints)
				{
					break;
				}
				DriveInsane(item);
				num += combatPower;
				num2++;
				pawn = item;
			}
			if (num == 0f)
			{
				return false;
			}
			string str;
			string str2;
			LetterDef baseLetterDef;
			if (num2 == 1)
			{
				str = "LetterLabelAnimalInsanitySingle".Translate(pawn.LabelShort, pawn.Named("ANIMAL"));
				str2 = "AnimalInsanitySingle".Translate(pawn.LabelShort, pawn.Named("ANIMAL"));
				baseLetterDef = LetterDefOf.ThreatSmall;
			}
			else
			{
				str = "LetterLabelAnimalInsanityMultiple".Translate(animalDef.GetLabelPlural());
				str2 = "AnimalInsanityMultiple".Translate(animalDef.GetLabelPlural());
				baseLetterDef = LetterDefOf.ThreatBig;
			}
			SendStandardLetter(str, str2, baseLetterDef, parms, pawn);
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(map);
			if (map == Find.CurrentMap)
			{
				Find.CameraDriver.shaker.DoShake(1f);
			}
			return true;
		}
	}
}
