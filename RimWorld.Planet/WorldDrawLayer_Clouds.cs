using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_Clouds : WorldDrawLayer
{
	private Material material;

	private Texture2D noiseTexture;

	private Texture2D sunsetColorRamp;

	private Texture2D cloudTexture;

	private float opacity = 1f;

	private float opacityVel;

	private bool set;

	private const int SubdivisionsCount = 4;

	private const float OpacityChangeTime = 0.15f;

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		if (material == null)
		{
			material = new Material(WorldMaterials.Clouds);
			material.SetVector(ShaderPropertyIDs.Seed, new Vector2(Random.value, Random.value));
			material.SetFloat(ShaderPropertyIDs.PlanetRadius, planetLayer.Radius);
			material.SetVector(ShaderPropertyIDs.PlanetOrigin, planetLayer.Origin);
			noiseTexture = ContentFinder<Texture2D>.Get("Other/Perlin");
			sunsetColorRamp = ContentFinder<Texture2D>.Get("World/SunsetGradient");
			cloudTexture = ContentFinder<Texture2D>.Get("World/CloudMap");
			material.SetTexture(ShaderPropertyIDs.NoiseTex, noiseTexture);
			material.SetTexture(ShaderPropertyIDs.SunsetColorRamp, sunsetColorRamp);
			material.SetTexture(ShaderPropertyIDs.CloudMap, cloudTexture);
			set = false;
		}
		SphereGenerator.Generate(4, planetLayer.Radius + 0.2f, Vector3.forward, 180f, out var outVerts, out var outIndices);
		LayerSubMesh subMesh = GetSubMesh(material);
		subMesh.verts.AddRange(outVerts);
		subMesh.tris.AddRange(outIndices);
		FinalizeMesh(MeshParts.All);
	}

	private float GetTargetOpacity()
	{
		if (PlanetLayer.Selected == planetLayer && !WorldRendererUtility.WorldBackgroundNow)
		{
			return 0f;
		}
		return 1f;
	}

	public override void Render()
	{
		opacity = ((!set) ? GetTargetOpacity() : Mathf.SmoothDamp(opacity, GetTargetOpacity(), ref opacityVel, 0.15f));
		material.SetFloat(ShaderPropertyIDs.CloudShaderOpacity, opacity);
		set = true;
		base.Render();
	}
}
