using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Asteroid : QuestNode
{
	public PlanetLayerDef layerDef;

	public WorldObjectDef worldObjectDef;

	public List<ThingDef> mineables;

	private const int MaxDistanceFromColony = 3;

	private const int MinDistanceFromColony = 1;

	private static readonly IntRange TimeoutDays = new IntRange(45, 60);

	protected override bool TestRunInt(Slate slate)
	{
		PlanetTile tile;
		return TryFindSiteTile(out tile);
	}

	protected override void RunInt()
	{
		if (ModsConfig.OdysseyActive)
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			TryFindSiteTile(out var tile);
			SpaceMapParent spaceMapParent = (SpaceMapParent)WorldObjectMaker.MakeWorldObject(worldObjectDef);
			spaceMapParent.Tile = tile;
			if (mineables != null)
			{
				ThingDef thingDef = mineables.RandomElement();
				slate.Set("resource", thingDef.label);
				spaceMapParent.nameInt = "AsteroidName".Translate(thingDef.label.Named("RESOURCE"));
				spaceMapParent.preciousResource = thingDef;
			}
			slate.Set("worldObject", spaceMapParent);
			quest.SpawnWorldObject(spaceMapParent);
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("worldObject.MapRemoved");
			int delayTicks = TimeoutDays.RandomInRange * 60000;
			quest.WorldObjectTimeout(spaceMapParent, delayTicks);
			quest.Delay(delayTicks, delegate
			{
				QuestGen_End.End(quest, QuestEndOutcome.Fail);
			});
			quest.End(QuestEndOutcome.Success, 0, null, inSignal);
		}
	}

	private bool TryFindSiteTile(out PlanetTile tile)
	{
		tile = PlanetTile.Invalid;
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		PlanetTile tile2;
		PlanetTile origin = (TileFinder.TryFindRandomPlayerTile(out tile2, allowCaravans: false, null, canBeSpace: true) ? tile2 : new PlanetTile(0, Find.WorldGrid.Surface));
		if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(origin, layerDef, out var layer))
		{
			return false;
		}
		FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(origin, 1f, 3f);
		return layer.FastTileFinder.Query(query).TryRandomElement(out tile);
	}
}
