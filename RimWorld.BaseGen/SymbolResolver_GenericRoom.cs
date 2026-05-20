using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_GenericRoom : SymbolResolver
{
	public string interior;

	public bool useRandomCarpet;

	public bool allowRoof = true;

	public override void Resolve(ResolveParams rp)
	{
		BaseGen.symbolStack.Push("doors", rp);
		if (!interior.NullOrEmpty())
		{
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect.ContractedBy(1);
			BaseGen.symbolStack.Push(interior, resolveParams);
		}
		ResolveParams resolveParams2 = rp;
		if (useRandomCarpet)
		{
			resolveParams2.floorDef = DefDatabase<TerrainDef>.AllDefsListForReading.Where((TerrainDef x) => x.IsCarpet).RandomElement();
		}
		resolveParams2.noRoof = !allowRoof;
		BaseGen.symbolStack.Push("emptyRoom", resolveParams2);
	}
}
