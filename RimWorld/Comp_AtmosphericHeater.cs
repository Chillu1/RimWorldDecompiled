using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Comp_AtmosphericHeater : CompTempControl, IThingGlower
{
	private class Command_GroupedTempChange : Command_Action
	{
		private Comp_AtmosphericHeater heater;

		private float offset;

		public Command_GroupedTempChange(Comp_AtmosphericHeater heater, float offset)
		{
			this.heater = heater;
			this.offset = offset;
		}

		public override void ProcessInput(Event ev)
		{
		}

		public override void ProcessGroupInput(Event ev, List<Gizmo> group)
		{
			heater.InterfaceChangeTargetTemperature(offset);
		}
	}

	private const int DisabledTextureIndex = 0;

	private const int EnabledTextureIndex = 1;

	private const int MaxOffset = 10;

	private CompPowerTrader powerTraderComp;

	private CompHeatPusher heatPusherComp;

	private CompRefuelable refuelableComp;

	private GameCondition_UnnaturalHeat Condition
	{
		get
		{
			GameCondition_UnnaturalHeat gameCondition_UnnaturalHeat = parent.Map.gameConditionManager.GetActiveCondition(GameConditionDefOf.UnnaturalHeat) as GameCondition_UnnaturalHeat;
			if (gameCondition_UnnaturalHeat == null)
			{
				gameCondition_UnnaturalHeat = (GameCondition_UnnaturalHeat)GameConditionMaker.MakeCondition(GameConditionDefOf.UnnaturalHeat);
				parent.Map.GameConditionManager.RegisterCondition(gameCondition_UnnaturalHeat);
				gameCondition_UnnaturalHeat.Permanent = true;
			}
			return gameCondition_UnnaturalHeat;
		}
	}

	public override float TargetTemperature
	{
		get
		{
			return Condition.heaterTargetTemp;
		}
		set
		{
			Condition.heaterTargetTemp = value;
		}
	}

	private bool Powered => powerTraderComp.PowerOn;

	public int TempOffset
	{
		get
		{
			if (!Powered || !refuelableComp.HasFuel || !(TargetTemperature > parent.Map.mapTemperature.OutdoorTemp))
			{
				return 0;
			}
			return 10;
		}
	}

	private bool Working
	{
		get
		{
			if (Powered && refuelableComp.HasFuel)
			{
				return TargetTemperature > parent.Map.mapTemperature.OutdoorTemp - 1f;
			}
			return false;
		}
	}

	public bool ShouldBeLitNow()
	{
		return Working;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Atmospheric heater"))
		{
			parent.Destroy();
			return;
		}
		base.PostSpawnSetup(respawningAfterLoad);
		powerTraderComp = parent.GetComp<CompPowerTrader>();
		heatPusherComp = parent.GetComp<CompHeatPusher>();
		refuelableComp = parent.GetComp<CompRefuelable>();
	}

	public override void CompTick()
	{
		if (Working)
		{
			powerTraderComp.PowerOutput = 0f - powerTraderComp.Props.PowerConsumption;
			refuelableComp.Notify_UsedThisTick();
			heatPusherComp.enabled = true;
		}
		else
		{
			powerTraderComp.PowerOutput = (0f - powerTraderComp.Props.PowerConsumption) * base.Props.lowPowerConsumptionFactor;
			heatPusherComp.enabled = false;
		}
		int num = (Working ? 1 : 0);
		if (parent.overrideGraphicIndex != num)
		{
			parent.overrideGraphicIndex = num;
			parent.DirtyMapMesh(parent.Map);
			parent.TryGetComp<CompGlower>()?.UpdateLit(parent.Map);
		}
		operatingAtHighPower = Working;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		float num = RoundedToCurrentTempModeOffset(-10f);
		Command_GroupedTempChange command_GroupedTempChange = new Command_GroupedTempChange(this, num);
		command_GroupedTempChange.defaultLabel = num.ToStringTemperatureOffset("F0");
		command_GroupedTempChange.defaultDesc = "CommandLowerTempDesc".Translate();
		command_GroupedTempChange.hotKey = KeyBindingDefOf.Misc5;
		command_GroupedTempChange.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower");
		yield return command_GroupedTempChange;
		float num2 = RoundedToCurrentTempModeOffset(-1f);
		Command_GroupedTempChange command_GroupedTempChange2 = new Command_GroupedTempChange(this, num2);
		command_GroupedTempChange2.defaultLabel = num2.ToStringTemperatureOffset("F0");
		command_GroupedTempChange2.defaultDesc = "CommandLowerTempDesc".Translate();
		command_GroupedTempChange2.hotKey = KeyBindingDefOf.Misc4;
		command_GroupedTempChange2.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower");
		yield return command_GroupedTempChange2;
		Command_Action command_Action = new Command_Action();
		command_Action.action = delegate
		{
			TargetTemperature = 21f;
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			ThrowCurrentTemperatureText();
		};
		command_Action.defaultLabel = "CommandResetTemp".Translate();
		command_Action.defaultDesc = "CommandResetTempDesc".Translate();
		command_Action.hotKey = KeyBindingDefOf.Misc1;
		command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempReset");
		yield return command_Action;
		float num3 = RoundedToCurrentTempModeOffset(1f);
		Command_GroupedTempChange command_GroupedTempChange3 = new Command_GroupedTempChange(this, num3);
		command_GroupedTempChange3.defaultLabel = "+" + num3.ToStringTemperatureOffset("F0");
		command_GroupedTempChange3.defaultDesc = "CommandRaiseTempDesc".Translate();
		command_GroupedTempChange3.hotKey = KeyBindingDefOf.Misc2;
		command_GroupedTempChange3.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise");
		yield return command_GroupedTempChange3;
		float num4 = RoundedToCurrentTempModeOffset(10f);
		Command_GroupedTempChange command_GroupedTempChange4 = new Command_GroupedTempChange(this, num4);
		command_GroupedTempChange4.defaultLabel = "+" + num4.ToStringTemperatureOffset("F0");
		command_GroupedTempChange4.defaultDesc = "CommandRaiseTempDesc".Translate();
		command_GroupedTempChange4.hotKey = KeyBindingDefOf.Misc3;
		command_GroupedTempChange4.icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise");
		yield return command_GroupedTempChange4;
	}
}
