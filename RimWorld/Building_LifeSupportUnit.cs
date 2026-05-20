using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_LifeSupportUnit : Building
{
	private Room cachedRoom;

	private const float EfficiencyFalloffSpan = 100f;

	private const float TargetTemperature = 20f;

	private const float EnergyPerSecond = 30f;

	private const float AirPerSecondPerHundredCells = 0.05f;

	private const float IntervalToPerSecond = 4.1666665f;

	private Room Room => cachedRoom ?? (cachedRoom = this.GetRoom());

	public override void TickRare()
	{
		if (base.Spawned)
		{
			ComputeTempChange();
			ComputeVacuum();
			cachedRoom = null;
		}
	}

	private void ComputeTempChange()
	{
		float ambientTemperature = base.AmbientTemperature;
		float num = ((ambientTemperature < 20f) ? 1f : ((!(ambientTemperature > 120f)) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f));
		float energyLimit = 30f * num * 4.1666665f;
		float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, 20f);
		if (!Mathf.Approximately(num2, 0f))
		{
			Room.Temperature += num2;
		}
	}

	private void ComputeVacuum()
	{
		if (base.Map.Biome.inVacuum && !Room.ExposedToSpace)
		{
			float num = 100f / (float)Room.CellCount * 0.05f * 4.1666665f;
			Room.Vacuum -= num;
		}
	}
}
