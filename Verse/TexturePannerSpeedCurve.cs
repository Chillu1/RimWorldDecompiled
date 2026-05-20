using LudeonTK;
using UnityEngine;

namespace Verse;

public class TexturePannerSpeedCurve : TexturePanner
{
	public ComplexCurve timescaleCurve;

	public TexturePannerSpeedCurve(Material material, Vector2 direction, ComplexCurve timescaleCurve, float speed)
		: base(material, Shader.PropertyToID("_MainTex"), direction, speed)
	{
		this.timescaleCurve = timescaleCurve;
	}

	public TexturePannerSpeedCurve(Material material, string property, ComplexCurve timescaleCurve, Vector2 direction, float speed)
		: base(material, Shader.PropertyToID(property), direction, speed)
	{
		this.timescaleCurve = timescaleCurve;
	}

	public TexturePannerSpeedCurve(Material material, int propertyID, ComplexCurve timescaleCurve, Vector2 direction, float speed)
		: base(material, propertyID, direction, speed)
	{
		this.timescaleCurve = timescaleCurve;
	}

	public override void Tick()
	{
		pan -= direction * speed * material.GetTextureScale(propertyID).x * timescaleCurve.Evaluate(Find.TickManager.TickRateMultiplier);
		material.SetTextureOffset(propertyID, pan);
	}
}
