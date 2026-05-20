using Verse;

namespace RimWorld
{
	public abstract class RoleRequirement
	{
		[NoTranslate]
		public string labelKey;

		public virtual string GetLabel(Precept_Role role)
		{
			return labelKey.Translate();
		}

		public string GetLabelCap(Precept_Role role)
		{
			return GetLabel(role).CapitalizeFirst();
		}

		public abstract bool Met(Pawn p, Precept_Role role);
	}
}
