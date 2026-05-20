using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_CreateIncidents : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	public SlateRef<IncidentDef> incidentDef;

	public SlateRef<int?> intervalTicks;

	public SlateRef<int?> randomIncidents;

	public SlateRef<int> startOffsetTicks;

	public SlateRef<int> duration;

	public SlateRef<float> points;

	public SlateRef<Faction> faction;

	protected override bool TestRunInt(Slate slate)
	{
		if (incidentDef.GetValue(slate) == null || points.GetValue(slate) < incidentDef.GetValue(slate).minThreatPoints || points.GetValue(slate) > incidentDef.GetValue(slate).maxThreatPoints)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		int value = duration.GetValue(slate);
		_ = QuestGen.quest;
		int value2 = startOffsetTicks.GetValue(slate);
		IncidentDef value3 = incidentDef.GetValue(slate);
		Map map = slate.Get<Map>("map");
		float value4 = points.GetValue(slate);
		Faction value5 = faction.GetValue(slate);
		string delayInSignal = slate.Get<string>("inSignal");
		string disableSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		int? value6 = randomIncidents.GetValue(slate);
		if (value6.HasValue)
		{
			for (int i = 0; i < value6; i++)
			{
				CreateDelayedIncident(Rand.Range(value2, value), delayInSignal, disableSignal, value3, map, value4, value5);
			}
		}
		int? value7 = intervalTicks.GetValue(slate);
		if (value7.HasValue)
		{
			int num = Mathf.FloorToInt((float)value / (float)value7.Value);
			for (int j = 0; j < num; j++)
			{
				int delayTicks = Mathf.Max(j * value7.Value, value2);
				CreateDelayedIncident(delayTicks, delayInSignal, disableSignal, value3, map, value4, value5);
			}
		}
	}

	private void CreateDelayedIncident(int delayTicks, string delayInSignal, string disableSignal, IncidentDef incident, Map map, float points, Faction faction)
	{
		Quest quest = QuestGen.quest;
		QuestPart_Delay questPart_Delay = new QuestPart_Delay();
		questPart_Delay.delayTicks = delayTicks;
		questPart_Delay.inSignalEnable = delayInSignal;
		questPart_Delay.inSignalDisable = disableSignal;
		questPart_Delay.debugLabel = questPart_Delay.delayTicks.ToStringTicksToDays() + "_" + incidentDef.ToString();
		quest.AddPart(questPart_Delay);
		QuestPart_Incident questPart_Incident = new QuestPart_Incident();
		questPart_Incident.incident = incident;
		questPart_Incident.inSignal = questPart_Delay.OutSignalCompleted;
		questPart_Incident.SetIncidentParmsAndRemoveTarget(new IncidentParms
		{
			forced = true,
			target = map,
			points = points,
			faction = faction
		});
		quest.AddPart(questPart_Incident);
	}
}
