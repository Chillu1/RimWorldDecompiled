using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Power : CompProperties
{
	public class PowerUpgrade
	{
		public ResearchProjectDef researchProject;

		public float factor;
	}

	public bool transmitsPower;

	private float basePowerConsumption;

	public float idlePowerDraw = -1f;

	public bool shortCircuitInRain;

	public bool showPowerNeededIfOff = true;

	public SoundDef soundPowerOn;

	public SoundDef soundPowerOff;

	public SoundDef soundAmbientPowered;

	public SoundDef soundAmbientProducingPower;

	public List<PowerUpgrade> powerUpgrades;

	public bool alwaysDisplayAsUsingPower;

	public float PowerConsumption
	{
		get
		{
			float num = basePowerConsumption;
			if (Current.ProgramState == ProgramState.Entry)
			{
				return num;
			}
			for (int i = 0; i < powerUpgrades?.Count; i++)
			{
				ResearchProjectDef researchProject = powerUpgrades[i].researchProject;
				if (researchProject != null && researchProject.IsFinished)
				{
					num *= powerUpgrades[i].factor;
				}
			}
			return num;
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (basePowerConsumption > 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "PowerConsumption".Translate(), PowerConsumption.ToString("F0") + " W", "Stat_Thing_PowerConsumption_Desc".Translate(), 5000);
		}
	}
}
