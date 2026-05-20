using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompRitualEffect_SpawnOnPawn : CompRitualEffect_IntervalSpawn
	{
		protected new CompProperties_RitualEffectSpawnOnPawn Props => (CompProperties_RitualEffectSpawnOnPawn)props;

		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return Vector3.zero;
		}

		protected abstract Pawn GetPawn(LordJob_Ritual ritual);

		public override void SpawnFleck(LordJob_Ritual ritual, Vector3? forcedPos = null, float? exactRotation = null)
		{
			if (Props.fleckDef == null)
			{
				return;
			}
			Pawn pawn = GetPawn(ritual);
			if (pawn != null && (Props.requiredTag.NullOrEmpty() || ritual.PawnTagSet(pawn, Props.requiredTag)))
			{
				Vector3 vector = props.offset.RotatedBy(pawn.Rotation);
				if (pawn.Rotation == Rot4.East)
				{
					vector += Props.eastRotationOffset;
				}
				else if (pawn.Rotation == Rot4.West)
				{
					vector += Props.westRotationOffset;
				}
				else if (pawn.Rotation == Rot4.North)
				{
					vector += Props.northRotationOffset;
				}
				else if (pawn.Rotation == Rot4.South)
				{
					vector += Props.southRotationOffset;
				}
				base.SpawnFleck(parent.ritual, pawn.Position.ToVector3Shifted() + vector, pawn.Rotation.AsAngle);
			}
			burstsDone++;
			lastSpawnTick = GenTicks.TicksGame;
		}

		public override Mote SpawnMote(LordJob_Ritual ritual, Vector3? forcedPos = null)
		{
			Mote result = null;
			if (Props.moteDef != null)
			{
				Pawn pawn = GetPawn(ritual);
				if (pawn != null && (Props.requiredTag.NullOrEmpty() || ritual.PawnTagSet(pawn, Props.requiredTag)))
				{
					Vector3 vector = props.offset.RotatedBy(pawn.Rotation);
					result = base.SpawnMote(parent.ritual, pawn.Position.ToVector3Shifted() + vector);
				}
				burstsDone++;
				lastSpawnTick = GenTicks.TicksGame;
			}
			return result;
		}

		public override Effecter SpawnEffecter(LordJob_Ritual ritual, TargetInfo target, Vector3? forcedPos = null)
		{
			Effecter result = null;
			if (Props.effecterDef != null)
			{
				Pawn pawn = GetPawn(ritual);
				if (pawn != null && (Props.requiredTag.NullOrEmpty() || ritual.PawnTagSet(pawn, Props.requiredTag)))
				{
					Vector3 value = props.offset.RotatedBy(pawn.Rotation);
					result = base.SpawnEffecter(parent.ritual, pawn, value);
				}
				burstsDone++;
				lastSpawnTick = GenTicks.TicksGame;
			}
			return result;
		}
	}
}
