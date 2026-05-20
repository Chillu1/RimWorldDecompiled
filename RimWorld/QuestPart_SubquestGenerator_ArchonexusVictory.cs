using System.Linq;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class QuestPart_SubquestGenerator_ArchonexusVictory : QuestPart_SubquestGenerator
{
	public Faction civilOutlander;

	public Faction roughTribe;

	public Faction roughOutlander;

	protected override Slate InitSlate()
	{
		Slate slate = new Slate();
		slate.Set("civilOutlander", civilOutlander);
		slate.Set("roughTribe", roughTribe);
		slate.Set("roughOutlander", roughOutlander);
		return slate;
	}

	protected override QuestScriptDef GetNextSubquestDef()
	{
		int index = quest.GetSubquests(QuestState.EndedSuccess).Count() % subquestDefs.Count;
		QuestScriptDef questScriptDef = subquestDefs[index];
		if (!questScriptDef.CanRun(InitSlate(), Find.World))
		{
			return null;
		}
		return questScriptDef;
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (civilOutlander == faction)
		{
			civilOutlander = null;
		}
		if (roughTribe == faction)
		{
			roughTribe = null;
		}
		if (roughOutlander == faction)
		{
			roughOutlander = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref civilOutlander, "civilOutlander");
		Scribe_References.Look(ref roughTribe, "roughTribe");
		Scribe_References.Look(ref roughOutlander, "roughOutlander");
	}
}
