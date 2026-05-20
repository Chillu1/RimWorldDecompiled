using Verse;

namespace LudeonTK;

public class Dialog_DevNoiseMap : Dialog_DevNoiseBase
{
	protected override string Title => "Map Noise Visualizer";

	protected override void OnNoiseChanged()
	{
		Find.CurrentMap?.mapDrawer.RegenerateEverythingNow();
	}

	protected override int GetSeed()
	{
		return Find.CurrentMap?.ConstantRandSeed ?? 0;
	}

	protected override void DoWindowListing()
	{
		if (Find.CurrentMap == null)
		{
			PrintLabel("Not viewing map");
			return;
		}
		base.DoWindowListing();
		IntVec3 intVec = UI.MouseCell();
		if (!intVec.InBounds(Find.CurrentMap))
		{
			PrintLabel("Out of bounds");
			return;
		}
		float num = (float)noise.GetValue(intVec.x, intVec.y, intVec.z);
		PrintLabel($"Selected cell: {intVec}");
		PrintLabel($"Noise value: {num:0.###}");
	}
}
