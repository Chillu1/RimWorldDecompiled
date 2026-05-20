using RimWorld;

namespace Verse
{
	public abstract class CompAIUsablePack : ThingComp
	{
		protected CompProperties_AIUSablePack Props => (CompProperties_AIUSablePack)props;

		public override void CompTick()
		{
			Pawn wearer = ((Apparel)parent).Wearer;
			if (CanOpportunisticallyUseNow(wearer))
			{
				UsePack(wearer);
			}
		}

		private bool CanOpportunisticallyUseNow(Pawn wearer)
		{
			if (wearer == null || wearer.Dead || !wearer.Spawned)
			{
				return false;
			}
			if (!wearer.IsHashIntervalTick(Props.checkInterval))
			{
				return false;
			}
			if (wearer.Downed || !wearer.Awake())
			{
				return false;
			}
			if (wearer.IsColonistPlayerControlled)
			{
				return false;
			}
			if (wearer.InMentalState)
			{
				return false;
			}
			return Rand.Value < ChanceToUse(wearer);
		}

		protected abstract float ChanceToUse(Pawn wearer);

		protected abstract void UsePack(Pawn wearer);
	}
}
