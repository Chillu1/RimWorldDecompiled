using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class MoteMaker
	{
		private static IntVec3[] UpRightPattern = new IntVec3[4]
		{
			new IntVec3(0, 0, 0),
			new IntVec3(1, 0, 0),
			new IntVec3(0, 0, 1),
			new IntVec3(1, 0, 1)
		};

		public static Mote ThrowMetaIcon(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (!cell.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated)
			{
				return null;
			}
			MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(moteDef);
			obj.Scale = 0.7f;
			obj.rotationRate = Rand.Range(-3f, 3f);
			obj.exactPosition = cell.ToVector3Shifted();
			obj.exactPosition += new Vector3(0.35f, 0f, 0.35f);
			obj.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value) * 0.1f;
			obj.SetVelocity(Rand.Range(30, 60), 0.42f);
			GenSpawn.Spawn(obj, cell, map);
			return obj;
		}

		public static void MakeStaticMote(IntVec3 cell, Map map, ThingDef moteDef, float scale = 1f)
		{
			MakeStaticMote(cell.ToVector3Shifted(), map, moteDef, scale);
		}

		public static Mote MakeStaticMote(Vector3 loc, Map map, ThingDef moteDef, float scale = 1f)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated)
			{
				return null;
			}
			Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
			obj.exactPosition = loc;
			obj.Scale = scale;
			GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
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

		public static void ThrowMetaPuffs(CellRect rect, Map map)
		{
			if (Find.TickManager.Paused)
			{
				return;
			}
			for (int i = rect.minX; i <= rect.maxX; i++)
			{
				for (int j = rect.minZ; j <= rect.maxZ; j++)
				{
					ThrowMetaPuffs(new TargetInfo(new IntVec3(i, 0, j), map));
				}
			}
		}

		public static void ThrowMetaPuffs(TargetInfo targ)
		{
			Vector3 a = (targ.HasThing ? targ.Thing.TrueCenter() : targ.Cell.ToVector3Shifted());
			int num = Rand.RangeInclusive(4, 6);
			for (int i = 0; i < num; i++)
			{
				ThrowMetaPuff(a + new Vector3(Rand.Range(-0.5f, 0.5f), 0f, Rand.Range(-0.5f, 0.5f)), targ.Map);
			}
		}

		public static void ThrowMetaPuff(Vector3 loc, Map map)
		{
			if (loc.ShouldSpawnMotesAt(map))
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_MetaPuff);
				obj.Scale = 1.9f;
				obj.rotationRate = Rand.Range(-60, 60);
				obj.exactPosition = loc;
				obj.SetVelocity(Rand.Range(0, 360), Rand.Range(0.6f, 0.78f));
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		private static MoteThrown NewBaseAirPuff()
		{
			MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_AirPuff);
			obj.Scale = 1.5f;
			obj.rotationRate = Rand.RangeInclusive(-240, 240);
			return obj;
		}

		public static void ThrowAirPuffUp(Vector3 loc, Map map)
		{
			if (loc.ToIntVec3().ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown moteThrown = NewBaseAirPuff();
				moteThrown.exactPosition = loc;
				moteThrown.exactPosition += new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
				moteThrown.SetVelocity(Rand.Range(-45, 45), Rand.Range(1.2f, 1.5f));
				GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowBreathPuff(Vector3 loc, Map map, float throwAngle, Vector3 inheritVelocity)
		{
			if (loc.ToIntVec3().ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown moteThrown = NewBaseAirPuff();
				moteThrown.exactPosition = loc;
				moteThrown.exactPosition += new Vector3(Rand.Range(-0.005f, 0.005f), 0f, Rand.Range(-0.005f, 0.005f));
				moteThrown.SetVelocity(throwAngle + (float)Rand.Range(-10, 10), Rand.Range(0.1f, 0.8f));
				moteThrown.Velocity += inheritVelocity * 0.5f;
				moteThrown.Scale = Rand.Range(0.6f, 0.7f);
				GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowDustPuff(IntVec3 cell, Map map, float scale)
		{
			ThrowDustPuff(cell.ToVector3() + new Vector3(Rand.Value, 0f, Rand.Value), map, scale);
		}

		public static void ThrowDustPuff(Vector3 loc, Map map, float scale)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_DustPuff);
				obj.Scale = 1.9f * scale;
				obj.rotationRate = Rand.Range(-60, 60);
				obj.exactPosition = loc;
				obj.SetVelocity(Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowDustPuffThick(Vector3 loc, Map map, float scale, Color color)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_DustPuffThick);
				obj.Scale = scale;
				obj.rotationRate = Rand.Range(-60, 60);
				obj.exactPosition = loc;
				obj.instanceColor = color;
				obj.SetVelocity(Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowTornadoDustPuff(Vector3 loc, Map map, float scale, Color color)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_TornadoDustPuff);
				obj.Scale = 1.9f * scale;
				obj.rotationRate = Rand.Range(-60, 60);
				obj.exactPosition = loc;
				obj.instanceColor = color;
				obj.SetVelocity(Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowSmoke(Vector3 loc, Map map, float size)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke);
				obj.Scale = Rand.Range(1.5f, 2.5f) * size;
				obj.rotationRate = Rand.Range(-30f, 30f);
				obj.exactPosition = loc;
				obj.SetVelocity(Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowFireGlow(IntVec3 c, Map map, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (vector.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
				if (vector.InBounds(map))
				{
					MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_FireGlow);
					obj.Scale = Rand.Range(4f, 6f) * size;
					obj.rotationRate = Rand.Range(-3f, 3f);
					obj.exactPosition = vector;
					obj.SetVelocity(Rand.Range(0, 360), 0.12f);
					GenSpawn.Spawn(obj, vector.ToIntVec3(), map);
				}
			}
		}

		public static void ThrowHeatGlow(IntVec3 c, Map map, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (vector.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
				if (vector.InBounds(map))
				{
					MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_HeatGlow);
					obj.Scale = Rand.Range(4f, 6f) * size;
					obj.rotationRate = Rand.Range(-3f, 3f);
					obj.exactPosition = vector;
					obj.SetVelocity(Rand.Range(0, 360), 0.12f);
					GenSpawn.Spawn(obj, vector.ToIntVec3(), map);
				}
			}
		}

		public static void ThrowMicroSparks(Vector3 loc, Map map)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_MicroSparks);
				obj.Scale = Rand.Range(0.8f, 1.2f);
				obj.rotationRate = Rand.Range(-12f, 12f);
				obj.exactPosition = loc;
				obj.exactPosition -= new Vector3(0.5f, 0f, 0.5f);
				obj.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value);
				obj.SetVelocity(Rand.Range(35, 45), 1.2f);
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowLightningGlow(Vector3 loc, Map map, float size)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_LightningGlow);
				obj.Scale = Rand.Range(4f, 6f) * size;
				obj.rotationRate = Rand.Range(-3f, 3f);
				obj.exactPosition = loc + size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
				obj.SetVelocity(Rand.Range(0, 360), 1.2f);
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void PlaceFootprint(Vector3 loc, Map map, float rot)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Footprint);
				obj.Scale = 0.5f;
				obj.exactRotation = rot;
				obj.exactPosition = loc;
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		public static void ThrowHorseshoe(Pawn thrower, IntVec3 targetCell)
		{
			ThrowObjectAt(thrower, targetCell, ThingDefOf.Mote_Horseshoe);
		}

		public static void ThrowStone(Pawn thrower, IntVec3 targetCell)
		{
			ThrowObjectAt(thrower, targetCell, ThingDefOf.Mote_Stone);
		}

		private static void ThrowObjectAt(Pawn thrower, IntVec3 targetCell, ThingDef mote)
		{
			if (thrower.Position.ShouldSpawnMotesAt(thrower.Map) && !thrower.Map.moteCounter.Saturated)
			{
				float num = Rand.Range(3.8f, 5.6f);
				Vector3 vector = targetCell.ToVector3Shifted() + Vector3Utility.RandomHorizontalOffset((1f - (float)thrower.skills.GetSkill(SkillDefOf.Shooting).Level / 20f) * 1.8f);
				vector.y = thrower.DrawPos.y;
				MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(mote);
				moteThrown.Scale = 1f;
				moteThrown.rotationRate = Rand.Range(-300, 300);
				moteThrown.exactPosition = thrower.DrawPos;
				moteThrown.SetVelocity((vector - moteThrown.exactPosition).AngleFlat(), num);
				moteThrown.airTimeLeft = Mathf.RoundToInt((moteThrown.exactPosition - vector).MagnitudeHorizontal() / num);
				GenSpawn.Spawn(moteThrown, thrower.Position, thrower.Map);
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

		public static Mote MakeAttachedOverlay(Thing thing, ThingDef moteDef, Vector3 offset, float scale = 1f, float solidTimeOverride = -1f)
		{
			Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
			obj.Attach(thing);
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
					MoteBubble moteBubble = thingList[j] as MoteBubble;
					if (moteBubble != null && moteBubble.link1.Linked && moteBubble.link1.Target.HasThing && moteBubble.link1.Target == pawn)
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

		public static MoteBubble MakeInteractionBubble(Pawn initiator, Pawn recipient, ThingDef interactionMote, Texture2D symbol)
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
			obj.SetupMoteBubble(symbol, recipient);
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
					ThrowDustPuff(cell, map, 1.2f);
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

		public static void MakeWaterSplash(Vector3 loc, Map map, float size, float velocity)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				MoteSplash obj = (MoteSplash)ThingMaker.MakeThing(ThingDefOf.Mote_WaterSplash);
				obj.Initialize(loc, size, velocity);
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			}
		}

		[Obsolete]
		public static void MakeBombardmentMote(IntVec3 cell, Map map)
		{
			MakeBombardmentMote_NewTmp(cell, map, 1f);
		}

		public static void MakeBombardmentMote_NewTmp(IntVec3 cell, Map map, float scale)
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
				mote.exactScale = new Vector3(x, 1f, width);
				mote.exactRotation = Mathf.Atan2(0f - vector.z, vector.x) * 57.29578f;
			}
			return mote;
		}
	}
}
