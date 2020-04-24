using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_ExtraFaction : QuestPartActivable
	{
		public ExtraFaction extraFaction;

		public List<Pawn> affectedPawns = new List<Pawn>();

		public bool areHelpers;

		private const int RelationsGainAvailableInTicks = 1800000;

		public override IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				foreach (Faction involvedFaction in base.InvolvedFactions)
				{
					yield return involvedFaction;
				}
				if (extraFaction != null && extraFaction.faction != null)
				{
					yield return extraFaction.faction;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref extraFaction, "extraFaction");
			Scribe_Collections.Look(ref affectedPawns, "affectedPawns", LookMode.Reference);
			Scribe_Values.Look(ref areHelpers, "areHelpers", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				affectedPawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			affectedPawns.Replace(replace, with);
		}

		public override void Cleanup()
		{
			base.Cleanup();
			SetRelationsGainTickForPawns();
		}

		protected override void Disable()
		{
			base.Disable();
			SetRelationsGainTickForPawns();
		}

		private void SetRelationsGainTickForPawns()
		{
			foreach (Pawn affectedPawn in affectedPawns)
			{
				if (affectedPawn.mindState != null)
				{
					affectedPawn.mindState.SetNoAidRelationsGainUntilTick(Find.TickManager.TicksGame + 1800000);
				}
			}
		}
	}
}
