using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;

namespace Verse.Noise;

public static class NoiseDebugUI
{
	private class Noise2D
	{
		public string name;

		private Texture2D tex;

		private ModuleBase noise;

		public Texture2D Texture
		{
			get
			{
				if (tex == null)
				{
					tex = NoiseRenderer.NoiseRendered(noise);
				}
				return tex;
			}
		}

		public Noise2D(Texture2D tex, string name)
		{
			this.tex = tex;
			this.name = name;
		}

		public Noise2D(ModuleBase noise, string name)
		{
			this.noise = noise;
			this.name = name;
		}
	}

	private class NoisePlanet
	{
		public string name;

		public ModuleBase noise;

		public NoisePlanet(ModuleBase noise, string name)
		{
			this.name = name;
			this.noise = noise;
		}
	}

	private static List<Noise2D> noises2D = new List<Noise2D>();

	private static List<NoisePlanet> planetNoises = new List<NoisePlanet>();

	private static Mesh planetNoiseMesh;

	private static NoisePlanet currentPlanetNoise;

	private static NoisePlanet lastDrawnPlanetNoise;

	private static List<Color32> planetNoiseMeshColors = new List<Color32>();

	private static List<Vector3> planetNoiseMeshVerts;

	public static IntVec2 RenderSize
	{
		set
		{
			NoiseRenderer.renderSize = value;
		}
	}

	public static void StoreTexture(Texture2D texture, string name)
	{
		Noise2D item = new Noise2D(texture, name);
		noises2D.Add(item);
	}

	public static void StoreNoiseRender(ModuleBase noise, string name, IntVec2 renderSize)
	{
		RenderSize = renderSize;
		StoreNoiseRender(noise, name);
	}

	public static void StoreNoiseRender(ModuleBase noise, string name)
	{
		if (Prefs.DevMode && DebugViewSettings.drawRecordedNoise)
		{
			Noise2D item = new Noise2D(noise, name);
			noises2D.Add(item);
		}
	}

	public static void StorePlanetNoise(ModuleBase noise, string name)
	{
		if (Prefs.DevMode && DebugViewSettings.drawRecordedNoise)
		{
			NoisePlanet item = new NoisePlanet(noise, name);
			planetNoises.Add(item);
		}
	}

	public static void NoiseDebugOnGUI()
	{
		if (!Prefs.DevMode || !DebugViewSettings.drawRecordedNoise)
		{
			return;
		}
		if (Widgets.ButtonText(new Rect(0f, 40f, 200f, 30f), "Clear noise renders"))
		{
			Clear();
		}
		if (Widgets.ButtonText(new Rect(200f, 40f, 200f, 30f), "Hide noise renders"))
		{
			DebugViewSettings.drawRecordedNoise = false;
		}
		if (WorldRendererUtility.WorldSelected)
		{
			if (planetNoises.Any() && Widgets.ButtonText(new Rect(400f, 40f, 200f, 30f), "Next planet noise"))
			{
				if (Event.current.button == 1)
				{
					if (currentPlanetNoise == null || planetNoises.IndexOf(currentPlanetNoise) == -1)
					{
						currentPlanetNoise = planetNoises[planetNoises.Count - 1];
					}
					else if (planetNoises.IndexOf(currentPlanetNoise) == 0)
					{
						currentPlanetNoise = null;
					}
					else
					{
						currentPlanetNoise = planetNoises[planetNoises.IndexOf(currentPlanetNoise) - 1];
					}
				}
				else if (currentPlanetNoise == null || planetNoises.IndexOf(currentPlanetNoise) == -1)
				{
					currentPlanetNoise = planetNoises[0];
				}
				else if (planetNoises.IndexOf(currentPlanetNoise) == planetNoises.Count - 1)
				{
					currentPlanetNoise = null;
				}
				else
				{
					currentPlanetNoise = planetNoises[planetNoises.IndexOf(currentPlanetNoise) + 1];
				}
			}
			if (currentPlanetNoise != null)
			{
				Rect rect = new Rect(605f, 40f, 300f, 30f);
				Text.Font = GameFont.Medium;
				Widgets.Label(rect, currentPlanetNoise.name);
				Text.Font = GameFont.Small;
			}
		}
		float num = 0f;
		float num2 = 90f;
		Text.Font = GameFont.Tiny;
		foreach (Noise2D item in noises2D)
		{
			Texture2D texture = item.Texture;
			if (num + (float)texture.width + 5f > (float)UI.screenWidth)
			{
				num = 0f;
				num2 += (float)(texture.height + 5 + 25);
			}
			GUI.DrawTexture(new Rect(num, num2, texture.width, texture.height), texture);
			Rect rect2 = new Rect(num, num2 - 15f, texture.width, texture.height);
			GUI.color = Color.black;
			Widgets.Label(rect2, item.name);
			GUI.color = Color.white;
			Widgets.Label(new Rect(rect2.x + 1f, rect2.y + 1f, rect2.width, rect2.height), item.name);
			num += (float)(texture.width + 5);
		}
	}

	public static void RenderPlanetNoise()
	{
		if (Prefs.DevMode && DebugViewSettings.drawRecordedNoise && currentPlanetNoise != null)
		{
			if (planetNoiseMesh == null)
			{
				SphereGenerator.Generate(6, 100.3f, Vector3.forward, 360f, out planetNoiseMeshVerts, out var outIndices);
				planetNoiseMesh = new Mesh();
				planetNoiseMesh.name = "NoiseDebugUI";
				planetNoiseMesh.SetVertices(planetNoiseMeshVerts);
				planetNoiseMesh.SetTriangles(outIndices, 0);
				lastDrawnPlanetNoise = null;
			}
			if (lastDrawnPlanetNoise != currentPlanetNoise)
			{
				UpdatePlanetNoiseVertexColors();
				lastDrawnPlanetNoise = currentPlanetNoise;
			}
			Graphics.DrawMesh(planetNoiseMesh, Vector3.zero, Quaternion.identity, WorldMaterials.VertexColor, WorldCameraManager.WorldLayer);
		}
	}

	public static void Clear()
	{
		for (int i = 0; i < noises2D.Count; i++)
		{
			Object.Destroy(noises2D[i].Texture);
		}
		noises2D.Clear();
		ClearPlanetNoises();
	}

	public static void ClearPlanetNoises()
	{
		planetNoises.Clear();
		currentPlanetNoise = null;
		lastDrawnPlanetNoise = null;
		if (planetNoiseMesh != null)
		{
			Mesh localPlanetNoiseMesh = planetNoiseMesh;
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Object.Destroy(localPlanetNoiseMesh);
			});
			planetNoiseMesh = null;
		}
	}

	private static void UpdatePlanetNoiseVertexColors()
	{
		planetNoiseMeshColors.Clear();
		for (int i = 0; i < planetNoiseMeshVerts.Count; i++)
		{
			byte b = (byte)Mathf.Clamp((currentPlanetNoise.noise.GetValue(planetNoiseMeshVerts[i]) * 0.5f + 0.5f) * 255f, 0f, 255f);
			planetNoiseMeshColors.Add(new Color32(b, b, b, byte.MaxValue));
		}
		planetNoiseMesh.SetColors(planetNoiseMeshColors);
	}
}
