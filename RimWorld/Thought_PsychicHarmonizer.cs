using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_PsychicHarmonizer : Thought_Memory
	{
		public Hediff harmonizer;

		public override string LabelCap => base.CurStage.label.Formatted(harmonizer.pawn.Named("HARMONIZER")).CapitalizeFirst();

		public override bool ShouldDiscard
		{
			get
			{
				Pawn pawn = harmonizer.pawn;
				if (pawn.health.Dead || pawn.needs == null || pawn.needs.mood == null)
				{
					return true;
				}
				if (base.pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicHarmonizer))
				{
					return true;
				}
				if (!pawn.Spawned && !base.pawn.Spawned && pawn.GetCaravan() == base.pawn.GetCaravan())
				{
					return false;
				}
				if (pawn.Spawned && base.pawn.Spawned && pawn.Map == base.pawn.Map)
				{
					return pawn.Position.DistanceTo(base.pawn.Position) > harmonizer.TryGetComp<HediffComp_PsychicHarmonizer>().Props.range;
				}
				return true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref harmonizer, "harmonizer");
		}

		public override float MoodOffset()
		{
			if (ThoughtUtility.ThoughtNullified(pawn, def))
			{
				return 0f;
			}
			if (ShouldDiscard)
			{
				return 0f;
			}
			float num = base.MoodOffset();
			float num2 = Mathf.Lerp(-1f, 1f, harmonizer.pawn.needs.mood.CurLevel);
			float statValue = harmonizer.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
			return num * num2 * statValue;
		}

		public override bool TryMergeWithExistingMemory(out bool showBubble)
		{
			showBubble = false;
			return false;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_PsychicHarmonizer thought_PsychicHarmonizer = other as Thought_PsychicHarmonizer;
			if (thought_PsychicHarmonizer == null)
			{
				return false;
			}
			if (base.GroupsWith(other))
			{
				return thought_PsychicHarmonizer.harmonizer == harmonizer;
			}
			return false;
		}
	}
}
