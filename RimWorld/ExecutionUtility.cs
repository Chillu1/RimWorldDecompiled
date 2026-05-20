using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class ExecutionUtility
{
	public static void DoExecutionByCut(Pawn executioner, Pawn victim, int bloodPerWeight = 8, bool spawnBlood = true)
	{
		ExecutionInt(executioner, victim, huntingExecution: false, bloodPerWeight, spawnBlood);
	}

	public static void DoHuntingExecution(Pawn executioner, Pawn victim)
	{
		ExecutionInt(executioner, victim, huntingExecution: true);
	}

	private static void ExecutionInt(Pawn executioner, Pawn victim, bool huntingExecution = false, int bloodPerWeight = 8, bool spawnBlood = true)
	{
		if (spawnBlood)
		{
			int num = Mathf.Max(GenMath.RoundRandom(victim.BodySize * (float)bloodPerWeight), 1);
			for (int i = 0; i < num; i++)
			{
				victim.health.DropBloodFilth();
			}
		}
		if (!huntingExecution && victim.RaceProps.Animal && ModsConfig.IdeologyActive)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SlaughteredAnimal, executioner.Named(HistoryEventArgsNames.Doer)));
		}
		BodyPartRecord bodyPartRecord = ExecuteCutPart(victim);
		int num2 = (int)victim.health.hediffSet.GetPartHealth(bodyPartRecord);
		DamageInfo damageInfo = new DamageInfo(DamageDefOf.ExecutionCut, Mathf.Min(num2 - 1, 1), 999f, -1f, executioner, bodyPartRecord, null, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty: false, spawnBlood);
		damageInfo.SetIgnoreArmor(ignoreArmor: true);
		if (ModsConfig.BiotechActive && victim.genes != null && victim.genes.HasActiveGene(GeneDefOf.Deathless))
		{
			damageInfo.SetAmount(9999f);
			damageInfo.SetAllowDamagePropagation(val: false);
		}
		victim.TakeDamage(damageInfo);
		if (!victim.Dead)
		{
			victim.Kill(damageInfo);
		}
		SoundDefOf.Execute_Cut.PlayOneShot(victim);
	}

	public static BodyPartRecord ExecuteCutPart(Pawn pawn)
	{
		BodyPartRecord bodyPartRecord = null;
		if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless))
		{
			bodyPartRecord = pawn.health.hediffSet.GetBrain();
		}
		if (bodyPartRecord == null)
		{
			bodyPartRecord = (from x in pawn.health.hediffSet.GetNotMissingParts()
				where x.def.executionPartPriority > 0f
				orderby x.def.executionPartPriority descending
				select x).FirstOrDefault();
		}
		if (bodyPartRecord != null)
		{
			return bodyPartRecord;
		}
		Log.Error("No good execution cut part found for " + pawn);
		return pawn.health.hediffSet.GetNotMissingParts().RandomElementByWeight((BodyPartRecord x) => x.coverageAbsWithChildren);
	}
}
