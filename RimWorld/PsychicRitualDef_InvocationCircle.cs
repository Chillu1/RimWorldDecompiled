using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_InvocationCircle : PsychicRitualDef
{
	public enum InvalidTargetReasonEnum
	{
		None,
		AreaNotClear
	}

	private class RitualQualityOffsetCount
	{
		public float offset;

		public int count;

		public RitualQualityOffsetCount(int count, float offset)
		{
			this.count = count;
			this.offset = offset;
		}
	}

	public FloatRange hoursUntilHoraxEffect;

	public FloatRange hoursUntilOutcome;

	public float invocationCircleRadius = 3.9f;

	[MustTranslate]
	public string outcomeDescription;

	public float psychicSensitivityPowerFactor = 0.25f;

	protected PsychicRitualRoleDef invokerRole;

	protected PsychicRitualRoleDef chanterRole;

	protected PsychicRitualRoleDef targetRole;

	protected PsychicRitualRoleDef defenderRole;

	protected IngredientCount requiredOffering;

	protected string timeAndOfferingLabelCached;

	public static readonly SimpleCurve PsychicSensitivityToPowerFactor = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 0.5f),
		new CurvePoint(2f, 0.9f),
		new CurvePoint(3f, 1f)
	};

	protected const int DurationTicksWaitPostEffect = 120;

	private static Dictionary<PsychicRitualRoleDef, List<IntVec3>> tmpParticipants = new Dictionary<PsychicRitualRoleDef, List<IntVec3>>(8);

	private List<Pawn> tmpGatheringPawns = new List<Pawn>(8);

	public virtual PsychicRitualRoleDef InvokerRole => invokerRole;

	public virtual PsychicRitualRoleDef ChanterRole => chanterRole;

	public virtual PsychicRitualRoleDef TargetRole => targetRole;

	public virtual PsychicRitualRoleDef DefenderRole => defenderRole;

	public virtual IngredientCount RequiredOffering => requiredOffering;

	public TaggedString CooldownLabel => "PsychicRitualCooldownLabel".Translate() + ": " + (cooldownHours * 2500).ToStringTicksToPeriod();

	public override List<PsychicRitualRoleDef> Roles
	{
		get
		{
			List<PsychicRitualRoleDef> roles = base.Roles;
			if (InvokerRole != null)
			{
				roles.Add(InvokerRole);
			}
			if (TargetRole != null)
			{
				roles.Add(TargetRole);
			}
			if (ChanterRole != null)
			{
				roles.Add(ChanterRole);
			}
			if (DefenderRole != null)
			{
				roles.Add(DefenderRole);
			}
			return roles;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		requiredOffering?.ResolveReferences();
		invokerRole = invokerRole ?? PsychicRitualRoleDefOf.Invoker;
		chanterRole = chanterRole ?? PsychicRitualRoleDefOf.Chanter;
	}

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		float randomInRange = hoursUntilOutcome.RandomInRange;
		IReadOnlyDictionary<PsychicRitualRoleDef, List<IntVec3>> readOnlyDictionary = GenerateRolePositions(psychicRitual.assignments);
		return new List<PsychicRitualToil>
		{
			new PsychicRitualToil_GatherForInvocation(psychicRitual, this, readOnlyDictionary),
			new PsychicRitualToil_InvokeHorax(InvokerRole, readOnlyDictionary.TryGetValue(InvokerRole), TargetRole, readOnlyDictionary.TryGetValue(TargetRole), ChanterRole, readOnlyDictionary.TryGetValue(ChanterRole), DefenderRole, readOnlyDictionary.TryGetValue(DefenderRole), RequiredOffering)
			{
				hoursUntilHoraxEffect = hoursUntilHoraxEffect.RandomInRange,
				hoursUntilOutcome = randomInRange
			},
			new PsychicRitualToil_Wait(120)
		};
	}

	public override bool IsValidTarget(TargetInfo target, out AnyEnum reason)
	{
		foreach (IntVec3 item in GenRadial.RadialCellsAround(target.Cell, invocationCircleRadius, useCenter: true))
		{
			if (!item.Standable(target.Map))
			{
				reason = AnyEnum.FromEnum(InvalidTargetReasonEnum.AreaNotClear);
				return false;
			}
		}
		reason = AnyEnum.None;
		return true;
	}

	public override TaggedString InvalidTargetReason(AnyEnum reason)
	{
		InvalidTargetReasonEnum? invalidTargetReasonEnum = reason.As<InvalidTargetReasonEnum>();
		if (invalidTargetReasonEnum.HasValue)
		{
			InvalidTargetReasonEnum valueOrDefault = invalidTargetReasonEnum.GetValueOrDefault();
			return valueOrDefault switch
			{
				InvalidTargetReasonEnum.None => TaggedString.Empty, 
				InvalidTargetReasonEnum.AreaNotClear => "PsychicRitualDef_InvocationCircle_AreaMustBeClear".Translate(), 
				_ => throw new InvalidOperationException($"Unknown reason {valueOrDefault}"), 
			};
		}
		return base.InvalidTargetReason(reason);
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return outcomeDescription.Formatted();
	}

	public override IEnumerable<TaggedString> OutcomeWarnings(PsychicRitualRoleAssignments assignments)
	{
		foreach (Pawn item in assignments.AssignedPawns(TargetRole))
		{
			if (item.HomeFaction != null && item.HomeFaction != Faction.OfPlayer && item.HomeFaction.def.humanlikeFaction && !item.HomeFaction.def.PermanentlyHostileTo(FactionDefOf.PlayerColony) && !item.HomeFaction.temporary && !item.HomeFaction.Hidden)
			{
				yield return "PsychicRitualFactionWarning".Translate(item.Named("PAWN"), item.HomeFaction.Named("FACTION")).Colorize(ColoredText.WarningColor);
			}
		}
	}

	public override TaggedString TimeAndOfferingLabel()
	{
		if (timeAndOfferingLabelCached != null)
		{
			return timeAndOfferingLabelCached;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(DurationLabel());
		stringBuilder.Append(CooldownLabel);
		if (!OfferingLabel().NullOrEmpty())
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(OfferingLabel());
		}
		timeAndOfferingLabelCached = stringBuilder.ToString();
		return timeAndOfferingLabelCached;
	}

	private TaggedString OfferingLabel()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (RequiredOffering != null)
		{
			stringBuilder.Append("PsychicRitualRequiredOffering".Translate().CapitalizeFirst());
			stringBuilder.Append(": ");
			stringBuilder.Append(RequiredOffering.SummaryFilterFirst);
		}
		return stringBuilder.ToString();
	}

	public TaggedString DurationLabel()
	{
		string value = ((int)(hoursUntilOutcome.Average * 2500f)).ToStringTicksToPeriod();
		TaggedString taggedString = ((hoursUntilOutcome.min != hoursUntilOutcome.max) ? "ExpectedLordJobDuration".Translate().CapitalizeFirst() : "PsychicRitualExpectedDurationLabel".Translate().CapitalizeFirst());
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(taggedString);
		stringBuilder.Append(": ");
		stringBuilder.Append(value);
		return stringBuilder.ToString();
	}

	private IReadOnlyDictionary<PsychicRitualRoleDef, List<IntVec3>> GenerateRolePositions(PsychicRitualRoleAssignments assignments)
	{
		tmpParticipants.ClearAndPoolValueLists();
		foreach (PsychicRitualRoleDef role in Roles)
		{
			tmpParticipants[role] = SimplePool<List<IntVec3>>.Get();
		}
		int num = assignments.RoleAssignedCount(ChanterRole) + assignments.RoleAssignedCount(InvokerRole);
		int num2 = 0;
		foreach (Pawn item in assignments.AssignedPawns(InvokerRole))
		{
			_ = item;
			int num3 = 0;
			IntVec3 cell;
			do
			{
				cell = assignments.Target.Cell;
				cell += IntVec3.FromPolar(360f * (float)num2++ / (float)num, invocationCircleRadius);
			}
			while (!cell.Walkable(assignments.Target.Map) && num3++ <= 10);
			if (num3 >= 10)
			{
				cell = assignments.Target.Cell;
			}
			tmpParticipants[InvokerRole].Add(cell);
		}
		foreach (Pawn item2 in assignments.AssignedPawns(ChanterRole))
		{
			_ = item2;
			IntVec3 cell2 = assignments.Target.Cell;
			cell2 += IntVec3.FromPolar(360f * (float)num2++ / (float)num, invocationCircleRadius);
			tmpParticipants[ChanterRole].Add(cell2);
		}
		foreach (Pawn item3 in assignments.AssignedPawns(TargetRole))
		{
			_ = item3;
			tmpParticipants[TargetRole].Add(assignments.Target.Cell);
		}
		if (DefenderRole != null)
		{
			num2 = 0;
			int num4 = assignments.RoleAssignedCount(DefenderRole);
			bool playerRitual = assignments.AllAssignedPawns.Any((Pawn x) => x.Faction == Faction.OfPlayer);
			foreach (Pawn item4 in assignments.AssignedPawns(DefenderRole))
			{
				_ = item4;
				IntVec3 cell3 = assignments.Target.Cell;
				cell3 += IntVec3.FromPolar(360f * (float)num2++ / (float)num4, invocationCircleRadius + 5f);
				cell3 = GetBestStandableRolePosition(playerRitual, cell3, assignments.Target.Cell, assignments.Target.Map);
				tmpParticipants[DefenderRole].Add(cell3);
			}
		}
		return tmpParticipants;
	}

	public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		using (new ProfilerBlock("PsychicRitualDef.BlockingIssues"))
		{
			tmpGatheringPawns.Clear();
			foreach (var (psychicRitualRoleDef2, collection) in assignments.RoleAssignments)
			{
				if (psychicRitualRoleDef2.CanHandleOfferings)
				{
					tmpGatheringPawns.AddRange(collection);
				}
			}
			tmpGatheringPawns.RemoveAll(map, (Map _map, Pawn _pawn) => _pawn.MapHeld != _map);
			if (TargetRole != null && InvokerRole != null)
			{
				Pawn pawn = assignments.FirstAssignedPawn(TargetRole);
				if (pawn != null)
				{
					Pawn pawn2 = assignments.FirstAssignedPawn(InvokerRole);
					if (pawn2 != null && pawn.IsPrisoner && !map.reachability.CanReach(assignments.Target.Cell, pawn.PositionHeld, PathEndMode.Touch, TraverseParms.For(pawn2)))
					{
						yield return "PsychicRitualTargetUnreachableByInvoker".Translate(pawn.Named("TARGET"), pawn2.Named("INVOKER"));
					}
				}
			}
			if (RequiredOffering != null && !PsychicRitualDef.OfferingReachable(map, tmpGatheringPawns, RequiredOffering, out var reachableCount))
			{
				yield return "PsychicRitualOfferingsInsufficient".Translate(RequiredOffering.SummaryFilterFirst, reachableCount);
			}
		}
	}

	public override void CalculateMaxPower(PsychicRitualRoleAssignments assignments, List<QualityFactor> powerFactorsOut, out float power)
	{
		power = 0f;
		foreach (Pawn item in assignments.AssignedPawns(InvokerRole))
		{
			float statValue = item.GetStatValue(StatDefOf.PsychicSensitivity);
			float num = PsychicSensitivityToPowerFactor.Evaluate(statValue);
			num *= psychicSensitivityPowerFactor;
			powerFactorsOut?.Add(new QualityFactor
			{
				label = "PsychicRitualDef_InvocationCircle_QualityFactor_PsychicSensitivity".Translate(item.Named("PAWN")),
				positive = (statValue >= 1f),
				count = statValue.ToStringPercent(),
				quality = num,
				toolTip = "PsychicRitualDef_InvocationCircle_QualityFactor_PsychicSensitivity_Tooltip".Translate(item.Named("PAWN"))
			});
			power += num;
		}
		base.CalculateMaxPower(assignments, powerFactorsOut, out var power2);
		power += power2;
		if (assignments.Target.Thing is Building building)
		{
			CalculateFacilityQualityOffset(powerFactorsOut, ref power, building);
		}
		power = Mathf.Clamp01(power);
	}

	private static void CalculateFacilityQualityOffset(List<QualityFactor> powerFactorsOut, ref float power, Building building)
	{
		Dictionary<ThingDef, RitualQualityOffsetCount> dictionary = new Dictionary<ThingDef, RitualQualityOffsetCount>();
		List<Thing> linkedFacilitiesListForReading = building.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading;
		for (int i = 0; i < linkedFacilitiesListForReading.Count; i++)
		{
			Thing thing = linkedFacilitiesListForReading[i];
			CompFacility compFacility = thing.TryGetComp<CompFacility>();
			if (compFacility?.StatOffsets == null)
			{
				continue;
			}
			for (int j = 0; j < compFacility.StatOffsets.Count; j++)
			{
				StatModifier statModifier = compFacility.StatOffsets[j];
				if (statModifier.stat == StatDefOf.PsychicRitualQuality)
				{
					if (dictionary.TryGetValue(thing.def, out var value))
					{
						value.count++;
						value.offset += statModifier.value;
					}
					else
					{
						dictionary.Add(thing.def, new RitualQualityOffsetCount(1, statModifier.value));
					}
				}
			}
		}
		foreach (KeyValuePair<ThingDef, RitualQualityOffsetCount> item in dictionary)
		{
			powerFactorsOut?.Add(new QualityFactor
			{
				label = Find.ActiveLanguageWorker.Pluralize(item.Key.label).CapitalizeFirst(),
				positive = true,
				count = item.Value.count + " / " + item.Key.GetCompProperties<CompProperties_Facility>().maxSimultaneous,
				quality = item.Value.offset,
				toolTip = "PsychicRitualDef_InvocationCircle_QualityFactor_Increase_Tooltip".Translate().CapitalizeFirst().EndWithPeriod()
			});
			power += item.Value.offset;
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (requiredOffering != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.PsychicRituals, "StatsReport_Offering".Translate(), requiredOffering.SummaryFilterFirst, "StatsReport_Offering_Desc".Translate(), 1000);
		}
		yield return new StatDrawEntry(StatCategoryDefOf.PsychicRituals, "StatsReport_RitualDuration".Translate(), Mathf.FloorToInt(hoursUntilOutcome.min * 2500f).ToStringTicksToPeriod(), "StatsReport_RitualDuration_Desc".Translate(), 500);
		yield return new StatDrawEntry(StatCategoryDefOf.PsychicRituals, "StatsReport_RitualCooldown".Translate(), (cooldownHours * 2500).ToStringTicksToPeriod(), "StatsReport_RitualCooldown_Desc".Translate(), 100);
	}

	public override void CheckPsychicRitualCancelConditions(PsychicRitual psychicRitual)
	{
		base.CheckPsychicRitualCancelConditions(psychicRitual);
		if (!psychicRitual.canceled && invokerRole != null)
		{
			Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(InvokerRole);
			if (pawn != null && pawn.DeadOrDowned)
			{
				psychicRitual.CancelPsychicRitual("PsychicRitualDef_InvocationCircle_InvokerLost".Translate(pawn.Named("PAWN")));
			}
		}
	}
}
