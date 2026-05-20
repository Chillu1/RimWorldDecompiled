using Verse;

namespace RimWorld;

public class CompPowerPlantGravcore : CompPowerPlant
{
	private const int CheckSubstructureInterval = 60;

	private bool onSubstructure;

	protected override float DesiredPowerOutput
	{
		get
		{
			if (!onSubstructure)
			{
				return 0f;
			}
			return base.DesiredPowerOutput;
		}
	}

	public override void CompTickInterval(int delta)
	{
		if (parent.Spawned && parent.IsHashIntervalTick(60, delta))
		{
			CheckSubstructure();
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		CheckSubstructure();
	}

	private void CheckSubstructure()
	{
		onSubstructure = parent.Map.terrainGrid.FoundationAt(parent.Position)?.IsSubstructure ?? false;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref onSubstructure, "onSubstructure", defaultValue: false);
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (onSubstructure)
		{
			return text;
		}
		return text + "\n" + ("Disabled".Translate() + ": " + "MessageMustBePlacedOnSubstructure".Translate()).Colorize(ColorLibrary.RedReadable);
	}
}
