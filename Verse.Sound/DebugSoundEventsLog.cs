using System.Linq;
using System.Text;

namespace Verse.Sound;

public static class DebugSoundEventsLog
{
	private static LogMessageQueue queue = new LogMessageQueue();

	public static string EventsListingDebugString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (LogMessage item in queue.Messages.Reverse())
			{
				stringBuilder.AppendLine(item.ToString());
			}
			return stringBuilder.ToString();
		}
	}

	public static void Notify_SoundEvent(SoundDef def, SoundInfo info)
	{
		if (DebugViewSettings.writeSoundEventsRecord)
		{
			string text = ((def == null) ? "null: " : ((!def.isUndefined) ? (def.sustain ? "SustainerSpawn: " : "OneShot: ") : "Undefined: "));
			string text2 = ((def != null) ? def.defName : "null");
			CreateRecord(text + text2 + " - " + info.ToString());
		}
	}

	public static void Notify_SustainerEnded(Sustainer sustainer, SoundInfo info)
	{
		CreateRecord("SustainerEnd: " + sustainer.def.defName + " - " + info.ToString());
	}

	private static void CreateRecord(string str)
	{
		queue.Enqueue(new LogMessage(str));
	}
}
