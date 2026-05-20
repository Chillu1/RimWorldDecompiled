using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_ReleaseParalyzedAnimals : QuestPart
	{
		public List<Pawn> pawns = new List<Pawn>();

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
				{
					yield return questLookTarget2;
				}
			}
		}

		private bool IsParalyzed(Pawn pawn)
		{
			return pawn.health.hediffSet.HasHediff(HediffDefOf.Abasia);
		}

		public override void Cleanup()
		{
			base.Cleanup();
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].RaceProps.Animal && IsParalyzed(pawns[i]) && pawns[i].Faction != null)
				{
					pawns[i].SetFaction(null);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			pawns.Replace(replace, with);
		}
	}
}
