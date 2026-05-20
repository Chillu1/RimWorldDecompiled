using Verse;

namespace RimWorld
{
	public class CompFireBurst : ThingComp
	{
		private CompExplosive compExplosive;

		private Effecter effecter;

		private CompProperties_FireBurst Props => (CompProperties_FireBurst)props;

		private CompExplosive CompExplosive
		{
			get
			{
				if (compExplosive == null)
				{
					compExplosive = parent.TryGetComp<CompExplosive>();
				}
				return compExplosive;
			}
		}

		public override void CompTick()
		{
			if (CompExplosive.wickStarted)
			{
				FireBurstUtility.ThrowFuelTick(parent.Position, Props.radius, parent.Map);
				if (CompExplosive.wickTicksLeft <= Props.ticksAwayFromDetonate && effecter == null)
				{
					effecter = EffecterDefOf.Fire_Burst.Spawn(parent.Position, parent.Map);
				}
			}
			effecter?.EffectTick(parent, parent);
		}
	}
}
