namespace RimWorld
{
	public struct ApparelRequirementWithSource
	{
		public ApparelRequirement requirement;

		public Precept_Role sourceRole;

		public RoyalTitle sourceTitle;

		public ApparelRequirementSource Source
		{
			get
			{
				if (sourceRole != null)
				{
					return ApparelRequirementSource.Role;
				}
				if (sourceTitle != null)
				{
					return ApparelRequirementSource.Title;
				}
				return ApparelRequirementSource.Invalid;
			}
		}

		public string SourceLabelCap
		{
			get
			{
				if (sourceTitle != null)
				{
					return sourceTitle.def.GetLabelCapFor(sourceTitle.pawn);
				}
				return sourceRole.LabelCap;
			}
		}

		public ApparelRequirementWithSource(ApparelRequirement requirement, Precept_Role role)
		{
			this.requirement = requirement;
			sourceRole = role;
			sourceTitle = null;
		}

		public ApparelRequirementWithSource(ApparelRequirement requirement, RoyalTitle title)
		{
			this.requirement = requirement;
			sourceTitle = title;
			sourceRole = null;
		}
	}
}
