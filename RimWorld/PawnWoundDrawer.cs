using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnWoundDrawer
	{
		private class Wound
		{
			private List<Vector2> locsPerSide = new List<Vector2>();

			private Material mat;

			private Quaternion quat;

			private static readonly Vector2 WoundSpan = new Vector2(0.18f, 0.3f);

			public Wound(Pawn pawn)
			{
				mat = pawn.RaceProps.FleshType.ChooseWoundOverlay();
				if (mat == null)
				{
					Log.ErrorOnce($"No wound graphics data available for flesh type {pawn.RaceProps.FleshType}", 76591733);
					mat = FleshTypeDefOf.Normal.ChooseWoundOverlay();
				}
				quat = Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up);
				for (int i = 0; i < 4; i++)
				{
					locsPerSide.Add(new Vector2(Rand.Value, Rand.Value));
				}
			}

			public void DrawWound(Vector3 drawLoc, Quaternion bodyQuat, Rot4 bodyRot, bool forPortrait)
			{
				Vector2 vector = locsPerSide[bodyRot.AsInt];
				drawLoc += new Vector3((vector.x - 0.5f) * WoundSpan.x, 0f, (vector.y - 0.5f) * WoundSpan.y);
				drawLoc.z -= 0.3f;
				GenDraw.DrawMeshNowOrLater(MeshPool.plane025, drawLoc, quat, mat, forPortrait);
			}
		}

		protected Pawn pawn;

		private List<Wound> wounds = new List<Wound>();

		private int MaxDisplayWounds = 3;

		public PawnWoundDrawer(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void RenderOverBody(Vector3 drawLoc, Mesh bodyMesh, Quaternion quat, bool forPortrait)
		{
			int num = 0;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.displayWound)
				{
					Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
					if (hediff_Injury == null || !hediff_Injury.IsPermanent())
					{
						num++;
					}
				}
			}
			int num2 = Mathf.CeilToInt((float)num / 2f);
			if (num2 > MaxDisplayWounds)
			{
				num2 = MaxDisplayWounds;
			}
			while (wounds.Count < num2)
			{
				wounds.Add(new Wound(pawn));
				PortraitsCache.SetDirty(pawn);
			}
			while (wounds.Count > num2)
			{
				wounds.Remove(wounds.RandomElement());
				PortraitsCache.SetDirty(pawn);
			}
			for (int j = 0; j < wounds.Count; j++)
			{
				wounds[j].DrawWound(drawLoc, quat, pawn.Rotation, forPortrait);
			}
		}
	}
}
