namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_ConditionCauser : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			ResolveParams resolveParams = rp;
			resolveParams.singleThingToSpawn = rp.conditionCauser;
			BaseGen.symbolStack.Push("thing", resolveParams);
		}
	}
}
