using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Ship_Core : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			float value = Rand.Value;
			rp.faction = Faction.OfPlayer;
			ResolveParams resolveParams = rp;
			resolveParams.rect = resolveParams.rect.ContractedBy(2);
			resolveParams.hpPercentRange = new FloatRange(Mathf.Lerp(1.2f, 0.2f, value + Rand.Range(-0.2f, 0.2f)), 1.5f);
			BaseGen.symbolStack.Push("ship_pregen", resolveParams);
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp);
			ResolveParams resolveParams2 = rp;
			resolveParams2.clearFillageOnly = true;
			resolveParams2.clearRoof = true;
			BaseGen.symbolStack.Push("clear", resolveParams2);
		}
	}
}
