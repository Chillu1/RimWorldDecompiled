using Verse;

namespace RimWorld
{
	public class HediffComp_ExplodeOnDeath : HediffComp
	{
		public HediffCompProperties_ExplodeOnDeath Props => (HediffCompProperties_ExplodeOnDeath)props;

		public override void Notify_PawnKilled()
		{
			GenExplosion.DoExplosion(base.Pawn.Position, base.Pawn.Map, Props.explosionRadius, Props.damageDef, base.Pawn, Props.damageAmount);
			if (Props.destroyGear)
			{
				base.Pawn.equipment.DestroyAllEquipment();
				base.Pawn.apparel.DestroyAll();
			}
		}

		public override void Notify_PawnDied()
		{
			if (Props.destroyBody)
			{
				base.Pawn.Corpse.Destroy();
			}
		}
	}
}
