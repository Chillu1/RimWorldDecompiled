using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IdeoRoleInstance : IExposable
	{
		public Precept sourceRole;

		public Pawn pawn;

		public List<Ability> abilities;

		public IdeoRoleInstance(Precept_Role sourceRole)
		{
			this.sourceRole = sourceRole;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);
			if (Scribe.mode != LoadSaveMode.PostLoadInit || abilities == null)
			{
				return;
			}
			foreach (Ability ability in abilities)
			{
				ability.pawn = pawn;
				ability.verb.caster = pawn;
				ability.sourcePrecept = sourceRole;
			}
		}
	}
}
