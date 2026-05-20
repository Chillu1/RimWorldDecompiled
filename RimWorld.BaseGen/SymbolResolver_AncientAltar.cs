using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_AncientAltar : SymbolResolver
{
	private const int AncillaryRoomSize = 11;

	private const int AncillaryRoomCornerRadius = 3;

	private const int EntrancePathLength = 3;

	private static readonly IntVec2 MainRoomSize = new IntVec2(17, 21);

	private const string UnfoggedSignalPrefix = "UnfoggedSignal";

	public static IntVec2 Size => new IntVec2(MainRoomSize.x + 22, MainRoomSize.z + 22);

	public override void Resolve(ResolveParams rp)
	{
		rp.floorDef = (Rand.Bool ? TerrainDefOf.FlagstoneSandstone : TerrainDefOf.TileSandstone);
		rp.wallStuff = BaseGenUtility.RandomCheapWallStuff(rp.faction, notVeryFlammable: true);
		CellRect rect = CellRect.CenteredOn(rp.rect.CenterCell, MainRoomSize.x, MainRoomSize.z);
		CellRect rect2 = new CellRect(rect.CenterCell.x - 5, rect.minZ + 2, 1, rect.Height - 4);
		CellRect rect3 = new CellRect(rect.CenterCell.x + 5, rect.minZ + 2, 1, rect.Height - 4);
		CellRect cellRect = rect.ExpandedBy(-1);
		CellRect rect4 = new CellRect(cellRect.minX, cellRect.minZ + 3, 1, cellRect.Height - 6);
		CellRect rect5 = new CellRect(cellRect.maxX, cellRect.minZ + 3, 1, cellRect.Height - 6);
		ResolveParams resolveParams = rp;
		resolveParams.rect = rect4;
		resolveParams.singleThingStuff = rp.wallStuff;
		if (rp.exteriorThreatPoints.HasValue && Faction.OfMechanoids != null)
		{
			ResolveParams resolveParams2 = rp;
			resolveParams2.rect = rp.rect.ExpandedBy(10);
			resolveParams2.sleepingMechanoidsWakeupSignalTag = rp.triggerSecuritySignal;
			resolveParams2.sendWokenUpMessage = false;
			resolveParams2.threatPoints = rp.exteriorThreatPoints.Value;
			BaseGen.symbolStack.Push("sleepingMechanoids", resolveParams2);
		}
		int num = Rand.Range(2, 6);
		for (int i = 0; i < num; i++)
		{
			resolveParams.singleThingDef = (Rand.Bool ? ThingDefOf.Sarcophagus : ThingDefOf.Urn);
			resolveParams.thingRot = Rot4.North;
			BaseGen.symbolStack.Push("edgeThing", resolveParams);
		}
		num = Rand.Range(1, 6);
		resolveParams.rect = rect5;
		for (int j = 0; j < num; j++)
		{
			resolveParams.singleThingDef = (Rand.Bool ? ThingDefOf.Sarcophagus : ThingDefOf.Urn);
			resolveParams.thingRot = Rot4.North;
			BaseGen.symbolStack.Push("edgeThing", resolveParams);
		}
		CellRect cellRect2 = new CellRect(rect.minX - 11 + 1, rect.minZ + 5, 11, 11);
		CellRect cellRect3 = new CellRect(rect.maxX, rect.minZ + 5, 11, 11);
		CellRect cellRect4 = new CellRect(rect.minX + rect.Width / 2 - 5, rect.maxZ, 11, 11);
		CellRect[] array = new CellRect[3] { cellRect2, cellRect3, cellRect4 };
		ResolveParams resolveParams3 = rp;
		resolveParams3.rect = rect.ContractedBy(3);
		resolveParams3.unfoggedSignalTag = "UnfoggedSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
		BaseGen.symbolStack.Push("unfoggedTrigger", resolveParams3);
		ResolveParams resolveParams4 = rp;
		resolveParams4.sound = SoundDefOf.AncientRelicRoomReveal;
		resolveParams4.soundOneShotActionSignalTag = resolveParams3.unfoggedSignalTag;
		BaseGen.symbolStack.Push("soundOneShotAction", resolveParams4);
		ResolveParams resolveParams5 = rp;
		resolveParams5.singleThingDef = ThingDefOf.SteleLarge;
		IntVec2 size = ThingDefOf.SteleLarge.size;
		resolveParams5.rect = new CellRect(rect2.minX + 1, rect.maxZ - size.x, size.x, size.z);
		BaseGen.symbolStack.Push("thing", resolveParams5);
		resolveParams5.rect = new CellRect(rect3.maxX - size.x, rect.maxZ - size.x, size.x, size.z);
		BaseGen.symbolStack.Push("thing", resolveParams5);
		ResolveParams resolveParams6 = rp;
		resolveParams6.singleThingDef = ThingDefOf.Column;
		resolveParams6.singleThingStuff = rp.wallStuff;
		resolveParams6.fillWithThingsPadding = 2;
		resolveParams6.rect = rect2;
		BaseGen.symbolStack.Push("fillWithThings", resolveParams6);
		resolveParams6.rect = rect3;
		BaseGen.symbolStack.Push("fillWithThings", resolveParams6);
		Thing item = rp.relicThing ?? ThingMaker.MakeThing(ThingDefOf.Beer);
		Thing thing = ThingMaker.MakeThing(ThingDefOf.Reliquary, BaseGenUtility.CheapStuffFor(ThingDefOf.Reliquary, rp.faction));
		thing.TryGetComp<CompThingContainer>().innerContainer.TryAdd(item);
		ResolveParams resolveParams7 = rp;
		resolveParams7.sound = SoundDefOf.AncientRelicTakenAlarm;
		resolveParams7.soundOneShotActionSignalTag = rp.triggerSecuritySignal;
		BaseGen.symbolStack.Push("soundOneShotAction", resolveParams7);
		CellRect rect6 = CellRect.CenteredOn(rect.CenterCell, thing.def.Size.x, thing.def.Size.z);
		ResolveParams resolveParams8 = rp;
		resolveParams8.rect = rect6;
		resolveParams8.triggerContainerEmptiedSignalTag = rp.triggerSecuritySignal;
		resolveParams8.triggerContainerEmptiedThing = thing;
		BaseGen.symbolStack.Push("containerEmptiedTrigger", resolveParams8);
		ResolveParams resolveParams9 = rp;
		resolveParams9.rect = rect6;
		resolveParams9.thingRot = Rot4.South;
		resolveParams9.singleThingToSpawn = thing;
		BaseGen.symbolStack.Push("thing", resolveParams9);
		CellRect rect7 = resolveParams9.rect.ExpandedBy(2);
		foreach (IntVec3 corner in rect7.Corners)
		{
			ResolveParams resolveParams10 = rp;
			resolveParams10.faction = Faction.OfAncients;
			resolveParams10.singleThingDef = ThingDefOf.AncientLamp;
			resolveParams10.rect = CellRect.CenteredOn(corner, 1, 1);
			BaseGen.symbolStack.Push("thing", resolveParams10);
		}
		ResolveParams resolveParams11 = rp;
		resolveParams11.rect = rect7;
		resolveParams11.floorDef = TerrainDefOf.PavedTile;
		BaseGen.symbolStack.Push("floor", resolveParams11);
		ResolveParams resolveParams12 = rp;
		resolveParams12.floorDef = TerrainDefOf.Gravel;
		BaseGen.symbolStack.Push("outdoorsPath", resolveParams12);
		ResolveParams resolveParams13 = rp;
		CellRect rect8 = (resolveParams13.rect = new CellRect(rect.minX + rect.Width / 2, rect.minZ, 1, 1));
		resolveParams13.singleThingDef = ThingDefOf.Door;
		resolveParams13.singleThingStuff = rp.wallStuff;
		BaseGen.symbolStack.Push("thing", resolveParams13);
		ResolveParams resolveParams14 = rp;
		resolveParams14.rect = rect8;
		resolveParams14.singleThingDef = ThingDefOf.AncientLamp;
		resolveParams14.rect = new CellRect(rect8.minX - 1, rect8.minZ - 1, 1, 1);
		BaseGen.symbolStack.Push("thing", resolveParams14);
		BaseGen.symbolStack.Push("clear", resolveParams14);
		resolveParams14.rect = new CellRect(rect8.maxX + 1, rect8.minZ - 1, 1, 1);
		BaseGen.symbolStack.Push("thing", resolveParams14);
		BaseGen.symbolStack.Push("clear", resolveParams14);
		ResolveParams resolveParams15 = resolveParams13;
		resolveParams15.rect = new CellRect(rect.minX - 1, cellRect2.minZ + 3, 2, cellRect2.Height - 6);
		BaseGen.symbolStack.Push("floor", resolveParams15);
		BaseGen.symbolStack.Push("clear", resolveParams15);
		ResolveParams resolveParams16 = resolveParams13;
		resolveParams16.rect = new CellRect(rect.maxX, cellRect3.minZ + 3, 2, cellRect3.Height - 6);
		BaseGen.symbolStack.Push("floor", resolveParams16);
		BaseGen.symbolStack.Push("clear", resolveParams16);
		ResolveParams resolveParams17 = resolveParams13;
		resolveParams17.rect = new CellRect(cellRect4.minX + 3, rect.maxZ, cellRect4.Width - 6, 2);
		BaseGen.symbolStack.Push("floor", resolveParams17);
		BaseGen.symbolStack.Push("clear", resolveParams17);
		resolveParams13.rect = rect;
		resolveParams13.cornerRadius = 3;
		BaseGen.symbolStack.Push("emptyRoomRounded", resolveParams13);
		resolveParams13.rect = new CellRect(rect8.minX, rect8.minZ - 3, 1, 3);
		BaseGen.symbolStack.Push("clear", resolveParams13);
		if (rp.interiorThreatPoints.HasValue)
		{
			ResolveParams resolveParams18 = rp;
			resolveParams18.threatPoints = rp.interiorThreatPoints.Value / (float)array.Length;
			foreach (CellRect cellRect5 in array)
			{
				resolveParams18.sleepingMechanoidsWakeupSignalTag = rp.triggerSecuritySignal;
				resolveParams18.ancientCryptosleepCasketOpenSignalTag = rp.triggerSecuritySignal;
				resolveParams18.rect = cellRect5.ContractedBy(3);
				resolveParams18.sendWokenUpMessage = false;
				BaseGen.symbolStack.Push("ancientComplex_interior_sleepingMechanoids", resolveParams18);
			}
		}
		ResolveParams resolveParams19 = rp;
		resolveParams19.cornerRadius = 3;
		CellRect[] array2 = array;
		foreach (CellRect rect9 in array2)
		{
			resolveParams19.rect = rect9;
			BaseGen.symbolStack.Push("emptyRoomRounded", resolveParams19);
		}
	}
}
