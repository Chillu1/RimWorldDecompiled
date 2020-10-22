using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_Heater : Building_TempControl
	{
		private const float EfficiencyFalloffSpan = 100f;

		public override void TickRare()
		{
			if (compPowerTrader.PowerOn)
			{
				float ambientTemperature = base.AmbientTemperature;
				float num = ((ambientTemperature < 20f) ? 1f : ((!(ambientTemperature > 120f)) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f));
				float energyLimit = compTempControl.Props.energyPerSecond * num * 4.16666651f;
				float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, compTempControl.targetTemperature);
				bool flag = !Mathf.Approximately(num2, 0f);
				CompProperties_Power props = compPowerTrader.Props;
				if (flag)
				{
					this.GetRoomGroup().Temperature += num2;
					compPowerTrader.PowerOutput = 0f - props.basePowerConsumption;
				}
				else
				{
					compPowerTrader.PowerOutput = (0f - props.basePowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
				}
				compTempControl.operatingAtHighPower = flag;
			}
		}
	}
}
