using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class UI_BackgroundMain : UIMenuBackground
{
	public Texture2D overrideBGImage;

	private Dictionary<ExpansionDef, float> expansionImageFades;

	private static readonly Vector2 BGPlanetSize = new Vector2(2048f, 1280f);

	private static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/HeroArt/BGPlanet");

	private float DeltaAlpha => Time.deltaTime * 2f;

	public void SetupExpansionFadeData()
	{
		expansionImageFades = new Dictionary<ExpansionDef, float>();
		foreach (ExpansionDef allExpansion in ModLister.AllExpansions)
		{
			expansionImageFades.Add(allExpansion, 0f);
		}
	}

	public override void BackgroundOnGUI()
	{
		Vector2 vector = ((overrideBGImage != null) ? new Vector2(overrideBGImage.width, overrideBGImage.height) : BGPlanetSize);
		bool flag = true;
		if ((float)UI.screenWidth > (float)UI.screenHeight * (vector.x / vector.y))
		{
			flag = false;
		}
		Rect rect;
		if (flag)
		{
			float height = UI.screenHeight;
			float num = (float)UI.screenHeight * (vector.x / vector.y);
			rect = new Rect((float)(UI.screenWidth / 2) - num / 2f, 0f, num, height);
		}
		else
		{
			float width = UI.screenWidth;
			float num2 = (float)UI.screenWidth * (vector.y / vector.x);
			rect = new Rect(0f, (float)(UI.screenHeight / 2) - num2 / 2f, width, num2);
		}
		GUI.DrawTexture(rect, overrideBGImage ?? BGPlanet, ScaleMode.ScaleToFit);
		if (Event.current.type == EventType.Repaint)
		{
			DoOverlay(rect);
		}
	}

	private void DoOverlay(Rect bgRect)
	{
		if (expansionImageFades == null)
		{
			return;
		}
		foreach (ExpansionDef allExpansion in ModLister.AllExpansions)
		{
			if (!allExpansion.isCore && !allExpansion.BackgroundImage.NullOrBad() && !(expansionImageFades[allExpansion] <= 0f))
			{
				if (allExpansion.BackgroundImage != overrideBGImage)
				{
					GUI.color = new Color(1f, 1f, 1f, expansionImageFades[allExpansion]);
					GUI.DrawTexture(bgRect, allExpansion.BackgroundImage, ScaleMode.ScaleAndCrop);
					GUI.color = Color.white;
				}
				expansionImageFades[allExpansion] = Mathf.Clamp01(expansionImageFades[allExpansion] - DeltaAlpha / 2f);
			}
		}
	}

	public void Notify_Hovered(ExpansionDef expansionDef)
	{
		if (Event.current.type == EventType.Repaint)
		{
			expansionImageFades[expansionDef] = Mathf.Clamp01(expansionImageFades[expansionDef] + DeltaAlpha);
		}
	}
}
