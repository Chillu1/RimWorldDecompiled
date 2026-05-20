using Verse;

namespace RimWorld
{
	public abstract class RoleEffect
	{
		[MustTranslate]
		public string label;

		[NoTranslate]
		public string labelKey;

		public virtual bool IsBad => false;

		public virtual string Label(Pawn pawn, Precept_Role role)
		{
			if (!labelKey.NullOrEmpty())
			{
				return labelKey.Translate(pawn.Named("PAWN"), role.LabelCap);
			}
			return label.Formatted(pawn.Named("PAWN"), role.LabelCap);
		}

		public virtual bool CanEquip(Pawn pawn, Thing thing)
		{
			return true;
		}

		public virtual void Notify_Tended(Pawn doctor, Pawn target)
		{
		}
	}
}
