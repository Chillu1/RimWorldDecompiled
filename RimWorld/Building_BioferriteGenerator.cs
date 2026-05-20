using Verse;

namespace RimWorld;

public class Building_BioferriteGenerator : Building
{
	[Unsaved(false)]
	private CompHeatPusherPowered heatPusher;

	private CompHeatPusherPowered HeatPusher => heatPusher ?? (heatPusher = GetComp<CompHeatPusherPowered>());

	public override bool IsWorking()
	{
		return HeatPusher.ShouldPushHeatNow;
	}
}
