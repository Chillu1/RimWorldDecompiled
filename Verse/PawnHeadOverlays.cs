using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class PawnHeadOverlays
	{
		private Pawn pawn;

		private const float AngerBlinkPeriod = 1.2f;

		private const float AngerBlinkLength = 0.4f;

		private static readonly Material UnhappyMat = MaterialPool.MatFrom("Things/Pawn/Effects/Unhappy");

		private static readonly Material MentalStateImminentMat = MaterialPool.MatFrom("Things/Pawn/Effects/MentalStateImminent");

		public PawnHeadOverlays(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void RenderStatusOverlays(Vector3 bodyLoc, Quaternion quat, Mesh headMesh)
		{
			if (!pawn.IsColonistPlayerControlled)
			{
				return;
			}
			Vector3 headLoc = bodyLoc + new Vector3(0f, 0f, 0.32f);
			if (pawn.needs.mood == null || pawn.Downed || pawn.HitPoints <= 0)
			{
				return;
			}
			if (pawn.mindState.mentalBreaker.BreakExtremeIsImminent)
			{
				if (Time.time % 1.2f < 0.4f)
				{
					DrawHeadGlow(headLoc, MentalStateImminentMat);
				}
			}
			else if (pawn.mindState.mentalBreaker.BreakExtremeIsApproaching && Time.time % 1.2f < 0.4f)
			{
				DrawHeadGlow(headLoc, UnhappyMat);
			}
		}

		private void DrawHeadGlow(Vector3 headLoc, Material mat)
		{
			Graphics.DrawMesh(MeshPool.plane20, headLoc, Quaternion.identity, mat, 0);
		}
	}
}
