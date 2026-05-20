using UnityEngine;
using Verse;

namespace RimWorld;

public class GameCondition_HeatVentHeat : GameCondition
{
	private const float TempChangePerTick = 0.001f;

	private const float DefaultTargetTemp = 20f;

	private float tempOffset;

	private int TargetTempOffset => 15;

	public override int TransitionTicks => 2500;

	public override void PostMake()
	{
		base.PostMake();
		tempOffset = 0f;
	}

	public override float TemperatureOffset()
	{
		return tempOffset;
	}

	public override void GameConditionTick()
	{
		tempOffset += Mathf.Sign((float)TargetTempOffset - tempOffset) * 0.001f;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref tempOffset, "tempOffset", 0f);
	}
}
