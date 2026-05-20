using UnityEngine;

namespace Verse;

public class Pawn_RotationTracker : IExposable
{
	private Pawn pawn;

	public Pawn_RotationTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void Notify_Spawned()
	{
		UpdateRotation();
	}

	public void UpdateRotation()
	{
		if (pawn.Destroyed)
		{
			return;
		}
		if (pawn.kindDef.useFixedRotation)
		{
			pawn.Rotation = pawn.kindDef.fixedRotation;
		}
		else
		{
			if (pawn.jobs.HandlingFacing || (pawn.stances.stunner.Stunned && pawn.stances.stunner.DisableRotation))
			{
				return;
			}
			if (pawn.stances.curStance is Stance_Busy stance_Busy && stance_Busy.focusTarg.IsValid)
			{
				if (stance_Busy.focusTarg.HasThing)
				{
					Face(stance_Busy.focusTarg.Thing.DrawPos);
				}
				else
				{
					FaceCell(stance_Busy.focusTarg.Cell);
				}
				return;
			}
			if (pawn.pather.Moving)
			{
				if (pawn.pather.curPath != null && pawn.pather.curPath.NodesLeftCount >= 1)
				{
					FaceAdjacentCell(pawn.pather.nextCell);
				}
				return;
			}
			if (pawn.jobs.curJob != null)
			{
				LocalTargetInfo target = pawn.CurJob.GetTarget(pawn.jobs.curDriver.rotateToFace);
				FaceTarget(target);
			}
			if (pawn.Drafted)
			{
				pawn.Rotation = Rot4.South;
			}
		}
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		UpdateRotation();
	}

	private void FaceAdjacentCell(IntVec3 c)
	{
		if (!(c == pawn.Position))
		{
			IntVec3 intVec = c - pawn.Position;
			if (intVec.x > 0)
			{
				pawn.Rotation = Rot4.East;
			}
			else if (intVec.x < 0)
			{
				pawn.Rotation = Rot4.West;
			}
			else if (intVec.z > 0)
			{
				pawn.Rotation = Rot4.North;
			}
			else
			{
				pawn.Rotation = Rot4.South;
			}
		}
	}

	public void FaceCell(IntVec3 c)
	{
		if (!(c == pawn.Position))
		{
			float angle = (c - pawn.Position).ToVector3().AngleFlat();
			pawn.Rotation = RotFromAngleBiased(angle);
		}
	}

	public void Face(Vector3 p)
	{
		if (!(p == pawn.DrawPos))
		{
			float angle = (p - pawn.DrawPos).AngleFlat();
			pawn.Rotation = RotFromAngleBiased(angle);
		}
	}

	public void FaceTarget(LocalTargetInfo target)
	{
		if (!target.IsValid)
		{
			return;
		}
		if (target.HasThing)
		{
			Thing thing = (target.Thing.Spawned ? target.Thing : ThingOwnerUtility.GetFirstSpawnedParentThing(target.Thing));
			if (thing == null || !thing.Spawned)
			{
				return;
			}
			bool flag = false;
			IntVec3 c = default(IntVec3);
			CellRect cellRect = thing.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					if (pawn.Position == new IntVec3(j, 0, i))
					{
						Face(thing.DrawPos);
						return;
					}
				}
			}
			for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
			{
				for (int l = cellRect.minX; l <= cellRect.maxX; l++)
				{
					IntVec3 intVec = new IntVec3(l, 0, k);
					if (intVec.AdjacentToCardinal(pawn.Position))
					{
						FaceAdjacentCell(intVec);
						return;
					}
					if (intVec.AdjacentTo8Way(pawn.Position))
					{
						flag = true;
						c = intVec;
					}
				}
			}
			if (flag)
			{
				if (DebugViewSettings.drawPawnRotatorTarget)
				{
					pawn.Map.debugDrawer.FlashCell(pawn.Position, 0.6f, "jbthing");
					GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), c.ToVector3Shifted());
				}
				FaceAdjacentCell(c);
			}
			else
			{
				Face(thing.DrawPos);
			}
		}
		else if (pawn.Position.AdjacentTo8Way(target.Cell))
		{
			if (DebugViewSettings.drawPawnRotatorTarget)
			{
				pawn.Map.debugDrawer.FlashCell(pawn.Position, 0.2f, "jbloc");
				GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), target.Cell.ToVector3Shifted());
			}
			FaceAdjacentCell(target.Cell);
		}
		else if (target.Cell.IsValid && target.Cell != pawn.Position)
		{
			Face(target.Cell.ToVector3());
		}
	}

	public static Rot4 RotFromAngleBiased(float angle)
	{
		if (angle < 30f)
		{
			return Rot4.North;
		}
		if (angle < 150f)
		{
			return Rot4.East;
		}
		if (angle < 210f)
		{
			return Rot4.South;
		}
		if (angle < 330f)
		{
			return Rot4.West;
		}
		return Rot4.North;
	}

	public void ExposeData()
	{
	}
}
