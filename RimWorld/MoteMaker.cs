using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class MoteMaker
{
	private static IntVec3[] UpRightPattern = new IntVec3[4]
	{
		new IntVec3(0, 0, 0),
		new IntVec3(1, 0, 0),
		new IntVec3(0, 0, 1),
		new IntVec3(1, 0, 1)
	};

	public static Mote MakeStaticMote(IntVec3 cell, Map map, ThingDef moteDef, float scale = 1f)
	{
		return MakeStaticMote(cell.ToVector3Shifted(), map, moteDef, scale);
	}

	public static Mote MakeStaticMote(Vector3 loc, Map map, ThingDef moteDef, float scale = 1f, bool makeOffscreen = false, float exactRot = 0f)
	{
		if (!makeOffscreen)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated)
			{
				return null;
			}
		}
		else if (!loc.InBounds(map) || map.moteCounter.Saturated)
		{
			return null;
		}
		Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
		GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
		obj.exactPosition = loc;
		obj.Scale = scale;
		obj.exactRotation = exactRot;
		return obj;
	}

	public static void ThrowText(Vector3 loc, Map map, string text, float timeBeforeStartFadeout = -1f)
	{
		ThrowText(loc, map, text, Color.white, timeBeforeStartFadeout);
	}

	public static void ThrowText(Vector3 loc, Map map, string text, Color color, float timeBeforeStartFadeout = -1f)
	{
		IntVec3 intVec = loc.ToIntVec3();
		if (intVec.InBounds(map))
		{
			MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text);
			moteText.exactPosition = loc;
			moteText.SetVelocity(Rand.Range(5, 35), Rand.Range(0.42f, 0.45f));
			moteText.text = text;
			moteText.textColor = color;
			if (timeBeforeStartFadeout >= 0f)
			{
				moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
			}
			GenSpawn.Spawn(moteText, intVec, map);
		}
	}

	public static Mote MakeStunOverlay(Thing stunnedThing)
	{
		Mote obj = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_Stun);
		obj.Attach(stunnedThing);
		GenSpawn.Spawn(obj, stunnedThing.Position, stunnedThing.Map);
		return obj;
	}

	public static MoteDualAttached MakeInteractionOverlay(ThingDef moteDef, TargetInfo A, TargetInfo B)
	{
		MoteDualAttached obj = (MoteDualAttached)ThingMaker.MakeThing(moteDef);
		obj.Scale = 0.5f;
		obj.Attach(A, B);
		GenSpawn.Spawn(obj, A.Cell, A.Map ?? B.Map);
		return obj;
	}

	public static MoteDualAttached MakeInteractionOverlay(ThingDef moteDef, TargetInfo A, TargetInfo B, Vector3 offsetA, Vector3 offsetB)
	{
		MoteDualAttached obj = (MoteDualAttached)ThingMaker.MakeThing(moteDef);
		obj.Scale = 0.5f;
		obj.Attach(A, B, offsetA, offsetB);
		GenSpawn.Spawn(obj, A.Cell, A.Map ?? B.Map);
		return obj;
	}

	public static Mote MakeAttachedOverlay(Thing thing, ThingDef moteDef, Vector3 offset, float scale = 1f, float solidTimeOverride = -1f)
	{
		Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
		obj.Attach(thing, offset);
		obj.Scale = scale;
		obj.exactPosition = thing.DrawPos + offset;
		obj.solidTimeOverride = solidTimeOverride;
		GenSpawn.Spawn(obj, thing.Position, thing.MapHeld);
		return obj;
	}

	public static void MakeColonistActionOverlay(Pawn pawn, ThingDef moteDef)
	{
		MoteThrownAttached obj = (MoteThrownAttached)ThingMaker.MakeThing(moteDef);
		obj.Attach(pawn);
		obj.exactPosition = pawn.DrawPos;
		obj.Scale = 1.5f;
		obj.SetVelocity(Rand.Range(20f, 25f), 0.4f);
		GenSpawn.Spawn(obj, pawn.Position, pawn.Map);
	}

	private static MoteBubble ExistingMoteBubbleOn(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return null;
		}
		for (int i = 0; i < 4; i++)
		{
			if (!(pawn.Position + UpRightPattern[i]).InBounds(pawn.Map))
			{
				continue;
			}
			List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] is MoteBubble moteBubble && moteBubble.link1.Linked && moteBubble.link1.Target.HasThing && moteBubble.link1.Target == pawn)
				{
					return moteBubble;
				}
			}
		}
		return null;
	}

	public static MoteBubble MakeMoodThoughtBubble(Pawn pawn, Thought thought)
	{
		if (Current.ProgramState != ProgramState.Playing)
		{
			return null;
		}
		if (!pawn.Spawned)
		{
			return null;
		}
		float num = thought.MoodOffset();
		if (num == 0f)
		{
			return null;
		}
		MoteBubble moteBubble = ExistingMoteBubbleOn(pawn);
		if (moteBubble != null)
		{
			if (moteBubble.def == ThingDefOf.Mote_Speech)
			{
				return null;
			}
			if (moteBubble.def == ThingDefOf.Mote_ThoughtBad || moteBubble.def == ThingDefOf.Mote_ThoughtGood)
			{
				moteBubble.Destroy();
			}
		}
		MoteBubble obj = (MoteBubble)ThingMaker.MakeThing((num > 0f) ? ThingDefOf.Mote_ThoughtGood : ThingDefOf.Mote_ThoughtBad);
		obj.SetupMoteBubble(thought.Icon, null);
		obj.Attach(pawn);
		GenSpawn.Spawn(obj, pawn.Position, pawn.Map);
		return obj;
	}

	public static MoteBubble MakeThoughtBubble(Pawn pawn, string iconPath, bool maintain = false)
	{
		ExistingMoteBubbleOn(pawn)?.Destroy();
		MoteBubble obj = (MoteBubble)ThingMaker.MakeThing(maintain ? ThingDefOf.Mote_ForceJobMaintained : ThingDefOf.Mote_ForceJob);
		obj.SetupMoteBubble(ContentFinder<Texture2D>.Get(iconPath), null);
		obj.Attach(pawn);
		GenSpawn.Spawn(obj, pawn.Position, pawn.Map);
		return obj;
	}

	public static MoteBubble MakeInteractionBubble(Pawn initiator, Pawn recipient, ThingDef interactionMote, Texture2D symbol, Color? iconColor = null)
	{
		MoteBubble moteBubble = ExistingMoteBubbleOn(initiator);
		if (moteBubble != null)
		{
			if (moteBubble.def == ThingDefOf.Mote_Speech)
			{
				moteBubble.Destroy();
			}
			if (moteBubble.def == ThingDefOf.Mote_ThoughtBad || moteBubble.def == ThingDefOf.Mote_ThoughtGood)
			{
				moteBubble.Destroy();
			}
		}
		MoteBubble obj = (MoteBubble)ThingMaker.MakeThing(interactionMote);
		obj.SetupMoteBubble(symbol, recipient, iconColor);
		obj.Attach(initiator);
		GenSpawn.Spawn(obj, initiator.Position, initiator.Map);
		return obj;
	}

	public static MoteBubble MakeSpeechBubble(Pawn initiator, Texture2D symbol)
	{
		MoteBubble moteBubble = ExistingMoteBubbleOn(initiator);
		if (moteBubble != null)
		{
			if (moteBubble.def == ThingDefOf.Mote_Speech)
			{
				moteBubble.Destroy();
			}
			if (moteBubble.def == ThingDefOf.Mote_ThoughtBad || moteBubble.def == ThingDefOf.Mote_ThoughtGood)
			{
				moteBubble.Destroy();
			}
		}
		MoteBubble obj = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_Speech);
		obj.SetupMoteBubble(symbol, null);
		obj.Attach(initiator);
		GenSpawn.Spawn(obj, initiator.Position, initiator.Map);
		return obj;
	}

	public static void ThrowExplosionCell(IntVec3 cell, Map map, ThingDef moteDef, Color color)
	{
		if (cell.ShouldSpawnMotesAt(map))
		{
			Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
			obj.exactRotation = 90 * Rand.RangeInclusive(0, 3);
			obj.exactPosition = cell.ToVector3Shifted();
			obj.instanceColor = color;
			GenSpawn.Spawn(obj, cell, map);
			if (Rand.Value < 0.7f)
			{
				FleckMaker.ThrowDustPuff(cell, map, 1.2f);
			}
		}
	}

	public static void ThrowExplosionInteriorMote(Vector3 loc, Map map, ThingDef moteDef)
	{
		if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
		{
			MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(moteDef);
			obj.Scale = Rand.Range(3f, 4.5f);
			obj.rotationRate = Rand.Range(-30f, 30f);
			obj.exactPosition = loc;
			obj.SetVelocity(Rand.Range(0, 360), Rand.Range(0.48f, 0.72f));
			GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
		}
	}

	public static void MakeBombardmentMote(IntVec3 cell, Map map, float scale)
	{
		Mote obj = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_Bombardment);
		obj.exactPosition = cell.ToVector3Shifted();
		obj.Scale = 150f * scale;
		obj.rotationRate = 1.2f;
		GenSpawn.Spawn(obj, cell, map);
	}

	public static void MakePowerBeamMote(IntVec3 cell, Map map)
	{
		Mote obj = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam);
		obj.exactPosition = cell.ToVector3Shifted();
		obj.Scale = 90f;
		obj.rotationRate = 1.2f;
		GenSpawn.Spawn(obj, cell, map);
	}

	public static void PlaceTempRoof(IntVec3 cell, Map map)
	{
		if (cell.ShouldSpawnMotesAt(map))
		{
			Mote obj = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_TempRoof);
			obj.exactPosition = cell.ToVector3Shifted();
			GenSpawn.Spawn(obj, cell, map);
		}
	}

	public static Mote MakeConnectingLine(Vector3 start, Vector3 end, ThingDef moteType, Map map, float width = 1f)
	{
		Vector3 vector = end - start;
		float x = vector.MagnitudeHorizontal();
		Mote mote = MakeStaticMote(start + vector * 0.5f, map, moteType);
		if (mote != null)
		{
			mote.linearScale = new Vector3(x, 1f, width);
			mote.exactRotation = Mathf.Atan2(0f - vector.z, vector.x) * 57.29578f;
		}
		return mote;
	}
}
