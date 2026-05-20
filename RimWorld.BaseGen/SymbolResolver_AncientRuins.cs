namespace RimWorld.BaseGen;

public class SymbolResolver_AncientRuins : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		resolveParams.wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, notVeryFlammable: true);
		resolveParams.chanceToSkipWallBlock = rp.chanceToSkipWallBlock ?? 0.1f;
		resolveParams.clearEdificeOnly = rp.clearEdificeOnly ?? true;
		resolveParams.noRoof = rp.noRoof ?? true;
		BaseGen.symbolStack.Push("emptyRoom", resolveParams);
	}
}
