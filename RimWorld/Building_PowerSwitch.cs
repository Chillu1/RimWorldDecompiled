using System.Text;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_PowerSwitch : Building
{
	private bool wantsOnOld = true;

	private CompFlickable flickableComp;

	public override bool TransmitsPowerNow => FlickUtility.WantsToBeOn(this);

	public override Graphic Graphic => flickableComp.CurrentGraphic;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		flickableComp = GetComp<CompFlickable>();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (flickableComp == null)
			{
				flickableComp = GetComp<CompFlickable>();
			}
			wantsOnOld = !FlickUtility.WantsToBeOn(this);
			UpdatePowerGrid();
		}
	}

	protected override void ReceiveCompSignal(string signal)
	{
		switch (signal)
		{
		case "FlickedOff":
		case "FlickedOn":
		case "ScheduledOn":
		case "ScheduledOff":
			UpdatePowerGrid();
			break;
		}
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		if (stringBuilder.Length != 0)
		{
			stringBuilder.AppendLine();
		}
		stringBuilder.Append("PowerSwitch_Power".Translate() + ": ");
		if (FlickUtility.WantsToBeOn(this))
		{
			stringBuilder.Append("On".Translate().ToLower());
		}
		else
		{
			stringBuilder.Append("Off".Translate().ToLower());
		}
		return stringBuilder.ToString();
	}

	private void UpdatePowerGrid()
	{
		if (FlickUtility.WantsToBeOn(this) != wantsOnOld)
		{
			if (base.Spawned)
			{
				base.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(base.PowerComp);
			}
			wantsOnOld = FlickUtility.WantsToBeOn(this);
		}
	}
}
