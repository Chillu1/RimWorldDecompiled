using RimWorld.BaseGen;
using Verse;

namespace RimWorld;

public class GenStep_ArchonexusResearchBuildings_SecondCycle : GenStep_ArchonexusResearchBuildings
{
	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		ResolveParams resolveParams = new ResolveParams
		{
			rect = CellRect.CenteredOn(loc, Size.x, Size.z),
			minorBuildingCount = 4
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("archonexusResearchBuildings", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
	}
}
