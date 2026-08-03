using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Speedchoice {
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	public class Speedchoice : BaseUnityPlugin {
		public const string PluginGUID = "com.lukkster.Speedchoice";
		public const string PluginName = "Speedchoice";
		public const string PluginVersion = "0.0.1";
		private readonly Harmony harmony = new(PluginGUID);
		private static Speedchoice speedchoice;
		private void Start() {
			harmony.PatchAll();
			speedchoice = this;
		}

		private struct Settings {
			// Ranges
			public float boatSpeed = 0;
			public float timeToRest = 0;
			public float trophyOdds = 0;
			// Checkboxes
			public bool bossReveals = false;
			public bool bossSkills = false;
			public bool cheatDeath = false;
			public bool dropMaterials = false; // TODO - Doesn't work on drag-n-drop.
			public bool fastCrops = false;
			public bool fastFermenters = false;
			public bool harmlessPieces = false;
			public bool increasedExp = false;
			public bool instantUpgrades = false;
			public bool lootlessBosses = false;
			public bool noBuildStations = false;
			public bool noCraftCost = false;
			public bool noCraftLevels = false;
			public bool showDeaths = false;
			public bool showLogouts = false;
			public bool showTimer = false;
			public bool structureLoot = false;
			public bool wherePortal = false;
			// Secret Settings
			public bool alwaysRocky = false;
			public float runSpeed = 1;
			public float jumpForce = 1;
			public float sailForce = 1;
			public bool unlockPieces = false;
			public bool unlockRecipes = false;
			// Timer
			public int time = 0;
			public bool isTimerRunning = false;

			public Settings() {
			}
		}
		private static Settings settings = new();

		// UI and save data
		#region Settings UI: FejdStartup.OnServerOptions, FejdStartup.OnServerOptionsDone, ServerOptionsGUI.OnPresetButton
		// To avoid class-conflicts, we're going with "profiles", "ranges", and "checkboxes" for "presets", "sliders", and "toggles" respectively. 
		private struct Profile {
			public List<string> keys;
			public string toolTip;
			public Dictionary<string, float> ranges;
			public Dictionary<string, bool> checkBoxes;

			public Profile(List<string> keys, string toolTip, Dictionary<string, float> ranges, Dictionary<string, bool> checkBoxes) {
				this.keys = keys;
				this.toolTip = toolTip;
				this.ranges = ranges;
				this.checkBoxes = checkBoxes;
			}
		}
		private readonly static Dictionary<string, Profile> profiles = new() {{
				"Speedchoice", new Profile(new List<string>{"eventrate 0", "teleportall", "nobuildcost"},
				"Recommended Speedchoice settings. Faster gameplay, no grinding required, and Boss Reveals make the whole game completable in one sitting.",
				new Dictionary<string, float>{
					{ "Boat Speed", 2 },
					{ "Time to Rest", 1 },
					{ "Trophy Odds", 1 }
				},
				new Dictionary<string, bool>{
					{ "Boss Reveals", true },
					{ "Boss Skills", true },
					{ "Cheat Death", true },
					{ "Drop Materials", true },
					{ "Fast Crops", true },
					{ "Fast Fermenters", true },
					{ "Harmless Pieces", true },
					{ "Increased Exp", false },
					{ "Instant Upgrades", true },
					{ "Lootless Bosses", false },
					{ "No Build Stations", true },
					{ "No Craft Cost", true },
					{ "No Craft Levels", false },
					{ "Show Deaths", true },
					{ "Show Logouts", true },
					{ "Show Timer", true },
					{ "Structure Loot", true },
					{ "Where's my Portal?", true }
				}
			)}, {
				"Trailblazer", new Profile(new List<string>{"deathkeepequip", "skillreductionrate 15", "resourcerate 200", "eventrate 0", "teleportall", "nobuildcost"},
				"OatHorse's Trailblazer settings, without the trophy hunt aspect.",
				new Dictionary<string, float>{
					{ "Boat Speed", 2 },
					{ "Time to Rest", 1 },
					{ "Trophy Odds", 2 },
				},
				new Dictionary<string, bool>{
					{ "Boss Reveals", true },
					{ "Boss Skills", true },
					{ "Cheat Death", true },
					{ "Drop Materials", false },
					{ "Fast Crops", true },
					{ "Fast Fermenters", true },
					{ "Harmless Pieces", false },
					{ "Increased Exp", true },
					{ "Instant Upgrades", false },
					{ "Lootless Bosses", false },
					{ "No Build Stations", true },
					{ "No Craft Cost", true },
					{ "No Craft Levels", true },
					{ "Show Deaths", true },
					{ "Show Logouts", true },
					{ "Show Timer", true },
					{ "Structure Loot", true },
					{ "Where's my Portal?", true }
				}
			)}, {
				"Blazing", new Profile(new List<string>{"eventrate 0", "teleportall", "nobuildcost"},
				"Accelerated version of a Reverse Boss playthrough. Defeat the bosses in order, and get rewarded in Skill Levels. However, the bosses have no drops. Thus no Forsaken Powers, or Moder's Tears for late game crafting.",
				new Dictionary<string, float>{
					{ "Boat Speed", 2 },
					{ "Time to Rest", 2 },
					{ "Trophy Odds", 1 },
				},
				new Dictionary<string, bool>{
					{ "Boss Reveals", true },
					{ "Boss Skills", true },
					{ "Cheat Death", true },
					{ "Drop Materials", true },
					{ "Fast Crops", true },
					{ "Fast Fermenters", true },
					{ "Harmless Pieces", true },
					{ "Increased Exp", false },
					{ "Instant Upgrades", true },
					{ "Lootless Bosses", true },
					{ "No Build Stations", true },
					{ "No Craft Cost", true },
					{ "No Craft Levels", false },
					{ "Show Deaths", true },
					{ "Show Logouts", true },
					{ "Show Timer", true },
					{ "Structure Loot", true },
					{ "Where's my Portal?", true }
				}
			)}
		};
		private struct RangeOption {
			public string name;
			public string toolTip;
			public RangeOption(string name, string toolTip) {
				this.name = name;
				this.toolTip = toolTip;
			}
		}
		private struct Range {
			public Func<float> Get;
			public Action<float> Set;
			public string toolTip;
			public List<RangeOption> rangeOptions;
			public Range(Func<float> getter, Action<float> setter, string toolTip, List<RangeOption> rangeOptions) {
				Get = getter;
				Set = setter;
				this.toolTip = toolTip;
				this.rangeOptions = rangeOptions;
			}
		}
		private readonly static SortedDictionary<string, Range> ranges = new() {{
				"Boat Speed", new Range(() => settings.boatSpeed, val => settings.boatSpeed = val,
				"Changes the speed at which ships sail.",
				new List<RangeOption> {
					new("Normal", "Boats travel normally."),
					new("Fast", "Boats are 2.5 times faster, 2 times faster while paddling."),
					new("Dangerous", "Boats are 10 times faster, 8 times faster while paddling."),
					new("Capsize", "Boats travel ... faster.")
				}
			)}, {
				"Time to Rest", new Range(() => settings.timeToRest, val => settings.timeToRest = val,
				"Adjusts how long it takes to become rested.",
				new List<RangeOption> {
					new("Normal", "The default of 20 seconds."),
					new("Fast", "Spend 10 seconds to become rested."),
					new("Instant", "Resting happens instantly.")
				}
			)}, {
				"Trophy Odds", new Range(() => settings.trophyOdds, val => settings.trophyOdds = val,
				"Adds an extra reroll for trophy drops to all enemies.",
				new List<RangeOption> {
					new("Normal", "Vanilla drop rates."),
					new("Often", "Trophies drop 50%, multiplicably, more often. Thus a Deer trophy would have 75% drop rate, boosted from it's vanilla 50%. A Deathsquito trophy would have 52.5%, from 5%."),
					new("Always", "All creatures that can drop trophies, will drop trophies.")
				}
			)}
		};
		private struct CheckBox {
			public Func<bool> Get;
			public Action<bool> Set;
			public string toolTip;

			public CheckBox(Func<bool> getter, Action<bool> setter, string toolTip) {
				Get = getter;
				Set = setter;
				this.toolTip = toolTip;
			}
		}
		private readonly static Dictionary<string, CheckBox> checkBoxes = new() {{
				"Boss Reveals", new CheckBox(() => settings.bossReveals, val => settings.bossReveals = val,
				"Upon defeating a Forsaken, the next Forsaken's locations are revealed."
			)}, {
				"Boss Skills", new CheckBox(() => settings.bossSkills, val => settings.bossSkills = val,
				"Upon defeating a Forsaken, all skills will be leveled up."
			)}, {
				"Lootless Bosses", new CheckBox(() => settings.lootlessBosses, val => settings.lootlessBosses = val,
				"Forsaken do not drop items, including both their Trophies, and their \"progressive\" materials."
			)}, {
				"Cheat Death", new CheckBox(() => settings.cheatDeath, val => settings.cheatDeath = val,
				"If one logs out shortly after dying, they'll log in where they died rather than their spawn point."
			)}, {
				"Drop Materials", new CheckBox(() => settings.dropMaterials, val => settings.dropMaterials = val,
				"Materials, items used for crafting exclusively, are immediately removed from the inventory."
			)}, {
				"Harmless Pieces", new CheckBox(() => settings.harmlessPieces, val => settings.harmlessPieces = val,
				"Player built structures, such as campfires or spikes, do no damage to creatures."
			)}, {
				"Fast Crops", new CheckBox(() => settings.fastCrops, val => settings.fastCrops = val,
				"Crops grow quickly."
			)}, {
				"Fast Fermenters", new CheckBox(() => settings.fastFermenters, val => settings.fastFermenters = val,
				"Fermenters brew quickly."
			)}, {
				"Increased Exp", new CheckBox(() => settings.increasedExp, val => settings.increasedExp = val,
				"Increases Skill experience gains by 500%."
			)}, {
				"Instant Upgrades", new CheckBox(() => settings.instantUpgrades, val => settings.instantUpgrades = val,
				"Instantly upgrades any upgradeable tool or armor."
			)}, {
				"Structure Loot", new CheckBox(() => settings.structureLoot, val => settings.structureLoot = val,
				"Naturally occurring structures, such as houses, will drop into materials when deconstructed with a hammer in \"No Build Cost\"."
			)}, {
				"Where's my Portal?", new CheckBox(() => settings.wherePortal, val => settings.wherePortal = val,
				"Adds a portal pin to the map upon placing a portal."
			)}, {
				"No Build Stations", new CheckBox(() => settings.noBuildStations, val => settings.noBuildStations = val,
				"Player built pieces can be constructed without the required nearby crafting stations. For instance a Portal can be built without a nearby Workbench."
			)}, {
				"No Craft Cost", new CheckBox(() => settings.noCraftCost, val => settings.noCraftCost = val,
				"Items can be crafted without consuming or requiring the materials to do so."
			)}, {
				"No Craft Levels", new CheckBox(() => settings.noCraftLevels, val => settings.noCraftLevels = val,
				"Items can be crafted without the prerequisite Crafting Station Level to do so."
			)}, {
				"Show Deaths", new CheckBox(() => settings.showDeaths, val => settings.showDeaths = val,
				"Adds an UI element showing the number of Deaths."
			)}, {
				"Show Logouts", new CheckBox(() => settings.showLogouts, val => settings.showLogouts = val,
				"Adds an UI element showing the number of Logouts."
			)}, {
				"Show Timer", new CheckBox(() => settings.showTimer, val => settings.showTimer = val,
				"Adds an UI element showing how long there has been activity in the world. Starts on first input, and pauses when the game pauses."
			)}
		};

		[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnServerOptions))]
		private class FejdStartup_OnServerOptions {
			private static float profileSep;
			private static float rangeSep;
			private static float checkBoxSep;
			private static GameObject serverOptions;

			private static void Postfix(FejdStartup __instance) {
				if (GameObject.Find(checkBoxes.Keys.First()) == null) {
					serverOptions = GameObject.Find("StartGui_ServerOptions");
					profileSep = GetSeperation("Presets/Easy", "Presets/Casual");
					rangeSep = GetSeperation("Modifiers/Combat", "Modifiers/Death");
					checkBoxSep = GetSeperation("Modifiers/NoBuildCost", "Modifiers/Passivemobs");

					int row = 1;
					int col = 0;
					foreach (KeyValuePair<string, Profile> entry in profiles) {
						col++;
						if (col > 3) {
							col = 1;
							row++;
						}
						AddProfile(row, col, entry);
					}
					float expandPanel = row * profileSep;
					string[] belowPresets = { "Presets/Default", "Modifiers" };
					foreach (string objectId in belowPresets) {
						MoveDown(objectId, expandPanel);
					}

					row = 0;
					foreach (KeyValuePair<string, Range> entry in ranges) {
						row++;
						AddRange(row, entry);
					}
					string[] belowSliders = { "Modifiers/NoBuildCost", "Modifiers/PlayerBasedEvents", "Modifiers/Fire", "Modifiers/Passivemobs", "Modifiers/Nomap" };
					foreach (string objectId in belowSliders) {
						MoveDown(objectId, row * rangeSep);
					}
					expandPanel += row * rangeSep;

					row = 2;
					col = 0;
					foreach (KeyValuePair<string, CheckBox> entry in checkBoxes) {
						col++;
						if (col > 3) {
							col = 1;
							row++;
						}
						AddCheckBox(row, col, entry);
					}

					ExpandPanel(expandPanel + (row - 1) * checkBoxSep);
				}

				Load(__instance.m_world);
				foreach (KeyValuePair<string, Range> entry in ranges) {
					GameObject.Find(entry.Key).GetComponentInChildren<Slider>().value = entry.Value.Get();
				}
				foreach (KeyValuePair<string, CheckBox> entry in checkBoxes) {
					GameObject.Find(entry.Key).GetComponentInChildren<Toggle>().isOn = entry.Value.Get();
				}
			}
			private static float GetSeperation(string a, string b) {
				Transform aTransform = serverOptions.transform.Find("panel/" + a);
				Transform bTransform = serverOptions.transform.Find("panel/" + b);
				float aY = aTransform.GetComponent<RectTransform>().anchoredPosition.y;
				float bY = bTransform.GetComponent<RectTransform>().anchoredPosition.y;
				return Math.Abs(aY - bY);
			}
			private static void AddProfile(int row, int col, KeyValuePair<string, Profile> entry) {
				Transform presets = serverOptions.transform.Find("panel/Presets");
				Transform copyFrom;
				if (col == 1) {
					copyFrom = presets.transform.Find("Casual");
				}
				else if (col == 2) {
					copyFrom = presets.transform.Find("Hammer");
				}
				else {
					copyFrom = presets.transform.Find("Immersive");
				}
				GameObject newPreset = UnityEngine.Object.Instantiate(copyFrom.gameObject, presets);
				TMP_Text newText = newPreset.transform.Find("Text")?.GetComponent<TMP_Text>();
				newText.text = entry.Key;
				newPreset.name = entry.Key;
				KeyButton keybutton = newPreset?.GetComponent<KeyButton>();
				keybutton.m_keys = entry.Value.keys;
				keybutton.m_preset = WorldPresets.Default;
				keybutton.m_toolTip = entry.Value.toolTip;
				MoveDown("Presets/" + entry.Key, profileSep * row);
			}
			private static void AddRange(int row, KeyValuePair<string, Range> entry) {
				Transform modifiers = serverOptions.transform.Find("panel/Modifiers");
				Transform copyFrom = modifiers.transform.Find("Portals");
				GameObject newSlider = UnityEngine.Object.Instantiate(copyFrom.gameObject, modifiers);
				TextMeshProUGUI newText = newSlider.transform.Find("label")?.GetComponent<TextMeshProUGUI>();
				newText.text = entry.Key;
				newSlider.name = entry.Key;
				MoveDown("Modifiers/" + entry.Key, rangeSep * row);
				Transform sliderTransform = newSlider.transform.Find("slider");
				Slider slider = sliderTransform?.GetComponent<Slider>();
				slider.maxValue = entry.Value.rangeOptions.Count() - 1;
				slider.value = 0;
				KeySlider keySlider = sliderTransform?.GetComponent<KeySlider>();
				keySlider.m_toolTip = entry.Value.toolTip;
				keySlider.m_settings = new List<KeySlider.SliderSetting>();
				foreach (RangeOption rangeOption in entry.Value.rangeOptions) {
					keySlider.m_settings.Add(new() {
						m_name = rangeOption.name,
						m_toolTip = rangeOption.toolTip
					});
				}
			}
			private static void AddCheckBox(int row, int col, KeyValuePair<string, CheckBox> entry) {
				Transform modifiers = serverOptions.transform.Find("panel/Modifiers");
				Transform copyFrom;
				if (col == 1) {
					copyFrom = modifiers.transform.Find("NoBuildCost");
				}
				else if (col == 2) {
					copyFrom = modifiers.transform.Find("PlayerBasedEvents");
				}
				else {
					copyFrom = modifiers.transform.Find("Fire");
				}
				GameObject newToggle = UnityEngine.Object.Instantiate(copyFrom.gameObject, modifiers);
				TextMeshProUGUI newText = newToggle.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
				newText.text = entry.Key;
				newToggle.name = entry.Key;
				MoveDown("Modifiers/" + entry.Key, checkBoxSep * row);
				newToggle.GetComponent<KeyToggle>().m_toolTip = entry.Value.toolTip;
			}
			private static void MoveDown(string objectId, float distance) {
				GameObject gameObject = GameObject.Find("GuiRoot/GUI/StartGui/StartGame/StartGui_ServerOptions/panel/" + objectId);
				RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
				rectTransform.anchoredPosition -= new Vector2(0, distance);
			}
			private static void ExpandPanel(float distance, int max = -10, int min = -1070) {
				Transform panel = serverOptions.transform.Find("panel");
				RectTransform rectTransform = panel.GetComponent<RectTransform>();
				rectTransform.offsetMax += new Vector2(0, distance / 2);
				float offset = 0;
				if (rectTransform.offsetMax.y > max) {
					offset = rectTransform.offsetMax.y - max;
					rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, max);
				}
				rectTransform.offsetMin -= new Vector2(0, (distance / 2) + offset);
				if (rectTransform.offsetMin.y < min) {
					offset = min - rectTransform.offsetMin.y;
					rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, min);
				}
				MoveDown("Modifiers", (-1 * distance) - (offset / 2));
			}
		}

		[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnServerOptionsDone))]
		private class FejdStartup_OnServerOptionsDone {
			[HarmonyPrefix]
			private static void Prefix(FejdStartup __instance) {
				Boolean isUpdated = false;
				foreach (KeyValuePair<string, Range> entry in ranges) {
					Assign(entry, ref isUpdated);
				}
				foreach (KeyValuePair<string, CheckBox> entry in checkBoxes) {
					Assign(entry, ref isUpdated);
				}
				if (isUpdated) {
					Save(__instance.m_world);
				}
			}
			private static void Assign(KeyValuePair<string, Range> entry, ref Boolean isUpdated) {
				float value = GameObject.Find(entry.Key).GetComponentInChildren<Slider>().value;
				isUpdated = isUpdated || (value != entry.Value.Get());
				entry.Value.Set(value);
			}
			private static void Assign(KeyValuePair<string, CheckBox> entry, ref Boolean isUpdated) {
				Boolean isOn = GameObject.Find(entry.Key).GetComponentInChildren<Toggle>().isOn;
				isUpdated = isUpdated || (isOn != entry.Value.Get());
				entry.Value.Set(isOn);
			}
		}

		[HarmonyPatch(typeof(ServerOptionsGUI), nameof(ServerOptionsGUI.OnPresetButton))]
		private class ServerOptionsGUI_OnPresetButton {
			[HarmonyPrefix]
			private static void Postfix(KeyButton button) {
				foreach (KeyValuePair<string, Range> entry in ranges) {
					GameObject.Find(entry.Key).GetComponentInChildren<Slider>().value = 0;
				}
				foreach (KeyValuePair<string, CheckBox> entry in checkBoxes) {
					GameObject.Find(entry.Key).GetComponentInChildren<Toggle>().isOn = false;
				}
				if (profiles.TryGetValue(button.GetName(), out Profile profile)) {
					foreach (KeyValuePair<string, float> entry in profile.ranges) {
						GameObject.Find(entry.Key).GetComponentInChildren<Slider>().value = entry.Value;
					}
					foreach (KeyValuePair<string, bool> entry in profile.checkBoxes) {
						GameObject.Find(entry.Key).GetComponentInChildren<Toggle>().isOn = entry.Value;
					}
				}
			}
		}
		#endregion
		#region save, load, harmlessStructures, noBuildStations, unlockPieces: Game.Start
		private static string GetSavePath(World world) {
			string baseDir = Path.Combine(Utils.GetSaveDataPath(FileHelpers.FileSource.Local), "worlds");
			return Path.Combine(baseDir, $"{world.m_fileName}_speedchoice.json");
		}
		private static string FromSettings() {
			return JsonConvert.SerializeObject(settings, Formatting.Indented);
		}
		private static void ToSettings(String json) {
			settings = JsonConvert.DeserializeObject<Settings>(json);
		}
		private static void Save(World world) {
			File.WriteAllText(GetSavePath(world), FromSettings());
		}
		private static void Load(World world) {
			if (File.Exists(GetSavePath(world))) {
				ToSettings(File.ReadAllText(GetSavePath(world)));
			}
			else {
				Debug.LogWarning("No save data found, defaulting");
				settings = new();
			}
		}
		#endregion
		#region ZRoutedRpc Registers, harmlessStructures, noBuildStations, unlockPieces: ZoneSystem_Start
		private struct Prefab {
			public string name;
			public string aoe;
			public HitData.DamageTypes damage;

			public Prefab(string name, string aoe, HitData.DamageTypes damage) {
				this.name = name;
				this.aoe = aoe;
				this.damage = damage;
			}
		}
		private static readonly List<Prefab> harmingPrefabs = new() {
            // Stakes
            new("piece_sharpstakes", "HIT AREA",  new() { m_pierce = 15 }),
			new("piece_dvergr_sharpstakes", "Colliders/HIT AREA",  new() { m_pierce = 15 }),
			new("piece_stakewall_blackwood", "HIT AREA",  new() { m_pierce = 120 }),
            // Fires
            new("piece_brazierfloor02", "_enabled_high/FireBurn",  new() { m_fire = 10 }),
			new("bonfire", "_enabled/FireBurn",  new() { m_fire = 20 }),
			new("fire_pit", "_enabled_high/FireBurn",  new() { m_fire = 10 }),
			new("piece_brazierceiling01", "_enabled_high/FireBurn",  new() { m_fire = 10 }),
			new("hearth", "_enabled_high/FireBurn",  new() { m_fire = 10 }),
			new("fire_pit_iron", "_enabled_high/FireBurn",  new() { m_fire = 10 }),
			new("piece_brazierfloor01", "_enabled_high/FireBurn",  new() { m_fire = 10 }),
            // Snap Trap
            new("piece_trap_troll", "Damage Area",  new() { m_blunt = 50, m_pierce = 50, m_chop = 50 })
		};
		[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
		private class ZoneSystem_Start {
			private static void Postfix() {
				if (ZNet.instance.IsServer()) {
					Load(ZNet.m_world);

					// harmlessStructures
					if (settings.harmlessPieces) {
						foreach (Prefab prefab in harmingPrefabs) {
							ZNetScene.instance.GetPrefab(prefab.name).transform.Find(prefab.aoe).GetComponent<Aoe>().m_damage = new();
						}
					}
					else {
						foreach (Prefab prefab in harmingPrefabs) {
							ZNetScene.instance.GetPrefab(prefab.name).transform.Find(prefab.aoe).GetComponent<Aoe>().m_damage = prefab.damage;
						}
					}

					// noBuildStations
					if (settings.noBuildStations) {
						ZoneSystem.instance.SetGlobalKey(GlobalKeys.NoWorkbench);
					}
					else {
						ZoneSystem.instance.RemoveGlobalKey(GlobalKeys.NoWorkbench);
					}

					// unlockPieces
					if (settings.unlockPieces) {
						ZoneSystem.instance.SetGlobalKey(GlobalKeys.AllPiecesUnlocked);
					}
					else {
						ZoneSystem.instance.RemoveGlobalKey(GlobalKeys.AllPiecesUnlocked);
					}
				}
				ZRoutedRpc.instance.Register<string>("FromServer", FromServer);
				ZRoutedRpc.instance.Register<string>("StartTimer", StartTimer);
			}
			private static void FromServer(long sender, string json) {
				ToSettings(json);
				if (settings.isTimerRunning) {
					speedchoice.StartCoroutine(TimerUpdate());
				}
			}
			private static void StartTimer(long sender, string json) {
				if (!settings.isTimerRunning) {
					settings.isTimerRunning = true;
					speedchoice.StartCoroutine(TimerUpdate());
				}
			}
		}
		#endregion
		#region server connections: ZRoutedRpc_AddPeer, ZRoutedRpc_RemovePeer
		[HarmonyPatch(typeof(ZRoutedRpc), nameof(ZRoutedRpc.AddPeer))]
		private class ZRoutedRpc_AddPeer {
			private static void Postfix(ZRoutedRpc __instance, ZNetPeer peer) {
				if (__instance.m_server) {
					__instance.InvokeRoutedRPC(peer.m_uid, "FromServer", FromSettings());
				}
			}
		}
		[HarmonyPatch(typeof(ZRoutedRpc), nameof(ZRoutedRpc.RemovePeer))]
		private class ZRoutedRpc_RemovePeer {
			private static void Postfix(ZRoutedRpc __instance) {
				if (__instance.m_server && __instance.m_peers.Count < 1) {
					speedchoice.StopAllCoroutines();
					settings.isTimerRunning = false;
				}
			}
		}
		#endregion

		// Ranges
		#region boatSpeed: Ship.GetSailForce
		private readonly static Dictionary<float, float> boatSpeeds = new() {
			{ 1, 2.5f },
			{ 2, 10f },
			{ 3, 100f }
		};
		[HarmonyPatch(typeof(Ship), nameof(Ship.GetSailForce))]
		public class Ship_GetSailForce {
			private static void Postfix(ref Vector3 __result) {
				if (boatSpeeds.TryGetValue(settings.boatSpeed, out float boatSpeed)) {
					__result *= boatSpeed;
				}
				__result *= settings.sailForce;
			}
		}
		[HarmonyPatch(typeof(Ship), nameof(Ship.Awake))]
		public class Ship_Awake {
			private static void Postfix(Ship __instance) {
				if (boatSpeeds.TryGetValue(settings.boatSpeed, out float boatSpeed)) {
					__instance.m_backwardForce *= boatSpeed * 0.8f;
				}
				__instance.m_backwardForce *= settings.sailForce;
			}
		}
		#endregion
		#region timeToRest: SE_Cozy.Setup
		private readonly static Dictionary<float, float> restDelays = new() {
			{ 1, 10 },
			{ 2, 0 }
		};
		[HarmonyPatch(typeof(SE_Cozy), nameof(SE_Cozy.Setup), new[] { typeof(Character) })]
		public static class SE_Cozy_Setup {
			static void Postfix(SE_Cozy __instance) {
				if (restDelays.TryGetValue(settings.timeToRest, out float delay)) {
					__instance.m_delay = delay;
				}
			}
		}
		#endregion
		#region trophyOdds, lootlessBosses: CharacterDrop.GenerateDropList, Trader.GetAvailableItems
		private readonly static Dictionary<string, string> trophies = new() {
			{ "$enemy_abomination", "TrophyAbomination" },
			{ "$enemy_asksvin", "TrophyAsksvin" },
			{ "$enemy_bjorn", "TrophyBjorn" },
			{ "$enemy_blob", "TrophyBlob" },
			{ "$enemy_blobelite", "TrophyBlob" },
			{ "$enemy_blobtar", "TrophyGrowth" },
			{ "$enemy_boar", "TrophyBoar" },
			{ "$enemy_bonemass", "TrophyBonemass" },
			{ "$enemy_bonemawserpent", "TrophyBonemawSerpent" },
			{ "$enemy_charred_archer", "TrophyCharredArcher" },
			{ "$enemy_charred_mage", "TrophyCharredMage" },
			{ "$enemy_charred_melee", "TrophyCharredMelee" },
			{ "$enemy_deathsquito", "TrophyDeathsquito" },
			{ "$enemy_deer", "TrophyDeer" },
			{ "$enemy_dragon", "TrophyDragonQueen" },
			{ "$enemy_drake", "TrophyHatchling" },
			{ "$enemy_draugr", "TrophyDraugr" },
			{ "$enemy_draugrelite", "TrophyDraugrElite" },
			{ "$enemy_dvergr", "TrophyDvergr" },
			{ "$enemy_dvergr_mage", "TrophyDvergr" },
			{ "$enemy_eikthyr", "TrophyEikthyr" },
			{ "$enemy_fader", "TrophyFader" },
			{ "$enemy_fallenvalkyrie", "TrophyFallenValkyrie" },
			{ "$enemy_fenring", "TrophyFenring" },
			{ "$enemy_fenringcultist", "TrophyCultist" },
			{ "$enemy_fenringcultist_hildir", "TrophyCultist_Hildir" },
			{ "$enemy_gdking", "TrophyTheElder" },
			{ "$enemy_ghost", "TrophyGhost" },
			{ "$enemy_gjall", "TrophyGjall" },
			{ "$enemy_goblin", "TrophyGoblin" },
			{ "$enemy_goblin_hildir", "TrophyGoblinBruteBrosShaman" },
			{ "$enemy_goblinbrute", "TrophyGoblinBrute" },
			{ "$enemy_goblinbrute_hildircombined", "TrophyGoblinBruteBrosBrute" },
			{ "$enemy_goblinking", "TrophyGoblinKing" },
			{ "$enemy_goblinshaman", "TrophyGoblinShaman" },
			{ "$enemy_greydwarf", "TrophyGreydwarf" },
			{ "$enemy_greydwarfbrute", "TrophyGreydwarfBrute" },
			{ "$enemy_greydwarfshaman", "TrophyGreydwarfShaman" },
			{ "$enemy_hare", "TrophyHare" },
			{ "$enemy_kvastur", "TrophyKvastur" },
			{ "$enemy_leech", "TrophyLeech" },
			{ "$enemy_lox", "TrophyLox" },
			{ "$enemy_morgen", "TrophyMorgen" },
			{ "$enemy_neck", "TrophyNeck" },
			{ "$enemy_seeker", "TrophySeeker" },
			{ "$enemy_seekerbrute", "TrophySeekerBrute" },
			{ "$enemy_seekerqueen", "TrophySeekerQueen" },
			{ "$enemy_serpent", "TrophySerpent" },
			{ "$enemy_skeleton", "TrophySkeleton" },
			{ "$enemy_skeletonfire", "TrophySkeletonHildir" },
			{ "$enemy_skeletonpoison", "TrophySkeletonPoison" },
			{ "$enemy_stonegolem", "TrophySGolem" },
			{ "$enemy_surtling", "TrophySurtling" },
			{ "$enemy_thehive,", "TrophyHatchling" },
			{ "$enemy_tick", "TrophyTick" },
			{ "$enemy_troll", "TrophyFrostTroll" },
			{ "$enemy_ulv", "TrophyUlv" },
			{ "$enemy_unbjorn", "TrophyBjornUndead" },
			{ "$enemy_volture", "TrophyVolture" },
			{ "$enemy_wolf", "TrophyWolf" },
			{ "$enemy_wraith", "TrophyWraith" }
		};
		private readonly static Dictionary<float, float> trophyOdds = new() {
			{ 0f, -1f },
			{ 1f, 0.5f },
			{ 2f, 1f }
		};
		[HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
		private class CharacterDrop_GenerateDropList {
			private static void Postfix(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result) {
				// trophyOdds
				if (trophyOdds.TryGetValue(settings.trophyOdds, out float percent) && trophies.TryGetValue(__instance.GetComponent<Character>().m_name, out String trophy)) {
					bool gotTrophy = false;
					foreach (KeyValuePair<GameObject, int> droppedItem in __result) {
						if (trophy == droppedItem.Key.name) {
							gotTrophy = true;
						}
					}
					if (!gotTrophy && UnityEngine.Random.value <= percent) {
						CharacterDrop.Drop trophyDrop = __instance.m_drops.Find(drop => drop.m_prefab.name == trophy);
						__result.Add(new KeyValuePair<GameObject, int>(trophyDrop.m_prefab, 1));
					}
				}
				// lootlessBosses
				if (settings.lootlessBosses && levels.ContainsKey(__instance.GetComponent<Character>().m_name)) {
					__result = new List<KeyValuePair<GameObject, int>>();
				}
			}
		}
		[HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
		private class Trader_GetAvailableItems {
			private static bool Prefix(Trader __instance, ref List<Trader.TradeItem> __result) {
				if (settings.lootlessBosses) {
					__result = new();
					foreach (Trader.TradeItem item in __instance.m_items) {
						if (string.IsNullOrEmpty(item.m_requiredGlobalKey)) {
							__result.Add(item);
						}
					}
					return false;
				}
				return true;
			}
		}
		#endregion

		// Checkboxes
		#region bossReveals, bossSkills, increasedExp: Character.OnDeath, Skill.Raise
		private struct Poi {
			public string prefabName;
			public Minimap.PinType pinType;
			public string name;
			public float radius;

			public Poi(string prefabName, Minimap.PinType pinType, string name, float radius) {
				this.prefabName = prefabName;
				this.pinType = pinType;
				this.name = name;
				this.radius = radius;
			}
		}
		private const float bossRadius = 500f;
		private const float vendorRadius = 100f;
		private readonly static Dictionary<string, List<Poi>> reveals = new() {
			{ "$enemy_eikthyr", new() { new Poi("GDKing", Minimap.PinType.Boss, "Elder", bossRadius), new Poi("Vendor_BlackForest", Minimap.PinType.Icon3, "Haldor", vendorRadius) } },
			{ "$enemy_gdking", new() { new Poi("Bonemass", Minimap.PinType.Boss, "Bonemass", bossRadius), new Poi("BogWitch_Camp", Minimap.PinType.Icon3, "BogWitch", vendorRadius) } },
			{ "$enemy_bonemass", new() { new Poi("Dragonqueen", Minimap.PinType.Boss, "Moder", bossRadius) } },
			{ "$enemy_dragon", new() { new Poi("GoblinKing", Minimap.PinType.Boss, "Yagluth", bossRadius), new Poi("Hildir_camp", Minimap.PinType.Icon3, "Hildir", vendorRadius) } },
			{ "$enemy_goblinking", new() { new Poi("Mistlands_DvergrBossEntrance1", Minimap.PinType.Boss, "The Queen", bossRadius) } },
			{ "$enemy_seekerqueen", new() { new Poi("FaderLocation", Minimap.PinType.Boss, "Fader", bossRadius), new Poi("PlaceofMystery1", Minimap.PinType.Icon3, "Mysterious Location", vendorRadius) } }
		};

		private readonly static Dictionary<string, int> levels = new() {
			{ "$enemy_fader",  100 },
			{ "$enemy_seekerqueen",  80 },
			{ "$enemy_goblinking",  60 },
			{ "$enemy_dragon",  50 },
			{ "$enemy_bonemass",  40 },
			{ "$enemy_gdking",  30 },
			{ "$enemy_eikthyr",  20 }
		};

		[HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
		private class Character_OnDeath {
			private static void Postfix(Character __instance) {
				if (settings.bossReveals && reveals.TryGetValue(__instance.m_name, out List<Poi> pois)) {
					foreach (Poi poi in pois) {
						foreach (KeyValuePair<Vector2i, ZoneSystem.LocationInstance> entry in ZoneSystem.instance.m_locationInstances) {
							if (entry.Value.m_location.m_prefabName == poi.prefabName) {
								Minimap.instance.DiscoverLocation(entry.Value.m_position, poi.pinType, poi.name, false);
								Minimap.instance.Explore(entry.Value.m_position, poi.radius);
							}
						}
					}
				}
				if (settings.bossSkills && levels.TryGetValue(__instance.m_name, out int level)) {
					foreach (KeyValuePair<Skills.SkillType, Skills.Skill> entry in Player.m_localPlayer.GetSkills().m_skillData) {
						if (entry.Value.m_level < level) {
							entry.Value.m_level = level;
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Skills.Skill), nameof(Skills.Skill.Raise))]
		private class Skills_Skill_Raise {
			private static void Prefix(ref float factor) {
				if (settings.increasedExp) {
					factor *= 6;
				}
			}
			private static void Postfix(Skills.Skill __instance, ref bool __result) {
				if (settings.bossSkills) {
					foreach (KeyValuePair<string, int> entry in levels) {
						if (ZoneSystem.instance.GetGlobalKey("defeated_" + entry.Key.Substring(7))) {
							if (__instance.m_level < entry.Value) {
								__instance.m_level = entry.Value;
								__instance.m_accumulator = 0f;
								__result = true;
							}
							return;
						}
					}
				}
			}
		}
		#endregion
		#region cheatDeath, showLogouts: Game.Shutdown
		[HarmonyPatch(typeof(Game), nameof(Game.Shutdown))]
		private class Game_Shutdown {
			private static void Prefix(Game __instance) {
				// Player.m_localPlayer can be null while in loading screens
				if (Player.m_localPlayer != null) {
					// cheatDeath
					if (settings.cheatDeath && Player.m_localPlayer.IsDead()) {
						__instance.GetPlayerProfile().SetLogoutPoint(Player.m_localPlayer.transform.position);
					}

					// showLogouts
					Dictionary<string, string> customData = Player.m_localPlayer.m_customData;
					int logoutCount = 1;
					if (customData.TryGetValue(logoutsKey, out string savedLogouts)) {
						Int32.TryParse(savedLogouts, out logoutCount);
						logoutCount++;
					}
					customData[logoutsKey] = logoutCount.ToString();
				}
				speedchoice.StopAllCoroutines();
				settings.isTimerRunning = false;
				rockyied = false;
				if (ZNet.instance.IsServer()) {
					Save(ZNet.m_world);
				}
			}
		}
		#endregion
		#region dropMaterials: Humanoid.Pickup, Inventory.AddItem
		private readonly static List<ItemDrop.ItemData.ItemType> drops = new() {
			ItemDrop.ItemData.ItemType.Material, ItemDrop.ItemData.ItemType.Trophy
		};
		private readonly static List<String> exhausts = new() {
			"$item_barberkit", "$item_barleyflour", "$item_barrelrings", "$item_dragontear", "$item_scythehandle", "$item_yagluththing"
		};
		private readonly static List<String> retains = new() {
			"$item_amber", "$item_amberpearl", "$item_asksvin_meat", "$item_barley", "$item_barleywinebase", "$item_bjorn_meat", "$item_blackcore", "$item_blackmetalscrap",
			"$item_catapult_ammo", "$item_bonefragments", "$item_bonemawmeat", "$item_breaddough", "$item_bronzescrap", "$item_bug_meat", "$item_catapult_training_ammo",
			"$item_charredbone", "$item_chicken_meat", "$item_coal", "$item_coins", "$item_copperore", "$item_copperscrap", "$item_deer_meat", "$item_eitr", "$item_finewood",
			"$item_fireworkrocket_blue", "$item_fireworkrocket_cyan", "$item_fireworkrocket_green", "$item_fireworkrocket_purple", "$item_fireworkrocket_red",
			"$item_fireworkrocket_white", "$item_fireworkrocket_yellow", "$item_fishandbreaduncooked", "$item_fish_raw", "$item_fish_raw", "$item_flametalore_old",
			"$item_flametalore", "$item_flax", "$item_hare_meat", "$item_honeyglazedchickenuncooked", "$item_ironore", "$item_ironscrap", "$item_loxmeat", "$item_loxpie_uncooked",
			"$item_magicallystuffedmushroomuncooked", "$item_meadbasebugrepellent", "$item_meadbasebzerker", "$item_meadbaseeitr_lingering", "$item_meadbaseeitr",
			"$item_meadbasefrostresist", "$item_meadbasehasty", "$item_meadbasehealth_lingering", "$item_meadbasehealth_major", "$item_meadbasehealth_medium", "$item_meadbasehealth",
			"$item_meadbaselightfoot", "$item_meadbasepoisonresist", "$item_meadbasestamina_lingering", "$item_meadbasestamina_medium", "$item_meadbasestamina",
			"$item_meadbasestrength", "$item_meadbaseswimmer", "$item_meadbasetamer", "$item_meadbasetasty", "$item_meatplatteruncooked", "$item_mistharesupremeuncooked",
			"$item_necktail", "$item_piquantpie_uncooked", "$item_boar_meat", "$item_resin", "$item_roastedcrustpie_uncooked", "$item_roundlog", "$item_ruby", "$item_sap",
			"$item_serpentmeat", "$item_silvernecklace", "$item_silverore", "$item_softtissue", "$item_surtlingcore", "$item_thunderstone", "$item_tinore", "$item_trophy_bonemass",
			"$item_trophy_deer", "$item_trophy_dragonqueen", "$item_trophy_eikthyr", "$item_trophy_fader", "$item_trophy_goblinking", "$item_trophy_seeker_brute",
			"$item_trophy_seekerqueen", "$item_trophy_elder", "$item_vikingcupcake_uncooked", "$item_volture_meat", "$item_witheredbone", "$item_wolf_meat", "$item_wood"
		};
		private static void RemoveItem(Inventory inventory, ItemDrop.ItemData itemData) {
			if (settings.dropMaterials && (drops.Contains(itemData.m_shared.m_itemType) || exhausts.Contains(itemData.m_shared.m_name)) && !retains.Contains(itemData.m_shared.m_name)) {
				inventory.RemoveItem(itemData);
			}
		}
		[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Pickup))]
		private class Humanoid_Pickup {
			private static void Postfix(Humanoid __instance, GameObject go) {
				if (__instance.IsPlayer()) {
					RemoveItem(__instance.m_inventory, go.GetComponent<ItemDrop>().m_itemData);
				}
			}
		}
		[HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
		private class Inventory_AddItem {
			private static void Postfix(Inventory __instance, ItemDrop.ItemData item) {
				if (Player.m_localPlayer != null && Player.m_localPlayer.GetInventory() == __instance) {
					RemoveItem(__instance, item);
				}
			}
		}
		[HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData), typeof(Vector2i) })]
		private class Inventory_AddItem_2 {
			private static void Postfix(Inventory __instance, ItemDrop.ItemData item) {
				if (Player.m_localPlayer != null && Player.m_localPlayer.GetInventory() == __instance) {
					RemoveItem(__instance, item);
				}
			}
		}
		[HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
		private class Inventory_AddItem_4 {
			private static void Postfix(Inventory __instance, ItemDrop.ItemData item) {
				if (Player.m_localPlayer != null && Player.m_localPlayer.GetInventory() == __instance) {
					RemoveItem(__instance, item);
				}
			}
		}
		#endregion
		#region fastCrops: Plant.TimeSincePlanted
		[HarmonyPatch(typeof(Plant), nameof(Plant.TimeSincePlanted))]
		private static class Plant_TimeSincePlanted {
			private static void Postfix(Plant __instance, ref double __result) {
				if (settings.fastCrops) {
					__result = (double) __instance.m_growTimeMax + 1;
				}
			}
		}
		#endregion
		#region fastFermenters: Fermenter.Awake
		[HarmonyPatch(typeof(Fermenter), nameof(Fermenter.Awake))]
		private static class Fermenter_Awake {
			private static void Postfix(Fermenter __instance) {
				if (settings.fastFermenters) {
					__instance.m_fermentationDuration = 10;
				}
			}
		}
		[HarmonyPatch(typeof(Fermenter), nameof(Fermenter.DelayedTap))]
		private static class Fermenter_DelayedTap {
			private static void Prefix(Fermenter __instance) {
				if (settings.fastFermenters) {
					__instance.GetItemConversion(__instance.m_delayedTapItem).m_producedItems = 10;
				}
			}
		}
		#endregion
		#region instantUpgrades: ObjectDB.Awake, InventoryGui.SetupCrafting
		private static Dictionary<string, Recipe> upgradeAbles = new();
		[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
		private class ObjectDB_Awake {
			private static void Postfix() {
				upgradeAbles.Clear();
				foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
					if (recipe.m_item != null && recipe.m_item.m_itemData.m_shared.m_maxQuality > 1) {
						upgradeAbles.Add(recipe.m_item.m_itemData.m_shared.m_name, recipe);
					}
				}
			}
		}

		private static void UpgradeInventory(Player player) {
			if (!settings.instantUpgrades) {
				return;
			}
			CraftingStation station = player.GetCurrentCraftingStation();
			if (station == null) {
				return;
			}
			foreach (ItemDrop.ItemData item in player.GetInventory().GetAllItems()) {
				if (upgradeAbles.TryGetValue(item.m_shared.m_name, out Recipe recipe)) {
					if ((recipe.m_craftingStation != null && station.m_name == recipe.m_craftingStation.m_name) ||
						(recipe.m_craftingStation == null && station.m_name == "$piece_workbench")) {
						int maxLevel = MaxLevel(station, recipe);
						if (item.m_quality < maxLevel) {
							item.m_quality = maxLevel;
							item.m_durability = item.m_shared.m_maxDurability + item.m_shared.m_durabilityPerLevel * item.m_quality;
						}
					}
				}
			}
		}
		private static int MaxLevel(CraftingStation station, Recipe recipe) {
			if (settings.noCraftLevels) {
				return 4;
			}
			int stationLevel = 1;
			if (station != null) {
				stationLevel = station.GetLevel();
			}
			int maxLevel = stationLevel - recipe.m_minStationLevel;
			if (recipe.m_minStationLevel != 0) {
				maxLevel++;
			}
			if (maxLevel > 4) {
				maxLevel = 4;
			}
			return maxLevel;
		}
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupCrafting))]
		private class InventoryGui_SetupCrafting {
			private static void Prefix() {
				UpgradeInventory(Player.m_localPlayer);
			}
		}
		#endregion
		#region noBuildStations: Hud.SetUpPieceInfo
		[HarmonyPatch(typeof(Hud), nameof(Hud.SetupPieceInfo))]
		private class Hud_SetupPieceInfo {
			private static void Postfix(Hud __instance, Piece piece) {
				if (settings.noBuildStations) {
					GameObject obj = __instance.m_requirementItems[piece.m_resources.Length];
					Image component = obj.transform.Find("res_icon").GetComponent<Image>();
					TMP_Text component3 = obj.transform.Find("res_amount").GetComponent<TMP_Text>();
					component.color = Color.white;
					component3.text = "";
					component3.color = Color.white;
				}
			}
		}
		#endregion
		#region noCraftCost: Player.HaveRequirementItems, InventoryGui.SetupRequirement, InventoryGui.DoCrafting
		[HarmonyPatch(typeof(Player), nameof(Player.HaveRequirementItems))]
		private class Player_HaveRequirementItems {
			private static void Postfix(bool discover, ref bool __result) {
				if (settings.noCraftCost && !discover) {
					__result = true;
				}
			}
		}

		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
		private class InventoryGui_SetupRequirement {
			private static void Postfix(Transform elementRoot) {
				if (settings.noCraftCost) {
					elementRoot.transform.Find("res_amount").GetComponent<TMP_Text>().color = Color.white;
				}
			}
		}

		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
		private class InventoryGui_DoCrafting {
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				var targetMethod = typeof(Player).GetMethod(nameof(Player.NoCostCheat));
				var replacementMethod = typeof(InventoryGui_DoCrafting).GetMethod(nameof(InventoryGui_DoCrafting.NoCostCheat));

				foreach (var instruction in instructions) {
					if (instruction.Calls(targetMethod)) {
						yield return new CodeInstruction(OpCodes.Call, replacementMethod);
					}
					else {
						yield return instruction;
					}
				}
			}
			public bool NoCostCheat() {
				if (settings.noCraftCost) {
					return true;
				}
				return Player.m_localPlayer.m_noPlacementCost;
			}
			private static void Postfix(Player player) {
				UpgradeInventory(player);
			}
		}
		#endregion
		#region noCraftLevels: Player.RequiredCraftingStation, InventoryGui.UpdateRecipe
		[HarmonyPatch(typeof(Player), nameof(Player.RequiredCraftingStation))]
		private class Player_RequiredCraftingStation {
			private static void Prefix(ref bool checkLevel) {
				if (settings.noCraftLevels) {
					checkLevel = false;
				}
			}
		}

		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe))]
		private class InventoryGui_UpdateRecipe {
			private static void Postfix(InventoryGui __instance) {
				if (settings.noCraftLevels) {
					__instance.m_minStationLevelText.color = __instance.m_minStationLevelBasecolor;
				}
			}
		}
		#endregion
		#region showDeaths, showTimer, unlockRecipes, runSpeed, jumpForce: FejdStartup.Start, Player.OnSpawned
		private static TMP_FontAsset font;
		[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
		private class FejdStartup_Start {
			private static void Postfix() {
				font = GameObject.Find("Topic")?.GetComponent<TextMeshProUGUI>().font;
			}
		}

		private static Sprite GetItemIcon(string itemPrefab) {
			return ObjectDB.instance.GetItemPrefab(itemPrefab).GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
		}
		private static TMPro.TextMeshProUGUI deaths;
		private static void UpdateDeaths() {
			if (deaths != null) {
				deaths.text = Game.instance.GetPlayerProfile().m_playerStats[PlayerStatType.Deaths].ToString();
			}
		}
		private readonly static string logoutsKey = "logouts";
		private static TMPro.TextMeshProUGUI logouts;
		private static void UpdateLogouts() {
			if (Player.m_localPlayer.m_customData.TryGetValue(logoutsKey, out string savedLogouts)) {
				logouts.text = savedLogouts;
			}
			else {
				logouts.text = "0";
			}
		}

		private static TMPro.TextMeshProUGUI timer;
		private static bool unlockedRecipes = false;
		[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
		private class Player_OnSpawned {
			private static Transform healthpanel;
			private static GameObject LeftOf(string name, Transform parent, RectTransform from) {
				GameObject gameObject = new(name);
				gameObject.transform.SetParent(parent);
				RectTransform goTransform = gameObject.AddComponent<RectTransform>();
				goTransform.sizeDelta = from.sizeDelta;
				goTransform.anchoredPosition = from.anchoredPosition - new Vector2(from.sizeDelta.x + 1, 0);
				goTransform.anchorMax = from.anchorMax;
				goTransform.anchorMin = from.anchorMin;
				return gameObject;
			}
			private static void DefaultFont(ref TMPro.TextMeshProUGUI tmpGui, Color color, float ratio) {
				tmpGui.font = font;
				tmpGui.material = font.material;
				tmpGui.color = color;
				tmpGui.alignment = TextAlignmentOptions.Center;
				tmpGui.fontMaterial.EnableKeyword("OUTLINE_ON");
				tmpGui.outlineColor = Color.black;
				tmpGui.outlineWidth = 0.1f;
				tmpGui.fontStyle = FontStyles.Bold;
				tmpGui.fontSize = Hud.instance.transform.Find("hudroot/healthpanel/food0").GetComponent<RectTransform>().sizeDelta.x * ratio;
			}
			private static TMPro.TextMeshProUGUI AddCounter(string name, string item, int y) {
				Transform healthPanel = Hud.instance.transform.Find("hudroot/healthpanel");
				RectTransform foodTransform = healthPanel.transform.Find("food" + y).GetComponent<RectTransform>();

				GameObject background = LeftOf(name + "Bkg", healthPanel, foodTransform);
				JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(foodTransform.GetComponent<UnityEngine.UI.Image>()), background.AddComponent<UnityEngine.UI.Image>());

				GameObject icon = LeftOf(name + "Icon", healthPanel, foodTransform);
				icon.AddComponent<UnityEngine.UI.Image>().sprite = GetItemIcon(item);

				GameObject text = LeftOf(name + "Text", healthPanel, foodTransform);
				TMPro.TextMeshProUGUI tmpGui = text.AddComponent<TextMeshProUGUI>();
				DefaultFont(ref tmpGui, Color.white, 1f / 2);
				tmpGui.alignment = TextAlignmentOptions.BottomRight;
				return tmpGui;
			}
			private static TMPro.TextMeshProUGUI AddTimer() {
				Transform hudroot = Hud.instance.transform.Find("hudroot");
				RectTransform healthTransform = hudroot.Find("healthpanel").GetComponent<RectTransform>();
				GameObject clock = new("timer");
				clock.transform.SetParent(hudroot);

				RectTransform clockTransform = clock.AddComponent<RectTransform>();
				clockTransform.sizeDelta = new Vector2(1000, 0);
				clockTransform.anchoredPosition = healthTransform.anchoredPosition + new Vector2(71 * healthTransform.sizeDelta.x / 100, -112 * healthTransform.sizeDelta.y / 100);
				clockTransform.anchorMax = healthTransform.anchorMax;
				clockTransform.anchorMin = healthTransform.anchorMin;

				TMPro.TextMeshProUGUI tmpGui = clock.AddComponent<TextMeshProUGUI>();
				DefaultFont(ref tmpGui, new(1f, 0.717f, 0.360f, 1f), 2f / 3);
				tmpGui.text = $"<mspace=0.5em>{TimeSpan.FromSeconds(settings.time)}</mspace>";
				return tmpGui;
			}
			private static void Postfix(Player __instance) {
				// showDeaths
				healthpanel = Hud.instance.transform.Find("hudroot/healthpanel");
				int y = 0;
				if (settings.showDeaths && deaths == null) {
					deaths = AddCounter("deaths", "Charredskull", y);
					UpdateDeaths();
					y = 1;
				}
				// showLogouts
				if (settings.showLogouts && logouts == null) {
					logouts = AddCounter("logouts", "RoundLog", y);
					UpdateLogouts();
				}
				// showTimer
				if (settings.showTimer && timer == null) {
					timer = AddTimer();
				}

				// unlockRecipes
				if (settings.unlockRecipes && !unlockedRecipes) {
					unlockedRecipes = true;
					foreach (GameObject prefab in ObjectDB.instance.m_items) {
						ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
						if (itemDrop == null || itemDrop.m_itemData == null) {
							continue;
						}
						string name = itemDrop.m_itemData.m_shared.m_name;
						if (!__instance.m_knownMaterial.Contains(name)) {
							__instance.m_knownMaterial.Add(name);
						}
					}
					__instance.UpdateKnownRecipesList();
					MessageHud.instance.m_unlockMsgQueue.Clear();
					MessageHud.instance.m_unlockMsgCount = 0;
				}

				// runSpeed
				__instance.m_runSpeed *= settings.runSpeed;
				// jumpForce
				__instance.m_jumpForce *= settings.jumpForce;
			}
		}
		#endregion
		#region showDeaths: Player.OnDeath
		[HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
		private class Player_OnDeath {
			private static void Postfix() {
				UpdateDeaths();
			}
		}
		#endregion
		#region showTimer: Player.Update
		[HarmonyPatch(typeof(Player), nameof(Player.Update))]
		private class Player_Update {
			private static void Postfix() {
				if (!settings.isTimerRunning && Game.instance && Input.anyKeyDown) {
					settings.isTimerRunning = true;
					speedchoice.StartCoroutine(TimerUpdate());
					ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "StartTimer", "StartTimer");
				}
			}
		}
		private static IEnumerator TimerUpdate() {
			while (true) {
				if (Game.instance && !Game.IsPaused()) {
					if (timer != null) {
						timer.text = $"<mspace=0.5em>{TimeSpan.FromSeconds(settings.time)}</mspace>";
					}
					settings.time++;
				}
				yield return new WaitForSeconds(1f);
			}
		}
		#endregion
		#region structureLoot, wherePortal: Player.PlacePiece, TeleportWorld.SetText, Piece.DropResources
		private static bool overridePieceDrops = false;
		[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece), new[] { typeof(Piece), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
		private class Player_PlacePiece {
			private static void Postfix(Piece piece, Vector3 pos) {
				if (settings.wherePortal && piece != null && (piece.name == "portal_wood" || piece.name == "portal_stone")) {
					Minimap.instance.AddPin(pos, Minimap.PinType.Icon4, "", save: true, isChecked: false);
				}
			}
		}
		[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.SetText))]
		private class TeleportWorld_SetText {
			private static void Postfix(TeleportWorld __instance, string text) {
				if (settings.wherePortal) {
					Minimap.instance.RemovePin(__instance.transform.position, 0.1f);
					Minimap.instance.AddPin(__instance.transform.position, Minimap.PinType.Icon4, text, save: true, isChecked: false);
				}
			}
		}
		[HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
		private class Piece_DropResources {
			private static void Prefix(Piece __instance) {
				if (settings.structureLoot && !__instance.IsPlacedByPlayer()) {
					overridePieceDrops = true;
				}
			}
			private static void Postfix(Piece __instance) {
				overridePieceDrops = false;
				if (settings.wherePortal && __instance.GetComponent<TeleportWorld>() != null) {
					Minimap.instance.RemovePin(__instance.transform.position, 0.1f);
				}
			}
		}
		[HarmonyPatch(typeof(Piece), nameof(Piece.FreeBuildKey))]
		private class Piece_FreeBuildKey {
			private static bool Prefix(Piece __instance, ref GlobalKeys __result) {
				if (settings.structureLoot && overridePieceDrops && !__instance.IsPlacedByPlayer()) {
					// A "planned" NG+ world level key that is never actually used.
					__result = GlobalKeys.WorldLevel;
					return false;
				}
				return true;
			}
		}
		#endregion

		// Secret Settings
		#region alwaysRocky: ZoneSystem.PlaceVegetation, Pickable.Awake
		private static bool rockyied = false;
		[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
		private class ZoneSystem_PlaceVegetation {
			[HarmonyPrefix]
			private static void Prefix(ZoneSystem __instance) {
				if (settings.alwaysRocky && !rockyied) {
					foreach (ZoneSystem.ZoneVegetation zoneVegetation in __instance.m_vegetation) {
						if (zoneVegetation.m_prefab != null && zoneVegetation.m_prefab.name == "Pickable_Stone") {
							zoneVegetation.m_prefab = ZNetScene.instance.GetPrefab("Pickable_StoneRock");
						}
					}
					rockyied = true;
				}
			}
		}
		[HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
		private class Pickable_Awake {
			private static void Prefix(Pickable __instance) {
				if (settings.alwaysRocky && __instance.m_itemPrefab.name == "Stone") {
					__instance.m_itemPrefab = ObjectDB.instance.GetItemPrefab("StoneRock");
				}
			}
		}
		#endregion
	}
}