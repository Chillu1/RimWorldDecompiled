using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompHitchingSpot : ThingComp
	{
		private static readonly Vector3 HitchAdustment = new Vector3(0f, 0f, 0.15f);

		private List<Pawn> ropedPawns = new List<Pawn>();

		private Graphic hitchGraphic;

		private Graphic HitchGraphic
		{
			get
			{
				if (hitchGraphic == null)
				{
					hitchGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/CaravanHitchingPin", ShaderDatabase.Cutout, Vector2.one, Color.white);
				}
				return hitchGraphic;
			}
		}

		public void AddPawn(Pawn ropedPawn)
		{
			ropedPawns.Add(ropedPawn);
		}

		public void RemovePawn(Pawn unropedPawn)
		{
			ropedPawns.Remove(unropedPawn);
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (ropedPawns.Count > 0)
			{
				HitchGraphic.DrawFromDef(parent.DrawPos + HitchAdustment + Altitudes.AltIncVect * 0.25f, Rot4.North, null);
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Faction == Faction.OfPlayer)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandFormCaravan".Translate();
				command_Action.defaultDesc = "CommandFormCaravanDesc".Translate() + "\n\n" + "CommandFormCaravanDescAtSpot".Translate();
				command_Action.icon = FormCaravanComp.FormCaravanCommand;
				command_Action.action = delegate
				{
					Find.WindowStack.Add(new Dialog_FormCaravan(parent.Map, reform: false, null, mapAboutToBeRemoved: false, parent.Position));
				};
				yield return command_Action;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref ropedPawns, "ropedPawns", LookMode.Reference);
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			foreach (Pawn ropedPawn in ropedPawns)
			{
				if (ropedPawn == null || ropedPawn.roping == null || ropedPawn.roping.RopedToHitchingSpot != parent)
				{
					RemovePawn(ropedPawn);
				}
			}
		}
	}
}
