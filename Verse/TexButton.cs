using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	internal class TexButton
	{
		public static readonly Texture2D CloseXBig = ContentFinder<Texture2D>.Get("UI/Widgets/CloseX");

		public static readonly Texture2D CloseXSmall = ContentFinder<Texture2D>.Get("UI/Widgets/CloseXSmall");

		public static readonly Texture2D NextBig = ContentFinder<Texture2D>.Get("UI/Widgets/NextArrow");

		public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");

		public static readonly Texture2D ReorderUp = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderUp");

		public static readonly Texture2D ReorderDown = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderDown");

		public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");

		public static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");

		public static readonly Texture2D Suspend = ContentFinder<Texture2D>.Get("UI/Buttons/Suspend");

		public static readonly Texture2D SelectOverlappingNext = ContentFinder<Texture2D>.Get("UI/Buttons/SelectNextOverlapping");

		public static readonly Texture2D Info = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton");

		public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");

		public static readonly Texture2D Banish = ContentFinder<Texture2D>.Get("UI/Buttons/Banish");

		public static readonly Texture2D OpenStatsReport = ContentFinder<Texture2D>.Get("UI/Buttons/OpenStatsReport");

		public static readonly Texture2D RenounceTitle = ContentFinder<Texture2D>.Get("UI/Buttons/Renounce");

		public static readonly Texture2D Copy = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");

		public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste");

		public static readonly Texture2D Drop = ContentFinder<Texture2D>.Get("UI/Buttons/Drop");

		public static readonly Texture2D Ingest = ContentFinder<Texture2D>.Get("UI/Buttons/Ingest");

		public static readonly Texture2D DragHash = ContentFinder<Texture2D>.Get("UI/Buttons/DragHash");

		public static readonly Texture2D ToggleLog = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleLog");

		public static readonly Texture2D OpenDebugActionsMenu = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenDebugActionsMenu");

		public static readonly Texture2D OpenInspector = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspector");

		public static readonly Texture2D OpenInspectSettings = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspectSettings");

		public static readonly Texture2D ToggleGodMode = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleGodMode");

		public static readonly Texture2D TogglePauseOnError = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/TogglePauseOnError");

		public static readonly Texture2D ToggleTweak = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleTweak");

		public static readonly Texture2D Add = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Add");

		public static readonly Texture2D NewItem = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/NewItem");

		public static readonly Texture2D Reveal = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reveal");

		public static readonly Texture2D Collapse = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Collapse");

		public static readonly Texture2D Empty = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Empty");

		public static readonly Texture2D Save = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Save");

		public static readonly Texture2D NewFile = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/NewFile");

		public static readonly Texture2D RenameDev = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Rename");

		public static readonly Texture2D Reload = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reload");

		public static readonly Texture2D Play = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Play");

		public static readonly Texture2D Stop = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Stop");

		public static readonly Texture2D RangeMatch = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/RangeMatch");

		public static readonly Texture2D InspectModeToggle = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/InspectModeToggle");

		public static readonly Texture2D CenterOnPointsTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/CenterOnPoints");

		public static readonly Texture2D CurveResetTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/CurveReset");

		public static readonly Texture2D QuickZoomHor1Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomHor1");

		public static readonly Texture2D QuickZoomHor100Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomHor100");

		public static readonly Texture2D QuickZoomHor20kTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomHor20k");

		public static readonly Texture2D QuickZoomVer1Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomVer1");

		public static readonly Texture2D QuickZoomVer100Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomVer100");

		public static readonly Texture2D QuickZoomVer20kTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomVer20k");

		public static readonly Texture2D IconBlog = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Blog");

		public static readonly Texture2D IconForums = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Forums");

		public static readonly Texture2D IconTwitter = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Twitter");

		public static readonly Texture2D IconBook = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Book");

		public static readonly Texture2D IconSoundtrack = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Soundtrack");

		public static readonly Texture2D ShowLearningHelper = ContentFinder<Texture2D>.Get("UI/Buttons/ShowLearningHelper");

		public static readonly Texture2D ShowZones = ContentFinder<Texture2D>.Get("UI/Buttons/ShowZones");

		public static readonly Texture2D ShowFertilityOverlay = ContentFinder<Texture2D>.Get("UI/Buttons/ShowFertilityOverlay");

		public static readonly Texture2D ShowTerrainAffordanceOverlay = ContentFinder<Texture2D>.Get("UI/Buttons/ShowTerrainAffordanceOverlay");

		public static readonly Texture2D ShowBeauty = ContentFinder<Texture2D>.Get("UI/Buttons/ShowBeauty");

		public static readonly Texture2D ShowRoomStats = ContentFinder<Texture2D>.Get("UI/Buttons/ShowRoomStats");

		public static readonly Texture2D ShowColonistBar = ContentFinder<Texture2D>.Get("UI/Buttons/ShowColonistBar");

		public static readonly Texture2D ShowRoofOverlay = ContentFinder<Texture2D>.Get("UI/Buttons/ShowRoofOverlay");

		public static readonly Texture2D AutoHomeArea = ContentFinder<Texture2D>.Get("UI/Buttons/AutoHomeArea");

		public static readonly Texture2D AutoRebuild = ContentFinder<Texture2D>.Get("UI/Buttons/AutoRebuild");

		public static readonly Texture2D CategorizedResourceReadout = ContentFinder<Texture2D>.Get("UI/Buttons/ResourceReadoutCategorized");

		public static readonly Texture2D LockNorthUp = ContentFinder<Texture2D>.Get("UI/Buttons/LockNorthUp");

		public static readonly Texture2D UsePlanetDayNightSystem = ContentFinder<Texture2D>.Get("UI/Buttons/UsePlanetDayNightSystem");

		public static readonly Texture2D ShowExpandingIcons = ContentFinder<Texture2D>.Get("UI/Buttons/ShowExpandingIcons");

		public static readonly Texture2D ShowWorldFeatures = ContentFinder<Texture2D>.Get("UI/Buttons/ShowWorldFeatures");

		public static readonly Texture2D[] SpeedButtonTextures = new Texture2D[5]
		{
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause"),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Normal"),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Fast"),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast"),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast")
		};
	}
}
