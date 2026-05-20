using UnityEngine;
using Verse;

namespace RimWorld;

public class CompLowPowerInSpace : ThingComp
{
	private bool lowPowerMode;

	private CompPowerTrader intPowerTrader;

	private CompProperties_LowPowerUnlessVacuum Props => (CompProperties_LowPowerUnlessVacuum)props;

	private CompPowerTrader PowerTrader => intPowerTrader ?? (intPowerTrader = parent.GetComp<CompPowerTrader>());

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		CheckUpdatePowerMode();
	}

	public override void CompTick()
	{
		if (parent.IsHashIntervalTick(250))
		{
			CheckUpdatePowerMode();
		}
	}

	public override void CompTickRare()
	{
		CheckUpdatePowerMode();
	}

	private void CheckUpdatePowerMode()
	{
		if (!PowerTrader.Off)
		{
			lowPowerMode = !NeedsPower();
			if (lowPowerMode)
			{
				PowerTrader.PowerOutput = (0f - PowerTrader.Props.PowerConsumption) * Props.lowPowerConsumptionFactor;
			}
			else
			{
				PowerTrader.PowerOutput = 0f - PowerTrader.Props.PowerConsumption;
			}
		}
	}

	private bool NeedsPower()
	{
		if (Props.checkRoomVacuum)
		{
			Room room = parent.GetRoom();
			if (room.ExposedToSpace)
			{
				return false;
			}
			if (Mathf.Approximately(room.Vacuum, 0f))
			{
				return false;
			}
		}
		if (!parent.Map.Biome.inVacuum)
		{
			return false;
		}
		return true;
	}

	public override string CompInspectStringExtra()
	{
		return string.Format(arg1: (PowerTrader.Off ? ((string)"PowerConsumptionOff".Translate()) : ((!lowPowerMode) ? ((string)"PowerConsumptionHigh".Translate()) : ((string)"PowerConsumptionLow".Translate()))).CapitalizeFirst(), format: "{0}: {1}", arg0: "PowerConsumptionMode".Translate());
	}
}
