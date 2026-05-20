using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class BodyTypeDef : Def
{
	public class WoundAnchor
	{
		[NoTranslate]
		public string tag;

		public BodyPartGroupDef group;

		public bool? narrowCrown;

		public Rot4? rotation;

		public bool canMirror = true;

		public Vector3 offset;

		public PawnOverlayDrawer.OverlayLayer layer;

		public Color debugColor;

		public float range;
	}

	[NoTranslate]
	public string bodyNakedGraphicPath;

	[NoTranslate]
	public string bodyDessicatedGraphicPath;

	public List<WoundAnchor> woundAnchors;

	public float woundScale = 1f;

	public Vector2 headOffset;

	public Vector2 bodyGraphicScale = Vector2.one;

	public float bedOffset;

	public List<AttachPoint> attachPoints;

	public List<AttachPoint> attachPointsDessicated;

	public virtual Texture2D Icon => ContentFinder<Texture2D>.Get(bodyNakedGraphicPath + "_south");
}
