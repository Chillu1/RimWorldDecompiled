using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_EdgeMannedMortar : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		CellRect mortarRect;
		if (base.CanResolve(rp))
		{
			return TryFindRandomInnerRectTouchingEdge(rp.rect, out mortarRect);
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		if (TryFindRandomInnerRectTouchingEdge(rp.rect, out var mortarRect))
		{
			Rot4 value = (mortarRect.Cells.Any((IntVec3 x) => x.x == rp.rect.minX) ? Rot4.West : (mortarRect.Cells.Any((IntVec3 x) => x.x == rp.rect.maxX) ? Rot4.East : ((!mortarRect.Cells.Any((IntVec3 x) => x.z == rp.rect.minZ)) ? Rot4.North : Rot4.South)));
			ResolveParams resolveParams = rp;
			resolveParams.rect = mortarRect;
			resolveParams.thingRot = value;
			BaseGen.symbolStack.Push("mannedMortar", resolveParams);
		}
	}

	private bool TryFindRandomInnerRectTouchingEdge(CellRect rect, out CellRect mortarRect)
	{
		Map map = BaseGen.globalSettings.map;
		IntVec2 size = new IntVec2(3, 3);
		if (rect.TryFindRandomInnerRectTouchingEdge(size, out mortarRect, (CellRect x) => x.Cells.All((IntVec3 y) => y.Standable(map) && y.GetEdifice(map) == null) && GenConstruct.TerrainCanSupport(x, map, ThingDefOf.Turret_Mortar)))
		{
			return true;
		}
		if (rect.TryFindRandomInnerRectTouchingEdge(size, out mortarRect, (CellRect x) => x.Cells.All((IntVec3 y) => y.Standable(map) && y.GetEdifice(map) == null)))
		{
			return true;
		}
		return false;
	}
}
