using RimWorld;
using UnityEngine;

namespace Verse;

public struct PawnDrawParms
{
	public Pawn pawn;

	public Matrix4x4 matrix;

	public Rot4 facing;

	public RotDrawMode rotDrawMode;

	public PawnPosture posture;

	public PawnRenderFlags flags;

	public Color tint;

	public Building_Bed bed;

	public ulong skipFlags;

	public bool flipHead;

	public bool coveredInFoam;

	public bool dead;

	public bool crawling;

	public bool swimming;

	public Thing carriedThing;

	public Color? statueColor;

	public static readonly PawnDrawParms Default = new PawnDrawParms
	{
		facing = Rot4.South,
		rotDrawMode = RotDrawMode.Fresh,
		posture = PawnPosture.Standing,
		flags = (PawnRenderFlags.Headgear | PawnRenderFlags.Clothes),
		tint = Color.white
	};

	public bool DrawNow => flags.FlagSet(PawnRenderFlags.DrawNow);

	public bool Portrait => flags.FlagSet(PawnRenderFlags.Portrait);

	public bool Cache => flags.FlagSet(PawnRenderFlags.Cache);

	public bool Statue => flags.FlagSet(PawnRenderFlags.Statue);

	public static PawnDrawParms DefaultFor(Pawn pawn)
	{
		PawnDrawParms result = Default;
		result.pawn = pawn;
		result.facing = pawn.Rotation;
		result.dead = pawn.Dead;
		result.crawling = pawn.Crawling;
		result.swimming = pawn.Swimming;
		return result;
	}

	public bool ShouldRecache(PawnDrawParms other)
	{
		if (pawn != null && !(facing != other.facing) && rotDrawMode == other.rotDrawMode && posture == other.posture && flags == other.flags && bed == other.bed && skipFlags == other.skipFlags && flipHead == other.flipHead && coveredInFoam == other.coveredInFoam && carriedThing == other.carriedThing && dead == other.dead && crawling == other.crawling && swimming == other.swimming)
		{
			Color? color = statueColor;
			Color? color2 = other.statueColor;
			if (color.HasValue != color2.HasValue)
			{
				return true;
			}
			if (!color.HasValue)
			{
				return false;
			}
			return color.GetValueOrDefault() != color2.GetValueOrDefault();
		}
		return true;
	}
}
