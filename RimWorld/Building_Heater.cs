using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_Heater : Building_TempControl
{
	private const float EfficiencyFalloffSpan = 100f;

	public override void TickRare()
	{
		if (base.Spawned && compPowerTrader.PowerOn)
		{
			float ambientTemperature = base.AmbientTemperature;
			float num = ((ambientTemperature < 20f) ? 1f : ((!(ambientTemperature > 120f)) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f));
			float energyLimit = compTempControl.Props.energyPerSecond * num * 4.1666665f;
			float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, compTempControl.TargetTemperature);
			bool flag = !Mathf.Approximately(num2, 0f);
			CompProperties_Power props = compPowerTrader.Props;
			if (flag)
			{
				this.GetRoom().Temperature += num2;
				compPowerTrader.PowerOutput = 0f - props.PowerConsumption;
			}
			else
			{
				compPowerTrader.PowerOutput = (0f - props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
			}
			compTempControl.operatingAtHighPower = flag;
		}
	}
}
