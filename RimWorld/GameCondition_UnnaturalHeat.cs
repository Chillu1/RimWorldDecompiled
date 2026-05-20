using UnityEngine;
using Verse;

namespace RimWorld;

public class GameCondition_UnnaturalHeat : GameCondition
{
	private const float TempChangePerTick = 0.001f;

	private const float DefaultTargetTemp = 20f;

	private float tempOffset;

	public float heaterTargetTemp = 20f;

	private int TargetTempOffset
	{
		get
		{
			int num = 0;
			foreach (Building item in base.SingleMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.AtmosphericHeater))
			{
				num += item.TryGetComp<Comp_AtmosphericHeater>().TempOffset;
			}
			return num;
		}
	}

	public override int TransitionTicks => 2500;

	public override void PostMake()
	{
		base.PostMake();
		tempOffset = 0f;
	}

	public override float TemperatureOffset()
	{
		if (!base.Permanent)
		{
			return Mathf.Lerp(0f, tempOffset, Mathf.Min(1f, (float)base.TicksLeft / (float)TransitionTicks));
		}
		return tempOffset;
	}

	public override void GameConditionTick()
	{
		tempOffset += Mathf.Sign((float)TargetTempOffset - tempOffset) * 0.001f;
		if (base.SingleMap.listerThings.ThingsOfDef(ThingDefOf.AtmosphericHeater).Count == 0)
		{
			base.Permanent = false;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref tempOffset, "tempOffset", 0f);
		Scribe_Values.Look(ref heaterTargetTemp, "heaterTargetTemp", 20f);
	}
}
