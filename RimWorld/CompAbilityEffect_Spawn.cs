using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Spawn : CompAbilityEffect
	{
		public new CompProperties_AbilitySpawn Props => (CompProperties_AbilitySpawn)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			GenSpawn.Spawn(Props.thingDef, target.Cell, parent.pawn.Map);
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			if (target.Cell.Filled(parent.pawn.Map))
			{
				if (throwMessages)
				{
					Messages.Message("AbilityOccupiedCells".Translate(parent.def.LabelCap), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}
	}
}
