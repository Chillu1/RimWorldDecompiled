using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Roof : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		if (rp.noRoof.HasValue && rp.noRoof.Value)
		{
			return;
		}
		RoofGrid roofGrid = BaseGen.globalSettings.map.roofGrid;
		RoofDef def = rp.roofDef ?? RoofDefOf.RoofConstructed;
		foreach (IntVec3 item in rp.rect)
		{
			if (!roofGrid.Roofed(item))
			{
				roofGrid.SetRoof(item, def);
			}
		}
	}
}
