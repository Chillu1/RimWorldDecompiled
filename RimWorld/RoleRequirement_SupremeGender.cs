using Verse;

namespace RimWorld
{
	public class RoleRequirement_SupremeGender : RoleRequirement
	{
		private bool ActiveFor(Precept_Role role)
		{
			if (role.restrictToSupremeGender)
			{
				return role.ideo.SupremeGender != Gender.None;
			}
			return false;
		}

		public override string GetLabel(Precept_Role role)
		{
			if (!ActiveFor(role))
			{
				return string.Empty;
			}
			return labelKey.Translate(role.ideo.SupremeGender.GetLabel());
		}

		public override bool Met(Pawn pawn, Precept_Role role)
		{
			if (!ActiveFor(role))
			{
				return true;
			}
			return pawn.gender == role.ideo.SupremeGender;
		}
	}
}
