using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public static class PawnRenderUtility
{
	private static readonly Color BaseRottenColor = new Color(0.29f, 0.25f, 0.22f);

	public static readonly Color DessicatedColorInsect = new Color(0.8f, 0.8f, 0.8f);

	private static readonly Vector3 BaseCarriedOffset = new Vector3(0f, 0f, -0.1f);

	private static readonly Vector3 EqLocNorth = new Vector3(0f, 0f, -0.11f);

	private static readonly Vector3 EqLocEast = new Vector3(0.22f, 0f, -0.22f);

	private static readonly Vector3 EqLocSouth = new Vector3(0f, 0f, -0.22f);

	private static readonly Vector3 EqLocWest = new Vector3(-0.22f, 0f, -0.22f);

	public const float Layer_Carried = 90f;

	public const float Layer_Carried_Behind = -10f;

	public static readonly List<PawnRenderNodeProperties> EmptyRenderNodeProperties = new List<PawnRenderNodeProperties>();

	private static readonly Dictionary<Material, Material> overlayMats = new Dictionary<Material, Material>();

	public static void ResetStaticData()
	{
		overlayMats.Clear();
	}

	public static bool RenderAsPack(this Apparel apparel)
	{
		if (apparel.def.apparel.LastLayer.IsUtilityLayer)
		{
			if (apparel.def.apparel.wornGraphicData != null)
			{
				return apparel.def.apparel.wornGraphicData.renderUtilityAsPack;
			}
			return true;
		}
		return false;
	}

	public static Color GetRottenColor(Color color)
	{
		return new Color(color.r * 0.25f + BaseRottenColor.r * 0.75f, color.g * 0.25f + BaseRottenColor.g * 0.75f, color.b * 0.25f + BaseRottenColor.b * 0.75f, color.a);
	}

	public static void DrawCarriedWeapon(ThingWithComps weapon, Vector3 drawPos, Rot4 facing, float equipmentDrawDistanceFactor)
	{
		int num = 143;
		switch (facing.AsInt)
		{
		case 0:
			drawPos += EqLocNorth * equipmentDrawDistanceFactor;
			break;
		case 1:
			drawPos += EqLocEast * equipmentDrawDistanceFactor;
			break;
		case 2:
			drawPos += EqLocSouth * equipmentDrawDistanceFactor;
			break;
		case 3:
			drawPos += EqLocWest * equipmentDrawDistanceFactor;
			num = 217;
			break;
		}
		DrawEquipmentAiming(weapon, drawPos, num);
	}

	public static void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
	{
		float num = aimAngle - 90f;
		Mesh mesh;
		if (aimAngle > 20f && aimAngle < 160f)
		{
			mesh = MeshPool.plane10;
			num += eq.def.equippedAngleOffset;
		}
		else if (aimAngle > 200f && aimAngle < 340f)
		{
			mesh = MeshPool.plane10Flip;
			num -= 180f;
			num -= eq.def.equippedAngleOffset;
		}
		else
		{
			mesh = MeshPool.plane10;
			num += eq.def.equippedAngleOffset;
		}
		num %= 360f;
		CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
		if (compEquippable != null)
		{
			EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
			drawLoc += drawOffset;
			num += angleOffset;
		}
		Material material = ((!(eq.Graphic is Graphic_StackCount graphic_StackCount)) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq));
		Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(num, Vector3.up));
		Graphics.DrawMesh(mesh, matrix, material, 0);
	}

	public static float CrawlingHeadAngle(Rot4 rotation)
	{
		switch (rotation.AsInt)
		{
		case 0:
			return 0f;
		case 1:
			return -50f;
		case 2:
			return 0f;
		case 3:
			return 50f;
		default:
		{
			Rot4 rot = rotation;
			Log.Warning("Invalid rotation: " + rot.ToString());
			return 0f;
		}
		}
	}

	public static float CrawlingBodyAngle(Rot4 rot)
	{
		switch (rot.AsInt)
		{
		case 0:
			return 0f;
		case 1:
			return 75f;
		case 2:
			return 180f;
		case 3:
			return 285f;
		default:
		{
			Rot4 rot2 = rot;
			Log.Warning("Invalid rotation: " + rot2.ToString());
			return 0f;
		}
		}
	}

	public static void DrawCarriedThing(Pawn pawn, Vector3 drawLoc, Thing carriedThing)
	{
		CalculateCarriedDrawPos(pawn, carriedThing, ref drawLoc, out var flip);
		carriedThing.DrawNowAt(drawLoc, flip);
	}

	public static void CalculateCarriedDrawPos(Pawn pawn, Thing carriedThing, ref Vector3 carryDrawPos, out bool flip)
	{
		flip = false;
		if (pawn.CurJob != null && pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref carryDrawPos, ref flip))
		{
			return;
		}
		CompPushable compPushable = carriedThing.TryGetComp<CompPushable>();
		if (compPushable != null)
		{
			Vector2 vector = Vector2Utility.RotatedBy(degrees: (!pawn.pather.MovingNow) ? (pawn.Rotation.AsAngle - 90f) : (pawn.pather.nextCell - pawn.Position).ToVector3().ToAngleFlat(), v: new Vector2(compPushable.Props.offsetDistance, 0f));
			Vector3 target = new Vector3(vector.x, 0f, 0f - vector.y);
			float deltaTime = Find.TickManager.TickRateMultiplier / 60f;
			if (!Find.TickManager.Paused)
			{
				compPushable.drawPos = Vector3.SmoothDamp(compPushable.drawPos, target, ref compPushable.drawVel, compPushable.Props.smoothTime, 10f, deltaTime);
			}
			carryDrawPos += compPushable.drawPos;
			if (compPushable.drawPos.y < 0f)
			{
				carryDrawPos.y = pawn.DrawPos.y + AltitudeForLayer(-10f);
			}
			return;
		}
		Rot4 rotation = pawn.Rotation;
		Vector3 baseCarriedOffset = BaseCarriedOffset;
		switch (rotation.AsInt)
		{
		case 0:
			baseCarriedOffset.z = 0f;
			break;
		case 1:
			baseCarriedOffset.x = 0.18f;
			break;
		case 3:
			baseCarriedOffset.x = -0.18f;
			break;
		}
		carryDrawPos += baseCarriedOffset;
		if (pawn.DevelopmentalStage == DevelopmentalStage.Child)
		{
			carryDrawPos.z -= 0.1f;
		}
		if (pawn.Rotation == Rot4.North)
		{
			carryDrawPos.y -= 0.03658537f;
		}
		else
		{
			carryDrawPos.y += 0.03658537f;
		}
	}

	public static void DrawEquipmentAndApparelExtras(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
	{
		if (pawn.equipment?.Primary != null)
		{
			Job curJob = pawn.CurJob;
			if (curJob != null && curJob.def?.neverShowWeapon == false)
			{
				Stance_Busy stance_Busy = pawn.stances?.curStance as Stance_Busy;
				float equipmentDrawDistanceFactor = pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
				float num = 0f;
				if (!flags.HasFlag(PawnRenderFlags.NeverAimWeapon) && stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
				{
					Vector3 vector = (stance_Busy.focusTarg.HasThing ? stance_Busy.focusTarg.Thing.DrawPos : stance_Busy.focusTarg.Cell.ToVector3Shifted());
					if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
					{
						num = (vector - pawn.DrawPos).AngleFlat();
					}
					Verb currentEffectiveVerb = pawn.CurrentEffectiveVerb;
					if (currentEffectiveVerb != null && currentEffectiveVerb.AimAngleOverride.HasValue)
					{
						num = currentEffectiveVerb.AimAngleOverride.Value;
					}
					drawPos += new Vector3(0f, 0f, 0.4f + pawn.equipment.Primary.def.equippedDistanceOffset).RotatedBy(num) * equipmentDrawDistanceFactor;
					DrawEquipmentAiming(pawn.equipment.Primary, drawPos, num);
				}
				else if (CarryWeaponOpenly(pawn))
				{
					DrawCarriedWeapon(pawn.equipment.Primary, drawPos, facing, equipmentDrawDistanceFactor);
				}
			}
		}
		if (pawn.apparel == null)
		{
			return;
		}
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			item.DrawWornExtras();
		}
	}

	public static float AltitudeForLayer(float layer)
	{
		return Mathf.Clamp(layer, -10f, 100f) * 0.0003658537f;
	}

	public static bool CarryWeaponOpenly(Pawn pawn)
	{
		if (pawn.carryTracker?.CarriedThing != null)
		{
			return false;
		}
		if (pawn.Drafted)
		{
			return true;
		}
		if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
		{
			return true;
		}
		if (pawn.mindState?.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)
		{
			return true;
		}
		Lord lord = pawn.GetLord();
		if (lord?.LordJob != null && lord.LordJob.AlwaysShowWeapon)
		{
			return true;
		}
		if (pawn.CompsWantHoldWeapon())
		{
			return true;
		}
		return false;
	}

	public static void SetMatPropBlockOverlay(MaterialPropertyBlock block, Color color, float opacity = 0.5f)
	{
		block.SetColor(ShaderPropertyIDs.OverlayColor, color);
		block.SetFloat(ShaderPropertyIDs.OverlayOpacity, opacity);
	}
}
