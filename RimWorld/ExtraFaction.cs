using Verse;

namespace RimWorld
{
	public class ExtraFaction : IExposable
	{
		public Faction faction;

		public ExtraFactionType factionType;

		public ExtraFaction()
		{
		}

		public ExtraFaction(Faction faction, ExtraFactionType factionType)
		{
			this.faction = faction;
			this.factionType = factionType;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref factionType, "factionType", ExtraFactionType.HomeFaction);
		}
	}
}
