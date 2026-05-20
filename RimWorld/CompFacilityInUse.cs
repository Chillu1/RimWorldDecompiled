using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompFacilityInUse : ThingComp
{
	[Unsaved(false)]
	private bool operatingAtHighPower;

	[Unsaved(false)]
	private Effecter effecterInUse;

	[Unsaved(false)]
	private CompPowerTrader intPowerTrader;

	public CompProperties_FacilityInUse Props => props as CompProperties_FacilityInUse;

	public CompPowerTrader PowerTrader => intPowerTrader ?? (intPowerTrader = parent.GetComp<CompPowerTrader>());

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		effecterInUse?.Cleanup();
		effecterInUse = null;
	}

	public override void CompTickRare()
	{
		CompTick();
	}

	public override void CompTickLong()
	{
		CompTick();
	}

	public override void CompTick()
	{
		List<Thing> list = parent.TryGetComp<CompFacility>()?.LinkedBuildings;
		if (list == null)
		{
			return;
		}
		CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
		Thing thing = null;
		foreach (Thing item in list)
		{
			if (compPowerTrader.PowerOn && BuildingInUse(item))
			{
				thing = item;
				break;
			}
		}
		bool flag = thing != null;
		operatingAtHighPower = false;
		if (Props.inUsePowerConsumption.HasValue)
		{
			float num = compPowerTrader.Props.PowerConsumption;
			if (flag)
			{
				num = Props.inUsePowerConsumption.Value;
				operatingAtHighPower = true;
			}
			compPowerTrader.PowerOutput = 0f - num;
		}
		if (Props.effectInUse == null)
		{
			return;
		}
		if (flag)
		{
			if (effecterInUse == null)
			{
				effecterInUse = Props.effectInUse.Spawn();
				effecterInUse.Trigger(parent, thing);
			}
			effecterInUse.EffectTick(parent, thing);
		}
		if (!flag && effecterInUse != null)
		{
			effecterInUse.Cleanup();
			effecterInUse = null;
		}
	}

	private bool BuildingInUse(Thing building)
	{
		if (building is Building_Bed { AnyOccupants: not false })
		{
			return true;
		}
		return false;
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Format(arg1: (PowerTrader.Off ? ((string)"PowerConsumptionOff".Translate()) : (operatingAtHighPower ? ((string)"PowerConsumptionHigh".Translate()) : ((string)"PowerConsumptionLow".Translate()))).CapitalizeFirst(), format: "{0}: {1}", arg0: "PowerConsumptionMode".Translate()));
		return stringBuilder.ToString();
	}
}
