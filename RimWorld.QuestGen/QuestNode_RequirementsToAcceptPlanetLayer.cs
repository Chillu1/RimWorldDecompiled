using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_RequirementsToAcceptPlanetLayer : QuestNode
{
	public SlateRef<bool> canBeSpace;

	public SlateRef<List<PlanetLayerDef>> layerWhitelist;

	public SlateRef<List<PlanetLayerDef>> layerBlacklist;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestGen.quest.AddPart(new QuestPart_RequirementsToAcceptPlanetLayer
		{
			canBeSpace = canBeSpace.GetValue(slate),
			layerWhitelist = layerWhitelist.GetValue(slate),
			layerBlacklist = layerBlacklist.GetValue(slate),
			mapParent = slate.Get<Map>("map").Parent
		});
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
