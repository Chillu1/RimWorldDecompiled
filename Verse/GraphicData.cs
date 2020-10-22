using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class GraphicData
	{
		[NoTranslate]
		public string texPath;

		public Type graphicClass;

		public ShaderTypeDef shaderType;

		public List<ShaderParameter> shaderParameters;

		public Color color = Color.white;

		public Color colorTwo = Color.white;

		public Vector2 drawSize = Vector2.one;

		public Vector3 drawOffset = Vector3.zero;

		public Vector3? drawOffsetNorth;

		public Vector3? drawOffsetEast;

		public Vector3? drawOffsetSouth;

		public Vector3? drawOffsetWest;

		public float onGroundRandomRotateAngle;

		public bool drawRotated = true;

		public bool allowFlip = true;

		public float flipExtraRotation;

		public ShadowData shadowData;

		public DamageGraphicData damageData;

		public LinkDrawerType linkType;

		public LinkFlags linkFlags;

		[Unsaved(false)]
		private Graphic cachedGraphic;

		public bool Linked => linkType != LinkDrawerType.None;

		public Graphic Graphic
		{
			get
			{
				if (cachedGraphic == null)
				{
					Init();
				}
				return cachedGraphic;
			}
		}

		public void CopyFrom(GraphicData other)
		{
			texPath = other.texPath;
			graphicClass = other.graphicClass;
			shaderType = other.shaderType;
			color = other.color;
			colorTwo = other.colorTwo;
			drawSize = other.drawSize;
			drawOffset = other.drawOffset;
			drawOffsetNorth = other.drawOffsetNorth;
			drawOffsetEast = other.drawOffsetEast;
			drawOffsetSouth = other.drawOffsetSouth;
			drawOffsetWest = other.drawOffsetSouth;
			onGroundRandomRotateAngle = other.onGroundRandomRotateAngle;
			drawRotated = other.drawRotated;
			allowFlip = other.allowFlip;
			flipExtraRotation = other.flipExtraRotation;
			shadowData = other.shadowData;
			damageData = other.damageData;
			linkType = other.linkType;
			linkFlags = other.linkFlags;
			cachedGraphic = null;
		}

		private void Init()
		{
			if (graphicClass == null)
			{
				cachedGraphic = null;
				return;
			}
			ShaderTypeDef cutout = shaderType;
			if (cutout == null)
			{
				cutout = ShaderTypeDefOf.Cutout;
			}
			Shader shader = cutout.Shader;
			cachedGraphic = GraphicDatabase.Get(graphicClass, texPath, shader, drawSize, color, colorTwo, this, shaderParameters);
			if (onGroundRandomRotateAngle > 0.01f)
			{
				cachedGraphic = new Graphic_RandomRotated(cachedGraphic, onGroundRandomRotateAngle);
			}
			if (Linked)
			{
				cachedGraphic = GraphicUtility.WrapLinked(cachedGraphic, linkType);
			}
		}

		public void ResolveReferencesSpecial()
		{
			if (damageData != null)
			{
				damageData.ResolveReferencesSpecial();
			}
		}

		public Vector3 DrawOffsetForRot(Rot4 rot)
		{
			return rot.AsInt switch
			{
				0 => drawOffsetNorth ?? drawOffset, 
				1 => drawOffsetEast ?? drawOffset, 
				2 => drawOffsetSouth ?? drawOffset, 
				3 => drawOffsetWest ?? drawOffset, 
				_ => drawOffset, 
			};
		}

		public Graphic GraphicColoredFor(Thing t)
		{
			if (t.DrawColor.IndistinguishableFrom(Graphic.Color) && t.DrawColorTwo.IndistinguishableFrom(Graphic.ColorTwo))
			{
				return Graphic;
			}
			return Graphic.GetColoredVersion(Graphic.Shader, t.DrawColor, t.DrawColorTwo);
		}

		internal IEnumerable<string> ConfigErrors(ThingDef thingDef)
		{
			if (graphicClass == null)
			{
				yield return "graphicClass is null";
			}
			if (texPath.NullOrEmpty())
			{
				yield return "texPath is null or empty";
			}
			if (thingDef != null)
			{
				if (thingDef.drawerType == DrawerType.RealtimeOnly && Linked)
				{
					yield return "does not add to map mesh but has a link drawer. Link drawers can only work on the map mesh.";
				}
				if (!thingDef.rotatable && (drawOffsetNorth.HasValue || drawOffsetEast.HasValue || drawOffsetSouth.HasValue || drawOffsetWest.HasValue))
				{
					yield return "not rotatable but has rotational draw offset(s).";
				}
			}
			if ((shaderType == ShaderTypeDefOf.Cutout || shaderType == ShaderTypeDefOf.CutoutComplex) && thingDef.mote != null && (thingDef.mote.fadeInTime > 0f || thingDef.mote.fadeOutTime > 0f))
			{
				yield return "mote fades but uses cutout shader type. It will abruptly disappear when opacity falls under the cutout threshold.";
			}
		}
	}
}
