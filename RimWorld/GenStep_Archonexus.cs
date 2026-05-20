using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_Archonexus : GenStep_Scatterer
{
	private const int Size = 60;

	public override int SeedPart => 473957495;

	public override void Generate(Map map, GenStepParams parms)
	{
		count = 1;
		nearMapCenter = true;
		base.Generate(map, parms);
	}

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (!c.Standable(map))
		{
			return false;
		}
		if (c.Roofed(map))
		{
			return false;
		}
		ThingDef archonexusCore = ThingDefOf.ArchonexusCore;
		IntVec3 c2 = ThingUtility.InteractionCellWhenAt(archonexusCore, c, archonexusCore.defaultPlacingRot, map);
		if (!map.reachability.CanReachMapEdge(c2, TraverseParms.For(TraverseMode.PassDoors)))
		{
			return false;
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
	{
		SitePartParams parms2 = parms.sitePart.parms;
		ResolveParams resolveParams = new ResolveParams
		{
			threatPoints = parms2.threatPoints,
			rect = CellRect.CenteredOn(c, 60, 60)
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("archonexus", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
	}
}
