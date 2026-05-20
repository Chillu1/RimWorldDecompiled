using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompPowerPlantSolar : CompPowerPlant
{
	private const float NightPower = 0f;

	private static readonly Vector2 BarSize = new Vector2(2.3f, 0.14f);

	private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

	private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

	protected override float DesiredPowerOutput => Mathf.Lerp(0f, 0f - base.Props.PowerConsumption, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;

	private float RoofedPowerOutputFactor
	{
		get
		{
			int num = 0;
			int num2 = 0;
			foreach (IntVec3 item in parent.OccupiedRect())
			{
				num++;
				if (parent.Map.roofGrid.Roofed(item))
				{
					num2++;
				}
			}
			return (float)(num - num2) / (float)num;
		}
	}

	public override void PostDraw()
	{
		base.PostDraw();
		GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest
		{
			center = parent.DrawPos + Vector3.up * 0.1f,
			size = BarSize,
			fillPercent = base.PowerOutput / (0f - base.Props.PowerConsumption),
			filledMat = PowerPlantSolarBarFilledMat,
			unfilledMat = PowerPlantSolarBarUnfilledMat,
			margin = 0.15f
		};
		Rot4 rotation = parent.Rotation;
		rotation.Rotate(RotationDirection.Clockwise);
		r.rotation = rotation;
		GenDraw.DrawFillableBar(r);
	}
}
