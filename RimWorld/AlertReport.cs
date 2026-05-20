using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

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
				for (int i = 0; i < culpritsThings.Count; i++)
				{
					yield return culpritsThings[i];
				}
			}
			if (culpritsPawns != null)
			{
				for (int i = 0; i < culpritsPawns.Count; i++)
				{
					yield return culpritsPawns[i];
				}
			}
			if (culpritsCaravans != null)
			{
				for (int i = 0; i < culpritsCaravans.Count; i++)
				{
					yield return culpritsCaravans[i];
				}
			}
			if (culpritTarget.HasValue)
			{
				yield return culpritTarget.Value;
			}
			if (culpritsTargets != null)
			{
				for (int i = 0; i < culpritsTargets.Count; i++)
				{
					yield return culpritsTargets[i];
				}
			}
		}
	}

	public static AlertReport Active => new AlertReport
	{
		active = true
	};

	public static AlertReport Inactive => new AlertReport
	{
		active = false
	};

	public static AlertReport CulpritIs(GlobalTargetInfo culp)
	{
		AlertReport result = new AlertReport
		{
			active = culp.IsValid
		};
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
		return new AlertReport
		{
			active = b
		};
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
