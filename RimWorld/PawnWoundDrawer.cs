using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnWoundDrawer : PawnOverlayDrawer
{
	public BodyPartRecord debugDrawPart;

	public bool debugDrawAllParts;

	private const int MaxVisibleHediffsNonHuman = 5;

	private static Color OldWoundColor = new Color(0.3f, 0.3f, 0f, 1f);

	private const float WoundScale = 0.25f;

	private static Vector3 VecOneFlipped = new Vector3(-1f, 1f, 1f);

	private readonly List<BodyTypeDef.WoundAnchor> tmpAnchors = new List<BodyTypeDef.WoundAnchor>();

	public PawnWoundDrawer(Pawn pawn)
		: base(pawn)
	{
	}

	protected override void WriteCache(CacheKey key, PawnDrawParms parms, List<DrawCall> writeTarget)
	{
		Rot4 pawnRot = key.pawnRot;
		Mesh bodyMesh = key.bodyMesh;
		OverlayLayer layer = key.layer;
		DebugDraw(writeTarget, parms.matrix, bodyMesh, pawnRot, layer);
		Graphic graphic = ((layer == OverlayLayer.Body) ? pawn.Drawer.renderer.BodyGraphic : pawn.Drawer.renderer.HeadGraphic);
		List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
		for (int i = 0; i < pawn.health.hediffSet.hediffs.Count && (pawn.RaceProps.Humanlike || writeTarget.Count < 5); i++)
		{
			Hediff hediff = pawn.health.hediffSet.hediffs[i];
			if (hediff.Part == null || !hediff.Visible || !hediff.def.displayWound || (hediff is Hediff_MissingPart && !missingPartsCommonAncestors.Contains(hediff)))
			{
				continue;
			}
			if (hediff is Hediff_AddedPart && pawn.apparel != null)
			{
				bool flag = false;
				foreach (Apparel item in pawn.apparel.WornApparel)
				{
					if (item.def.apparel.blocksAddedPartWoundGraphics && item.def.apparel.CoversBodyPart(hediff.Part))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			float range = 0f;
			string text = null;
			Vector3 anchorOffset = Vector3.zero;
			if (pawn.story?.bodyType?.woundAnchors != null)
			{
				tmpAnchors.AddRange(PawnDrawUtility.FindAnchors(pawn, hediff.Part));
				if (tmpAnchors.Count > 0)
				{
					for (int num = tmpAnchors.Count - 1; num >= 0; num--)
					{
						if (tmpAnchors[num].layer != layer || !PawnDrawUtility.AnchorUsable(pawn, tmpAnchors[num], pawnRot))
						{
							tmpAnchors.RemoveAt(num);
						}
					}
					if (tmpAnchors.Count == 0)
					{
						continue;
					}
					BodyTypeDef.WoundAnchor woundAnchor = tmpAnchors.RandomElement();
					PawnDrawUtility.CalcAnchorData(pawn, woundAnchor, pawnRot, out anchorOffset, out range);
					range = hediff.def.woundAnchorRange ?? range;
					text = woundAnchor.tag;
				}
				tmpAnchors.Clear();
			}
			else
			{
				GetDefaultAnchor(bodyMesh, out anchorOffset, out range);
			}
			Rand.PushState(pawn.thingIDNumber * i * pawnRot.AsInt);
			try
			{
				FleshTypeDef.ResolvedWound resolvedWound = pawn.RaceProps.FleshType.ChooseWoundOverlay(hediff);
				if (resolvedWound == null || (!resolvedWound.wound.displayPermanent && hediff is Hediff_Injury hd && hd.IsPermanent()))
				{
					continue;
				}
				Vector3 vector = resolvedWound.wound.drawOffsetSouth;
				if (pawnRot.IsHorizontal)
				{
					vector = resolvedWound.wound.drawOffsetEastWest.ScaledBy((pawnRot == Rot4.East) ? Vector3.one : VecOneFlipped);
				}
				Vector3 vector2 = anchorOffset + vector;
				Vector4? maskTexOffset = null;
				Vector4? maskTexScale = null;
				bool flip;
				Material material = resolvedWound.GetMaterial(pawnRot, out flip);
				if (resolvedWound.wound.flipOnWoundAnchorTag != null && resolvedWound.wound.flipOnWoundAnchorTag == text && resolvedWound.wound.flipOnRotation == pawnRot)
				{
					flip = !flip;
				}
				Mesh mesh = MeshPool.GridPlane(Vector2.one * 0.25f, flip);
				if (!pawn.def.race.Humanlike)
				{
					Vector3 vector3 = Rand.InsideUnitCircleVec3 * range;
					if (flip)
					{
						vector3.x *= -1f;
					}
					vector2 += vector3;
					material = MaterialPool.MatFrom(new MaterialRequest
					{
						maskTex = (Texture2D)graphic.MatAt(pawnRot).mainTexture,
						mainTex = material.mainTexture,
						color = material.color,
						shader = material.shader
					});
					Vector3 drawPos = pawn.DrawPos;
					Vector3 vector4 = drawPos + vector2 - mesh.bounds.extents;
					Vector3 vector5 = drawPos - bodyMesh.bounds.extents;
					Vector3 size = bodyMesh.bounds.size;
					Vector3 size2 = mesh.bounds.size;
					bool flag2 = (graphic.EastFlipped && pawnRot == Rot4.East) || (graphic.WestFlipped && pawnRot == Rot4.West);
					maskTexScale = new Vector4(size2.x / size.x, size2.z / size.z);
					maskTexOffset = new Vector4((vector4.x - vector5.x) / size.x, (vector4.z - vector5.z) / size.z, flag2 ? 1 : 0);
				}
				Matrix4x4 matrix = Matrix4x4.TRS(vector2, Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up), Vector3.one * resolvedWound.wound.scale);
				Color? colorOverride = null;
				if (resolvedWound.wound.tintWithSkinColor && pawn.story != null)
				{
					colorOverride = pawn.story.SkinColor;
				}
				if (pawn.Corpse != null && pawn.Corpse.GetRotStage() == RotStage.Rotting)
				{
					colorOverride = OldWoundColor;
				}
				if (pawn.IsMutant && pawn.mutant.Def.woundColor.HasValue)
				{
					colorOverride = pawn.mutant.Def.woundColor;
				}
				writeTarget.Add(new DrawCall
				{
					overlayMat = material,
					matrix = matrix,
					overlayMesh = mesh,
					displayOverApparel = resolvedWound.wound.displayOverApparel,
					colorOverride = colorOverride,
					maskTexScale = maskTexScale,
					maskTexOffset = maskTexOffset
				});
			}
			finally
			{
				Rand.PopState();
			}
		}
	}

	private void GetDefaultAnchor(Mesh bodyMesh, out Vector3 anchorOffset, out float range)
	{
		anchorOffset = bodyMesh.bounds.center;
		range = Mathf.Min(bodyMesh.bounds.extents.x, bodyMesh.bounds.extents.z) / 2f;
	}

	private void DebugDraw(List<DrawCall> writeTarget, Matrix4x4 matrix, Mesh bodyMesh, Rot4 pawnRot, OverlayLayer layer)
	{
		if (debugDrawPart != null)
		{
			bool flag = false;
			foreach (BodyTypeDef.WoundAnchor item in PawnDrawUtility.FindAnchors(pawn, debugDrawPart))
			{
				if (PawnDrawUtility.AnchorUsable(pawn, item, pawnRot))
				{
					flag = true;
					Material overlayMat = MaterialPool.MatFrom(new MaterialRequest(BaseContent.WhiteTex, ShaderDatabase.SolidColor, item.debugColor));
					PawnDrawUtility.CalcAnchorData(pawn, item, pawnRot, out var anchorOffset, out var range);
					matrix *= Matrix4x4.Translate(anchorOffset) * Matrix4x4.Scale(Vector3.one * (range * pawn.story.bodyType.woundScale));
					if (item.layer == layer)
					{
						writeTarget.Add(new DrawCall
						{
							overlayMat = overlayMat,
							matrix = matrix,
							overlayMesh = MeshPool.circle,
							displayOverApparel = true,
							maskTexOffset = Vector4.zero,
							maskTexScale = Vector4.one
						});
					}
				}
			}
			if (!flag)
			{
				GetDefaultAnchor(bodyMesh, out var anchorOffset2, out var range2);
				matrix *= Matrix4x4.Translate(anchorOffset2) * Matrix4x4.Scale(Vector3.one * range2);
				writeTarget.Add(new DrawCall
				{
					overlayMat = BaseContent.BadMat,
					matrix = matrix,
					overlayMesh = MeshPool.circle,
					displayOverApparel = true,
					maskTexOffset = Vector4.zero,
					maskTexScale = Vector4.one
				});
			}
		}
		else
		{
			if (!debugDrawAllParts)
			{
				return;
			}
			if (pawn.story?.bodyType?.woundAnchors != null)
			{
				foreach (BodyTypeDef.WoundAnchor woundAnchor in pawn.story.bodyType.woundAnchors)
				{
					if (PawnDrawUtility.AnchorUsable(pawn, woundAnchor, pawnRot))
					{
						Material overlayMat2 = MaterialPool.MatFrom(new MaterialRequest(BaseContent.WhiteTex, ShaderDatabase.SolidColor, woundAnchor.debugColor));
						PawnDrawUtility.CalcAnchorData(pawn, woundAnchor, pawnRot, out var anchorOffset3, out var range3);
						matrix *= Matrix4x4.Translate(anchorOffset3) * Matrix4x4.Scale(Vector3.one * range3);
						if (woundAnchor.layer == layer)
						{
							writeTarget.Add(new DrawCall
							{
								overlayMat = overlayMat2,
								matrix = matrix,
								overlayMesh = MeshPool.circle,
								displayOverApparel = true,
								maskTexOffset = Vector4.zero,
								maskTexScale = Vector4.one
							});
						}
					}
				}
				return;
			}
			GetDefaultAnchor(bodyMesh, out var anchorOffset4, out var range4);
			matrix *= Matrix4x4.Translate(anchorOffset4) * Matrix4x4.Scale(Vector3.one * range4);
			writeTarget.Add(new DrawCall
			{
				overlayMat = BaseContent.BadMat,
				matrix = matrix,
				overlayMesh = MeshPool.circle,
				displayOverApparel = true,
				maskTexOffset = Vector4.zero,
				maskTexScale = Vector4.one
			});
		}
	}
}
