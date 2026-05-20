using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompPowerPlantElectroharvester : CompPowerPlant
{
	private CompFacility facilityComp;

	protected override float DesiredPowerOutput => GetPowerOutput();

	public CompFacility FacilityComp => facilityComp ?? (facilityComp = parent.GetComp<CompFacility>());

	public List<Thing> Platforms => FacilityComp.LinkedBuildings;

	private float GetPowerOutput()
	{
		float num = 0f;
		foreach (Thing platform in Platforms)
		{
			if (platform is Building_HoldingPlatform { Occupied: not false } building_HoldingPlatform)
			{
				num += (float)Mathf.RoundToInt(building_HoldingPlatform.HeldPawn.BodySize * (0f - base.Props.PowerConsumption) * 0.1f);
			}
		}
		return Mathf.Clamp(num, 0f, 0f - base.Props.PowerConsumption);
	}

	public override string CompInspectStringExtra()
	{
		string text = "";
		if (Platforms.Empty())
		{
			text = "ElectroharvesterNoPlatform".Translate() + "\n";
		}
		else if (GetPowerOutput() == 0f)
		{
			text = "ElectroharvesterNoEntity".Translate() + "\n";
		}
		return text + base.CompInspectStringExtra();
	}
}
