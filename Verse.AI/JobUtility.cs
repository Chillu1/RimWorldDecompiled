using RimWorld;
using System;

namespace Verse.AI
{
	public static class JobUtility
	{
		private static bool startingErrorRecoverJob;

		public static void TryStartErrorRecoverJob(Pawn pawn, string message, Exception exception = null, JobDriver concreteDriver = null)
		{
			string msg = message;
			AppendVarsInfoToDebugMessage(pawn, ref msg, concreteDriver);
			if (exception != null)
			{
				msg = msg + "\n" + exception;
			}
			Log.Error(msg);
			if (pawn.jobs != null)
			{
				if (pawn.jobs.curJob != null)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Errored, startNewJob: false);
				}
				if (startingErrorRecoverJob)
				{
					Log.Error("An error occurred while starting an error recover job. We have to stop now to avoid infinite loops. This means that the pawn is now jobless which can cause further bugs. pawn=" + pawn.ToStringSafe());
					return;
				}
				startingErrorRecoverJob = true;
				try
				{
					pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait, 150));
				}
				finally
				{
					startingErrorRecoverJob = false;
				}
			}
		}

		public static string GetResolvedJobReport(string baseText, LocalTargetInfo a)
		{
			return GetResolvedJobReport(baseText, a, LocalTargetInfo.Invalid, LocalTargetInfo.Invalid);
		}

		public static string GetResolvedJobReport(string baseText, LocalTargetInfo a, LocalTargetInfo b)
		{
			return GetResolvedJobReport(baseText, a, b, LocalTargetInfo.Invalid);
		}

		public static string GetResolvedJobReport(string baseText, LocalTargetInfo a, LocalTargetInfo b, LocalTargetInfo c)
		{
			GetText(a, out string backCompatibleText2, out object obj2);
			GetText(b, out string backCompatibleText3, out object obj3);
			GetText(c, out string backCompatibleText4, out object obj4);
			return GetResolvedJobReportRaw(baseText, backCompatibleText2, obj2, backCompatibleText3, obj3, backCompatibleText4, obj4);
			void GetText(LocalTargetInfo x, out string backCompatibleText, out object obj)
			{
				if (!x.IsValid)
				{
					backCompatibleText = "UnknownLower".Translate();
					obj = backCompatibleText;
				}
				else if (x.HasThing)
				{
					backCompatibleText = x.Thing.LabelShort;
					obj = x.Thing;
				}
				else
				{
					backCompatibleText = "AreaLower".Translate();
					obj = backCompatibleText;
				}
			}
		}

		public static string GetResolvedJobReportRaw(string baseText, string aText, object aObj, string bText, object bObj, string cText, object cObj)
		{
			baseText = baseText.Formatted(aObj.Named("TargetA"), bObj.Named("TargetB"), cObj.Named("TargetC"));
			baseText = baseText.Replace("TargetA", aText);
			baseText = baseText.Replace("TargetB", bText);
			baseText = baseText.Replace("TargetC", cText);
			return baseText;
		}

		private static void AppendVarsInfoToDebugMessage(Pawn pawn, ref string msg, JobDriver concreteDriver)
		{
			if (concreteDriver != null)
			{
				msg = msg + " driver=" + concreteDriver.GetType().Name + " (toilIndex=" + concreteDriver.CurToilIndex + ")";
				if (concreteDriver.job != null)
				{
					msg = msg + " driver.job=(" + concreteDriver.job.ToStringSafe() + ")";
				}
			}
			else if (pawn.jobs != null)
			{
				if (pawn.jobs.curDriver != null)
				{
					msg = msg + " curDriver=" + pawn.jobs.curDriver.GetType().Name + " (toilIndex=" + pawn.jobs.curDriver.CurToilIndex + ")";
				}
				if (pawn.jobs.curJob != null)
				{
					msg = msg + " curJob=(" + pawn.jobs.curJob.ToStringSafe() + ")";
				}
			}
		}
	}
}
