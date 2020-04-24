using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ApparelProperties
	{
		public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();

		public List<ApparelLayerDef> layers = new List<ApparelLayerDef>();

		[NoTranslate]
		public string wornGraphicPath = "";

		public bool useWornGraphicMask;

		[NoTranslate]
		public List<string> tags = new List<string>();

		[NoTranslate]
		public List<string> defaultOutfitTags;

		public bool canBeGeneratedToSatisfyWarmth = true;

		public float wearPerDay = 0.4f;

		public bool careIfWornByCorpse = true;

		public bool hatRenderedFrontOfFace;

		public bool useDeflectMetalEffect;

		public Gender gender;

		[Unsaved(false)]
		private float cachedHumanBodyCoverage = -1f;

		[Unsaved(false)]
		private BodyPartGroupDef[][] interferingBodyPartGroups;

		private static BodyPartGroupDef[] apparelRelevantGroups;

		public ApparelLayerDef LastLayer
		{
			get
			{
				if (layers.Count > 0)
				{
					return layers[layers.Count - 1];
				}
				Log.ErrorOnce("Failed to get last layer on apparel item (see your config errors)", 31234937);
				return ApparelLayerDefOf.Belt;
			}
		}

		public float HumanBodyCoverage
		{
			get
			{
				if (cachedHumanBodyCoverage < 0f)
				{
					cachedHumanBodyCoverage = 0f;
					List<BodyPartRecord> allParts = BodyDefOf.Human.AllParts;
					for (int i = 0; i < allParts.Count; i++)
					{
						if (CoversBodyPart(allParts[i]))
						{
							cachedHumanBodyCoverage += allParts[i].coverageAbs;
						}
					}
				}
				return cachedHumanBodyCoverage;
			}
		}

		public bool CorrectGenderForWearing(Gender wearerGender)
		{
			if (gender == Gender.None)
			{
				return true;
			}
			return gender == wearerGender;
		}

		public static void ResetStaticData()
		{
			apparelRelevantGroups = DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => td.IsApparel).SelectMany((ThingDef td) => td.apparel.bodyPartGroups).Distinct()
				.ToArray();
		}

		public IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (layers.NullOrEmpty())
			{
				yield return parentDef.defName + " apparel has no layers.";
			}
		}

		public bool CoversBodyPart(BodyPartRecord partRec)
		{
			for (int i = 0; i < partRec.groups.Count; i++)
			{
				if (bodyPartGroups.Contains(partRec.groups[i]))
				{
					return true;
				}
			}
			return false;
		}

		public string GetCoveredOuterPartsString(BodyDef body)
		{
			return (from part in body.AllParts.Where((BodyPartRecord x) => x.depth == BodyPartDepth.Outside && x.groups.Any((BodyPartGroupDef y) => bodyPartGroups.Contains(y))).Distinct()
				select part.Label).ToCommaList().CapitalizeFirst();
		}

		public string GetLayersString()
		{
			return layers.Select((ApparelLayerDef layer) => layer.label).ToCommaList().CapitalizeFirst();
		}

		public BodyPartGroupDef[] GetInterferingBodyPartGroups(BodyDef body)
		{
			if (interferingBodyPartGroups == null || interferingBodyPartGroups.Length != DefDatabase<BodyDef>.DefCount)
			{
				interferingBodyPartGroups = new BodyPartGroupDef[DefDatabase<BodyDef>.DefCount][];
			}
			if (interferingBodyPartGroups[body.index] == null)
			{
				BodyPartGroupDef[] array = (from bpgd in body.AllParts.Where((BodyPartRecord part) => part.groups.Any((BodyPartGroupDef @group) => bodyPartGroups.Contains(@group))).ToArray().SelectMany((BodyPartRecord bpr) => bpr.groups)
						.Distinct()
					where apparelRelevantGroups.Contains(bpgd)
					select bpgd).ToArray();
				interferingBodyPartGroups[body.index] = array;
			}
			return interferingBodyPartGroups[body.index];
		}
	}
}
