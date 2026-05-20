using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class WeatherOverlayDualPanner : SkyOverlay
{
	private Vector2 worldPan1 = Vector2.zero;

	private Vector2 worldPan2 = Vector2.zero;

	public Material worldOverlayMat;

	public Material screenOverlayMat;

	protected float worldOverlayPanSpeed1;

	protected float worldOverlayPanSpeed2;

	protected Vector2 worldPanDir1;

	protected Vector2 worldPanDir2;

	private static readonly int RenderLayer = LayerMask.NameToLayer("GravshipExclude");

	private static readonly int MainTex2 = Shader.PropertyToID("_MainTex2");

	private static readonly int MainTex = Shader.PropertyToID("_MainTex");

	public WeatherOverlayDualPanner()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			SetOverlayColor(Color.clear);
		});
	}

	public override void TickOverlay(Map map, float lerpFactor)
	{
		if (!(worldOverlayMat == null))
		{
			worldPan1 -= worldPanDir1 * (worldOverlayPanSpeed1 * worldOverlayMat.GetTextureScale(MainTex).x * Find.TickManager.TickRateMultiplier);
			worldOverlayMat.SetTextureOffset(MainTex, worldPan1);
			if (worldOverlayMat.HasProperty(MainTex2))
			{
				worldPan2 -= worldPanDir2 * (worldOverlayPanSpeed2 * worldOverlayMat.GetTextureScale(MainTex2).x * Find.TickManager.TickRateMultiplier);
				worldOverlayMat.SetTextureOffset(MainTex2, worldPan2);
			}
		}
	}

	public override void DrawOverlay(Map map)
	{
		if (worldOverlayMat != null)
		{
			SkyOverlay.DrawWorldOverlay(map, worldOverlayMat, GetRenderLayer());
		}
		if (screenOverlayMat != null)
		{
			SkyOverlay.DrawScreenOverlay(screenOverlayMat, GetRenderLayer());
		}
	}

	public override void SetOverlayColor(Color color)
	{
		if (worldOverlayMat != null)
		{
			worldOverlayMat.color = color;
		}
		if (screenOverlayMat != null)
		{
			screenOverlayMat.color = color;
		}
	}

	protected virtual int GetRenderLayer()
	{
		return RenderLayer;
	}

	public override void Reset()
	{
		worldPan1 = Vector2.zero;
		worldPan2 = Vector2.zero;
		if (worldOverlayMat != null)
		{
			worldOverlayMat.SetTextureOffset(MainTex, worldPan1);
			if (worldOverlayMat.HasProperty(MainTex2))
			{
				worldOverlayMat.SetTextureOffset(MainTex2, worldPan2);
			}
		}
	}

	public override string ToString()
	{
		if (worldOverlayMat != null)
		{
			return worldOverlayMat.name;
		}
		if (screenOverlayMat != null)
		{
			return screenOverlayMat.name;
		}
		return base.ToString();
	}
}
