using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_GravshipLaunch : RitualOutcomeEffectWorker_FromQuality
{
	public RitualOutcomeEffectWorker_GravshipLaunch()
	{
	}

	public RitualOutcomeEffectWorker_GravshipLaunch(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		if (progress < 1f)
		{
			Messages.Message("GravshipLaunchInterrupted".Translate(), MessageTypeDefOf.NegativeEvent);
			return;
		}
		try
		{
			CompPilotConsole consoleComp = jobRitual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>();
			float quality = GetQuality(jobRitual, progress);
			consoleComp.engine.launchInfo = new LaunchInfo
			{
				pilot = jobRitual.PawnWithRole("pilot"),
				copilot = jobRitual.PawnWithRole("copilot"),
				quality = quality,
				doNegativeOutcome = Rand.Chance(GravshipUtility.NegativeLandingOutcomeFromQuality(quality))
			};
			if (jobRitual.Map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
			{
				consoleComp.StartChoosingDestination_NewTemp();
				return;
			}
			GravshipUtility.PreLaunchConfirmation(consoleComp.engine, delegate
			{
				consoleComp.StartChoosingDestination_NewTemp();
			});
		}
		catch (Exception ex)
		{
			Log.Error("Error launching gravship: " + ex);
		}
	}
}
