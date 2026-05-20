using System.Text;
using Verse;

namespace RimWorld;

public class Building_Vent : Building_TempControl
{
	private CompFlickable flickableComp;

	private VacuumComponent intVacuum;

	public override Graphic Graphic => flickableComp.CurrentGraphic;

	private VacuumComponent Vacuum => intVacuum ?? (intVacuum = base.Map.GetComponent<VacuumComponent>());

	public override bool ExchangeVacuum
	{
		get
		{
			if (!base.ExchangeVacuum)
			{
				return FlickUtility.WantsToBeOn(this);
			}
			return true;
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		flickableComp = GetComp<CompFlickable>();
	}

	public override void TickRare()
	{
		if (FlickUtility.WantsToBeOn(this))
		{
			GenTemperature.EqualizeTemperaturesThroughBuilding(this, 14f, twoWay: true);
			base.Map.gasGrid.EqualizeGasThroughBuilding(this, twoWay: true);
		}
	}

	protected override void ReceiveCompSignal(string signal)
	{
		if (signal == "FlickedOn" || signal == "FlickedOff")
		{
			Vacuum.Dirty();
		}
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		if (!FlickUtility.WantsToBeOn(this))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("VentClosed".Translate());
		}
		return stringBuilder.ToString();
	}
}
