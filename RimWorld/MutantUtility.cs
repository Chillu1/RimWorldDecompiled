using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class MutantUtility
{
	public const float ShamblerLOS = 20f;

	public const float ShamblerBuildingDamageFactor = 1.5f;

	private const float ActivateRadius = 10f;

	public static IntRange CallIntervalRange = new IntRange(500, 1000);

	private static List<Hediff> tmpHediffs = new List<Hediff>();

	public static bool CanResurrectAsShambler(Corpse corpse, bool ignoreIndoors = false)
	{
		if (corpse?.InnerPawn == null)
		{
			return false;
		}
		if (!corpse.InnerPawn.RaceProps.IsFlesh)
		{
			return false;
		}
		if (!corpse.InnerPawn.RaceProps.canBecomeShambler)
		{
			return false;
		}
		if (corpse.InnerPawn.IsMutant)
		{
			return false;
		}
		if (corpse is UnnaturalCorpse)
		{
			return false;
		}
		Room room = corpse.PositionHeld.GetRoom(corpse.MapHeld);
		if (room != null && !ignoreIndoors && corpse.PositionHeld.Roofed(corpse.MapHeld) && (room.ProperRoom || room.IsDoorway))
		{
			return false;
		}
		if (!Find.Storyteller.difficulty.childShamblersAllowed && !corpse.InnerPawn.ageTracker.Adult)
		{
			return false;
		}
		Hediff_DeathRefusal firstHediff = corpse.InnerPawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
		if (firstHediff != null && (firstHediff.InProgress || firstHediff.UsesLeft > 0))
		{
			return false;
		}
		return true;
	}

	public static void ResurrectAsShambler(Pawn pawn, int lifespanTicks = -1, Faction faction = null)
	{
		RotStage rotStage = pawn.Corpse.GetRotStage();
		pawn.mutant = new Pawn_MutantTracker(pawn, MutantDefOf.Shambler, rotStage);
		Hediff_Shambler obj = pawn.health.AddHediff(MutantDefOf.Shambler.hediff) as Hediff_Shambler;
		HediffComp_DisappearsAndKills hediffComp_DisappearsAndKills = obj.TryGetComp<HediffComp_DisappearsAndKills>();
		if (hediffComp_DisappearsAndKills != null)
		{
			if (lifespanTicks > 0)
			{
				hediffComp_DisappearsAndKills.disappearsAfterTicks = lifespanTicks;
				hediffComp_DisappearsAndKills.ticksToDisappear = lifespanTicks;
			}
			else
			{
				hediffComp_DisappearsAndKills.disabled = true;
			}
		}
		obj?.StartRising(lifespanTicks);
		if (faction == null && MutantDefOf.Shambler.defaultFaction != null)
		{
			faction = Find.FactionManager.FirstFactionOfDef(MutantDefOf.Shambler.defaultFaction);
		}
		if (faction != null && pawn.Faction != faction)
		{
			pawn.SetFaction(faction);
		}
	}

	public static void SetFreshPawnAsMutant(Pawn pawn, MutantDef mutant)
	{
		RotStage rotStage = (mutant.useCorpseGraphics ? Gen.RandomEnumValue<RotStage>(disallowFirstValue: false) : RotStage.Fresh);
		if (pawn.equipment?.Primary != null)
		{
			pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
		}
		if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.Xenotype != XenotypeDefOf.Baseliner && rotStage == RotStage.Dessicated)
		{
			rotStage = RotStage.Fresh;
		}
		if (rotStage == RotStage.Dessicated)
		{
			pawn.apparel?.DestroyAll();
		}
		SetPawnAsMutantInstantly(pawn, mutant, rotStage);
	}

	public static void SetPawnAsMutantInstantly(Pawn pawn, MutantDef mutant, RotStage rotStage = RotStage.Fresh)
	{
		pawn.mutant = new Pawn_MutantTracker(pawn, mutant, rotStage);
		pawn.mutant.Turn(clearLord: true);
		if (mutant.defaultFaction != null)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(mutant.defaultFaction);
			if (faction != null && pawn.Faction != faction)
			{
				pawn.SetFaction(faction);
			}
		}
		RegenerateHealth(pawn);
	}

	public static Thing FindShamblerTarget(Pawn pawn)
	{
		TargetScanFlags flags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
		Thing thing = null;
		if (thing == null)
		{
			thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, flags, null, 0f, 20f, default(IntVec3), float.MaxValue, canBashDoors: true, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences: true);
		}
		return thing;
	}

	public static void ActivateNearbyShamblers(Pawn pawn, Thing target)
	{
		if (!pawn.SpawnedOrAnyParentSpawned)
		{
			return;
		}
		foreach (Pawn spawnedShambler in pawn.MapHeld.mapPawns.SpawnedShamblers)
		{
			if (pawn.Faction == spawnedShambler.Faction && pawn.Position.InHorDistOf(spawnedShambler.Position, 10f))
			{
				(spawnedShambler.mutant.Hediff as Hediff_Shambler)?.Notify_DelayedAlert(target);
			}
		}
	}

	public static void RegenerateHealth(Pawn pawn)
	{
		if (!pawn.IsMutant || pawn.Dead)
		{
			return;
		}
		MutantDef def = pawn.mutant.Def;
		tmpHediffs.Clear();
		tmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
		foreach (Hediff tmpHediff in tmpHediffs)
		{
			Hediff_Injury hediff_Injury = tmpHediff as Hediff_Injury;
			Hediff_MissingPart hediff_MissingPart = tmpHediff as Hediff_MissingPart;
			Hediff_Addiction hediff_Addiction = tmpHediff as Hediff_Addiction;
			Hediff_ChemicalDependency hediff_ChemicalDependency = tmpHediff as Hediff_ChemicalDependency;
			if (def.isImmuneToInfections && tmpHediff.def.isInfection)
			{
				pawn.health.RemoveHediff(tmpHediff);
			}
			if (def.removeChronicIllnesses && tmpHediff.def.chronic)
			{
				pawn.health.RemoveHediff(tmpHediff);
			}
			else if (def.removeAddictions && (hediff_Addiction != null || hediff_ChemicalDependency != null))
			{
				pawn.health.RemoveHediff(tmpHediff);
			}
			else if (def.removeAllInjuries && hediff_Injury != null)
			{
				pawn.health.RemoveHediff(tmpHediff);
			}
			else if (def.removePermanentInjuries && hediff_Injury != null && !hediff_Injury.IsPermanent())
			{
				pawn.health.RemoveHediff(tmpHediff);
			}
			else if (def.removesHediffs.Contains(tmpHediff.def))
			{
				pawn.health.RemoveHediff(tmpHediff);
			}
			else if (def.restoreLegs && hediff_MissingPart != null && (hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) || hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbSegment)) && (hediff_MissingPart.Part.parent == null || pawn.health.hediffSet.GetNotMissingParts().Contains(hediff_MissingPart.Part.parent)))
			{
				pawn.health.RestorePart(hediff_MissingPart.Part);
			}
		}
		tmpHediffs.Clear();
	}

	public static void RestoreBodyParts(Pawn pawn)
	{
		foreach (Hediff item in pawn.health.hediffSet.hediffs.Where((Hediff hediff) => hediff is Hediff_MissingPart).ToList())
		{
			if (item.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) || !item.Part.def.canSuggestAmputation)
			{
				pawn.health.RestorePart(item.Part);
			}
			if (item.Part.def == BodyPartDefOf.Arm && Rand.Bool)
			{
				pawn.health.RestorePart(item.Part);
			}
		}
		pawn.health.RestorePart(pawn.health.hediffSet.GetBrain());
	}

	public static void RestoreUntilNotDowned(Pawn pawn)
	{
		tmpHediffs.Clear();
		tmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
		int num = 0;
		while (num < 300 && pawn.Downed)
		{
			num++;
			Hediff hediff = tmpHediffs.RandomElement();
			if (hediff is Hediff_Injury)
			{
				hediff.Severity -= 1f;
				pawn.health.Notify_HediffChanged(hediff);
			}
		}
		tmpHediffs.Clear();
		if (pawn.Downed)
		{
			RestoreBodyParts(pawn);
		}
		if (pawn.Downed)
		{
			RegenerateHealth(pawn);
		}
	}

	public static bool CheckShamblerHostility(Pawn a, Pawn b)
	{
		if (a.Faction == b.Faction)
		{
			return false;
		}
		if ((a.IsShambler && a.Faction.HostileTo(b.Faction)) || (b.IsShambler && b.Faction.HostileTo(a.Faction)))
		{
			if (a.IsAnimal)
			{
				if (a.Faction != null)
				{
					if (a.RaceProps.herdAnimal)
					{
						return a.IsReleasedToAttack();
					}
					return true;
				}
				return false;
			}
			if (b.IsAnimal)
			{
				if (b.Faction != null)
				{
					if (b.RaceProps.herdAnimal)
					{
						return b.IsReleasedToAttack();
					}
					return true;
				}
				return false;
			}
			return true;
		}
		if ((a.IsShambler && a.Faction == null && b.Faction != null) || (b.IsShambler && b.Faction == null && a.Faction != null))
		{
			return true;
		}
		return false;
	}

	public static bool ShamblerShouldCollideWith(Pawn me, Pawn other)
	{
		if (me.mindState.anyCloseHostilesRecently || other.mindState.anyCloseHostilesRecently)
		{
			return true;
		}
		if (other.mindState.lastAttackedTarget.Thing == null)
		{
			return false;
		}
		if (other.mindState.lastAttackTargetTick > Find.TickManager.TicksGame - 300 && other.mindState.lastAttackedTarget.Thing.Spawned)
		{
			return true;
		}
		return false;
	}

	public static bool CanUseDrug(Pawn pawn, ThingDef drug)
	{
		if (!pawn.IsMutant || !pawn.mutant.HasTurned)
		{
			return true;
		}
		if (!pawn.mutant.Def.canUseDrugs || !pawn.mutant.Def.drugWhitelist.Contains(drug))
		{
			return false;
		}
		return true;
	}

	public static Color GetMutantSkinColor(Pawn pawn, Color baseColor)
	{
		if (!pawn.IsMutant || !pawn.mutant.HasTurned)
		{
			return baseColor;
		}
		if (pawn.IsShambler)
		{
			return GetShamblerColor(baseColor);
		}
		if (pawn.mutant.Def.skinColorTint.HasValue)
		{
			return baseColor * (1f - pawn.mutant.Def.skinColorTintStrength) + pawn.mutant.Def.skinColorTint.Value * pawn.mutant.Def.skinColorTintStrength;
		}
		if (pawn.mutant.Def.skinColorOverride.HasValue)
		{
			return pawn.mutant.Def.skinColorOverride.Value;
		}
		return baseColor;
	}

	public static Color GetShamblerColor(Color color)
	{
		Color.RGBToHSV(color, out var H, out var S, out var V);
		return Color.HSVToRGB((H + 0.05f) % 1f, Mathf.Clamp(S * 0.5f, 0.2f, 0.5f), Mathf.Max(V * 0.5f, 0.3f));
	}
}
