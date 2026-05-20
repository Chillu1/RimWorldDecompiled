using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_ConnectToTree : RitualOutcomeEffectWorker_FromQuality
{
	private const float NumMossPerQuality = 50f;

	public override bool SupportsAttachableOutcomeEffect => false;

	public RitualOutcomeEffectWorker_ConnectToTree()
	{
	}

	public RitualOutcomeEffectWorker_ConnectToTree(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		Thing thing = jobRitual.selectedTarget.Thing;
		float quality = GetQuality(jobRitual, progress);
		int num = Mathf.Max(1, Mathf.RoundToInt(quality * 50f));
		CompSpawnSubplantDuration compSpawnSubplantDuration = thing.TryGetComp<CompSpawnSubplantDuration>();
		if (compSpawnSubplantDuration != null)
		{
			foreach (Pawn key in totalPresence.Keys)
			{
				_ = key;
				for (int i = 0; i < num; i++)
				{
					compSpawnSubplantDuration.DoGrowSubplant(force: true);
				}
			}
			compSpawnSubplantDuration.SetupNextSubplantTick();
		}
		Pawn pawn = jobRitual.PawnWithRole("connector");
		CompTreeConnection compTreeConnection = thing.TryGetComp<CompTreeConnection>();
		if (pawn != null && compTreeConnection != null)
		{
			compTreeConnection.ConnectToPawn(pawn, quality);
			Find.LetterStack.ReceiveLetter("LetterLabelPawnConnected".Translate(thing.Named("TREE")), "LetterTextPawnConnected".Translate(thing.Named("TREE"), pawn.Named("CONNECTOR")), LetterDefOf.RitualOutcomePositive, pawn, null, null, new List<ThingDef> { thing.def });
		}
	}
}
