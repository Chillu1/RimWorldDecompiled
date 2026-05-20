using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SpawnSkyfaller : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<ThingDef> skyfallerDef;

	public SlateRef<IEnumerable<Thing>> innerThings;

	public SlateRef<IntVec3?> cell;

	public SlateRef<Pawn> factionOfForSafeSpot;

	public SlateRef<bool> lookForSafeSpot;

	public SlateRef<bool> tryLandInShipLandingZone;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Map map = QuestGen.slate.Get<Map>("map");
		Skyfaller thing = SkyfallerMaker.MakeSkyfaller(skyfallerDef.GetValue(slate), innerThings.GetValue(slate));
		QuestPart_SpawnThing questPart_SpawnThing = new QuestPart_SpawnThing();
		questPart_SpawnThing.thing = thing;
		questPart_SpawnThing.mapParent = map.Parent;
		if (factionOfForSafeSpot.GetValue(slate) != null)
		{
			questPart_SpawnThing.factionForFindingSpot = factionOfForSafeSpot.GetValue(slate).Faction;
		}
		if (cell.GetValue(slate).HasValue)
		{
			questPart_SpawnThing.cell = cell.GetValue(slate).Value;
		}
		questPart_SpawnThing.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_SpawnThing.lookForSafeSpot = lookForSafeSpot.GetValue(slate);
		questPart_SpawnThing.tryLandInShipLandingZone = tryLandInShipLandingZone.GetValue(slate);
		QuestGen.quest.AddPart(questPart_SpawnThing);
	}
}
