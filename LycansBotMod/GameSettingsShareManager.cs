using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using LycansNewRoles;
using TMPro;
using UnityEngine.UI;

namespace LycansBotMod;

// Encodes/decodes GameConfig's public static dropdown/toggle/dictionary fields as a shareable clipboard code.
internal static class GameSettingsShareManager
{
	//Lycans New Roles Config version prefix for the shareable code.
	private const string CodePrefix = "LNRCFG1|";

	internal static string ExportCode(GameSettingsUI gameSettingsUI)
	{
		List<string> entries = new List<string>();
		foreach ((string key, object control) in EnumerateControls(gameSettingsUI))
		{
			string value = control switch
			{
				TMP_Dropdown dropdown => dropdown.value.ToString(CultureInfo.InvariantCulture),
				Toggle toggle => toggle.isOn ? "1" : "0",
				_ => null
			};
			if (value != null)
			{
				entries.Add(key + "=" + value);
			}
		}

		string raw = CodePrefix + string.Join(";", entries);
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
	}

	internal static bool TryApplyCode(GameSettingsUI gameSettingsUI, string code, out int appliedCount, out int skippedCount, out string error)
	{
		appliedCount = 0;
		skippedCount = 0;
		error = null;

		if (string.IsNullOrWhiteSpace(code))
		{
			error = "Settings code is empty.";
			return false;
		}

		string raw;
		try
		{
			raw = Encoding.UTF8.GetString(Convert.FromBase64String(code.Trim()));
		}
		catch (FormatException)
		{
			error = "Not a valid settings code.";
			return false;
		}

		if (!raw.StartsWith(CodePrefix, StringComparison.Ordinal))
		{
			error = "Settings code is from an incompatible mod version.";
			return false;
		}

		Dictionary<string, string> values = new Dictionary<string, string>();
		foreach (string entry in raw.Substring(CodePrefix.Length).Split(';'))
		{
			int separatorIndex = entry.IndexOf('=');
			if (separatorIndex <= 0)
			{
				continue;
			}
			values[entry.Substring(0, separatorIndex)] = entry.Substring(separatorIndex + 1);
		}

		foreach ((string key, object control) in EnumerateControls(gameSettingsUI))
		{
			if (!values.TryGetValue(key, out string value))
			{
				continue;
			}

			try
			{
				if (control is TMP_Dropdown dropdown)
				{
					int index = int.Parse(value, CultureInfo.InvariantCulture);
					if (index >= 0 && index < dropdown.options.Count)
					{
						dropdown.value = index;
						appliedCount++;
					}
					else
					{
						skippedCount++;
					}
				}
				else if (control is Toggle toggle)
				{
					toggle.isOn = value == "1";
					appliedCount++;
				}
			}
			catch (Exception e)
			{
				skippedCount++;
				Plugin.BotLogger.LogWarning((object)("GameSettingsShareManager: skipped entry '" + key + "': " + e.Message));
			}
		}

		return true;
	}

	// Reflects GameSettingsUI's base-game private fields and GameConfig's public static fields so newly added settings are picked up automatically.
	private static IEnumerable<(string Key, object Control)> EnumerateControls(GameSettingsUI gameSettingsUI)
	{
		if (gameSettingsUI != null)
		{
			foreach (FieldInfo field in typeof(GameSettingsUI).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.FieldType != typeof(TMP_Dropdown) && field.FieldType != typeof(Toggle))
				{
					continue;
				}

				object control = field.GetValue(gameSettingsUI);
				if (control != null)
				{
					yield return (field.Name, control);
				}
			}
		}

		foreach (FieldInfo field in typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (field.FieldType == typeof(TMP_Dropdown) || field.FieldType == typeof(Toggle))
			{
				object control = field.GetValue(null);
				if (control != null)
				{
					yield return (field.Name, control);
				}
				continue;
			}

			if (!IsToggleDictionary(field.FieldType))
			{
				continue;
			}

			if (!(field.GetValue(null) is IDictionary dictionary))
			{
				continue;
			}

			foreach (DictionaryEntry entry in dictionary)
			{
				if (entry.Value != null)
				{
					yield return (field.Name + ":" + entry.Key, entry.Value);
				}
			}
		}
	}

	private static bool IsToggleDictionary(Type type)
	{
		return type.IsGenericType
			&& type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
			&& type.GetGenericArguments()[1] == typeof(Toggle);
	}
}
