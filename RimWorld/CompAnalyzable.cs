using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class CompAnalyzable : CompInteractable
{
	public new CompProperties_Analyzable Props => (CompProperties_Analyzable)props;

	public Pawn Pawn => parent as Pawn;

	public abstract int AnalysisID { get; }

	public override string ExposeKey => "Analyzable";

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !Find.AnalysisManager.HasAnalysisWithID(AnalysisID))
		{
			Find.AnalysisManager.AddAnalysisTask(AnalysisID, Props.analysisRequiredRange.RandomInRange);
		}
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		Building_ResearchBench bench = null;
		if (ValidateTarget(target, showMessages: false) && (Props.canStudyInPlace || StudyUtility.TryFindResearchBench(target.Pawn, out bench)))
		{
			CompForbiddable compForbiddable = parent?.TryGetComp<CompForbiddable>();
			if (compForbiddable != null)
			{
				compForbiddable.Forbidden = false;
			}
			target.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.AnalyzeItem, parent, bench, bench?.Position ?? IntVec3.Invalid), JobTag.Misc);
		}
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
		if (!result.Accepted)
		{
			return result;
		}
		Find.AnalysisManager.TryGetAnalysisProgress(AnalysisID, out var details);
		if (!Props.allowRepeatAnalysis && details != null && details.Satisfied)
		{
			return "AlreadyAnalyzed".Translate();
		}
		if (activateBy != null && StatDefOf.ResearchSpeed.Worker.IsDisabledFor(activateBy))
		{
			return "Incapable".Translate();
		}
		if (activateBy != null && !Props.canStudyInPlace && !StudyUtility.TryFindResearchBench(activateBy, out var _))
		{
			return "NoResearchBench".Translate();
		}
		if (activateBy == null && !Props.canStudyInPlace && !parent.MapHeld.listerBuildings.ColonistsHaveResearchBench())
		{
			return "NoResearchBench".Translate();
		}
		return true;
	}

	public virtual void OnAnalyzed(Pawn pawn)
	{
		if (!Find.AnalysisManager.TryIncrementAnalysisProgress(AnalysisID, out var details) && Find.AnalysisManager.IsAnalysisComplete(AnalysisID))
		{
			SendLetter(Props.repeatCompletedLetterLabel, Props.repeatCompletedLetter, Props.repeatCompletedLetterDef, pawn);
		}
		else
		{
			SendAppropriateProgressLetter(pawn, details);
		}
		if (Props.destroyedOnAnalyzed)
		{
			parent.Destroy();
		}
	}

	private void SendAppropriateProgressLetter(Pawn pawn, AnalysisDetails details)
	{
		if (details.Satisfied)
		{
			SendLetter(Props.completedLetterLabel, Props.completedLetter, Props.completedLetterDef, pawn);
			return;
		}
		string text = "";
		if (Props.showProgress)
		{
			text = $" {details.timesDone}/{details.required}";
		}
		string letter = ((details.timesDone <= Props.progressedLetters.Count) ? Props.progressedLetters[details.timesDone - 1] : Props.progressedLetters.Last());
		SendLetter(Props.progressedLetterLabel + text, letter, Props.progressedLetterDef, pawn);
	}

	private void SendLetter(string label, string letter, LetterDef def, Pawn pawn)
	{
		if (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(letter))
		{
			string formattedLetterString = GetFormattedLetterString(label, pawn);
			string formattedLetterString2 = GetFormattedLetterString(letter, pawn);
			Find.LetterStack.ReceiveLetter(formattedLetterString, formattedLetterString2, def);
		}
	}

	private string GetFormattedLetterString(string text, Pawn pawn)
	{
		if (ExtraNamedArg.HasValue)
		{
			return text.Formatted(pawn.Named("PAWN"), ExtraNamedArg.Value).Resolve();
		}
		return text.Formatted(pawn.Named("PAWN")).Resolve();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Finish analysis",
				action = delegate
				{
					OnAnalyzed(Find.CurrentMap.mapPawns.FreeColonistsSpawned.First());
				}
			};
		}
	}
}
