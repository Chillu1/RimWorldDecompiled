using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class ApparelUtility
{
	public struct LayerGroupPair : IEquatable<LayerGroupPair>
	{
		private readonly ApparelLayerDef layer;

		private readonly BodyPartGroupDef group;

		public LayerGroupPair(ApparelLayerDef layer, BodyPartGroupDef group)
		{
			this.layer = layer;
			this.group = group;
		}

		public override bool Equals(object rhs)
		{
			if (!(rhs is LayerGroupPair))
			{
				return false;
			}
			return Equals((LayerGroupPair)rhs);
		}

		public bool Equals(LayerGroupPair other)
		{
			if (other.layer == layer)
			{
				return other.group == group;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (17 * 23 + layer.GetHashCode()) * 23 + group.GetHashCode();
		}
	}

	public static bool IsRequirementActive(ApparelRequirement requirement, ApparelRequirementSource source, Pawn pawn, out string disabledByLabel)
	{
		bool flag = false;
		if (pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TraitDefOf.Nudist))
		{
			flag = true;
		}
		else if (pawn.Ideo != null && pawn.Ideo.IdeoPrefersNudity())
		{
			flag = true;
		}
		if (flag)
		{
			disabledByLabel = "Nudism".Translate();
			return false;
		}
		if (source != ApparelRequirementSource.Title && pawn.royalty != null && !pawn.royalty.AllTitlesForReading.NullOrEmpty())
		{
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
			{
				if (!item.def.requiredApparel.NullOrEmpty())
				{
					disabledByLabel = item.def.GetLabelCapFor(pawn);
					return false;
				}
			}
		}
		disabledByLabel = null;
		return true;
	}

	public static Apparel GetApparelReplacedByNewApparel(Pawn pawn, Apparel newApparel)
	{
		for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
		{
			if (!CanWearTogether(newApparel.def, pawn.apparel.WornApparel[i].def, pawn.RaceProps.body))
			{
				return pawn.apparel.WornApparel[i];
			}
		}
		return null;
	}

	public static bool CanWearTogether(ThingDef A, ThingDef B, BodyDef body)
	{
		bool flag = false;
		for (int i = 0; i < A.apparel.layers.Count; i++)
		{
			for (int j = 0; j < B.apparel.layers.Count; j++)
			{
				if (A.apparel.layers[i] == B.apparel.layers[j])
				{
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (!flag)
		{
			return true;
		}
		List<BodyPartGroupDef> bodyPartGroups = A.apparel.bodyPartGroups;
		List<BodyPartGroupDef> bodyPartGroups2 = B.apparel.bodyPartGroups;
		BodyPartGroupDef[] interferingBodyPartGroups = A.apparel.GetInterferingBodyPartGroups(body);
		BodyPartGroupDef[] interferingBodyPartGroups2 = B.apparel.GetInterferingBodyPartGroups(body);
		for (int k = 0; k < bodyPartGroups.Count; k++)
		{
			if (interferingBodyPartGroups2.Contains(bodyPartGroups[k]))
			{
				return false;
			}
		}
		for (int l = 0; l < bodyPartGroups2.Count; l++)
		{
			if (interferingBodyPartGroups.Contains(bodyPartGroups2[l]))
			{
				return false;
			}
		}
		return true;
	}

	public static void GenerateLayerGroupPairs(BodyDef body, ThingDef td, Action<LayerGroupPair> callback)
	{
		for (int i = 0; i < td.apparel.layers.Count; i++)
		{
			ApparelLayerDef layer = td.apparel.layers[i];
			BodyPartGroupDef[] interferingBodyPartGroups = td.apparel.GetInterferingBodyPartGroups(body);
			for (int j = 0; j < interferingBodyPartGroups.Length; j++)
			{
				callback(new LayerGroupPair(layer, interferingBodyPartGroups[j]));
			}
		}
	}

	public static bool HasPartsToWear(Pawn p, ThingDef apparel)
	{
		IEnumerable<BodyPartRecord> notMissingParts = p.health.hediffSet.GetNotMissingParts();
		List<BodyPartGroupDef> groups = apparel.apparel.bodyPartGroups;
		int i;
		for (i = 0; i < groups.Count; i++)
		{
			if (notMissingParts.Any((BodyPartRecord x) => x.IsInGroup(groups[i])))
			{
				return true;
			}
		}
		return false;
	}
}
