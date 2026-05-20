using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld
{
	public class GenStep_AncientComplex : GenStep_LargeRuins
	{
		private LayoutStructureSketch structureSketch;

		private static readonly IntVec2 DefaultComplexSize = new IntVec2(80, 80);

		private IntVec2? actualMin;

		private IntVec2? actualMax;

		public override int SeedPart => 235635649;

		private IntVec2 Size => new IntVec2(structureSketch.structureLayout.container.Width + 10, structureSketch.structureLayout.container.Height + 10);

		protected override LayoutDef LayoutDef => null;

		protected override Faction Faction => Faction.OfAncients;

		protected override IntRange RuinsMinMaxRange => IntRange.One;

		protected override TerrainAffordanceDef MinAffordance => null;

		protected override bool AvoidWaterRoads => false;

		protected override IntVec2 MinSize => actualMin ?? DefaultComplexSize;

		protected override IntVec2 MaxSize => actualMax ?? new IntVec2(100, 100);

		public override void Generate(Map map, GenStepParams parms)
		{
			structureSketch = parms.sitePart.parms.ancientLayoutStructureSketch;
			parms.sitePart.parms.ancientLayoutStructureSketch = null;
			if (structureSketch?.structureLayout == null)
			{
				TryRecoverEmptySketch(parms);
			}
			map.layoutStructureSketches.Add(structureSketch);
			actualMin = (actualMax = Size);
			base.Generate(map, parms);
			actualMin = (actualMax = null);
		}

		protected override LayoutStructureSketch GenerateAndSpawn(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
		{
			CellRect container = structureSketch.structureLayout.container;
			if (!rect.TryFindRandomInnerRect(new IntVec2(container.Width, container.Height), out var rect2))
			{
				rect2 = rect;
				Log.Error($"Attempted to generate and spawn an anicent complex, but could not find rect of valid size {Size} within provided Rect {rect}");
			}
			ResolveParams parms2 = new ResolveParams
			{
				ancientLayoutStructureSketch = structureSketch,
				threatPoints = parms.sitePart.parms.threatPoints,
				rect = rect2,
				thingSetMakerDef = parms.sitePart.parms.ancientComplexRewardMaker
			};
			FormCaravanComp component = parms.sitePart.site.GetComponent<FormCaravanComp>();
			if (component != null)
			{
				component.foggedRoomsCheckRect = parms2.rect;
			}
			MapGenerator.UsedRects.Add(parms2.rect);
			GenerateComplex(map, parms2);
			return structureSketch;
		}

		private void TryRecoverEmptySketch(GenStepParams parms)
		{
			bool flag = false;
			foreach (Quest item in Find.QuestManager.QuestsListForReading)
			{
				if (item.TryGetFirstPartOfType<QuestPart_SpawnWorldObject>(out var part) && part.worldObject == parms.sitePart.site && item.root.root is QuestNode_Root_AncientComplex questNode_Root_AncientComplex)
				{
					structureSketch = questNode_Root_AncientComplex.QuestSetupComplex(item, parms.sitePart.parms.points);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				StructureGenParams parms2 = new StructureGenParams
				{
					size = DefaultComplexSize
				};
				structureSketch = LayoutDefOf.AncientComplex.Worker.GenerateStructureSketch(parms2);
				Log.Warning("Failed to recover lost complex from any quest. Generating default.");
			}
		}

		protected virtual void GenerateComplex(Map map, ResolveParams parms)
		{
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientComplex", parms);
			RimWorld.BaseGen.BaseGen.Generate();
		}
	}
}
