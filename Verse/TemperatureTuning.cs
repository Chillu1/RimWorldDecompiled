namespace Verse
{
	public static class TemperatureTuning
	{
		public const float MinimumTemperature = -273.15f;

		public const float MaximumTemperature = 1000f;

		public const float DefaultTemperature = 21f;

		public const float DeepUndergroundTemperature = 15f;

		public static readonly SimpleCurve SeasonalTempVariationCurve = new SimpleCurve
		{
			new CurvePoint(0f, 3f),
			new CurvePoint(0.1f, 4f),
			new CurvePoint(1f, 28f)
		};

		public const float DailyTempVariationAmplitude = 7f;

		public const float DailySunEffect = 14f;

		public const float FoodRefrigerationTemp = 10f;

		public const float FoodFreezingTemp = 0f;

		public const int RoomTempEqualizeInterval = 120;

		public const int Door_TempEqualizeIntervalOpen = 34;

		public const int Door_TempEqualizeIntervalClosed = 375;

		public const float Door_TempEqualizeRate = 1f;

		public const float Vent_TempEqualizeRate = 14f;

		public const float InventoryTemperature = 14f;

		public const float DropPodTemperature = 14f;

		public const float TradeShipTemperature = 14f;
	}
}
