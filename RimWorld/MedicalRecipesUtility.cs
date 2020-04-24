using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class MedicalRecipesUtility
	{
		public static bool IsCleanAndDroppable(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.Dead)
			{
				return false;
			}
			if (pawn.RaceProps.Animal)
			{
				return false;
			}
			if (part.def.spawnThingOnRemoved != null)
			{
				return IsClean(pawn, part);
			}
			return false;
		}

		public static bool IsClean(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.Dead)
			{
				return false;
			}
			return !pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part).Any();
		}

		public static void RestorePartAndSpawnAllPreviousParts(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
		{
			SpawnNaturalPartIfClean(pawn, part, pos, map);
			SpawnThingsFromHediffs(pawn, part, pos, map);
			pawn.health.RestorePart(part);
		}

		public static Thing SpawnNaturalPartIfClean(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
		{
			if (IsCleanAndDroppable(pawn, part))
			{
				return GenSpawn.Spawn(part.def.spawnThingOnRemoved, pos, map);
			}
			return null;
		}

		public static void SpawnThingsFromHediffs(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
		{
			if (pawn.health.hediffSet.GetNotMissingParts().Contains(part))
			{
				foreach (Hediff item in pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part))
				{
					if (item.def.spawnThingOnRemoved != null)
					{
						GenSpawn.Spawn(item.def.spawnThingOnRemoved, pos, map);
					}
				}
				for (int i = 0; i < part.parts.Count; i++)
				{
					SpawnThingsFromHediffs(pawn, part.parts[i], pos, map);
				}
			}
		}

		public static IEnumerable<BodyPartRecord> GetFixedPartsToApplyOn(RecipeDef recipe, Pawn pawn, Func<BodyPartRecord, bool> validator = null)
		{
			int l = 0;
			while (l < recipe.appliedOnFixedBodyParts.Count)
			{
				BodyPartDef part = recipe.appliedOnFixedBodyParts[l];
				List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
				for (int i = 0; i < bpList.Count; i++)
				{
					BodyPartRecord bodyPartRecord = bpList[i];
					if (bodyPartRecord.def == part && (validator == null || validator(bodyPartRecord)))
					{
						yield return bodyPartRecord;
					}
				}
				int num = l + 1;
				l = num;
			}
			l = 0;
			while (l < recipe.appliedOnFixedBodyPartGroups.Count)
			{
				BodyPartGroupDef group = recipe.appliedOnFixedBodyPartGroups[l];
				List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
				for (int i = 0; i < bpList.Count; i++)
				{
					BodyPartRecord bodyPartRecord2 = bpList[i];
					if (bodyPartRecord2.groups != null && bodyPartRecord2.groups.Contains(group) && (validator == null || validator(bodyPartRecord2)))
					{
						yield return bodyPartRecord2;
					}
				}
				int num = l + 1;
				l = num;
			}
		}
	}
}
