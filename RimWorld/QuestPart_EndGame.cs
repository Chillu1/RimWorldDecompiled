using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class QuestPart_EndGame : QuestPart
	{
		public string inSignal;

		public string introText;

		public string endingText;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal) || ShipCountdown.CountingDown)
			{
				return;
			}
			if (!Find.TickManager.Paused)
			{
				Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
			}
			if (!signal.args.TryGetArg("SENTCOLONISTS", out List<Pawn> arg))
			{
				arg = null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (arg != null)
			{
				for (int i = 0; i < arg.Count; i++)
				{
					stringBuilder.AppendLine("   " + arg[i].LabelCap);
				}
				Find.StoryWatcher.statsRecord.colonistsLaunched += arg.Count;
			}
			ShipCountdown.InitiateCountdown(GameVictoryUtility.MakeEndCredits(introText, endingText, stringBuilder.ToString()));
			if (arg == null)
			{
				return;
			}
			for (int j = 0; j < arg.Count; j++)
			{
				if (!arg[j].Destroyed)
				{
					arg[j].Destroy();
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref introText, "introText");
			Scribe_Values.Look(ref endingText, "endingText");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
		}
	}
}
