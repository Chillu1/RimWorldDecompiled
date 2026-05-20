using System.Linq;
using Verse;

namespace RimWorld;

public static class TutorSystem
{
	public static bool TutorialMode
	{
		get
		{
			if (Find.Storyteller != null && Find.Storyteller.def != null)
			{
				return Find.Storyteller.def.tutorialMode;
			}
			return false;
		}
	}

	public static bool AdaptiveTrainingEnabled
	{
		get
		{
			if (!Prefs.AdaptiveTrainingEnabled)
			{
				return false;
			}
			if (Find.Storyteller != null && Find.Storyteller.def != null && Find.Storyteller.def.disableAdaptiveTraining)
			{
				return false;
			}
			return true;
		}
	}

	public static void Notify_Event(string eventTag, IntVec3 cell)
	{
		Notify_Event(new EventPack(eventTag, cell));
	}

	public static void Notify_Event(EventPack ep)
	{
		if (!TutorialMode)
		{
			return;
		}
		if (DebugViewSettings.logTutor)
		{
			EventPack eventPack = ep;
			Log.Message("Notify_Event: " + eventPack.ToString());
		}
		if (Current.Game == null)
		{
			return;
		}
		Lesson current = Find.ActiveLesson.Current;
		if (Find.ActiveLesson.Current != null)
		{
			Find.ActiveLesson.Current.Notify_Event(ep);
		}
		foreach (InstructionDef allDef in DefDatabase<InstructionDef>.AllDefs)
		{
			if (allDef.eventTagInitiate == ep.Tag && (allDef.eventTagInitiateSource == null || (current != null && allDef.eventTagInitiateSource == current.Instruction)) && (TutorialMode || !allDef.tutorialModeOnly))
			{
				Find.ActiveLesson.Activate(allDef);
				break;
			}
		}
	}

	public static bool AllowAction(EventPack ep)
	{
		if (!TutorialMode)
		{
			return true;
		}
		if (DebugViewSettings.logTutor)
		{
			EventPack eventPack = ep;
			Log.Message("AllowAction: " + eventPack.ToString());
		}
		if (ep.Cells != null && ep.Cells.Count() == 1)
		{
			return AllowAction(new EventPack(ep.Tag, ep.Cells.First()));
		}
		if (Find.ActiveLesson.Current != null)
		{
			AcceptanceReport acceptanceReport = Find.ActiveLesson.Current.AllowAction(ep);
			if (!acceptanceReport.Accepted)
			{
				Messages.Message((!acceptanceReport.Reason.NullOrEmpty()) ? acceptanceReport.Reason : Find.ActiveLesson.Current.DefaultRejectInputMessage, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
		}
		return true;
	}
}
