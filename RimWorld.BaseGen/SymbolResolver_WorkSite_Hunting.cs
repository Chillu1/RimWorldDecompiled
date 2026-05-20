using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_WorkSite_Hunting : SymbolResolver_WorkSite
{
	private const int AnimalCorpseCountMin = 3;

	private const int AnimalCorpseCountMax = 6;

	public override void Resolve(ResolveParams rp)
	{
		CellRect rect = rp.rect;
		Rot4 butcherTableRot = Rot4.Random;
		CellRect rect2 = rect.ContractedBy(1);
		switch (butcherTableRot.AsByte)
		{
		case 0:
			Place(new CellRect(rect2.minX, rect2.maxZ, rect2.Width, 1));
			break;
		case 1:
			Place(new CellRect(rect2.maxX, rect2.minZ, 1, rect2.Height));
			break;
		case 2:
			Place(new CellRect(rect2.minX, rect2.minZ, rect2.Width, 1));
			break;
		case 3:
			Place(new CellRect(rect2.minX, rect2.minZ, 1, rect2.Height));
			break;
		}
		BiomeDef biome = BaseGen.globalSettings.map.Parent.Biome;
		List<Thing> list = new List<Thing>();
		ThingDef leather = null;
		if (!rp.stockpileConcreteContents.NullOrEmpty())
		{
			leather = rp.stockpileConcreteContents.FirstOrDefault((Thing t) => t.def.IsLeather)?.def;
		}
		if (leather == null)
		{
			leather = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.race != null && def.race.leatherDef != null).RandomElement().race.leatherDef;
		}
		PawnKindDef pawnKindDef = biome.AllWildAnimals.Where((PawnKindDef def) => def.RaceProps.leatherDef == leather).RandomElementWithFallback();
		if (pawnKindDef == null)
		{
			pawnKindDef = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef def) => def.RaceProps.leatherDef == leather).RandomElement();
		}
		int num = Rand.RangeInclusive(3, 6);
		for (int num2 = 0; num2 < num; num2++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef);
			pawn.Kill(null, null);
			list.Add(pawn.Corpse);
		}
		BaseGen.symbolStack.Push("stockpile", new ResolveParams
		{
			rect = rect2,
			stockpileConcreteContents = list
		});
		BaseGen.symbolStack.Push("filth", new ResolveParams
		{
			rect = rect2,
			filthDef = ThingDefOf.Filth_Blood,
			filthDensity = new FloatRange(0.33f, 1.25f)
		});
		base.Resolve(rp);
		void Place(CellRect r)
		{
			BaseGen.symbolStack.Push("edgeThing", new ResolveParams
			{
				singleThingDef = ThingDefOf.TableButcher,
				rect = r,
				edgeThingAvoidOtherEdgeThings = true,
				faction = rp.faction,
				thingRot = butcherTableRot
			});
		}
	}
}
