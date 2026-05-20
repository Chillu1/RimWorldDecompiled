using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_MechanitorComplexQuest : StorytellerComp
	{
		private StorytellerCompProperties_MechanitorComplexQuest Props => (StorytellerCompProperties_MechanitorComplexQuest)props;

		private bool ExistingMechanitorOrMechlink
		{
			get
			{
				if (!MechanitorUtility.AnyMechanitorInPlayerFaction())
				{
					return MechanitorUtility.AnyMechlinkInMap();
				}
				return true;
			}
		}

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Find.History.mechanoidDatacoreOpportunityAvailable && !Find.History.mechanoidDatacoreReadOrLost)
			{
				yield break;
			}
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			if (!Props.blockedByQueuedOrActiveQuests.NullOrEmpty())
			{
				for (int i = 0; i < questsListForReading.Count; i++)
				{
					if (Props.blockedByQueuedOrActiveQuests.Contains(questsListForReading[i].root) && (questsListForReading[i].State == QuestState.NotYetAccepted || questsListForReading[i].State == QuestState.Ongoing))
					{
						yield break;
					}
				}
			}
			int num = -1;
			for (int j = 0; j < questsListForReading.Count; j++)
			{
				if (questsListForReading[j].root == Props.incident.questScriptDef && questsListForReading[j].cleanupTick > num)
				{
					num = questsListForReading[j].cleanupTick;
				}
			}
			if (num > 0 && (Find.TickManager.TicksGame - num).TicksToDays() < Props.minSpacingDays)
			{
				yield break;
			}
			float num2 = Props.mtbDays;
			if (Props.existingMechanitorOrMechlinkMTBFactor != 1f && ExistingMechanitorOrMechlink)
			{
				num2 *= Props.existingMechanitorOrMechlinkMTBFactor;
			}
			if (Rand.MTBEventOccurs(num2, 60000f, 1000f))
			{
				IncidentParms parms = GenerateParms(Props.incident.category, target);
				if (Props.incident.Worker.CanFireNow(parms))
				{
					yield return new FiringIncident(Props.incident, this, parms);
				}
			}
		}
	}
}
