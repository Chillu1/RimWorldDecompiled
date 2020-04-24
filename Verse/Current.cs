using RimWorld.Planet;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Verse
{
	public static class Current
	{
		private static ProgramState programStateInt;

		private static Root rootInt;

		private static Root_Entry rootEntryInt;

		private static Root_Play rootPlayInt;

		private static Camera cameraInt;

		private static CameraDriver cameraDriverInt;

		private static ColorCorrectionCurves colorCorrectionCurvesInt;

		private static SubcameraDriver subcameraDriverInt;

		private static Game gameInt;

		private static World creatingWorldInt;

		public static Root Root => rootInt;

		public static Root_Entry Root_Entry => rootEntryInt;

		public static Root_Play Root_Play => rootPlayInt;

		public static Camera Camera => cameraInt;

		public static CameraDriver CameraDriver => cameraDriverInt;

		public static ColorCorrectionCurves ColorCorrectionCurves => colorCorrectionCurvesInt;

		public static SubcameraDriver SubcameraDriver => subcameraDriverInt;

		public static Game Game
		{
			get
			{
				return gameInt;
			}
			set
			{
				gameInt = value;
			}
		}

		public static World CreatingWorld
		{
			get
			{
				return creatingWorldInt;
			}
			set
			{
				creatingWorldInt = value;
			}
		}

		public static ProgramState ProgramState
		{
			get
			{
				return programStateInt;
			}
			set
			{
				programStateInt = value;
			}
		}

		public static void Notify_LoadedSceneChanged()
		{
			cameraInt = GameObject.Find("Camera").GetComponent<Camera>();
			if (GenScene.InEntryScene)
			{
				ProgramState = ProgramState.Entry;
				rootEntryInt = GameObject.Find("GameRoot").GetComponent<Root_Entry>();
				rootPlayInt = null;
				rootInt = rootEntryInt;
				cameraDriverInt = null;
				colorCorrectionCurvesInt = null;
			}
			else if (GenScene.InPlayScene)
			{
				ProgramState = ProgramState.MapInitializing;
				rootEntryInt = null;
				rootPlayInt = GameObject.Find("GameRoot").GetComponent<Root_Play>();
				rootInt = rootPlayInt;
				cameraDriverInt = cameraInt.GetComponent<CameraDriver>();
				colorCorrectionCurvesInt = cameraInt.GetComponent<ColorCorrectionCurves>();
				subcameraDriverInt = GameObject.Find("Subcameras").GetComponent<SubcameraDriver>();
			}
		}
	}
}
