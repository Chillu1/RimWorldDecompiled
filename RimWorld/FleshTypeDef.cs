using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class FleshTypeDef : Def
{
	public class HediffWound
	{
		public bool onlyHumanlikes = true;

		public HediffDef hediff;

		public List<Wound> wounds;

		private List<ResolvedWound> woundsResolved;

		public ResolvedWound ChooseWoundOverlay(Hediff hediff)
		{
			if (wounds == null || (onlyHumanlikes && !hediff.pawn.RaceProps.Humanlike))
			{
				return null;
			}
			if (woundsResolved == null)
			{
				woundsResolved = wounds.Select((Wound wound) => wound.Resolve()).ToList();
			}
			return woundsResolved.Where((ResolvedWound w) => w.wound.Fits(hediff)).RandomElementWithFallback();
		}
	}

	public class Wound
	{
		[NoTranslate]
		public string texture;

		[NoTranslate]
		public string textureSouth;

		[NoTranslate]
		public string textureEast;

		[NoTranslate]
		public string textureNorth;

		[NoTranslate]
		public string textureWest;

		public bool flipSouth;

		public bool flipEast;

		public bool flipNorth;

		public bool flipWest;

		public float scale = 1f;

		public Vector3 drawOffsetSouth;

		public Vector3 drawOffsetEastWest;

		public Color color = Color.white;

		public bool tintWithSkinColor;

		[NoTranslate]
		public string flipOnWoundAnchorTag;

		public Rot4 flipOnRotation;

		public BodyPartDef onlyOnPart;

		public bool displayOverApparel = true;

		public bool displayPermanent;

		public bool? missingBodyPartFresh;

		public bool Fits(Hediff hediff)
		{
			if (onlyOnPart != null && (hediff.Part == null || hediff.Part.def != onlyOnPart))
			{
				return false;
			}
			if (missingBodyPartFresh.HasValue && hediff is Hediff_MissingPart hediff_MissingPart && missingBodyPartFresh.Value != (hediff_MissingPart.IsFresh || hediff_MissingPart.Bleeding))
			{
				return false;
			}
			return true;
		}

		public ResolvedWound Resolve()
		{
			Shader shader = (tintWithSkinColor ? ShaderDatabase.WoundSkin : ShaderDatabase.Wound);
			if (texture != null)
			{
				return new ResolvedWound(this, MaterialPool.MatFrom(texture, shader, color));
			}
			return new ResolvedWound(this, MaterialPool.MatFrom(textureSouth, shader, color), MaterialPool.MatFrom(textureEast, shader, color), MaterialPool.MatFrom(textureNorth, shader, color), MaterialPool.MatFrom(textureWest, shader, color), flipSouth, flipEast, flipNorth, flipWest);
		}
	}

	public class ResolvedWound
	{
		public Material material;

		public Material matSouth;

		public Material matEast;

		public Material matNorth;

		public Material matWest;

		public bool flipSouth;

		public bool flipEast;

		public bool flipNorth;

		public bool flipWest;

		public Wound wound;

		public ResolvedWound(Wound wound, Material material)
		{
			this.wound = wound;
			matSouth = (matEast = (matNorth = (matWest = material)));
		}

		public ResolvedWound(Wound wound, Material matSouth, Material matEast, Material matNorth, Material matWest, bool flipSouth, bool flipEast, bool flipNorth, bool flipWest)
		{
			this.wound = wound;
			this.matSouth = matSouth;
			this.matEast = matEast;
			this.matNorth = matNorth;
			this.matWest = matWest;
			this.flipSouth = flipSouth;
			this.flipEast = flipEast;
			this.flipNorth = flipNorth;
			this.flipWest = flipWest;
		}

		public Material GetMaterial(Rot4 rotation, out bool flip)
		{
			flip = false;
			if (rotation == Rot4.South)
			{
				flip = flipSouth;
				return matSouth;
			}
			if (rotation == Rot4.East)
			{
				flip = flipEast;
				return matEast;
			}
			if (rotation == Rot4.West)
			{
				flip = flipWest;
				return matWest;
			}
			if (rotation == Rot4.North)
			{
				flip = flipNorth;
				return matNorth;
			}
			return matSouth;
		}
	}

	public ThoughtDef ateDirect;

	public ThoughtDef ateAsIngredient;

	public ThingCategoryDef corpseCategory;

	public EffecterDef damageEffecter;

	public bool isOrganic = true;

	public List<Wound> genericWounds;

	public List<Wound> bandagedWounds;

	public List<HediffWound> hediffWounds;

	private List<ResolvedWound> bandagedWoundsResolved;

	private List<ResolvedWound> woundsResolved;

	public ResolvedWound ChooseBandagedOverlay()
	{
		if (bandagedWounds == null)
		{
			return null;
		}
		if (bandagedWoundsResolved == null)
		{
			bandagedWoundsResolved = bandagedWounds.Select((Wound wound) => wound.Resolve()).ToList();
		}
		return bandagedWoundsResolved.RandomElement();
	}

	public ResolvedWound ChooseWoundOverlay(Hediff hediff)
	{
		if (genericWounds == null)
		{
			return null;
		}
		if (hediffWounds != null)
		{
			foreach (HediffWound hediffWound in hediffWounds)
			{
				if (hediffWound.hediff != hediff.def)
				{
					continue;
				}
				ResolvedWound resolvedWound = hediffWound.ChooseWoundOverlay(hediff);
				if (resolvedWound != null)
				{
					if (resolvedWound.wound.missingBodyPartFresh.HasValue && !resolvedWound.wound.missingBodyPartFresh.Value)
					{
						return resolvedWound;
					}
					if (hediff.IsTended())
					{
						return ChooseBandagedOverlay();
					}
					return resolvedWound;
				}
			}
		}
		if (hediff is Hediff_Injury || hediff is Hediff_MissingPart { IsFresh: not false })
		{
			if (hediff.IsTended())
			{
				return ChooseBandagedOverlay();
			}
			if (woundsResolved == null)
			{
				woundsResolved = genericWounds.Select((Wound wound) => wound.Resolve()).ToList();
			}
			return woundsResolved.RandomElement();
		}
		return null;
	}
}
