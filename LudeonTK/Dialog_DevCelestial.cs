using RimWorld;
using UnityEngine;
using Verse;

namespace LudeonTK;

[StaticConstructorOnStartup]
public class Dialog_DevCelestial : Window_Dev
{
	private Vector2 windowPosition;

	private const string Title = "Celestial Debugger";

	public override bool IsDebug => true;

	protected override float Margin => 4f;

	public override Vector2 InitialSize => new Vector2(230f, 275f);

	public Dialog_DevCelestial()
	{
		draggable = true;
		focusWhenOpened = false;
		drawShadow = false;
		closeOnAccept = false;
		closeOnCancel = false;
		preventCameraMotion = false;
		drawInScreenshotMode = false;
		windowPosition = Prefs.DevPalettePosition;
		onlyDrawInDevMode = true;
		doCloseX = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		Rect rect = new Rect(inRect.x, inRect.y, inRect.width, 24f);
		DevGUI.Label(rect, "Celestial Debugger");
		float y = rect.height + 6f;
		Text.Font = GameFont.Tiny;
		Map currentMap = Find.CurrentMap;
		GenCelestial.LightInfo lightSourceInfo = GenCelestial.GetLightSourceInfo(currentMap, GenCelestial.LightType.Shadow);
		GenCelestial.LightInfo lightSourceInfo2 = GenCelestial.GetLightSourceInfo(currentMap, GenCelestial.LightType.LightingSun);
		Vector2 vector = Find.WorldGrid.LongLatOf(currentMap.Tile);
		PrintLabel("Map", inRect, ref y);
		PrintLabel($"Day progress: {GenLocalDate.DayPercent(currentMap) * 100f:0}%", inRect, ref y);
		PrintLabel($"Long, Lat: {vector.x:0.0}, {vector.y:0.0}", inRect, ref y);
		PrintLabel("", inRect, ref y);
		PrintLabel("Shadow Info", inRect, ref y);
		PrintLabel($"Vector: {lightSourceInfo.vector}", inRect, ref y);
		PrintLabel($"Intensity: {lightSourceInfo.intensity * 100f:0}%", inRect, ref y);
		PrintLabel("", inRect, ref y);
		PrintLabel("Sun Info", inRect, ref y);
		PrintLabel($"Vector: {lightSourceInfo2.vector}", inRect, ref y);
		PrintLabel($"Intensity: {lightSourceInfo2.intensity * 100f:0}%", inRect, ref y);
	}

	private void PrintLabel(string text, Rect container, ref float y)
	{
		DevGUI.Label(new Rect(container.x, y, container.width, 20f), text);
		y += 20f;
	}
}
