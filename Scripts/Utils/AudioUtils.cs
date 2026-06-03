using Godot;

public static class AudioUtils
{
	private const float MuteDb = -80f;
	private const float MinLinear = 0.0001f;

	// =========================
	// Volume Conversion
	// =========================

	public static float PercentToDb(float percent)
	{
		percent = Mathf.Clamp(percent, 0f, 100f);

		float linear = percent / 100f;

		if (linear <= MinLinear)
			return MuteDb;

		return Mathf.LinearToDb(linear);
	}

	public static float DbToPercent(float db)
	{
		if (db <= MuteDb)
			return 0f;

		float linear = Mathf.DbToLinear(db);
		return Mathf.Clamp(linear * 100f, 0f, 100f);
	}

	// =========================
	// Audio Bus Helpers
	// =========================

	public static void SetBusVolume(string busName, float percent)
	{
		int index = AudioServer.GetBusIndex(busName);

		if (index < 0)
			return;

		AudioServer.SetBusVolumeDb(index, PercentToDb(percent));
	}

	public static float GetBusVolumePercent(string busName)
	{
		int index = AudioServer.GetBusIndex(busName);

		if (index < 0)
			return 100f;

		float db = AudioServer.GetBusVolumeDb(index);
		return DbToPercent(db);
	}

	// =========================
	// Config Helpers
	// =========================

	public static bool TryGetConfigPercent(ConfigFile config, string section, string key, out float value)
	{
		value = 100f;

		if (config == null)
			return false;

		if (!config.HasSectionKey(section, key))
			return false;

		value = (float)(double)config.GetValue(section, key, 100.0);
		return true;
	}

	public static void ApplyBusFromConfig(ConfigFile config, string configKey, string busName)
	{
		if (TryGetConfigPercent(config, "audio", configKey, out float value))
		{
			SetBusVolume(busName, value);
		}
	}
}
