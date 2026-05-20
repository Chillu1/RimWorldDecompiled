using Verse;

namespace RimWorld;

public class QuestPart_Filter_FactionHostileToOtherFaction : QuestPart_Filter
{
	public Faction faction;

	public Faction other;

	protected override bool Pass(SignalArgs args)
	{
		if (faction == null || other == null || faction == other)
		{
			return false;
		}
		return faction.HostileTo(other);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref faction, "faction");
		Scribe_References.Look(ref other, "other");
	}
}
