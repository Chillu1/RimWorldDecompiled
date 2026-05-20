using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Pawn_RopeTracker : IExposable
	{
		public const float RopeLength = 8f;

		private static readonly Vector3 CaravanHitchingPostOffset = new Vector3(0f, 0f, 0.19f);

		private static readonly string RopeTexPath = "UI/Overlays/Rope";

		private static readonly Material RopeLineMat = MaterialPool.MatFrom(RopeTexPath, ShaderDatabase.Transparent, GenColor.FromBytes(99, 70, 41));

		private Pawn pawn;

		private LocalTargetInfo ropedTo = LocalTargetInfo.Invalid;

		private List<Pawn> ropees = new List<Pawn>();

		private Building hitchingSpotInt;

		public bool IsRoped => ropedTo.IsValid;

		public bool IsRopedByPawn => RopedByPawn != null;

		public bool IsRopedToSpot
		{
			get
			{
				if (ropedTo.IsValid)
				{
					return !IsRopedByPawn;
				}
				return false;
			}
		}

		public bool IsRopedToHitchingPost => RopedToHitchingSpot != null;

		public bool IsRopingOthers => Ropees.Any();

		public bool HasAnyRope
		{
			get
			{
				if (!IsRoped)
				{
					return IsRopingOthers;
				}
				return true;
			}
		}

		public Pawn RopedByPawn => ropedTo.Thing as Pawn;

		public IntVec3 RopedToSpot
		{
			get
			{
				if (!IsRopedToSpot)
				{
					return IntVec3.Invalid;
				}
				return ropedTo.Cell;
			}
		}

		public LocalTargetInfo RopedTo => ropedTo;

		public List<Pawn> Ropees => ropees;

		public Building RopedToHitchingSpot => hitchingSpotInt;

		public bool AnyRopeesFenceBlocked
		{
			get
			{
				if (ropees.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < ropees.Count; i++)
				{
					if (ropees[i].FenceBlocked)
					{
						return true;
					}
				}
				return false;
			}
		}

		public string InspectLine
		{
			get
			{
				if (IsRopedByPawn)
				{
					return "RopedByPawn".Translate() + ": " + RopedByPawn.LabelShort;
				}
				if (ropedTo.HasThing)
				{
					return "RopedToThing".Translate() + ": " + ropedTo.Label;
				}
				return "RopedToSpot".Translate();
			}
		}

		public Pawn_RopeTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void RopingTick()
		{
			if (!HasAnyRope)
			{
				return;
			}
			if (pawn.Dead || pawn.Downed || pawn.Drafted || (!pawn.Awake() && IsRopedByPawn) || ShouldDropRopesDueToMentalState() || pawn.IsBurning())
			{
				BreakAllRopes();
				return;
			}
			if (ropees.Any() && !IsStillDoingRopingJob(pawn))
			{
				BreakAllRopes();
				return;
			}
			if (ropedTo.IsValid && !pawn.CanReach(ropedTo, PathEndMode.Touch, Danger.Deadly))
			{
				BreakRopeWithRoper();
			}
			if (IsRopedToHitchingPost && !RopedToHitchingSpot.Spawned)
			{
				UnropeFromSpot();
			}
		}

		private bool IsStillDoingRopingJob(Pawn roper)
		{
			if (roper.jobs.curJob != null)
			{
				return roper.jobs.curDriver is JobDriver_RopeToDestination;
			}
			return true;
		}

		private bool ShouldDropRopesDueToMentalState()
		{
			if (pawn.InMentalState)
			{
				return pawn.MentalStateDef != MentalStateDefOf.Roaming;
			}
			return false;
		}

		public void RopingDraw()
		{
			if (ropedTo.IsValid)
			{
				Vector3 b = ropedTo.CenterVector3.Yto0();
				if (!ropedTo.HasThing && ropedTo.Cell.GetFirstThing(pawn.Map, ThingDefOf.CaravanPackingSpot) != null)
				{
					b += CaravanHitchingPostOffset;
				}
				GenDraw.DrawLineBetween(pawn.DrawPos.Yto0(), b, AltitudeLayer.PawnRope.AltitudeFor(), RopeLineMat);
			}
		}

		public void Notify_DeSpawned()
		{
			BreakAllRopes();
		}

		private void BreakRopeWithRoper()
		{
			RopedByPawn?.roping.DropRope(pawn);
			ropedTo = LocalTargetInfo.Invalid;
		}

		public void BreakAllRopes()
		{
			BreakRopeWithRoper();
			DropRopes();
			TryUnropeFromHitchingSpot();
		}

		public void RopeToSpot(IntVec3 spot)
		{
			Building edifice = spot.GetEdifice(pawn.Map);
			CreateRope(spot, pawn);
			if (edifice != null && edifice.def == ThingDefOf.CaravanPackingSpot)
			{
				hitchingSpotInt = edifice;
				RopedToHitchingSpot.GetComp<CompHitchingSpot>().AddPawn(pawn);
			}
		}

		public void TryUnropeFromHitchingSpot()
		{
			if (RopedToHitchingSpot != null)
			{
				RopedToHitchingSpot.GetComp<CompHitchingSpot>().RemovePawn(pawn);
			}
			hitchingSpotInt = null;
		}

		public void RopePawn(Pawn ropee)
		{
			CreateRope(pawn, ropee);
		}

		private static void CreateRope(LocalTargetInfo roperTarget, Pawn ropee)
		{
			Pawn pawn = roperTarget.Thing as Pawn;
			ropee.roping.DropRopes();
			ropee.roping.TryUnropeFromHitchingSpot();
			ropee.roping.ropedTo = roperTarget;
			if (pawn != null)
			{
				pawn.roping.ropees.Add(ropee);
				ReachabilityUtility.ClearCacheFor(pawn);
			}
			ReachabilityUtility.ClearCacheFor(ropee);
			if (ropee.jobs != null && ropee.CurJob != null)
			{
				ropee.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public void UnropeFromSpot()
		{
			if (!IsRopedToSpot)
			{
				Log.Warning($"Tried to unrope {pawn} from spot, but not roped to a spot");
				return;
			}
			TryUnropeFromHitchingSpot();
			ropedTo = LocalTargetInfo.Invalid;
			ReachabilityUtility.ClearCacheFor(pawn);
		}

		public void DropRope(Pawn ropee)
		{
			if (pawn != ropee.roping.RopedByPawn)
			{
				Log.Warning($"{pawn} tried to drop for {ropee} but ropee holder was {ropee.roping.RopedByPawn}");
			}
			TryUnropeFromHitchingSpot();
			ropee.roping.ropedTo = LocalTargetInfo.Invalid;
			ropees.Remove(ropee);
			ReachabilityUtility.ClearCacheFor(ropee);
			ReachabilityUtility.ClearCacheFor(pawn);
		}

		public void DropRopes()
		{
			if (ropees.Count == 0)
			{
				return;
			}
			foreach (Pawn item in new List<Pawn>(ropees))
			{
				DropRope(item);
			}
		}

		public void ExposeData()
		{
			Scribe_TargetInfo.Look(ref ropedTo, "ropedTo", LocalTargetInfo.Invalid);
			Scribe_References.Look(ref hitchingSpotInt, "hitchingPostInt");
			Scribe_Collections.Look(ref ropees, "ropees", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				ropees.RemoveAll((Pawn x) => x.DestroyedOrNull());
			}
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"{pawn}");
			if (ropedTo.HasThing)
			{
				stringBuilder.Append($" ropedBy: {ropedTo.Thing}");
			}
			else if (ropedTo.IsValid)
			{
				stringBuilder.Append($" roped at: {ropedTo.Cell}");
			}
			stringBuilder.AppendLine();
			foreach (Pawn ropee in Ropees)
			{
				stringBuilder.AppendLine($"  ropee: {ropee}");
			}
			return stringBuilder.ToString();
		}
	}
}
