using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompCauseGameCondition_TemperatureOffset : CompCauseGameCondition
{
	public float temperatureOffset;

	private const float MaxTempForMinOffset = -5f;

	private const float MinTempForMaxOffset = 20f;

	public new CompProperties_CausesGameCondition_ClimateAdjuster Props => (CompProperties_CausesGameCondition_ClimateAdjuster)props;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		temperatureOffset = Props.temperatureOffsetRange.min;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref temperatureOffset, "temperatureOffset", 0f);
	}

	private string GetFloatStringWithSign(float val)
	{
		if (val < 0f)
		{
			return val.ToString("0");
		}
		return "+" + val.ToString("0");
	}

	public void SetTemperatureOffset(float offset)
	{
		temperatureOffset = Props.temperatureOffsetRange.ClampToRange(offset);
		ReSetupAllConditions();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Prefs.DevMode && DebugSettings.godMode)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "-10";
			command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
			{
				SetTemperatureOffset(temperatureOffset - 10f);
			});
			command_Action.hotKey = KeyBindingDefOf.Misc1;
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "-1";
			command_Action2.action = (Action)Delegate.Combine(command_Action2.action, (Action)delegate
			{
				SetTemperatureOffset(temperatureOffset - 1f);
			});
			command_Action2.hotKey = KeyBindingDefOf.Misc2;
			yield return command_Action2;
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "+1";
			command_Action3.action = (Action)Delegate.Combine(command_Action3.action, (Action)delegate
			{
				SetTemperatureOffset(temperatureOffset + 1f);
			});
			command_Action3.hotKey = KeyBindingDefOf.Misc3;
			yield return command_Action3;
			Command_Action command_Action4 = new Command_Action();
			command_Action4.defaultLabel = "+10";
			command_Action4.action = (Action)Delegate.Combine(command_Action4.action, (Action)delegate
			{
				SetTemperatureOffset(temperatureOffset + 10f);
			});
			command_Action4.hotKey = KeyBindingDefOf.Misc4;
			yield return command_Action4;
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + ("Temperature".Translate() + ": " + GetFloatStringWithSign(temperatureOffset));
	}

	protected override void SetupCondition(GameCondition condition, Map map)
	{
		base.SetupCondition(condition, map);
		((GameCondition_TemperatureOffset)condition).tempOffset = temperatureOffset;
	}

	public override void RandomizeSettings(Site site)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
		{
			if (!(allWorldObject is Settlement settlement) || settlement.Faction != Faction.OfPlayer)
			{
				continue;
			}
			if (settlement.Map != null)
			{
				bool flag3 = false;
				foreach (GameCondition activeCondition in settlement.Map.GameConditionManager.ActiveConditions)
				{
					if (activeCondition is GameCondition_TemperatureOffset)
					{
						float num = activeCondition.TemperatureOffset();
						if (num > 0f)
						{
							flag3 = true;
							flag = true;
							flag2 = false;
						}
						else if (num < 0f)
						{
							flag3 = true;
							flag2 = true;
							flag = false;
						}
						if (flag3)
						{
							break;
						}
					}
				}
				if (flag3)
				{
					break;
				}
			}
			PlanetTile tile = allWorldObject.Tile;
			if ((float)Find.WorldGrid.TraversalDistanceBetween(site.Tile, tile, passImpassable: true, Props.worldRange + 1) <= (float)Props.worldRange)
			{
				float num2 = GenTemperature.MinTemperatureAtTile(tile);
				float num3 = GenTemperature.MaxTemperatureAtTile(tile);
				if (num2 < -5f)
				{
					flag2 = true;
				}
				if (num3 > 20f)
				{
					flag = true;
				}
			}
		}
		if (flag2 == flag)
		{
			temperatureOffset = (Rand.Bool ? Props.temperatureOffsetRange.min : Props.temperatureOffsetRange.max);
		}
		else if (flag2)
		{
			temperatureOffset = Props.temperatureOffsetRange.min;
		}
		else if (flag)
		{
			temperatureOffset = Props.temperatureOffsetRange.max;
		}
	}
}
