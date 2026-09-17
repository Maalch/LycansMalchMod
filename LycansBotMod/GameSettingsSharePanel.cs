using System;
using Fusion;
using HarmonyLib;
using LycansNewRoles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LycansBotMod;

[HarmonyPatch(typeof(GameSettingsUI), "Start")]
internal class GameSettingsSharePanelPatch
{
	private static void Postfix(GameSettingsUI __instance)
	{
		GameSettingsSharePanel.EnsureAttached(__instance);
	}
}

internal class GameSettingsSharePanel : MonoBehaviour
{
	private const float ButtonWidth = 220f;
	private const float ButtonHeight = 42f;
	private const float PanelWidth = 620f;
	private const float PanelHeight = 340f;

	private GameObject _button;
	private GameObject _panel;
	private TextMeshProUGUI _codeText;
	private TextMeshProUGUI _statusText;
	private GameSettingsUI _gameSettingsUI;

	internal static void EnsureAttached(GameSettingsUI gameSettingsUI)
	{
		if (((Component)gameSettingsUI).GetComponent<GameSettingsSharePanel>() == null)
		{
			((Component)gameSettingsUI).gameObject.AddComponent<GameSettingsSharePanel>();
		}
	}

	private void Awake()
	{
		try
		{
			_gameSettingsUI = GetComponent<GameSettingsUI>();
			BuildUi();
		}
		catch (Exception exception)
		{
			Plugin.BotLogger.LogError((object)("Unable to create game settings share panel: " + exception));
		}
	}

	private void Update()
	{
		if (_button == null || _panel == null)
		{
			return;
		}

		bool showControls = IsHost() && GameManager.LocalGameState == GameState.EGameState.Pregame;
		if (!showControls && _panel.activeSelf)
		{
			_panel.SetActive(false);
		}

		_button.SetActive(showControls && !_panel.activeSelf);
	}

	private void BuildUi()
	{
		TextMeshProUGUI textTemplate = Object.FindObjectOfType<TextMeshProUGUI>(true);
		if (textTemplate == null)
		{
			throw new InvalidOperationException("No TextMeshPro text element is available for the game settings share panel.");
		}

		_button = CreateButton("GameSettingsShareButton", "Share Settings", transform, new Vector2(0f, 1f), new Vector2(200f, -100f), new Vector2(ButtonWidth, ButtonHeight), textTemplate, Open);
		_panel = CreatePanel(textTemplate);
		_panel.SetActive(false);
	}

	private GameObject CreatePanel(TextMeshProUGUI textTemplate)
	{
		GameObject panel = new GameObject("GameSettingsSharePanel", typeof(RectTransform), typeof(Image));
		panel.transform.SetParent(transform, false);
		panel.transform.SetAsLastSibling();

		RectTransform panelTransform = panel.GetComponent<RectTransform>();
		panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
		panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
		panelTransform.pivot = new Vector2(0.5f, 0.5f);
		panelTransform.anchoredPosition = Vector2.zero;
		panelTransform.sizeDelta = new Vector2(PanelWidth, PanelHeight);
		panel.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.12f, 0.96f);

		CreateText("Title", "Share Game Settings", panel.transform, new Vector2(24f, -20f), new Vector2(-24f, -58f), textTemplate, 24f, FontStyles.Bold, TextAlignmentOptions.Left);
		_codeText = CreateText("CodeText", string.Empty, panel.transform, new Vector2(24f, -70f), new Vector2(-24f, -230f), textTemplate, 15f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
		_statusText = CreateText("StatusText", string.Empty, panel.transform, new Vector2(24f, -244f), new Vector2(-24f, -270f), textTemplate, 15f, FontStyles.Italic, TextAlignmentOptions.Center);
		_statusText.color = new Color(0.72f, 0.82f, 0.88f, 1f);

		CreateButton("CopyButton", "Copy to Clipboard", panel.transform, new Vector2(0f, 0f), new Vector2(150f, 24f), new Vector2(220f, ButtonHeight), textTemplate, CopyToClipboard);
		CreateButton("ApplyButton", "Apply from Clipboard", panel.transform, new Vector2(1f, 0f), new Vector2(-150f, 24f), new Vector2(220f, ButtonHeight), textTemplate, ApplyFromClipboard);
		CreateButton("CloseButton", "X", panel.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(40f, 40f), textTemplate, Close);
		return panel;
	}

