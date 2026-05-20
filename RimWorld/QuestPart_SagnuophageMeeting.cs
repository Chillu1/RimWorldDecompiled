using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_SagnuophageMeeting : QuestPart_MakeLord
	{
		public IntVec3 targetCell;

		public List<Thing> enemyTargets = new List<Thing>();

		public int meetingDurationTicks;

		public string outSignalMeetingStarted;

		public string outSignalMeetingCompleted;

		public string outSignalAllSanguophagesGone;

		public string inSignalDefend;

		public string inSignalQuestSuccess;

		public string inSignalQuestFail;

		public string questTag;

		protected override Lord MakeLord()
		{
			Lord lord = LordMaker.MakeNewLord(faction, new LordJob_SanguophageMeeting(targetCell, enemyTargets, meetingDurationTicks, outSignalMeetingStarted, outSignalMeetingCompleted, outSignalAllSanguophagesGone, inSignalDefend, inSignalQuestSuccess, inSignalQuestFail), base.Map);
			QuestUtility.AddQuestTag(ref lord.questTags, questTag);
			return lord;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref targetCell, "targetCell");
			Scribe_Collections.Look(ref enemyTargets, "enemyTargets", LookMode.Reference);
			Scribe_Values.Look(ref meetingDurationTicks, "meetingDurationTicks", 0);
			Scribe_Values.Look(ref inSignalDefend, "inSignalDefend");
			Scribe_Values.Look(ref outSignalMeetingStarted, "outSignalMeetingStarted");
			Scribe_Values.Look(ref outSignalMeetingCompleted, "outSignalMeetingCompleted");
			Scribe_Values.Look(ref outSignalAllSanguophagesGone, "outSignalAllSanguophagesGone");
			Scribe_Values.Look(ref inSignalQuestSuccess, "inSignalQuestSuccess");
			Scribe_Values.Look(ref inSignalQuestFail, "inSignalQuestFail");
			Scribe_Values.Look(ref questTag, "questTag");
		}
	}
}
