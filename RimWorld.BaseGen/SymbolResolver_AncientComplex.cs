using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_AncientComplex : SymbolResolver_AncientComplex_Base
{
	private static readonly FloatRange FilthDensity_DriedBlood = new FloatRange(0.01f, 0.025f);

	private static readonly FloatRange FilthDensity_MoldyUniform = new FloatRange(0.005f, 0.01f);

	private static readonly FloatRange FilthDensity_ScatteredDocuments = new FloatRange(0.005f, 0.015f);

	private static readonly IntRange CorpseRandomAgeRange = new IntRange(1080000000, 1260000000);

	protected override LayoutDef DefaultLayoutDef => LayoutDefOf.AncientComplex;

	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		resolveParams.filthDensity = FilthDensity_DriedBlood;
		resolveParams.filthDef = ThingDefOf.Filth_DriedBlood;
		BaseGen.symbolStack.Push("filth", resolveParams);
		resolveParams.ignoreDoorways = true;
		resolveParams.filthDensity = FilthDensity_MoldyUniform;
		resolveParams.filthDef = ThingDefOf.Filth_MoldyUniform;
		BaseGen.symbolStack.Push("filth", resolveParams);
		resolveParams.filthDensity = FilthDensity_ScatteredDocuments;
		resolveParams.filthDef = ThingDefOf.Filth_ScatteredDocuments;
		BaseGen.symbolStack.Push("filth", resolveParams);
		ResolveParams resolveParams2 = rp;
		resolveParams2.desiccatedCorpsePawnKind = PawnKindDefOf.AncientSoldier;
		resolveParams2.desiccatedCorpseRandomAgeRange = CorpseRandomAgeRange;
		BaseGen.symbolStack.Push("desiccatedCorpses", resolveParams2);
		ResolveParams resolveParams3 = rp;
		resolveParams3.floorDef = TerrainDefOf.PackedDirt;
		BaseGen.symbolStack.Push("outdoorsPath", resolveParams3);
		BaseGen.symbolStack.Push("ancientComplexDefences", rp);
		BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp);
		ResolveComplex(rp);
		ResolveParams resolveParams4 = rp;
		resolveParams4.rect = rp.rect.ExpandedBy(5);
		resolveParams4.floorDef = TerrainDefOf.Gravel;
		resolveParams4.chanceToSkipFloor = 0.05f;
		resolveParams4.floorOnlyIfTerrainSupports = true;
		BaseGen.symbolStack.Push("floor", resolveParams4);
		foreach (IntVec3 item in resolveParams4.rect)
		{
			Building edifice = item.GetEdifice(BaseGen.globalSettings.map);
			if (edifice != null && edifice.def.destroyable && edifice.def.IsBuildingArtificial)
			{
				edifice.Destroy();
			}
		}
	}
}
