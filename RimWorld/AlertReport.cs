using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct AlertReport
	{
		public bool active;

		public List<Thing> culpritsThings;

		public List<Pawn> culpritsPawns;

		public List<Caravan> culpritsCaravans;

		public List<GlobalTargetInfo> culpritsTargets;

		public GlobalTargetInfo? culpritTarget;

		public bool AnyCulpritValid
		{
			get
			{
				if (!culpritsThings.NullOrEmpty() || !culpritsPawns.NullOrEmpty() || !culpritsCaravans.NullOrEmpty())
				{
					return true;
				}
				if (culpritTarget.HasValue && culpritTarget.Value.IsValid)
				{
					return true;
				}
				if (culpritsTargets != null)
				{
					for (int i = 0; i < culpritsTargets.Count; i++)
					{
						if (culpritsTargets[i].IsValid)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public IEnumerable<GlobalTargetInfo> AllCulprits
		{
			get
			{
				if (culpritsThings != null)
				{
					for (int l = 0; l < culpritsThings.Count; l++)
					{
						yield return culpritsThings[l];
					}
				}
				if (culpritsPawns != null)
				{
					for (int l = 0; l < culpritsPawns.Count; l++)
					{
						yield return culpritsPawns[l];
					}
				}
				if (culpritsCaravans != null)
				{
					for (int l = 0; l < culpritsCaravans.Count; l++)
					{
						yield return culpritsCaravans[l];
					}
				}
				if (culpritTarget.HasValue)
				{
					yield return culpritTarget.Value;
				}
				if (culpritsTargets != null)
				{
					for (int l = 0; l < culpritsTargets.Count; l++)
					{
						yield return culpritsTargets[l];
					}
				}
			}
		}

		public static AlertReport Active
		{
			get
			{
				AlertReport result = default(AlertReport);
				result.active = true;
				return result;
			}
		}

		public static AlertReport Inactive
		{
			get
			{
				AlertReport result = default(AlertReport);
				result.active = false;
				return result;
			}
		}

		public static AlertReport CulpritIs(GlobalTargetInfo culp)
		{
			AlertReport result = default(AlertReport);
			result.active = culp.IsValid;
			if (culp.IsValid)
			{
				result.culpritTarget = culp;
			}
			return result;
		}

		public static AlertReport CulpritsAre(List<Thing> culprits)
		{
			AlertReport result = default(AlertReport);
			result.culpritsThings = culprits;
			result.active = result.AnyCulpritValid;
			return result;
		}

		public static AlertReport CulpritsAre(List<Pawn> culprits)
		{
			AlertReport result = default(AlertReport);
			result.culpritsPawns = culprits;
			result.active = result.AnyCulpritValid;
			return result;
		}

		public static AlertReport CulpritsAre(List<Caravan> culprits)
		{
			AlertReport result = default(AlertReport);
			result.culpritsCaravans = culprits;
			result.active = result.AnyCulpritValid;
			return result;
		}

		public static AlertReport CulpritsAre(List<GlobalTargetInfo> culprits)
		{
			AlertReport result = default(AlertReport);
			result.culpritsTargets = culprits;
			result.active = result.AnyCulpritValid;
			return result;
		}

		public static implicit operator AlertReport(bool b)
		{
			AlertReport result = default(AlertReport);
			result.active = b;
			return result;
		}

		public static implicit operator AlertReport(Thing culprit)
		{
			return CulpritIs(culprit);
		}

		public static implicit operator AlertReport(WorldObject culprit)
		{
			return CulpritIs(culprit);
		}

		public static implicit operator AlertReport(GlobalTargetInfo culprit)
		{
			return CulpritIs(culprit);
		}
	}
}
