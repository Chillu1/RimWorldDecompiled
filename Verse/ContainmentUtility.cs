using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class ContainmentUtility
{
	private static readonly SimpleCurve DaysInCaptivityEscapeFactorCurve = new SimpleCurve
	{
		new CurvePoint(15f, 1f),
		new CurvePoint(60f, 1.2f),
		new CurvePoint(120f, 1.5f),
		new CurvePoint(240f, 2f)
	};

	private static readonly SimpleCurve ColdEscapeFactorCurve = new SimpleCurve
	{
		new CurvePoint(-30f, 1.5f),
		new CurvePoint(0f, 1f)
	};

	private static readonly SimpleCurve EscapeIntervalMultiplierFromContainmentStrengthCurve = new SimpleCurve
	{
		new CurvePoint(-0.001f, 0.05f),
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 3f),
		new CurvePoint(200f, 8f)
	};

	private static readonly SimpleCurve StudyKnowledgeMultiplierFromContainmentStrengthCurve = new SimpleCurve
	{
		new CurvePoint(-0.001f, 0.5f),
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 1.3f),
		new CurvePoint(200f, 1.5f)
	};

	public static bool SafeContainerExistsFor(Thing thing)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		foreach (Thing item in thing.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
		{
			if (item.SafelyContains(thing))
			{
				return true;
			}
		}
		return false;
	}

	public static bool SafelyContains(this Thing container, Thing thing)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		CompEntityHolder compEntityHolder = container.TryGetComp<CompEntityHolder>();
		if (compEntityHolder != null)
		{
			return compEntityHolder.ContainmentStrength >= thing.GetStatValue(StatDefOf.MinimumContainmentStrength);
		}
		return false;
	}

	public static float GetEscapeIntervalMultiplier(Thing entity, CompEntityHolder entityHolder)
	{
		return EscapeIntervalMultiplierFromContainmentStrengthCurve.Evaluate(entityHolder.ContainmentStrength - entity.GetStatValue(StatDefOf.MinimumContainmentStrength));
	}

	public static float GetStudyKnowledgeAmountMultiplier(Thing entity, CompEntityHolder entityHolder)
	{
		return StudyKnowledgeMultiplierFromContainmentStrengthCurve.Evaluate(entityHolder.ContainmentStrength - entity.GetStatValue(StatDefOf.MinimumContainmentStrength));
	}

	public static IEnumerable<IntVec3> GetInhibitorAffectedCells(ThingDef def, IntVec3 center, Rot4 rot, Map map)
	{
		CompProperties_Facility comp = def.GetCompProperties<CompProperties_Facility>();
		if (comp == null || IsLinearBuildingBlocked(def, center, rot, map))
		{
			yield break;
		}
		bool edificeFound = false;
		for (float i = comp.minDistance; i < comp.maxDistance; i += 1f)
		{
			if (edificeFound)
			{
				break;
			}
			int num = Mathf.FloorToInt(i);
			IntVec3 intVec = center + IntVec3.South.RotatedBy(rot) * (num - 1 + def.size.z);
			if (intVec.InBounds(map))
			{
				if (intVec.GetEdifice(map) == null)
				{
					yield return intVec;
					continue;
				}
				edificeFound = true;
				yield return intVec;
			}
		}
	}

	public static IntVec3 GetLinearBuildingFrontCell(ThingDef def, IntVec3 center, Rot4 rot)
	{
		if (def.GetCompProperties<CompProperties_Facility>() == null)
		{
			return IntVec3.Invalid;
		}
		return center + IntVec3.South.RotatedBy(rot) * (def.size.z - 1);
	}

	public static bool IsLinearBuildingBlocked(ThingDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
	{
		if (def.GetCompProperties<CompProperties_Facility>() == null)
		{
			return false;
		}
		Building edifice = GetLinearBuildingFrontCell(def, center, rot).GetEdifice(map);
		if (edifice != null)
		{
			return edifice != thingToIgnore;
		}
		return false;
	}

	public static bool ShowContainmentStats(Thing thing)
	{
		CompStudiable compStudiable = thing.TryGetComp<CompStudiable>();
		if (compStudiable != null && thing is Pawn)
		{
			return compStudiable.RequiresHoldingPlatform;
		}
		if (thing.TryGetComp<CompHoldingPlatformTarget>()?.Props.heldPawnKind != null)
		{
			return true;
		}
		return false;
	}

	public static bool TryGetColdContainmentBonus(Thing thing, out float modifier)
	{
		if (thing.TryGetComp(out CompHoldingPlatformTarget comp) && comp.Props.getsColdContainmentBonus)
		{
			modifier = ColdEscapeFactorCurve.Evaluate(thing.AmbientTemperature);
			return true;
		}
		modifier = 0f;
		return false;
	}

	public static float InitiateEscapeMtbDays(Pawn pawn, StringBuilder sb = null)
	{
		if (!CanParticipateInEscape(pawn, sb))
		{
			return -1f;
		}
		CompHoldingPlatformTarget comp = pawn.GetComp<CompHoldingPlatformTarget>();
		float baseEscapeIntervalMtbDays = comp.Props.baseEscapeIntervalMtbDays;
		pawn.GetRoom();
		float num = 1f / Mathf.Clamp(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving), 0.01f, 1f);
		baseEscapeIntervalMtbDays *= num;
		if (sb != null && num > 1f)
		{
			sb.AppendLineIfNotEmpty();
			sb.Append("  - " + "FactorForMovement".Translate() + ": x" + num.ToStringPercent());
		}
		float x = (float)pawn.mindState.entityTicksInCaptivity / 60000f;
		float num2 = DaysInCaptivityEscapeFactorCurve.Evaluate(x);
		baseEscapeIntervalMtbDays *= num2;
		if (num2 > 1f && sb != null)
		{
			sb.AppendLineIfNotEmpty();
			sb.Append("  - " + "FactorForDaysInCaptivity".Translate() + ": x" + num2.ToStringPercent());
		}
		float escapeIntervalMultiplier = GetEscapeIntervalMultiplier(pawn, comp.HeldPlatform.GetComp<CompEntityHolder>());
		baseEscapeIntervalMtbDays *= escapeIntervalMultiplier;
		if (sb != null)
		{
			sb.AppendLineIfNotEmpty();
			sb.Append("  - " + "FactorContainmentStrength".Translate() + ": x" + escapeIntervalMultiplier.ToStringPercent());
		}
		if (comp.Props.getsColdContainmentBonus)
		{
			float num3 = ColdEscapeFactorCurve.Evaluate(pawn.AmbientTemperature);
			baseEscapeIntervalMtbDays *= num3;
			if (num3 > 1f && sb != null)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append("  - " + "FactorForColdCaptivity".Translate() + ": x" + num3.ToStringPercent());
			}
		}
		return baseEscapeIntervalMtbDays;
	}

	public static bool CanParticipateInEscape(Pawn pawn, StringBuilder sb = null)
	{
		if (pawn.Downed)
		{
			if (sb != null)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append("  - " + "EntityDowned".Translate() + ": x0%");
			}
			return false;
		}
		CompHoldingPlatformTarget comp = pawn.GetComp<CompHoldingPlatformTarget>();
		if (comp == null)
		{
			return false;
		}
		if (!comp.CurrentlyHeldOnPlatform)
		{
			return false;
		}
		if (comp.Props.baseEscapeIntervalMtbDays <= 0f)
		{
			if (sb != null)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append("  - " + "IncapableOfEscaping".Translate() + ": x0%");
			}
			return false;
		}
		return true;
	}
}
