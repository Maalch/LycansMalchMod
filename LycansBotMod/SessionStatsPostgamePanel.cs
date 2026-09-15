using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using HarmonyLib;
using LycansNewRoles;
using LycansNewRoles.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LycansBotMod;

[HarmonyPatch(typeof(UILastGameSummaryPanel), nameof(UILastGameSummaryPanel.Show))]
internal class SessionStatsPostgamePanelPatch
{
	private static void Postfix(UILastGameSummaryPanel __instance)
	{
		SessionStatsPostgamePanel.EnsureAttached(__instance);
	}
}

internal class SessionStatsPostgamePanel : MonoBehaviour
{
	private enum StatsView
	{
		Overview,
		Players,
		PlayerDetails
	}

	private const int PlayersPerPage = 5;
	private const float ButtonWidth = 180f;
	private const float ButtonHeight = 42f;
	private const float PanelWidth = 760f;
	private const float PanelHeight = 460f;

	private GameObject _button;
	private GameObject _panel;
	private TextMeshProUGUI _titleText;
	private TextMeshProUGUI _footerText;
	private readonly TextMeshProUGUI[] _contentRows = new TextMeshProUGUI[8];
	private readonly Image[] _playerRowBackgrounds = new Image[PlayersPerPage];
	private StatsView _view;
	private int _selectedPlayerIndex;
	private int _detailSection;

	internal static void EnsureAttached(UILastGameSummaryPanel summaryPanel)
	{
		if (summaryPanel.GetComponent<SessionStatsPostgamePanel>() == null)
		{
			summaryPanel.gameObject.AddComponent<SessionStatsPostgamePanel>();
		}
	}

	private void Awake()
	{
		try
		{
			BuildUi();
		}
		catch (System.Exception exception)
		{
			Plugin.BotLogger.LogError((object)("Unable to create session stats panel: " + exception));
		}
	}

	private void Update()
	{
		if (_button == null || _panel == null)
		{
			return;
		}

		bool showPostgameControls = IsHost() && UIManager.LastGameSummaryPanel != null && UIManager.LastGameSummaryPanel.Active;
		if (!showPostgameControls && _panel.activeSelf)
		{
			_panel.SetActive(false);
		}
		else if (showPostgameControls && Input.GetKeyDown(KeyCode.F1))
		{
			Toggle();
		}
		else if (showPostgameControls && _panel.activeSelf)
		{
			HandleKeyboardInput();
		}

		_button.SetActive(showPostgameControls && !_panel.activeSelf);
	}

	private void BuildUi()
	{
		TextMeshProUGUI textTemplate = Object.FindObjectOfType<TextMeshProUGUI>(true);
		if (textTemplate == null)
		{
			throw new System.InvalidOperationException("No TextMeshPro text element is available for the session stats panel.");
		}

		_button = CreateButton("SessionStatsButton", "F1 - Session Stats", transform, new Vector2(1f, 1f), new Vector2(-200f, -100f), new Vector2(ButtonWidth, ButtonHeight), textTemplate, Open);
		_panel = CreatePanel(textTemplate);
		_panel.SetActive(false);
	}

	private GameObject CreatePanel(TextMeshProUGUI textTemplate)
	{
		GameObject panel = new GameObject("SessionStatsPanel", typeof(RectTransform), typeof(Image));
		panel.transform.SetParent(transform, false);
		panel.transform.SetAsLastSibling();

		RectTransform panelTransform = panel.GetComponent<RectTransform>();
		panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
		panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
		panelTransform.pivot = new Vector2(0.5f, 0.5f);
		panelTransform.anchoredPosition = Vector2.zero;
		panelTransform.sizeDelta = new Vector2(PanelWidth, PanelHeight);
		panel.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.12f, 0.96f);

