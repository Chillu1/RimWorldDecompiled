using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_RefiringUniqueQuest : StorytellerComp
	{
		private bool generateSkipped;

		private int IntervalsPassed => Find.TickManager.TicksGame / 1000;

		private StorytellerCompProperties_RefiringUniqueQuest Props => (StorytellerCompProperties_RefiringUniqueQuest)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (!Props.incident.TargetAllowed(target))
			{
				yield break;
			}
			Quest quest = null;
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].root == Props.incident.questScriptDef && (quest == null || questsListForReading[i].appearanceTick > quest.appearanceTick))
				{
					quest = questsListForReading[i];
					break;
				}
			}
			if ((quest == null) ? ((!generateSkipped) ? (IntervalsPassed == (int)(Props.minDaysPassed * 60f) + 1) : ((float)GenTicks.TicksGame >= Props.minDaysPassed * 60000f)) : (Props.refireEveryDays >= 0f && ((quest.State != QuestState.EndedSuccess && quest.State != QuestState.Ongoing && quest.State != 0 && quest.cleanupTick >= 0 && IntervalsPassed == (int)((float)quest.cleanupTick + Props.refireEveryDays * 60000f) / 1000) ? true : false)))
			{
				IncidentParms parms = GenerateParms(Props.incident.category, target);
				if (Props.incident.Worker.CanFireNow(parms))
				{
					yield return new FiringIncident(Props.incident, this, parms);
				}
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			if ((float)GenTicks.TicksGame >= Props.minDaysPassed * 60000f)
			{
				generateSkipped = true;
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.incident;
		}
	}
}
