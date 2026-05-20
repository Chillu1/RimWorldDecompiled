using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class AnimationDefGenerator_Flying
{
	private static List<GraphicStateDef> generatedGraphicStateDefs = new List<GraphicStateDef>();

	private static List<AnimationDef> generatedAnimationDefs = new List<AnimationDef>();

	public static void InitializeNeededDefs(bool hotReload = false)
	{
		foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
		{
			if (allDef.flyingAnimationFramePathPrefix.NullOrEmpty())
			{
				continue;
			}
			bool flag = !allDef.flyingAnimationFramePathPrefixFemale.NullOrEmpty();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			List<string> list4 = (flag ? new List<string>() : list);
			List<string> list5 = (flag ? new List<string>() : list2);
			List<string> list6 = (flag ? new List<string>() : list3);
			for (int i = 1; i <= allDef.flyingAnimationFrameCount; i++)
			{
				list.Add(allDef.flyingAnimationFramePathPrefix + i + "_east");
				list2.Add(allDef.flyingAnimationFramePathPrefix + i + "_north");
				list3.Add(allDef.flyingAnimationFramePathPrefix + i + "_south");
				if (flag)
				{
					list4.Add(allDef.flyingAnimationFramePathPrefixFemale + i + "_east");
					list5.Add(allDef.flyingAnimationFramePathPrefixFemale + i + "_north");
					list6.Add(allDef.flyingAnimationFramePathPrefixFemale + i + "_south");
				}
			}
			bool flyingAnimationInheritColors = allDef.flyingAnimationInheritColors;
			float num = allDef.flyingAnimationDrawSize;
			bool flyingAnimationDrawSizeIsMultiplier = allDef.flyingAnimationDrawSizeIsMultiplier;
			int flyingAnimationTicksPerFrame = allDef.flyingAnimationTicksPerFrame;
			if (flyingAnimationInheritColors || flag)
			{
				Dictionary<Color, (AnimationDef, AnimationDef, AnimationDef)> dictionary = new Dictionary<Color, (AnimationDef, AnimationDef, AnimationDef)>();
				Dictionary<Color, (AnimationDef, AnimationDef, AnimationDef)> dictionary2 = (flag ? new Dictionary<Color, (AnimationDef, AnimationDef, AnimationDef)>() : dictionary);
				foreach (PawnKindLifeStage lifeStage in allDef.lifeStages)
				{
					Color color = lifeStage.bodyGraphicData.color;
					if (flyingAnimationDrawSizeIsMultiplier)
					{
						num = lifeStage.bodyGraphicData.drawSize.x * allDef.flyingAnimationDrawSize;
					}
					if (dictionary.TryGetValue(color, out var value))
					{
						(lifeStage.flyingAnimationEast, lifeStage.flyingAnimationNorth, lifeStage.flyingAnimationSouth) = value;
					}
					else
					{
						AnimationDef animationDef = CreateAnimFromFrames(list, flyingAnimationTicksPerFrame, num, allDef.defName + "FlyEast", color, hotReload);
						AnimationDef animationDef2 = CreateAnimFromFrames(list2, flyingAnimationTicksPerFrame, num, allDef.defName + "FlyNorth", color, hotReload);
						AnimationDef animationDef3 = CreateAnimFromFrames(list3, flyingAnimationTicksPerFrame, num, allDef.defName + "FlySouth", color, hotReload);
						lifeStage.flyingAnimationEast = animationDef;
						lifeStage.flyingAnimationNorth = animationDef2;
						lifeStage.flyingAnimationSouth = animationDef3;
						dictionary[color] = (animationDef, animationDef2, animationDef3);
					}
					if (lifeStage.femaleGraphicData != null)
					{
						color = lifeStage.femaleGraphicData.color;
						if (flyingAnimationDrawSizeIsMultiplier)
						{
							num = lifeStage.femaleGraphicData.drawSize.x * allDef.flyingAnimationDrawSize;
						}
						if (dictionary2.TryGetValue(color, out value))
						{
							(lifeStage.flyingAnimationEastFemale, lifeStage.flyingAnimationNorthFemale, lifeStage.flyingAnimationSouthFemale) = value;
							continue;
						}
						AnimationDef animationDef4 = CreateAnimFromFrames(list4, flyingAnimationTicksPerFrame, num, allDef.defName + "FlyEastFemale", color, hotReload);
						AnimationDef animationDef5 = CreateAnimFromFrames(list5, flyingAnimationTicksPerFrame, num, allDef.defName + "FlyNorthFemale", color, hotReload);
						AnimationDef animationDef6 = CreateAnimFromFrames(list6, flyingAnimationTicksPerFrame, num, allDef.defName + "FlySouthFemale", color, hotReload);
						lifeStage.flyingAnimationEastFemale = animationDef4;
						lifeStage.flyingAnimationNorthFemale = animationDef5;
						lifeStage.flyingAnimationSouthFemale = animationDef6;
						dictionary2[color] = (animationDef4, animationDef5, animationDef6);
					}
				}
				continue;
			}
			if (flyingAnimationDrawSizeIsMultiplier)
			{
				num = allDef.lifeStages.First().bodyGraphicData.drawSize.x * allDef.flyingAnimationDrawSize;
			}
			float drawSize = num;
			string nameRoot = allDef.defName + "FlyEast";
			bool hotReload2 = hotReload;
			AnimationDef flyingAnimationEast = CreateAnimFromFrames(list, flyingAnimationTicksPerFrame, drawSize, nameRoot, null, hotReload2);
			float drawSize2 = num;
			string nameRoot2 = allDef.defName + "FlyNorth";
			hotReload2 = hotReload;
			AnimationDef flyingAnimationNorth = CreateAnimFromFrames(list2, flyingAnimationTicksPerFrame, drawSize2, nameRoot2, null, hotReload2);
			float drawSize3 = num;
			string nameRoot3 = allDef.defName + "FlySouth";
			hotReload2 = hotReload;
			AnimationDef flyingAnimationSouth = CreateAnimFromFrames(list3, flyingAnimationTicksPerFrame, drawSize3, nameRoot3, null, hotReload2);
			foreach (PawnKindLifeStage lifeStage2 in allDef.lifeStages)
			{
				lifeStage2.flyingAnimationEast = flyingAnimationEast;
				lifeStage2.flyingAnimationNorth = flyingAnimationNorth;
				lifeStage2.flyingAnimationSouth = flyingAnimationSouth;
			}
		}
	}

	public static IEnumerable<GraphicStateDef> ImpliedGraphicStateDefs()
	{
		foreach (GraphicStateDef generatedGraphicStateDef in generatedGraphicStateDefs)
		{
			yield return generatedGraphicStateDef;
		}
		generatedGraphicStateDefs.Clear();
	}

	public static IEnumerable<AnimationDef> ImpliedAnimationDefs()
	{
		foreach (AnimationDef generatedAnimationDef in generatedAnimationDefs)
		{
			yield return generatedAnimationDef;
		}
		generatedAnimationDefs.Clear();
	}

	private static AnimationDef CreateAnimFromFrames(List<string> framePaths, int ticksPerFrame, float drawSize, string nameRoot, Color? tint = null, bool hotReload = false)
	{
		string text;
		if (!tint.HasValue)
		{
			text = nameRoot;
		}
		else
		{
			Color32 color = tint.Value;
			int num = (color.a << 24) | (color.r << 16) | (color.g << 8) | color.b;
			text = $"{nameRoot}_{num:x8}";
		}
		AnimationDef animationDef = (hotReload ? (DefDatabase<AnimationDef>.GetNamed(text, errorOnFail: false) ?? new AnimationDef()) : new AnimationDef());
		animationDef.durationTicks = (framePaths.Count + 1) * ticksPerFrame;
		animationDef.defName = text;
		animationDef.startOnRandomTick = false;
		animationDef.keyframeParts = new Dictionary<PawnRenderNodeTagDef, KeyframeAnimationPart>();
		PawnRenderNodeTagDef named = DefDatabase<PawnRenderNodeTagDef>.GetNamed("Body");
		KeyframeAnimationPart keyframeAnimationPart = (animationDef.keyframeParts[named] = new KeyframeAnimationPart());
		KeyframeAnimationPart keyframeAnimationPart3 = keyframeAnimationPart;
		keyframeAnimationPart3.workerType = typeof(AnimationWorker_Keyframes);
		keyframeAnimationPart3.keyframes = new List<Verse.Keyframe>(framePaths.Count);
		for (int i = 0; i < framePaths.Count; i++)
		{
			GraphicStateDef graphicStateDef = (hotReload ? (DefDatabase<GraphicStateDef>.GetNamed(text + "_" + i, errorOnFail: false) ?? new GraphicStateDef()) : new GraphicStateDef());
			graphicStateDef.defName = text + "_" + i;
			graphicStateDef.applySkinColorTint = true;
			graphicStateDef.defaultGraphicData = new GraphicData
			{
				graphicClass = typeof(Graphic_Single),
				drawSize = new Vector2(drawSize, drawSize),
				color = (tint ?? Color.white),
				texPath = framePaths[i]
			};
			keyframeAnimationPart3.keyframes.Add(new Verse.Keyframe
			{
				graphicState = graphicStateDef,
				tick = i * ticksPerFrame
			});
			generatedGraphicStateDefs.Add(graphicStateDef);
		}
		generatedAnimationDefs.Add(animationDef);
		return animationDef;
	}
}