		_titleText = CreateText("Title", "Session Stats", panel.transform, new Vector2(24f, -20f), new Vector2(-66f, -58f), textTemplate, 26f, FontStyles.Bold, TextAlignmentOptions.Left);
		for (int rowIndex = 0; rowIndex < PlayersPerPage; rowIndex++)
		{
			_playerRowBackgrounds[rowIndex] = CreatePlayerRowBackground(panel.transform, rowIndex);
		}
		for (int rowIndex = 0; rowIndex < _contentRows.Length; rowIndex++)
		{
			float top = -76f - rowIndex * 39f;
			_contentRows[rowIndex] = CreateText("ContentRow" + rowIndex, string.Empty, panel.transform, new Vector2(28f, top), new Vector2(-28f, top - 35f), textTemplate, 18f, FontStyles.Normal, TextAlignmentOptions.Left);
		}
		_footerText = CreateText("Footer", string.Empty, panel.transform, new Vector2(28f, -422f), new Vector2(-28f, -448f), textTemplate, 16f, FontStyles.Normal, TextAlignmentOptions.Center);
		_footerText.color = new Color(0.72f, 0.82f, 0.88f, 1f);
		CreateButton("CloseButton", "X", panel.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(40f, 40f), textTemplate, Close);
		return panel;
	}

	private static Image CreatePlayerRowBackground(Transform parent, int rowIndex)
	{
		float top = -70f - rowIndex * 61f;
		GameObject background = new GameObject("PlayerRowBackground" + rowIndex, typeof(RectTransform), typeof(Image));
		background.transform.SetParent(parent, false);
		RectTransform backgroundTransform = background.GetComponent<RectTransform>();
		backgroundTransform.anchorMin = new Vector2(0f, 1f);
		backgroundTransform.anchorMax = new Vector2(1f, 1f);
		backgroundTransform.pivot = new Vector2(0.5f, 1f);
		backgroundTransform.offsetMin = new Vector2(18f, top - 56f);
		backgroundTransform.offsetMax = new Vector2(-18f, top);
		Image image = background.GetComponent<Image>();
		image.color = new Color(0.11f, 0.16f, 0.19f, 0.9f);
		background.SetActive(false);
		return image;
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
		TextMeshProUGUI buttonLabel = CreateText("Label", label, button.transform, Vector2.zero, Vector2.zero, textTemplate, 18f, FontStyles.Bold, TextAlignmentOptions.Center);
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

	private void Open()
	{
		_view = StatsView.Overview;
		_selectedPlayerIndex = 0;
		_detailSection = 0;
		_panel.SetActive(true);
		RefreshView();
	}

	private void Close()
	{
		_panel.SetActive(false);
	}

	private void Toggle()
	{
		if (_panel.activeSelf)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	private void HandleKeyboardInput()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
		{
			if (_view == StatsView.PlayerDetails)
			{
				_view = StatsView.Players;
				RefreshView();
			}
			else
			{
				Close();
			}
			return;
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			MoveLeft();
			return;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			MoveRight();
			return;
		}
		if (_view == StatsView.Players && Input.GetKeyDown(KeyCode.UpArrow))
		{
			MovePlayerSelection(-1);
			return;
		}
		if (_view == StatsView.Players && Input.GetKeyDown(KeyCode.DownArrow))
		{
			MovePlayerSelection(1);
			return;
		}
		if (_view == StatsView.Players && Input.GetKeyDown(KeyCode.Return) && GetPlayers().Count > 0)
		{
			_view = StatsView.PlayerDetails;
			_detailSection = 0;
			RefreshView();
		}
	}

	private void MoveLeft()
	{
		if (_view == StatsView.PlayerDetails)
		{
			_detailSection = 0;
		}
		else if (_view == StatsView.Players)
		{
			_view = StatsView.Overview;
		}
		RefreshView();
	}

	private void MoveRight()
	{
		if (_view == StatsView.Overview)
		{
			_view = StatsView.Players;
		}
		else if (_view == StatsView.PlayerDetails)
		{
			_detailSection = 1;
		}
		RefreshView();
	}

	private void MovePlayerSelection(int direction)
	{
		List<PlayerStats> players = GetPlayers();
		if (players.Count == 0)
		{
			return;
		}
		_selectedPlayerIndex = Mathf.Clamp(_selectedPlayerIndex + direction, 0, players.Count - 1);
		RefreshView();
	}

	private void RefreshView()
	{
		List<PlayerStats> players = GetPlayers();
		_selectedPlayerIndex = Mathf.Clamp(_selectedPlayerIndex, 0, Mathf.Max(0, players.Count - 1));
		for (int rowIndex = 0; rowIndex < _contentRows.Length; rowIndex++)
		{
			_contentRows[rowIndex].gameObject.SetActive(false);
		}
		for (int rowIndex = 0; rowIndex < _playerRowBackgrounds.Length; rowIndex++)
		{
			_playerRowBackgrounds[rowIndex].gameObject.SetActive(false);
		}

		switch (_view)
		{
		case StatsView.Overview:
			RenderOverview(players);
			break;
		case StatsView.Players:
			RenderPlayers(players);
			break;
		case StatsView.PlayerDetails:
			RenderPlayerDetails(players);
			break;
		}
	}

	private void RenderOverview(List<PlayerStats> players)
	{
		GameStats game = SessionStats.Stats.CurrentGame;
		int winners = players.Count((PlayerStats player) => player.Victorious);
		int deaths = players.Count((PlayerStats player) => !string.IsNullOrEmpty(player.DeathType));
		_titleText.text = "Session Stats - Overview";
		SetRow(0, "Session: " + DisplayValue(SessionStats.Stats.Filename));
		SetRow(1, "Mod version: " + DisplayValue(SessionStats.Stats.ModVersion));
		SetRow(2, "Map: " + DisplayValue(game.MapName) + "    Mode: " + (game.IsBattleRoyale ? "Battle Royale" : "Standard"));
		SetRow(3, "Harvest: " + game.HarvestDone + " / " + game.HarvestGoal);
		SetRow(4, "Started: " + DisplayValue(game.StartDate));
		SetRow(5, "Ended: " + DisplayValue(game.EndDate) + "    Timing: " + DisplayValue(game.EndTiming));
		SetRow(6, "Players: " + players.Count + "    Winners: " + winners + "    Deaths: " + deaths);
		SetRow(7, "Recorded game events: " + (game.GameEvents == null ? 0 : game.GameEvents.Count));
		_footerText.text = "RIGHT: Players    F1 or ESC: Close";
	}

	private void RenderPlayers(List<PlayerStats> players)
	{
		if (players.Count == 0)
		{
			_titleText.text = "Session Stats - Players";
			SetRow(0, "No player stats were recorded for this game.");
			_footerText.text = "LEFT: Overview    F1 or ESC: Close";
			return;
		}

		int pageIndex = _selectedPlayerIndex / PlayersPerPage;
		int pageCount = Mathf.CeilToInt((float)players.Count / PlayersPerPage);
		int firstPlayerIndex = pageIndex * PlayersPerPage;
		_titleText.text = "Session Stats - Players " + (pageIndex + 1) + "/" + pageCount;
		for (int rowIndex = 0; rowIndex < PlayersPerPage; rowIndex++)
		{
			int playerIndex = firstPlayerIndex + rowIndex;
			if (playerIndex >= players.Count)
			{
				continue;
			}
			PlayerStats player = players[playerIndex];
			bool selected = playerIndex == _selectedPlayerIndex;
			_playerRowBackgrounds[rowIndex].gameObject.SetActive(true);
			_playerRowBackgrounds[rowIndex].color = selected ? new Color(0.16f, 0.42f, 0.54f, 0.95f) : new Color(0.11f, 0.16f, 0.19f, 0.9f);
			string outcome = player.Victorious ? "VICTORY" : "DEFEAT";
			string role = DisplayValue(player.MainRoleInitial);
			if (!string.IsNullOrEmpty(player.Power))
			{
				role += " / " + player.Power;
			}
			string status = string.IsNullOrEmpty(player.DeathType) ? "Survived" : "Died " + DisplayValue(player.DeathTiming) + ": " + DisplayValue(player.DeathType);
			SetRow(rowIndex, (selected ? "> " : "  ") + Shorten(DisplayValue(player.Username), 28) + "  " + outcome + "\n  " + Shorten(role, 62) + "\n  " + Shorten(status, 70), 15f);
			SetPlayerRowLayout(rowIndex);
		}
		_footerText.text = "UP/DOWN: Select    ENTER: Details    LEFT: Overview    F1 or ESC: Close";
	}

	private void RenderPlayerDetails(List<PlayerStats> players)
	{
		if (players.Count == 0)
		{
			_view = StatsView.Players;
			RenderPlayers(players);
			return;
		}

		PlayerStats player = players[_selectedPlayerIndex];
		if (_detailSection == 0)
		{
			_titleText.text = "Player: " + Shorten(DisplayValue(player.Username), 34) + " - Summary";
			SetRow(0, "Outcome: " + (player.Victorious ? "Victory" : "Defeat") + "    Loot collected: " + player.TotalCollectedLoot);
			SetRow(1, "Initial role: " + DisplayValue(player.MainRoleInitial));
			SetRow(2, "Power: " + DisplayValue(player.Power));
			SetRow(3, "Secondary role: " + DisplayValue(player.SecondaryRole));
			SetRow(4, "Role changes: " + FormatRoleChanges(player.MainRoleChanges));
			SetRow(5, "Death: " + FormatDeath(player));
			SetRow(6, "Killer: " + DisplayValue(player.KillerName));
			SetRow(7, "Color: " + DisplayValue(player.Color));
		}
		else
		{
			_titleText.text = "Player: " + Shorten(DisplayValue(player.Username), 34) + " - Activity";
			SetRow(0, "Talked outside meetings: " + FormatDuration(player.SecondsTalkedOutsideMeeting));
			SetRow(1, "Talked during meetings: " + FormatDuration(player.SecondsTalkedDuringMeeting));
			SetRow(2, "Standing still: " + FormatDuration(player.SecondsSpentImmobileStanding));
			SetRow(3, "Crouched still: " + FormatDuration(player.SecondsSpentImmobileCrouched));
			SetRow(4, "Walking: " + FormatDuration(player.SecondsSpentWalkingStanding));
			SetRow(5, "Walking crouched: " + FormatDuration(player.SecondsSpentWalkingCrouched));
			SetRow(6, "Running: " + FormatDuration(player.SecondsSpentRunning));
			SetRow(7, "Recorded votes: " + (player.Votes == null ? 0 : player.Votes.Count) + "    Actions: " + (player.Actions == null ? 0 : player.Actions.Count));
		}
		_footerText.text = "LEFT/RIGHT: Summary/Activity    ESC or BACKSPACE: Players    F1: Close";
	}

	private void SetRow(int rowIndex, string content, float fontSize = 18f)
	{
		TextMeshProUGUI row = _contentRows[rowIndex];
		float top = -76f - rowIndex * 39f;
		RectTransform rowTransform = row.GetComponent<RectTransform>();
		rowTransform.offsetMin = new Vector2(28f, top - 35f);
		rowTransform.offsetMax = new Vector2(-28f, top);
		row.fontSize = fontSize;
		row.text = content;
		row.gameObject.SetActive(true);
	}

	private void SetPlayerRowLayout(int rowIndex)
	{
		float top = -72f - rowIndex * 61f;
		RectTransform rowTransform = _contentRows[rowIndex].GetComponent<RectTransform>();
		rowTransform.offsetMin = new Vector2(28f, top - 52f);
		rowTransform.offsetMax = new Vector2(-28f, top);
	}

	private static List<PlayerStats> GetPlayers()
	{
		GameStats game = SessionStats.Stats == null ? null : SessionStats.Stats.CurrentGame;
		return game == null || game.PlayerStats == null ? new List<PlayerStats>() : game.PlayerStats;
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

	private static string DisplayValue(string value)
	{
		return string.IsNullOrEmpty(value) ? "Not recorded" : Shorten(value, 68);
	}

	private static string Shorten(string value, int maximumLength)
	{
		if (string.IsNullOrEmpty(value) || value.Length <= maximumLength)
		{
			return value;
		}
		return value.Substring(0, maximumLength - 3) + "...";
	}

	private static string FormatDuration(int totalSeconds)
	{
		TimeSpan duration = TimeSpan.FromSeconds(Mathf.Max(0, totalSeconds));
		return duration.TotalHours >= 1d ? duration.ToString("h\\:mm\\:ss") : duration.ToString("m\\:ss");
	}

	private static string FormatRoleChanges(List<PlayerStats.MainRoleChangeEvent> roleChanges)
	{
		if (roleChanges == null || roleChanges.Count == 0)
		{
			return "None";
		}
		return Shorten(string.Join(" -> ", roleChanges.Select((PlayerStats.MainRoleChangeEvent change) => change.NewMainRole)), 62);
	}

	private static string FormatDeath(PlayerStats player)
	{
		return string.IsNullOrEmpty(player.DeathType) ? "Survived" : DisplayValue(player.DeathTiming) + " - " + DisplayValue(player.DeathType);
	}
}