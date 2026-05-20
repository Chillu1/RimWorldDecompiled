using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class SanguophageUtility
{
	private static readonly FloatRange XPLossPercentFromDeathrestRange = new FloatRange(0.03f, 0.06f);

	private const float MinConsciousnessForHemogenExtraction = 0.45f;

	public static bool ShouldBeDeathrestingOrInComaInsteadOfDead(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!pawn.health.ShouldBeDead())
		{
			return false;
		}
		if (pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless))
		{
			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			if (brain != null && !pawn.health.hediffSet.PartIsMissing(brain) && pawn.health.hediffSet.GetPartHealth(brain) > 0f)
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryStartDeathrest(Pawn pawn, DeathrestStartReason reason)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!pawn.Spawned)
		{
			return false;
		}
		if (pawn.Deathresting)
		{
			return false;
		}
		Gene_Deathrest gene_Deathrest = pawn.genes?.GetFirstGeneOfType<Gene_Deathrest>();
		if (gene_Deathrest == null)
		{
			return false;
		}
		TaggedString label = "LetterLabelInvoluntaryDeathrest".Translate() + ": " + pawn.LabelShortCap;
		TaggedString letterText = "LetterTextInvoluntaryDeathrestOrComa".Translate(pawn.Named("PAWN"), "LetterTextDeathrest".Translate(pawn.Named("PAWN")).Named("HEDIFFINFO"));
		if (reason == DeathrestStartReason.LethalDamage)
		{
			DoXPLossFromDamage(pawn, ref letterText);
			letterText += "\n\n" + "Reason".Translate() + ": " + "DeathrestLethalDamage".Translate();
		}
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Find.LetterStack.ReceiveLetter(label, letterText, LetterDefOf.NegativeEvent, pawn);
		}
		gene_Deathrest.autoWake = reason != DeathrestStartReason.PlayerForced;
		pawn.health.AddHediff(HediffDefOf.Deathrest);
		return true;
	}

	public static bool TryStartRegenComa(Pawn pawn, DeathrestStartReason reason)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!pawn.health.hediffSet.HasHediff(HediffDefOf.RegenerationComa))
		{
			TaggedString label = "LetterLabelRegenerationComa".Translate() + ": " + pawn.LabelShortCap;
			TaggedString letterText = "LetterTextInvoluntaryDeathrestOrComa".Translate(pawn.Named("PAWN"), "LetterTextRegenerationComa".Translate(pawn.Named("PAWN")).Named("HEDIFFINFO"));
			if (reason == DeathrestStartReason.LethalDamage)
			{
				DoXPLossFromDamage(pawn, ref letterText);
			}
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Find.LetterStack.ReceiveLetter(label, letterText, LetterDefOf.NegativeEvent, pawn);
			}
			pawn.health.AddHediff(HediffDefOf.RegenerationComa);
			return true;
		}
		return false;
	}

	public static void DoXPLossFromDamage(Pawn pawn, ref TaggedString letterText)
	{
		Gene_Deathless gene_Deathless = pawn.genes?.GetFirstGeneOfType<Gene_Deathless>();
		if (gene_Deathless != null && Find.TickManager.TicksGame - gene_Deathless.lastSkillReductionTick >= 60000 && pawn.skills.skills.Where((SkillRecord x) => !x.TotallyDisabled && x.XpTotalEarned > 0f).TryRandomElementByWeight((SkillRecord x) => (float)x.GetLevel() * 10f, out var result))
		{
			float num = result.XpTotalEarned * XPLossPercentFromDeathrestRange.RandomInRange;
			letterText += "\n\n" + "DeathrestLostSkill".Translate(pawn.Named("PAWN"), result.def.label.Named("SKILL"), ((int)num).Named("XPLOSS"));
			result.Learn(0f - num, direct: true);
			gene_Deathless.lastSkillReductionTick = Find.TickManager.TicksGame;
		}
	}

	public static bool InSunlight(this IntVec3 cell, Map map)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		if (!map.roofGrid.Roofed(cell))
		{
			return map.skyManager.CurSkyGlow > 0.1f;
		}
		return false;
	}

	public static string DeathrestJobReport(Pawn pawn)
	{
		if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Deathrest) is Hediff_Deathrest { Paused: not false })
		{
			return "DeathrestPaused".Translate() + ": " + "LethalInjuries".Translate();
		}
		Gene_Deathrest firstGeneOfType = pawn.genes.GetFirstGeneOfType<Gene_Deathrest>();
		TaggedString taggedString = "Deathresting".Translate().CapitalizeFirst() + ": ";
		float deathrestPercent = firstGeneOfType.DeathrestPercent;
		if (deathrestPercent < 1f)
		{
			taggedString += Mathf.Min(deathrestPercent, 0.99f).ToStringPercent("F0");
		}
		else
		{
			taggedString += string.Format("{0} - {1}", "Complete".Translate().CapitalizeFirst(), "CanWakeSafely".Translate());
		}
		if (deathrestPercent < 1f)
		{
			taggedString += ", " + "DurationLeft".Translate((firstGeneOfType.MinDeathrestTicks - firstGeneOfType.deathrestTicks).ToStringTicksToPeriod());
		}
		return taggedString.Resolve();
	}

	public static bool WouldDieFromAdditionalBloodLoss(this Pawn pawn, float severity)
	{
		if (pawn.Dead || !pawn.RaceProps.IsFlesh)
		{
			return false;
		}
		float num = severity;
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
		if (firstHediffOfDef != null)
		{
			num += firstHediffOfDef.Severity;
		}
		if (num >= HediffDefOf.BloodLoss.lethalSeverity)
		{
			return true;
		}
		if (HediffDefOf.BloodLoss.stages[HediffDefOf.BloodLoss.StageAtSeverity(num)].lifeThreatening)
		{
			return true;
		}
		return false;
	}

	public static void DoBite(Pawn biter, Pawn victim, float targetHemogenGain, float nutritionGain, float targetBloodLoss, float victimResistanceGain, IntRange bloodFilthToSpawnRange, ThoughtDef thoughtDefToGiveTarget = null, ThoughtDef opinionThoughtToGiveTarget = null)
	{
		if (!ModLister.CheckBiotech("Sanguophage bite"))
		{
			return;
		}
		float num = HemogenGainBloodlossFactor(victim, targetBloodLoss);
		float num2 = targetHemogenGain * victim.BodySize * num;
		GeneUtility.OffsetHemogen(biter, num2);
		GeneUtility.OffsetHemogen(victim, 0f - num2);
		if (biter.needs?.food != null)
		{
			biter.needs.food.CurLevel += nutritionGain * num;
		}
		if (thoughtDefToGiveTarget != null)
		{
			victim.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(thoughtDefToGiveTarget), biter);
		}
		if (opinionThoughtToGiveTarget != null)
		{
			victim.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(opinionThoughtToGiveTarget), biter);
		}
		if (targetBloodLoss > 0f)
		{
			victim.health.AddHediff(HediffDefOf.BloodfeederMark, ExecutionUtility.ExecuteCutPart(victim));
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, victim);
			hediff.Severity = targetBloodLoss;
			victim.health.AddHediff(hediff);
		}
		if (victim.IsPrisoner && victimResistanceGain > 0f)
		{
			victim.guest.resistance = Mathf.Min(victim.guest.resistance + victimResistanceGain, victim.kindDef.initialResistanceRange.Value.TrueMax);
		}
		int randomInRange = bloodFilthToSpawnRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			IntVec3 c = victim.Position;
			if (randomInRange > 1 && Rand.Chance(0.8888f))
			{
				c = victim.Position.RandomAdjacentCell8Way();
			}
			if (c.InBounds(victim.MapHeld))
			{
				FilthMaker.TryMakeFilth(c, victim.MapHeld, victim.RaceProps.BloodDef, victim.LabelShort);
			}
		}
	}

	public static float HemogenGainBloodlossFactor(Pawn pawn, float targetBloodloss)
	{
		if (targetBloodloss > 0f)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null)
			{
				return Mathf.Min(1f, (HediffDefOf.BloodLoss.lethalSeverity - firstHediffOfDef.Severity) / targetBloodloss);
			}
		}
		return 1f;
	}

	public static bool CanSafelyBeQueuedForHemogenExtraction(Pawn pawn)
	{
		if (ModsConfig.BiotechActive && pawn.Spawned && pawn.BillStack != null && !pawn.BillStack.Bills.Any((Bill x) => x.recipe == RecipeDefOf.ExtractHemogenPack) && PawnConsciousEnoughForExtraction(pawn) && RecipeDefOf.ExtractHemogenPack.Worker.AvailableOnNow(pawn))
		{
			return !pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss);
		}
		return false;
	}

	private static bool PawnConsciousEnoughForExtraction(Pawn pawn)
	{
		return pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) > 0.45f;
	}
}
