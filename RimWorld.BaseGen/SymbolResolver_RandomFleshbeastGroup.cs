using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public class SymbolResolver_RandomFleshbeastGroup : SymbolResolver
{
	private static readonly IntRange DefaultFleshbeastsCountRange = new IntRange(2, 5);

	public override void Resolve(ResolveParams rp)
	{
		if (ModsConfig.AnomalyActive)
		{
			int num = rp.fleshbeastsCount ?? DefaultFleshbeastsCountRange.RandomInRange;
			Lord lord = rp.singlePawnLord;
			if (lord == null && num > 0)
			{
				lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), BaseGen.globalSettings.map);
			}
			for (int i = 0; i < num; i++)
			{
				PawnKindDef singlePawnKindDef = rp.singlePawnKindDef ?? FleshbeastUtility.AllFleshbeasts.RandomElement();
				ResolveParams resolveParams = rp;
				resolveParams.singlePawnKindDef = singlePawnKindDef;
				resolveParams.singlePawnLord = lord;
				resolveParams.faction = Faction.OfEntities;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
		}
	}
}
