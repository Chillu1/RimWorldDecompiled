using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
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
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			bool flag = false;
			for (int j = 0; j < hediffs.Count; j++)
			{
				if (hediffs[j] is Hediff_MissingPart)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
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
}
