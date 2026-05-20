using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GenerateThingSet : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<ThingSetMakerDef> thingSetMaker;

	public SlateRef<FloatRange?> totalMarketValueRange;

	public SlateRef<Thing> factionOf;

	public SlateRef<QualityGenerator?> qualityGenerator;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			totalMarketValueRange = totalMarketValueRange.GetValue(slate),
			makingFaction = factionOf.GetValue(slate)?.Faction,
			qualityGenerator = qualityGenerator.GetValue(slate)
		};
		List<Thing> list = thingSetMaker.GetValue(slate).root.Generate(parms);
		QuestGen.slate.Set(storeAs.GetValue(slate), list);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Pawn pawn)
			{
				QuestGen.AddToGeneratedPawns(pawn);
				if (!pawn.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
		}
	}
}
