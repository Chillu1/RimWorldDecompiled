using Verse;

namespace RimWorld;

public class GenStep_InsectLairSite : GenStep
{
	public override int SeedPart => 49817324;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (TileMutatorWorker_InsectMegahive.TryGetEntranceCell(map, out var entranceCell))
		{
			TileMutatorWorker_InsectMegahive.GenerateMegahiveEntrance(map, entranceCell, spawnGravcore: true);
		}
	}
}
