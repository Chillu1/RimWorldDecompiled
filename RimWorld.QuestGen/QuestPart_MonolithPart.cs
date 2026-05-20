using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_MonolithPart : QuestPart_DescriptionPart
{
	private Building_VoidMonolith monolith;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			bool flag = false;
			if (ModsConfig.IdeologyActive)
			{
				foreach (Quest item in Find.QuestManager.QuestsListForReading)
				{
					if (item.State == QuestState.Ongoing && (item.root == QuestScriptDefOf.EndGame_ArchonexusVictory_FirstCycle || item.root == QuestScriptDefOf.EndGame_ArchonexusVictory_SecondCycle))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				yield return monolith;
			}
		}
	}

	public QuestPart_MonolithPart()
	{
	}

	public QuestPart_MonolithPart(Building_VoidMonolith monolith)
	{
		inSignalEnable = "MonolithLevelChanged";
		this.monolith = monolith;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == "MonolithLevelChanged" || signal.tag == "EntityDiscovered")
		{
			UpdateDescription();
		}
	}

	private void UpdateDescription()
	{
		if (Find.Anomaly.LevelDef.postEndgame)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = Find.Anomaly.Level;
		if (Find.Anomaly.LevelDef == MonolithLevelDefOf.Gleaming)
		{
			num--;
		}
		stringBuilder.AppendLine(Find.Anomaly.LevelDef.extraQuestDescription);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(string.Concat("CurrentMonolithLevel".Translate() + ": ", num.ToString()));
		EntityCategoryDef entityCategoryDef = Find.Anomaly.NextLevelDef?.entityCatagoryCompletionRequired;
		if (entityCategoryDef != null)
		{
			stringBuilder.AppendLine();
			int num2 = Find.EntityCodex.DiscoveredCount(entityCategoryDef);
			if (num2 >= Find.Anomaly.NextLevelDef.entityCountCompletionRequired)
			{
				stringBuilder.AppendLine("VoidMonolithAllEntitiesDiscovered".Translate()).AppendLine(Find.Anomaly.LevelDef.activateQuestText);
			}
			else
			{
				int entityCountCompletionRequired = Find.Anomaly.NextLevelDef.entityCountCompletionRequired;
				stringBuilder.AppendLine("VoidMonolithRequiredEntityCategory".Translate(entityCountCompletionRequired, entityCategoryDef.label)).AppendLine().AppendLine("DiscoveredCount".Translate(num2 + " / " + entityCountCompletionRequired).CapitalizeFirst());
			}
		}
		resolvedDescriptionPart = stringBuilder.ToString();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref monolith, "monolith");
	}

	public override void Cleanup()
	{
		base.Cleanup();
		monolith = null;
	}
}
