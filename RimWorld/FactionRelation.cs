using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class FactionRelation : IExposable
	{
		public Faction other;

		public int goodwill = 100;

		public FactionRelationKind kind = FactionRelationKind.Neutral;

		public void CheckKindThresholds(Faction faction, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
		{
			FactionRelationKind previousKind = kind;
			sentLetter = false;
			if (kind != 0 && goodwill <= -75)
			{
				kind = FactionRelationKind.Hostile;
				faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
			}
			if (kind != FactionRelationKind.Ally && goodwill >= 75)
			{
				kind = FactionRelationKind.Ally;
				faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
			}
			if (kind == FactionRelationKind.Hostile && goodwill >= 0)
			{
				kind = FactionRelationKind.Neutral;
				faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
			}
			if (kind == FactionRelationKind.Ally && goodwill <= 0)
			{
				kind = FactionRelationKind.Neutral;
				faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref other, "other");
			Scribe_Values.Look(ref goodwill, "goodwill", 0);
			Scribe_Values.Look(ref kind, "kind", FactionRelationKind.Neutral);
			BackCompatibility.PostExposeData(this);
		}

		public override string ToString()
		{
			return string.Concat("(", other, ", goodwill=", goodwill.ToString("F1"), ", kind=", kind, ")");
		}
	}
}
