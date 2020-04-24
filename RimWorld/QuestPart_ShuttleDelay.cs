using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_ShuttleDelay : QuestPart_Delay
	{
		public List<Pawn> lodgers = new List<Pawn>();

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				for (int i = 0; i < lodgers.Count; i++)
				{
					yield return lodgers[i];
				}
			}
		}

		public override string ExtraInspectString(ISelectable target)
		{
			Pawn pawn = target as Pawn;
			if (pawn != null && lodgers.Contains(pawn))
			{
				return "ShuttleDelayInspectString".Translate(base.TicksLeft.ToStringTicksToPeriod());
			}
			return null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref lodgers, "lodgers", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				lodgers.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			if (Find.AnyPlayerHomeMap != null)
			{
				lodgers.AddRange(Find.RandomPlayerHomeMap.mapPawns.FreeColonists);
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			lodgers.Replace(replace, with);
		}
	}
}