	private void Open()
	{
		_panel.SetActive(true);
		RefreshCode();
		_statusText.text = string.Empty;
	}

	private void Close()
	{
		_panel.SetActive(false);
	}

	private void RefreshCode()
	{
		_codeText.text = GameSettingsShareManager.ExportCode(_gameSettingsUI);
	}

	private void CopyToClipboard()
	{
		RefreshCode();
		GUIUtility.systemCopyBuffer = _codeText.text;
		_statusText.text = "Code copied to clipboard.";
	}

	private void ApplyFromClipboard()
	{
		string code = GUIUtility.systemCopyBuffer;
		if (GameSettingsShareManager.TryApplyCode(_gameSettingsUI, code, out int appliedCount, out int skippedCount, out string error))
		{
			_statusText.text = $"Applied {appliedCount} settings ({skippedCount} skipped).";
			RefreshCode();
		}
		else
		{
			_statusText.text = "Error: " + error;
		}
	}

	private static GameObject CreateButton(string name, string label, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, TextMeshProUGUI textTemplate, UnityEngine.Events.UnityAction action)
	{
		GameObject button = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
		button.transform.SetParent(parent, false);

		RectTransform buttonTransform = button.GetComponent<RectTransform>();
		buttonTransform.anchorMin = anchor;
		buttonTransform.anchorMax = anchor;
		buttonTransform.pivot = new Vector2(0.5f, 0.5f);
		buttonTransform.anchoredPosition = anchoredPosition;
		buttonTransform.sizeDelta = size;

		Image image = button.GetComponent<Image>();
		image.color = new Color(0.16f, 0.42f, 0.54f, 1f);
		Button buttonComponent = button.GetComponent<Button>();
		buttonComponent.targetGraphic = image;
		buttonComponent.onClick.AddListener(action);
		TextMeshProUGUI buttonLabel = CreateText("Label", label, button.transform, Vector2.zero, Vector2.zero, textTemplate, 16f, FontStyles.Bold, TextAlignmentOptions.Center);
		RectTransform labelTransform = buttonLabel.GetComponent<RectTransform>();
		labelTransform.anchorMin = Vector2.zero;
		labelTransform.anchorMax = Vector2.one;
		labelTransform.offsetMin = Vector2.zero;
		labelTransform.offsetMax = Vector2.zero;
		return button;
	}

	private static TextMeshProUGUI CreateText(string name, string content, Transform parent, Vector2 topLeft, Vector2 bottomRight, TextMeshProUGUI textTemplate, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
	{
		GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
		textObject.transform.SetParent(parent, false);

		RectTransform textTransform = textObject.GetComponent<RectTransform>();
		textTransform.anchorMin = new Vector2(0f, 1f);
		textTransform.anchorMax = new Vector2(1f, 1f);
		textTransform.pivot = new Vector2(0.5f, 1f);
		textTransform.offsetMin = new Vector2(topLeft.x, bottomRight.y);
		textTransform.offsetMax = new Vector2(bottomRight.x, topLeft.y);

		TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
		text.font = textTemplate.font;
		text.fontSharedMaterial = textTemplate.fontSharedMaterial;
		text.fontSize = fontSize;
		text.fontStyle = fontStyle;
		text.alignment = alignment;
		text.color = Color.white;
		text.enableWordWrapping = true;
		text.overflowMode = TextOverflowModes.Ellipsis;
		text.text = content;
		return text;
	}

	private static bool IsHost()
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		NetworkRunner runner = ((SimulationBehaviour)GameManager.Instance).Runner;
		return runner != null && runner.IsServer;
	}
}
