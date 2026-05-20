using UnityEngine;

namespace Verse;

public class TexturePanner
{
	protected Material material;

	protected int propertyID;

	protected Vector2 pan;

	public Vector2 direction;

	public float speed;

	public TexturePanner(Material material, Vector2 direction, float speed)
		: this(material, Shader.PropertyToID("_MainTex"), direction, speed)
	{
	}

	public TexturePanner(Material material, string property, Vector2 direction, float speed)
		: this(material, Shader.PropertyToID(property), direction, speed)
	{
	}

	public TexturePanner(Material material, int propertyID, Vector2 direction, float speed)
	{
		this.material = material;
		this.propertyID = propertyID;
		this.direction = direction;
		this.direction.Normalize();
		this.speed = speed;
	}

	public virtual void Tick()
	{
		pan -= direction * speed * material.GetTextureScale(propertyID).x * Find.TickManager.TickRateMultiplier;
		material.SetTextureOffset(propertyID, pan);
	}
}
