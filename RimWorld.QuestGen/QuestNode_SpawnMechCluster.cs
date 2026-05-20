using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_SpawnMechCluster : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	[NoTranslate]
	public SlateRef<string> tag;

	public SlateRef<float?> points;

	public SlateRef<IntVec3?> dropSpot;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_MechCluster questPart_MechCluster = new QuestPart_MechCluster();
		questPart_MechCluster.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_MechCluster.tag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate));
		questPart_MechCluster.mapParent = slate.Get<Map>("map").Parent;
		questPart_MechCluster.sketch = GenerateSketch(slate);
		questPart_MechCluster.dropSpot = dropSpot.GetValue(slate) ?? IntVec3.Invalid;
		QuestGen.quest.AddPart(questPart_MechCluster);
		string text = "";
		if (questPart_MechCluster.sketch.pawns != null)
		{
			text += PawnUtility.PawnKindsToLineList(questPart_MechCluster.sketch.pawns.Select((MechClusterSketch.Mech m) => m.kindDef), "  - ", ColoredText.ThreatColor);
		}
		string[] array = (from t in questPart_MechCluster.sketch.buildingsSketch.Things
			where GenHostility.IsDefMechClusterThreat(t.def)
			group t by t.def.label).Select(delegate(IGrouping<string, SketchThing> grp)
		{
			int num = grp.Count();
			return num + " " + ((num > 1) ? Find.ActiveLanguageWorker.Pluralize(grp.Key, num) : grp.Key);
		}).ToArray();
		if (array.Any())
		{
			if (text != "")
			{
				text += "\n";
			}
			text += array.ToLineList(ColoredText.ThreatColor, "  - ");
		}
		if (text != "")
		{
			QuestGen.AddQuestDescriptionRules(new List<Rule>
			{
				new Rule_String("allThreats", text)
			});
		}
	}

	private MechClusterSketch GenerateSketch(Slate slate)
	{
		return MechClusterGenerator.GenerateClusterSketch(points.GetValue(slate) ?? slate.Get("points", 0f), slate.Get<Map>("map"));
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		if (Faction.OfMechanoids == null)
		{
			return false;
		}
		return slate.Get<Map>("map") != null;
	}
}
