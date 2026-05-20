using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class FactionRelation : IExposable
{
	public Faction other;

	public int baseGoodwill = 100;

	public FactionRelationKind kind = FactionRelationKind.Neutral;

	public FactionRelation()
	{
	}

	public FactionRelation(Faction other, FactionRelationKind kind)
	{
		this.other = other;
		this.kind = kind;
	}

	public void CheckKindThresholds(Faction faction, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
	{
		int num = faction.GoodwillWith(other);
		FactionRelationKind previousKind = kind;
		sentLetter = false;
		if (kind != FactionRelationKind.Hostile && num <= -75)
		{
			kind = FactionRelationKind.Hostile;
			faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
		}
		if (kind != FactionRelationKind.Ally && num >= 75)
		{
			kind = FactionRelationKind.Ally;
			faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
		}
		if (kind == FactionRelationKind.Hostile && num >= 0)
		{
			kind = FactionRelationKind.Neutral;
			faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
		}
		if (kind == FactionRelationKind.Ally && num <= 0)
		{
			kind = FactionRelationKind.Neutral;
			faction.Notify_RelationKindChanged(other, previousKind, canSendLetter, reason, lookTarget, out sentLetter);
		}
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref other, "other");
		Scribe_Values.Look(ref kind, "kind", FactionRelationKind.Neutral);
		Scribe_Values.Look(ref baseGoodwill, "goodwill", 0);
		BackCompatibility.PostExposeData(this);
	}

	public override string ToString()
	{
		return "(" + other?.ToString() + ", kind=" + kind.ToString() + ")";
	}
}
