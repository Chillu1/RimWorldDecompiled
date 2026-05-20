using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SpawnWorldObjects : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<WorldObject>> worldObjects;

	public SlateRef<PlanetTile?> tile;

	public SlateRef<List<ThingDef>> defsToExcludeFromHyperlinks;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (worldObjects.GetValue(slate) == null)
		{
			return;
		}
		string text = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		foreach (WorldObject item in worldObjects.GetValue(slate))
		{
			QuestPart_SpawnWorldObject questPart_SpawnWorldObject = new QuestPart_SpawnWorldObject();
			questPart_SpawnWorldObject.worldObject = item;
			questPart_SpawnWorldObject.inSignal = text;
			questPart_SpawnWorldObject.defsToExcludeFromHyperlinks = defsToExcludeFromHyperlinks.GetValue(slate);
			if (tile.GetValue(slate).HasValue)
			{
				item.Tile = tile.GetValue(slate) ?? PlanetTile.Invalid;
			}
			QuestGen.quest.AddPart(questPart_SpawnWorldObject);
		}
	}
}
