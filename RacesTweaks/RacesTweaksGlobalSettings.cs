using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using MCM.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.Localization;
using TaleWorlds.Library;

namespace RacesTweaks
{

    // 2) one entry per race ID

    // this is your per‑race settings POCO
    public class RacesTweaksSettings
    {
        public string RaceId { get; set; }
        public bool Debug { get; set; }
        public float MaxHitpointsMultipler { get; set; }
        public float MaxHitpointsFlat { get; set; }
        public float HealthRegenMultipler { get; set; }
        public float HealthRegenFlat { get; set; }
        public float PregnancyChanceMultipler { get; set; }
        public float PregnancyChanceFlat { get; set; }
        public float DamageAbsorptionMultipler { get; set; }
        public float DamageAbsorptionFlat { get; set; }
        public float MovementSpeedMultipler { get; set; }
        public float MovementSpeedFlat { get; set; }
        public float HandlingMultipler { get; set; }
        public float HandlingFlat { get; set; }
        public float ReloadMultipler { get; set; }
        public float ReloadFlat { get; set; }
        public float SwingMultipler { get; set; }
        public float SwingFlat { get; set; }
        public float AccuracyMultipler { get; set; }
        public float AccuracyFlat { get; set; }
        public float ArmorEncumbranceMultipler { get; set; }
        public float ArmorEncumbranceFlat { get; set; }
        public float WeaponsEncumbranceMultipler { get; set; }
        public float WeaponsEncumbranceFlat { get; set; }
        public float ArmorArmsMultipler { get; set; }
        public float ArmorArmsFlat { get; set; }
        public float ArmorHeadMultipler { get; set; }
        public float ArmorHeadFlat { get; set; }
        public float ArmorLegsMultipler { get; set; }
        public float ArmorLegsFlat { get; set; }
        public float ArmorTorsoMultipler { get; set; }
        public float ArmorTorsoFlat { get; set; }
        public float TotalDamageBonusMultipler { get; set; }
        public float TotalDamageBonusFlat { get; set; }
        public float UnarmedAttackDamageMultipler { get; set; }
        public float UnarmedAttackDamageFlat { get; set; }
        public float BowBonusMultipler { get; set; }
        public float BowBonusFlat { get; set; }
        public float CrossbowBonusMultipler { get; set; }
        public float CrossbowBonusFlat { get; set; }
        public float RangedBonusMultipler { get; set; }
        public float RangedBonusFlat { get; set; }
        public float OneHandedBonusMultipler { get; set; }
        public float OneHandedBonusFlat { get; set; }
        public float TwoHandedBonusMultipler { get; set; }
        public float TwoHandedBonusFlat { get; set; }
        public float SwordBonusMultipler { get; set; }
        public float SwordBonusFlat { get; set; }
        public float AxeBonusMultipler { get; set; }
        public float AxeBonusFlat { get; set; }
        public float MaceBonusMultipler { get; set; }
        public float MaceBonusFlat { get; set; }
        public float PolearmBonusMultipler { get; set; }
        public float PolearmBonusFlat { get; set; }
        public float KnifeBonusMultipler { get; set; }
        public float KnifeBonusFlat { get; set; }
        public float ThrowingAxeBonusMultipler { get; set; }
        public float ThrowingAxeBonusFlat { get; set; }
        public float ThrowingKnifeBonusMultipler { get; set; }
        public float ThrowingKnifeBonusFlat { get; set; }
        public float StoneBonusMultipler { get; set; }
        public float StoneBonusFlat { get; set; }
        public float JavelinBonusMultipler { get; set; }
        public float JavelinBonusFlat { get; set; }
        public float ShieldBonusMultipler { get; set; }
        public float ShieldBonusFlat { get; set; }
        public float PersuasionBonusMultipler { get; set; }
        public float PersuasionBonusFlat { get; set; }
        public int BecomeOldAge { get; set; }
        public int MaxAge { get; set; }
        public int MinPregnancyAge { get; set; }
        public int MaxPregnancyAge { get; set; }
        //grow mult
        public float AgeGrowthMultiplier { get; set; }

        // if you need your skill maps as CSV strings you parse later:
        public string SkillLearningLimitBonusMultiplerRaw { get; set; }
        public string SkillLearningLimitBonusFlatRaw { get; set; }
        public string SkillLearningRateBonusMultiplerRaw { get; set; }
        public string SkillLearningRateBonusFlatRaw { get; set; }

        public float MountExtraHitpointsMultipler { get; set; }
        public float MountExtraHitpointsFlat { get; set; }
        public float MountManeuverBonusMultipler { get; set; }
        public float MountManeuverBonusFlat { get; set; }
        public float MountSpeedBonusMultipler { get; set; }
        public float MountSpeedBonusFlat { get; set; }
        public float MountChargeBonusMultipler { get; set; }
        public float MountChargeBonusFlat { get; set; }
        public float MountDashBonusMultipler { get; set; }
        public float MountDashBonusFlat { get; set; }
        public float MountDifficultyBonusMultipler { get; set; }
        public float MountDifficultyBonusFlat { get; set; }

        public float BattleSurvivalBonusMultipler { get; set; }
        public float BattleSurvivalBonusFlat { get; set; }
        public float FoodConsumptionBonusMultipler { get; set; }
        public float FoodConsumptionBonusFlat { get; set; }
        public float NightStatsBonusMultipler { get; set; }
        public float NightStatsBonusFlat { get; set; }

        public override string ToString() => RaceId;
    }

    // 1) container for JSON persistence
    public class RaceTweaksData
    {
        public List<RacesTweaksSettings> Entries { get; set; } = new List<RacesTweaksSettings>();
    }

    public sealed class RaceTweaksGlobalSettings : AttributeGlobalSettings<RaceTweaksGlobalSettings>
    {
        public override string Id => "RaceTweaks";
        public override string DisplayName => new TextObject("{=RT_TITLE}Race Tweaks").ToString();
        public override string FolderName => "RaceTweaks";
        public override string FormatType => "json";

        private const string FILE_NAME = "racesettings.json";
        private readonly string _path;

        // inside RaceTweaksGlobalSettings class
        [SettingPropertyInteger("{=RT_DaysPerYear}Days per Hero Year (compatibility)",
            1, 365, Order = 0,
            HintText = "{=RT_DaysPerYear_H}Set this to the 'days per hero year' used by other aging mods (e.g. LifeIsShort). Default uses native 84.",
            RequireRestart = false)]
        [SettingPropertyGroup("Compatibility")]
        public int DaysPerHeroYearForCompatibility { get; set; } = 84;

        [SettingPropertyFloatingInteger("{=RT_AgeForGrowth}Age For Growth Multipler",
            18f, 365f, Order = 1,
            HintText = "{=RT_AgeForGrowth_H}At what age Age Growth Multipler should work (so we won't have immortality children if you set to 0)",
            RequireRestart = false)]
        [SettingPropertyGroup("Compatibility")]
        public float AgeForGrowth { get; set; } = 22f;

        public List<RacesTweaksSettings> EntriesList = new List<RacesTweaksSettings>();



        public RaceTweaksGlobalSettings()
        {
            var asm = typeof(RacesTweaksModule).Assembly;
            _path = Path.Combine(Path.GetDirectoryName(asm.Location), FILE_NAME);

            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var data = JsonConvert.DeserializeObject<RaceTweaksData>(json)
                           ?? new RaceTweaksData();
                EntriesList = data.Entries;
            }
            else
            {
                EntriesList = new List<RacesTweaksSettings>
                {
            new RacesTweaksSettings
            {
                RaceId = "giant",
                    Debug = false,
                    MaxHitpointsMultipler = 0.30f,
                    HealthRegenMultipler = 0.10f,
                    PregnancyChanceMultipler = -0.50f,
                    DamageAbsorptionMultipler = 0.20f,
                    MovementSpeedMultipler = -0.20f,
                    HandlingMultipler = -0.25f,
                    ReloadMultipler = -0.50f,
                    SwingMultipler = 0.30f,
                    AccuracyMultipler = -0.30f,
                    ArmorEncumbranceMultipler = 0.35f,
                    WeaponsEncumbranceMultipler = 0.35f,
                    ArmorArmsMultipler = 0.10f,
                    ArmorHeadMultipler = 0.10f,
                    ArmorLegsMultipler = 0.10f,
                    ArmorTorsoMultipler = 0.20f,
                    TotalDamageBonusMultipler = 0.15f,
                    UnarmedAttackDamageMultipler = 0.30f,
                    BowBonusMultipler = -0.50f,
                    CrossbowBonusMultipler = -0.50f,
                    RangedBonusMultipler = -0.50f,
                    OneHandedBonusMultipler = 0.20f,
                    TwoHandedBonusMultipler = 0.25f,
                    SwordBonusMultipler = 0.10f,
                    AxeBonusMultipler = 0.35f,
                    MaceBonusMultipler = 0.35f,
                    PolearmBonusMultipler = 0.25f,
                    KnifeBonusMultipler = -0.50f,
                    ThrowingAxeBonusMultipler = 0.03f,
                    StoneBonusMultipler = 0.30f,
                    JavelinBonusMultipler = 0.10f,
                    ShieldBonusMultipler = 0.15f,
                    PersuasionBonusMultipler = -0.30f,
                    SkillLearningLimitBonusMultiplerRaw = "Vigor: 0.15, Endurance: 0.10, Social: -0.05",
                    SkillLearningRateBonusMultiplerRaw = "Vigor: 0.15, Endurance: 0.10, Social: -0.05",
                    MountExtraHitpointsMultipler = 0.15f,
                    MountManeuverBonusMultipler = -0.20f,
                    MountSpeedBonusMultipler = -0.20f,
                    MountChargeBonusMultipler = 0.30f,
                    MountDashBonusMultipler = -0.10f,
                    MountDifficultyBonusMultipler = 0.20f,
                    BattleSurvivalBonusMultipler = 0.10f,
                    FoodConsumptionBonusMultipler = 0.20f
            },
            new RacesTweaksSettings
            {
                RaceId = "elf",
                    Debug = false,
                    MaxHitpointsMultipler = -0.05f,
                    HealthRegenMultipler = 0.20f,
                    PregnancyChanceMultipler = 0.15f,
                    MovementSpeedMultipler = 0.15f,
                    HandlingMultipler = 0.10f,
                    ReloadMultipler = 0.10f,
                    AccuracyMultipler = 0.20f,
                    ArmorEncumbranceMultipler = -0.10f,
                    WeaponsEncumbranceMultipler = -0.10f,
                    BowBonusMultipler = 0.30f,
                    CrossbowBonusMultipler = 0.10f,
                    RangedBonusMultipler = 0.20f,
                    SwordBonusMultipler = 0.10f,
                    AxeBonusMultipler = -0.10f,
                    MaceBonusMultipler = -0.10f,
                    KnifeBonusMultipler = 0.15f,
                    ThrowingKnifeBonusMultipler = 0.10f,
                    JavelinBonusMultipler = 0.10f,
                    PersuasionBonusMultipler = 0.25f,
                    BecomeOldAge = 500,
                    MaxAge = 1000,
                    MinPregnancyAge = 18,
                    MaxPregnancyAge = 498,
                    AgeGrowthMultiplier = 0.5f,
                    SkillLearningLimitBonusMultiplerRaw = "Vigor: -0.05, Control: 0.20, Cunning: 0.15, Endurance: -0.05, Social: 0.20",
                    SkillLearningRateBonusMultiplerRaw = "Vigor: -0.05, Control: 0.20, Cunning: 0.15, Endurance: -0.05, Social: 0.20",
                    MountExtraHitpointsMultipler = -0.10f,
                    MountManeuverBonusMultipler = 0.20f,
                    MountSpeedBonusMultipler = 0.20f,
                    MountDashBonusMultipler = 0.10f,
                    MountDifficultyBonusMultipler = -0.10f,
                    BattleSurvivalBonusMultipler = 0.10f,
                    FoodConsumptionBonusMultipler = -0.10f
            },
            new RacesTweaksSettings
            {
                RaceId = "orc",
                    Debug = false,
                    MaxHitpointsMultipler = 0.10f,
                    HealthRegenMultipler = 0.05f,
                    PregnancyChanceMultipler = -0.30f,
                    DamageAbsorptionMultipler = 0.08f,
                    HandlingMultipler = -0.10f,
                    ReloadMultipler = -0.20f,
                    SwingMultipler = 0.15f,
                    AccuracyMultipler = -0.20f,
                    ArmorEncumbranceMultipler = 0.06f,
                    WeaponsEncumbranceMultipler = 0.15f,
                    TotalDamageBonusMultipler = 0.15f,
                    UnarmedAttackDamageMultipler = 0.06f,
                    OneHandedBonusMultipler = 0.15f,
                    TwoHandedBonusMultipler = 0.10f,
                    AxeBonusMultipler = 0.18f,
                    MaceBonusMultipler = 0.15f,
                    PersuasionBonusMultipler = -0.20f,
                    BecomeOldAge = 40,
                    MaxAge = 80,
                    MinPregnancyAge = 18,
                    MaxPregnancyAge = 38,
                    SkillLearningLimitBonusMultiplerRaw = "Vigor: 0.10, Cunning: -0.05, Endurance: 0.05, Social: -0.05",
                    SkillLearningRateBonusMultiplerRaw = "Vigor: 0.10, Cunning: -0.05, Endurance: 0.05, Social: -0.05",
                    MountExtraHitpointsMultipler = 0.06f,
                    MountChargeBonusMultipler = 0.06f,
                    MountDashBonusMultipler = -0.06f,
                    MountDifficultyBonusMultipler = 0.06f,
                    BattleSurvivalBonusMultipler = 0.06f,
                    FoodConsumptionBonusMultipler = 0.06f
            },
            new RacesTweaksSettings
            {
                RaceId = "goblin",
                    Debug = false,
                    MaxHitpointsMultipler = -0.10f,
                    HealthRegenMultipler = -0.10f,
                    DamageAbsorptionMultipler = -0.10f,
                    MovementSpeedMultipler = 0.10f,
                    HandlingMultipler = 0.20f,
                    SwingMultipler = -0.10f,
                    AccuracyMultipler = 0.10f,
                    ArmorEncumbranceMultipler = -0.20f,
                    WeaponsEncumbranceMultipler = -0.10f,
                    TotalDamageBonusMultipler = -0.10f,
                    BowBonusMultipler = 0.10f,
                    OneHandedBonusMultipler = -0.10f,
                    TwoHandedBonusMultipler = -0.10f,
                    KnifeBonusMultipler = 0.20f,
                    ThrowingKnifeBonusMultipler = 0.10f,
                    ShieldBonusMultipler = -0.20f,
                    PersuasionBonusMultipler = -0.10f,
                    BecomeOldAge = 30,
                    MaxAge = 60,
                    MinPregnancyAge = 18,
                    MaxPregnancyAge = 28,
                    AgeGrowthMultiplier = 1.2f,
                    SkillLearningLimitBonusMultiplerRaw = "Vigor: -0.10, Cunning: -0.10, Endurance: -0.10",
                    SkillLearningRateBonusMultiplerRaw = "Vigor: -0.10, Cunning: -0.10, Endurance: -0.10",
                    MountExtraHitpointsMultipler = -0.10f,
                    MountManeuverBonusMultipler = 0.10f,
                    MountSpeedBonusMultipler = 0.10f,
                    MountDashBonusMultipler = 0.06f,
                    MountDifficultyBonusMultipler = -0.10f,
                    BattleSurvivalBonusMultipler = -0.10f,
                    FoodConsumptionBonusMultipler = -0.10f
            },
            new RacesTweaksSettings
            {
                RaceId = "dwarf",
                    Debug = false,
                    MaxHitpointsMultipler = 0.05f,
                    HealthRegenMultipler = 0.05f,
                    PregnancyChanceMultipler = -0.10f,
                    DamageAbsorptionMultipler = 0.12f,
                    MovementSpeedMultipler = -0.10f,
                    HandlingMultipler = -0.05f,
                    SwingMultipler = 0.05f,
                    ArmorEncumbranceMultipler = 0.15f,
                    WeaponsEncumbranceMultipler = 0.05f,
                    ArmorArmsMultipler = 0.05f,
                    ArmorHeadMultipler = 0.05f,
                    ArmorLegsMultipler = 0.05f,
                    ArmorTorsoMultipler = 0.15f,
                    TotalDamageBonusMultipler = 0.05f,
                    BowBonusMultipler = -0.10f,
                    CrossbowBonusMultipler = 0.10f,
                    OneHandedBonusMultipler = 0.10f,
                    TwoHandedBonusMultipler = 0.04f,
                    AxeBonusMultipler = 0.15f,
                    MaceBonusMultipler = 0.10f,
                    ShieldBonusMultipler = 0.10f,
                    BecomeOldAge = 115,
                    MaxAge = 300,
                    MinPregnancyAge = 18,
                    MaxPregnancyAge = 113,
                    AgeGrowthMultiplier = 0.8f,
                    SkillLearningLimitBonusMultiplerRaw = "Vigor: 0.05, Endurance: 0.10, Social: -0.05",
                    SkillLearningRateBonusMultiplerRaw = "Vigor: 0.05, Endurance: 0.10, Social: -0.05",
                    MountExtraHitpointsMultipler = 0.06f,
                    MountManeuverBonusMultipler = -0.06f,
                    MountSpeedBonusMultipler = -0.10f,
                    MountDifficultyBonusMultipler = 0.06f,
                    BattleSurvivalBonusMultipler = 0.06f,
                    FoodConsumptionBonusMultipler = -0.06f
            },
                };
                Save();
            }

            RefreshDropdown();
        }

        private void Save()
        {
            var data = new RaceTweaksData { Entries = EntriesList };
            File.WriteAllText(_path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private void RefreshDropdown()
        {
            var list = EntriesList.ToList();
            var old = _dropdown?.SelectedValue;
            var idx = list.IndexOf(old ?? list.FirstOrDefault());
            if (idx < 0) idx = 0;
            _dropdown = new Dropdown<RacesTweaksSettings>(list, idx);
            OnPropertyChanged(nameof(RaceSelector));
        }

        private Dropdown<RacesTweaksSettings> _dropdown;
        private RacesTweaksSettings _lastSelected;

        public RacesTweaksSettings CurrentEntry
            => EntriesList.ElementAtOrDefault(_dropdown.SelectedIndex)
               ?? new RacesTweaksSettings();

        // ─── selector ────────────────────────────────────────────
        [SettingPropertyDropdown("{=RT_SelectRace}Select Race",
            Order = 0, RequireRestart = false,
            HintText = "{=RT_SelectRace_H}Choose which RaceId to edit.")]
        [SettingPropertyGroup("General")]
        public Dropdown<RacesTweaksSettings> RaceSelector
        {
            get
            {
                var sel = _dropdown.SelectedValue;
                if (sel != _lastSelected)
                {
                    _lastSelected = sel;
                    OnPropertyChanged(nameof(CurrentEntry));
                }
                return _dropdown;
            }
        }

        // ─── buttons ──────────────────────────────────────────────
        [SettingPropertyButton("{=RT_Add}Add Race",
            Content = "{=RT_Add}Add Race", Order = 1)]
        [SettingPropertyGroup("General")]
        public Action AddRaceButton { get; set; } = () =>
        {
            Instance.EntriesList.Add(new RacesTweaksSettings { RaceId = "New Race" });
            Instance.RefreshDropdown();
            Instance.Save();
        };

        [SettingPropertyButton("{=RT_Remove}Remove Race",
            Content = "{=RT_Remove}Remove Race", Order = 2)]
        [SettingPropertyGroup("General")]
        public Action RemoveRaceButton { get; set; } = () =>
        {
            var i = Instance._dropdown.SelectedIndex;
            if (i >= 0 && i < Instance.EntriesList.Count)
            {
                Instance.EntriesList.RemoveAt(i);
                Instance.RefreshDropdown();
                Instance.Save();
            }
        };

        [SettingPropertyButton("{=RT_Clear}Clear All",
            Content = "{=RT_Clear}Clear All", Order = 3)]
        [SettingPropertyGroup("General")]
        public Action ClearRacesButton { get; set; } = () =>
        {
            if (Instance.EntriesList.Count > 1)
            {
                Instance.EntriesList.RemoveRange(1, Instance.EntriesList.Count - 1);
                Instance.RefreshDropdown();
                Instance.Save();
                InformationManager.DisplayMessage(
                    new InformationMessage("[RT] Cleared all tweaks (except first)", Colors.Green)
                );
            }
        };

        // ─── edit fields ───────────────────────────────────────────
        [SettingPropertyText("{=RT_RaceId}Race ID", HintText = "{=RT_RaceId}Race ID",
            Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("General")]
        public string RaceId
        {
            get => CurrentEntry.RaceId;
            set { CurrentEntry.RaceId = value; Save(); }
        }

        [SettingPropertyBool("{=RT_Debug}Enable Debug", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("General/Debug")]
        public bool Debug
        {
            get => CurrentEntry.Debug;
            set { CurrentEntry.Debug = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_MaxHP_Mult}Max HP Mult", -1f, 5f, Order = 0, HintText = "{=RT_MaxHP_Mult_H}Mutliply Max HP by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Health")]
        public float MaxHitpointsMultipler
        {
            get => CurrentEntry.MaxHitpointsMultipler;
            set { CurrentEntry.MaxHitpointsMultipler = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_MaxHP_Flat}Max HP Flat", -1000f, 1000f, Order = 1, HintText = "{=RT_MaxHP_Flat_H}Add to Max HP this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Health")]
        public float MaxHitpointsFlat
        {
            get => CurrentEntry.MaxHitpointsFlat;
            set { CurrentEntry.MaxHitpointsFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_Regen_Mult}Regen Mult", -1f, 5f, Order = 2, HintText = "{=RT_Regen_Mult_H}Mutliply Health Regen by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Health")]
        public float HealthRegenMultipler
        {
            get => CurrentEntry.HealthRegenMultipler;
            set { CurrentEntry.HealthRegenMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_Regen_Flat}Regen Flat", -50f, 50f, Order = 3, HintText = "{=RT_Regen_Flat_H}Add to Health Regen this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Health")]
        public float HealthRegenFlat
        {
            get => CurrentEntry.HealthRegenFlat;
            set { CurrentEntry.HealthRegenFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_PregMult}Pregnancy Chance Mult", -1f, 5f, Order = 0, HintText = "{=RT_PregMult_H}Mutliply Pregnancy Chance by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Pregnancy")]
        public float PregnancyChanceMultipler
        {
            get => CurrentEntry.PregnancyChanceMultipler;
            set { CurrentEntry.PregnancyChanceMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_PregFlat}Pregnancy Chance Flat", -1f, 1f, "#0%", Order = 1, HintText = "{=RT_PregFlat_H}Add to Pregnancy Chance this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Pregnancy")]
        public float PregnancyChanceFlat
        {
            get => CurrentEntry.PregnancyChanceFlat;
            set { CurrentEntry.PregnancyChanceFlat = value; Save(); }
        }
        [SettingPropertyInteger("{=RT_MinPregAge}Min Pregnancy Age", 0, 500, Order = 2, HintText = "{=RT_MinPregAge_H}Minimum age for pregnancy to occur", RequireRestart = false)]
        [SettingPropertyGroup("General/Pregnancy")]
        public int MinPregnancyAge
        {
            get => CurrentEntry.MinPregnancyAge;
            set { CurrentEntry.MinPregnancyAge = value; Save(); }
        }
        [SettingPropertyInteger("{=RT_MaxPregAge}Max Pregnancy Age", 0, 5000, Order = 3, HintText = "{=RT_MaxPregAge_H}Maximum age for pregnancy to occur", RequireRestart = false)]
        [SettingPropertyGroup("General/Pregnancy")]
        public int MaxPregnancyAge
        {
            get => CurrentEntry.MaxPregnancyAge;
            set { CurrentEntry.MaxPregnancyAge = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_DmgAbsMult}Dmg Absorption Mult", -1f, 5f, Order = 0, HintText = "{=RT_DmgAbsMult_H}Mutliply Damage Taken by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Defense")]
        public float DamageAbsorptionMultipler
        {
            get => CurrentEntry.DamageAbsorptionMultipler;
            set { CurrentEntry.DamageAbsorptionMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_DmgAbsFlat}Dmg Absorption Flat", -200f, 200f, Order = 1, HintText = "{=RT_DmgAbsFlat_H}Add to Damage Taken this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Defense")]
        public float DamageAbsorptionFlat
        {
            get => CurrentEntry.DamageAbsorptionFlat;
            set { CurrentEntry.DamageAbsorptionFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_MoveMult}Move Speed Mult", -1f, 5f, Order = 0, HintText = "{=RT_MoveMult_H}Mutliply Movement Speed by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float MovementSpeedMultipler
        {
            get => CurrentEntry.MovementSpeedMultipler;
            set { CurrentEntry.MovementSpeedMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MoveFlat}Move Speed Flat", -50f, 50f, Order = 1, HintText = "{=RT_MoveFlat_H}Add to Movement Speed this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float MovementSpeedFlat
        {
            get => CurrentEntry.MovementSpeedFlat;
            set { CurrentEntry.MovementSpeedFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_HandlingMult}Handling Mult", -1f, 5f, Order = 2, HintText = "{=RT_HandlingMult_H}Mutliply Weapon Handling by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float HandlingMultipler
        {
            get => CurrentEntry.HandlingMultipler;
            set { CurrentEntry.HandlingMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_HandlingFlat}Handling Flat", -50f, 50f, Order = 3, HintText = "{=RT_HandlingFlat_H}Add to Weapon Handling this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float HandlingFlat
        {
            get => CurrentEntry.HandlingFlat;
            set { CurrentEntry.HandlingFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_ReloadMult}Reload Mult", -1f, 5f, Order = 4, HintText = "{=RT_ReloadMult_H}Mutliply Reload by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float ReloadMultipler
        {
            get => CurrentEntry.ReloadMultipler;
            set { CurrentEntry.ReloadMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ReloadFlat}Reload Flat", -50f, 50f, Order = 5, HintText = "{=RT_ReloadFlat_H}Add to Reload this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float ReloadFlat
        {
            get => CurrentEntry.ReloadFlat;
            set { CurrentEntry.ReloadFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_SwingMult}Swing Mult", -1f, 5f, Order = 6, HintText = "{=RT_SwingMult_H}Mutliply Swing Weapon by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float SwingMultipler
        {
            get => CurrentEntry.SwingMultipler;
            set { CurrentEntry.SwingMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_SwingFlat}Swing Flat", -50f, 50f, Order = 7, HintText = "{=RT_SwingFlat_H}Add to Swing Weapon this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float SwingFlat
        {
            get => CurrentEntry.SwingFlat;
            set { CurrentEntry.SwingFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_AccMult}Accuracy Mult", -1f, 5f, Order = 8, HintText = "{=RT_AccMult_H}Mutliply Accuracy by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float AccuracyMultipler
        {
            get => CurrentEntry.AccuracyMultipler;
            set { CurrentEntry.AccuracyMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_AccFlat}Accuracy Flat", -50f, 50f, Order = 9, HintText = "{=RT_AccFlat_H}Add to Accuracy this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Abilities")]
        public float AccuracyFlat
        {
            get => CurrentEntry.AccuracyFlat;
            set { CurrentEntry.AccuracyFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_ArmEncMult}Armor Enc Mult", -1f, 5f, Order = 0, HintText = "{=RT_ArmEncMult_H}Mutliply Armor Encumbrance by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Encumbrance")]
        public float ArmorEncumbranceMultipler
        {
            get => CurrentEntry.ArmorEncumbranceMultipler;
            set { CurrentEntry.ArmorEncumbranceMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmEncFlat}Armor Enc Flat", -50f, 50f, Order = 1, HintText = "{=RT_ArmEncFlat_H}Add to Armor Encumbrance this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Encumbrance")]
        public float ArmorEncumbranceFlat
        {
            get => CurrentEntry.ArmorEncumbranceFlat;
            set { CurrentEntry.ArmorEncumbranceFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_WeapEncMult}Weap Enc Mult", -1f, 5f, Order = 2, HintText = "{=RT_WeapEncMult_H}Mutliply Weapon Encumbrance by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Encumbrance")]
        public float WeaponsEncumbranceMultipler
        {
            get => CurrentEntry.WeaponsEncumbranceMultipler;
            set { CurrentEntry.WeaponsEncumbranceMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_WeapEncFlat}Weap Enc Flat", -50f, 50f, Order = 3, HintText = "{=RT_WeapEncFlat_H}Add to Weapon Encumbrance this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Encumbrance")]
        public float WeaponsEncumbranceFlat
        {
            get => CurrentEntry.WeaponsEncumbranceFlat;
            set { CurrentEntry.WeaponsEncumbranceFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmArmMult}Armor Arms Multi", -1f, 5f, Order = 0, HintText = "{=RT_ArmArmMult_H}Mutliply Arms Armor by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorArmsMultipler
        {
            get => CurrentEntry.ArmorArmsMultipler;
            set { CurrentEntry.ArmorArmsMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmArmFlat}Armor Arms Flat", -50f, 50f, Order = 1, HintText = "{=RT_ArmArmFlat_H}Add to Arms Armor this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorArmsFlat
        {
            get => CurrentEntry.ArmorArmsFlat;
            set { CurrentEntry.ArmorArmsFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmHeadMult}Armor Head Multi", -1f, 5f, Order = 2, HintText = "{=RT_ArmHeadMult_H}Mutliply Head Armor by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorHeadMultipler
        {
            get => CurrentEntry.ArmorHeadMultipler;
            set { CurrentEntry.ArmorHeadMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmHeadFlat}Armor Head Flat", -50f, 50f, Order = 3, HintText = "{=RT_ArmHeadFlat_H}Add to Head Armor this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorHeadFlat
        {
            get => CurrentEntry.ArmorHeadFlat;
            set { CurrentEntry.ArmorHeadFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmLegsMult}Armor Legs Multi", -1f, 5f, Order = 4, HintText = "{=RT_ArmLegsMult_H}Mutliply Legs Armor by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorLegsMultipler
        {
            get => CurrentEntry.ArmorLegsMultipler;
            set { CurrentEntry.ArmorLegsMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmLegsFlat}Armor Legs Flat", -50f, 50f, Order = 5, HintText = "{=RT_ArmLegsFlat_H}Add to Legs Armor this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorLegsFlat
        {
            get => CurrentEntry.ArmorLegsFlat;
            set { CurrentEntry.ArmorLegsFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmTorsoMult}Armor Torso Multi", -1f, 5f, Order = 6, HintText = "{=RT_ArmTorsoMult_H}Mutliply Body Armor by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorTorsoMultipler
        {
            get => CurrentEntry.ArmorTorsoMultipler;
            set { CurrentEntry.ArmorTorsoMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ArmTorsoFlat}Armor Torso Flat", -50f, 50f, Order = 7, HintText = "{=RT_ArmTorsoFlat_H}Add to Body Armor this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle/Armor")]
        public float ArmorTorsoFlat
        {
            get => CurrentEntry.ArmorTorsoFlat;
            set { CurrentEntry.ArmorTorsoFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_TotalDmgMult}Total Dmg Mult", -1f, 5f, Order = 0, HintText = "{=RT_TotalDmgMult_H}Mutliply Total Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage")]
        public float TotalDamageBonusMultipler
        {
            get => CurrentEntry.TotalDamageBonusMultipler;
            set { CurrentEntry.TotalDamageBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_TotalDmgFlat}Total Dmg Flat", -50f, 50f, Order = 1, HintText = "{=RT_TotalDmgFlat_H}Add to Total Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage")]
        public float TotalDamageBonusFlat
        {
            get => CurrentEntry.TotalDamageBonusFlat;
            set { CurrentEntry.TotalDamageBonusFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_UnarmedMult}Unarmed Mult", -1f, 5f, Order = 2, HintText = "{=RT_UnarmedMult_H}Mutliply Unarmed Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage")]
        public float UnarmedAttackDamageMultipler
        {
            get => CurrentEntry.UnarmedAttackDamageMultipler;
            set { CurrentEntry.UnarmedAttackDamageMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_UnarmedFlat}Unarmed Flat", -50f, 50f, Order = 3, HintText = "{=RT_UnarmedFlat_H}Add to Unarmed Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage")]
        public float UnarmedAttackDamageFlat
        {
            get => CurrentEntry.UnarmedAttackDamageFlat;
            set { CurrentEntry.UnarmedAttackDamageFlat = value; Save(); }
        }

        // Bow
        [SettingPropertyFloatingInteger("{=RT_BowMult}Bow Mult", -1f, 5f, Order = 4, HintText = "{=RT_BowMult_H}Mutliply Bow Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Bow")]
        public float BowBonusMultipler
        {
            get => CurrentEntry.BowBonusMultipler;
            set { CurrentEntry.BowBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_BowFlat}Bow Flat", -50f, 50f, Order = 5, HintText = "{=RT_BowFlat_H}Add to Bow Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Bow")]
        public float BowBonusFlat
        {
            get => CurrentEntry.BowBonusFlat;
            set { CurrentEntry.BowBonusFlat = value; Save(); }
        }

        // Crossbow
        [SettingPropertyFloatingInteger("{=RT_CrossMult}Cross Mult", -1f, 5f, Order = 6, HintText = "{=RT_CrossMult_H}Mutliply Crossbow Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Crossbow")]
        public float CrossbowBonusMultipler
        {
            get => CurrentEntry.CrossbowBonusMultipler;
            set { CurrentEntry.CrossbowBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_CrossFlat}Cross Flat", -50f, 50f, Order = 7, HintText = "{=RT_CrossFlat_H}Add to Crossbow Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Crossbow")]
        public float CrossbowBonusFlat
        {
            get => CurrentEntry.CrossbowBonusFlat;
            set { CurrentEntry.CrossbowBonusFlat = value; Save(); }
        }

        // Ranged
        [SettingPropertyFloatingInteger("{=RT_RangedMult}Ranged Mult", -1f, 5f, Order = 8, HintText = "{=RT_RangedMult_H}Mutliply Ranged Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Ranged")]
        public float RangedBonusMultipler
        {
            get => CurrentEntry.RangedBonusMultipler;
            set { CurrentEntry.RangedBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_RangedFlat}Ranged Flat", -50f, 50f, Order = 9, HintText = "{=RT_RangedFlat_H}Add to Ranged Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Ranged")]
        public float RangedBonusFlat
        {
            get => CurrentEntry.RangedBonusFlat;
            set { CurrentEntry.RangedBonusFlat = value; Save(); }
        }

        // One‑Handed
        [SettingPropertyFloatingInteger("{=RT_1HMult}1H Mult", -1f, 5f, Order = 0, HintText = "{=RT_1HMult_H}Mutliply One-Handed Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/OneHanded")]
        public float OneHandedBonusMultipler
        {
            get => CurrentEntry.OneHandedBonusMultipler;
            set { CurrentEntry.OneHandedBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_1HFlat}1H Flat", -50f, 50f, Order = 1, HintText = "{=RT_1HFlat_H}Add to One-Handed Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/OneHanded")]
        public float OneHandedBonusFlat
        {
            get => CurrentEntry.OneHandedBonusFlat;
            set { CurrentEntry.OneHandedBonusFlat = value; Save(); }
        }

        // Two‑Handed
        [SettingPropertyFloatingInteger("{=RT_2HMult}2H Mult", -1f, 5f, Order = 2, HintText = "{=RT_2HMult_H}Mutliply Two-Handed Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/TwoHanded")]
        public float TwoHandedBonusMultipler
        {
            get => CurrentEntry.TwoHandedBonusMultipler;
            set { CurrentEntry.TwoHandedBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_2HFlat}2H Flat", -50f, 50f, Order = 3, HintText = "{=RT_2HFlat_H}Add to Two-Handed Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/TwoHanded")]
        public float TwoHandedBonusFlat
        {
            get => CurrentEntry.TwoHandedBonusFlat;
            set { CurrentEntry.TwoHandedBonusFlat = value; Save(); }
        }

        // Sword
        [SettingPropertyFloatingInteger("{=RT_SwordMult}Sword Mult", -1f, 5f, Order = 0, HintText = "{=RT_SwordMult_H}Mutliply Sword Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Sword")]
        public float SwordBonusMultipler
        {
            get => CurrentEntry.SwordBonusMultipler;
            set { CurrentEntry.SwordBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_SwordFlat}Sword Flat", -50f, 50f, Order = 1, HintText = "{=RT_SwordFlat_H}Add to Sword Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Sword")]
        public float SwordBonusFlat
        {
            get => CurrentEntry.SwordBonusFlat;
            set { CurrentEntry.SwordBonusFlat = value; Save(); }
        }

        // Axe
        [SettingPropertyFloatingInteger("{=RT_AxeMult}Axe Mult", -1f, 5f, Order = 2, HintText = "{=RT_AxeMult_H}Mutliply Axe Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Axe")]
        public float AxeBonusMultipler
        {
            get => CurrentEntry.AxeBonusMultipler;
            set { CurrentEntry.AxeBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_AxeFlat}Axe Flat", -50f, 50f, Order = 3, HintText = "{=RT_AxeFlat_H}Add to Axe Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Axe")]
        public float AxeBonusFlat
        {
            get => CurrentEntry.AxeBonusFlat;
            set { CurrentEntry.AxeBonusFlat = value; Save(); }
        }

        // Mace
        [SettingPropertyFloatingInteger("{=RT_MaceMult}Mace Mult", -1f, 5f, Order = 4, HintText = "{=RT_MaceMult_H}Mutliply Mace Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Mace")]
        public float MaceBonusMultipler
        {
            get => CurrentEntry.MaceBonusMultipler;
            set { CurrentEntry.MaceBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MaceFlat}Mace Flat", -50f, 50f, Order = 5, HintText = "{=RT_MaceFlat_H}Add to Mace Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Mace")]
        public float MaceBonusFlat
        {
            get => CurrentEntry.MaceBonusFlat;
            set { CurrentEntry.MaceBonusFlat = value; Save(); }
        }

        // Polearm
        [SettingPropertyFloatingInteger("{=RT_PoleMult}Polearm Mult", -1f, 5f, Order = 6, HintText = "{=RT_PoleMult_H}Mutliply Polearm Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Polearm")]
        public float PolearmBonusMultipler
        {
            get => CurrentEntry.PolearmBonusMultipler;
            set { CurrentEntry.PolearmBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_PoleFlat}Polearm Flat", -50f, 50f, Order = 7, HintText = "{=RT_PoleFlat_H}Add to Polearm Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Polearm")]
        public float PolearmBonusFlat
        {
            get => CurrentEntry.PolearmBonusFlat;
            set { CurrentEntry.PolearmBonusFlat = value; Save(); }
        }

        // Knife
        [SettingPropertyFloatingInteger("{=RT_KnifeMult}Knife Mult", -1f, 5f, Order = 8, HintText = "{=RT_KnifeMult_H}Mutliply Knife Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Knife")]
        public float KnifeBonusMultipler
        {
            get => CurrentEntry.KnifeBonusMultipler;
            set { CurrentEntry.KnifeBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_KnifeFlat}Knife Flat", -50f, 50f, Order = 9, HintText = "{=RT_KnifeFlat_H}Add to Knife Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Knife")]
        public float KnifeBonusFlat
        {
            get => CurrentEntry.KnifeBonusFlat;
            set { CurrentEntry.KnifeBonusFlat = value; Save(); }
        }

        // Throwing Axe
        [SettingPropertyFloatingInteger("{=RT_TXAMult}Throw Axe Mult", -1f, 5f, Order = 0, HintText = "{=RT_TXAMult_H}Mutliply Throwing Axe Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/ThrowingAxe")]
        public float ThrowingAxeBonusMultipler
        {
            get => CurrentEntry.ThrowingAxeBonusMultipler;
            set { CurrentEntry.ThrowingAxeBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_TXAFlat}Throw Axe Flat", -50f, 50f, Order = 1, HintText = "{=RT_TXAFlat_H}Add to Throwing Axe Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/ThrowingAxe")]
        public float ThrowingAxeBonusFlat
        {
            get => CurrentEntry.ThrowingAxeBonusFlat;
            set { CurrentEntry.ThrowingAxeBonusFlat = value; Save(); }
        }

        // Throwing Knife
        [SettingPropertyFloatingInteger("{=RT_TKMult}Throw Knife Mult", -1f, 5f, Order = 2, HintText = "{=RT_TKMult_H}Mutliply Throwing Knife Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/ThrowingKnife")]
        public float ThrowingKnifeBonusMultipler
        {
            get => CurrentEntry.ThrowingKnifeBonusMultipler;
            set { CurrentEntry.ThrowingKnifeBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_TKFlat}Throw Knife Flat", -50f, 50f, Order = 3, HintText = "{=RT_TKFlat_H}Add to Throwing Knife Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/ThrowingKnife")]
        public float ThrowingKnifeBonusFlat
        {
            get => CurrentEntry.ThrowingKnifeBonusFlat;
            set { CurrentEntry.ThrowingKnifeBonusFlat = value; Save(); }
        }

        // Stone
        [SettingPropertyFloatingInteger("{=RT_StoneMult}Stone Mult", -1f, 5f, Order = 4, HintText = "{=RT_StoneMult_H}Mutliply Stone Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Stone")]
        public float StoneBonusMultipler
        {
            get => CurrentEntry.StoneBonusMultipler;
            set { CurrentEntry.StoneBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_StoneFlat}Stone Flat", -50f, 50f, Order = 5, HintText = "{=RT_StoneFlat_H}Add to Stone Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Stone")]
        public float StoneBonusFlat
        {
            get => CurrentEntry.StoneBonusFlat;
            set { CurrentEntry.StoneBonusFlat = value; Save(); }
        }

        // Javelin
        [SettingPropertyFloatingInteger("{=RT_JavMult}Javelin Mult", -1f, 5f, Order = 6, HintText = "{=RT_JavMult_H}Mutliply Javelin Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Javelin")]
        public float JavelinBonusMultipler
        {
            get => CurrentEntry.JavelinBonusMultipler;
            set { CurrentEntry.JavelinBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_JavFlat}Javelin Flat", -50f, 50f, Order = 7, HintText = "{=RT_JavFlat_H}Add to Javelin Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Javelin")]
        public float JavelinBonusFlat
        {
            get => CurrentEntry.JavelinBonusFlat;
            set { CurrentEntry.JavelinBonusFlat = value; Save(); }
        }

        // Shield
        [SettingPropertyFloatingInteger("{=RT_ShieldMult}Shield Mult", -1f, 5f, Order = 8, HintText = "{=RT_ShieldMult_H}Mutliply Shield Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Shield")]
        public float ShieldBonusMultipler
        {
            get => CurrentEntry.ShieldBonusMultipler;
            set { CurrentEntry.ShieldBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_ShieldFlat}Shield Flat", -50f, 50f, Order = 9, HintText = "{=RT_ShieldFlat_H}Add to Shield Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Damage/Shield")]
        public float ShieldBonusFlat
        {
            get => CurrentEntry.ShieldBonusFlat;
            set { CurrentEntry.ShieldBonusFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_PersuadeMult}Persuasion Chance Mult", -1f, 5f, Order = 0, HintText = "{=RT_PersuadeMult_H}Mutliply Persuasion Chance by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Social")]
        public float PersuasionBonusMultipler
        {
            get => CurrentEntry.PersuasionBonusMultipler;
            set { CurrentEntry.PersuasionBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_PersuadeFlat}Persuasion Chance Flat", -1f, 1f, "#0%", Order = 1, HintText = "{=RT_PersuadeFlat_H}Add to Persuasion Chance this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Social")]
        public float PersuasionBonusFlat
        {
            get => CurrentEntry.PersuasionBonusFlat;
            set { CurrentEntry.PersuasionBonusFlat = value; Save(); }
        }

        [SettingPropertyInteger("{=RT_BecomeOld}Become Old Age", 0, 10000, Order = 0, HintText = "{=RT_BecomeOld_H}At what age hero becomes old and can die from old age.", RequireRestart = false)]
        [SettingPropertyGroup("General/Aging")]
        public int BecomeOldAge
        {
            get => CurrentEntry.BecomeOldAge;
            set { CurrentEntry.BecomeOldAge = value; Save(); }
        }
        [SettingPropertyInteger("{=RT_MaxAge}Max Age", 0, 10000, Order = 1, HintText = "{=RT_MaxAge_H}After this age, hero will die.", RequireRestart = false)]
        [SettingPropertyGroup("General/Aging")]
        public int MaxAge
        {
            get => CurrentEntry.MaxAge;
            set { CurrentEntry.MaxAge = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_AgeMult}Age Growth Multiplier",
            0f, 5f, Order = 2, HintText = "{=RT_AgeMult_H}0 = no aging; 1 = normal; >1 = faster.",
            RequireRestart = false)]
        [SettingPropertyGroup("General/Aging")]
        public float AgeGrowthMultiplier
        {
            get => CurrentEntry.AgeGrowthMultiplier;
            set { CurrentEntry.AgeGrowthMultiplier = value; Save(); }
        }

        [SettingPropertyText("{=RT_SkillLimitMult}Skill Limit Multipliers", Order = 0, HintText = "{=RT_SkillLimitMult_H}Mutliply Skill Limits by (1 + value). [E.g Vigor: 0.15, Cunning: -0.05, etc]", RequireRestart = false)]
        [SettingPropertyGroup("General/Skills/Limits")]
        public string SkillLearningLimitBonusMultiplerRaw
        {
            get => CurrentEntry.SkillLearningLimitBonusMultiplerRaw;
            set { CurrentEntry.SkillLearningLimitBonusMultiplerRaw = value; Save(); }
        }
        [SettingPropertyText("{=RT_SkillLimitFlat}Skill Limit Flats", Order = 1, HintText = "{=RT_SkillLimitFlat_H}Add to Skill Limits this value. [E.g Vigor: 0.15, Cunning: -0.05, etc]", RequireRestart = false)]
        [SettingPropertyGroup("General/Skills/Limits")]
        public string SkillLearningLimitBonusFlatRaw
        {
            get => CurrentEntry.SkillLearningLimitBonusFlatRaw;
            set { CurrentEntry.SkillLearningLimitBonusFlatRaw = value; Save(); }
        }
        [SettingPropertyText("{=RT_SkillRateMult}Skill Rate Multipliers", Order = 0, HintText = "{=RT_SkillRateMult_H}Mutliply Skill Rates by (1 + value). [E.g Vigor: 0.15, Cunning: -0.05, etc]", RequireRestart = false)]
        [SettingPropertyGroup("General/Skills/Rates")]
        public string SkillLearningRateBonusMultiplerRaw
        {
            get => CurrentEntry.SkillLearningRateBonusMultiplerRaw;
            set { CurrentEntry.SkillLearningRateBonusMultiplerRaw = value; Save(); }
        }
        [SettingPropertyText("{=RT_SkillRateFlat}Skill Rate Flats", Order = 1, HintText = "{=RT_SkillRateFlat_H}Add to Skill Rates this value. [E.g Vigor: 0.15, Cunning: -0.05, etc]", RequireRestart = false)]
        [SettingPropertyGroup("General/Skills/Rates")]
        public string SkillLearningRateBonusFlatRaw
        {
            get => CurrentEntry.SkillLearningRateBonusFlatRaw;
            set { CurrentEntry.SkillLearningRateBonusFlatRaw = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_MountHPMult}Mount HP Mult", -1f, 5f, Order = 0, HintText = "{=RT_MountHPMult_H}Mutliply hero's Mount Max HP by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountExtraHitpointsMultipler
        {
            get => CurrentEntry?.MountExtraHitpointsMultipler ?? 0.0f;
            set { CurrentEntry.MountExtraHitpointsMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountHPFlat}Mount HP Flat", -1000f, 1000f, Order = 1, HintText = "{=RT_MountHPFlat_H}Add to hero's Mount Max HP this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountExtraHitpointsFlat
        {
            get => CurrentEntry?.MountExtraHitpointsFlat ?? 0.0f;
            set { CurrentEntry.MountExtraHitpointsFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountManMult}Mount Maneuver Mult", -1f, 5f, Order = 2, HintText = "{=RT_MountManMult_H}Mutliply hero's Mount Maneuver by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountManeuverBonusMultipler
        {
            get => CurrentEntry?.MountManeuverBonusMultipler ?? 0.0f;
            set { CurrentEntry.MountManeuverBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountManFlat}Mount Maneuver Flat", -50f, 50f, Order = 3, HintText = "{=RT_MountManFlat_H}Add to hero's Mount Maneuver this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountManeuverBonusFlat
        {
            get => CurrentEntry?.MountManeuverBonusFlat ?? 0.0f;
            set { CurrentEntry.MountManeuverBonusFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountSpeedMult}Mount Speed Mult", -1f, 5f, Order = 4, HintText = "{=RT_MountSpeedMult_H}Mutliply hero's Mount Speed by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountSpeedBonusMultipler
        {
            get => CurrentEntry?.MountSpeedBonusMultipler ?? 0.0f;
            set { CurrentEntry.MountSpeedBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountSpeedFlat}Mount Speed Flat", -50f, 50f, Order = 5, HintText = "{=RT_MountSpeedFlat_H}Add to hero's Mount Speed this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountSpeedBonusFlat
        {
            get => CurrentEntry?.MountSpeedBonusFlat ?? 0.0f;
            set { CurrentEntry.MountSpeedBonusFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountChargeMult}Mount Charge Mult", -1f, 5f, Order = 6, HintText = "{=RT_MountChargeMult_H}Mutliply hero's Mount Charge Damage by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountChargeBonusMultipler
        {
            get => CurrentEntry?.MountChargeBonusMultipler ?? 0.0f;
            set { CurrentEntry.MountChargeBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountChargeFlat}Mount Charge Flat", -50f, 50f, Order = 7, HintText = "{=RT_MountChargeFlat_H}Add to hero's Mount Charge Damage this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountChargeBonusFlat
        {
            get => CurrentEntry?.MountChargeBonusFlat ?? 0.0f;
            set { CurrentEntry.MountChargeBonusFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountDashMult}Mount Dash Mult", -1f, 5f, Order = 8, HintText = "{=RT_MountDashMult_H}Mutliply hero's Mount Dash Acceleration by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountDashBonusMultipler
        {
            get => CurrentEntry?.MountDashBonusMultipler ?? 0.0f;
            set { CurrentEntry.MountDashBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountDashFlat}Mount Dash Flat", -50f, 50f, Order = 9, HintText = "{=RT_MountDashFlat_H}Add to hero's Mount Dash Acceleration this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountDashBonusFlat
        {
            get => CurrentEntry?.MountDashBonusFlat ?? 0.0f;
            set { CurrentEntry.MountDashBonusFlat = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountDiffMult}Mount Difficulty Mult", -1f, 5f, Order = 10, HintText = "{=RT_MountDiffMult_H}Mutliply hero's Mount Difficulty by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountDifficultyBonusMultipler
        {
            get => CurrentEntry?.MountDifficultyBonusMultipler ?? 0.0f;
            set { CurrentEntry.MountDifficultyBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_MountDiffFlat}Mount Difficulty Flat", -50f, 50f, Order = 11, HintText = "{=RT_MountDiffFlat_H}Add to hero's Mount Difficulty this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Mount")]
        public float MountDifficultyBonusFlat
        {
            get => CurrentEntry?.MountDifficultyBonusFlat ?? 0.0f;
            set { CurrentEntry.MountDifficultyBonusFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_BattleSurvMult}Battle Survival Mult", -1f, 5f, Order = 0, HintText = "{=RT_BattleSurvMult_H}Mutliply Battle Survival Chance (not die after battle) by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle")]
        public float BattleSurvivalBonusMultipler
        {
            get => CurrentEntry?.BattleSurvivalBonusMultipler ?? 0.0f;
            set
            {
                CurrentEntry.BattleSurvivalBonusMultipler = value; Save();
            }
        }
        [SettingPropertyFloatingInteger("{=RT_BattleSurvFlat}Battle Survival Flat", -1f, 1f, "#0%", Order = 1, HintText = "{=RT_BattleSurvFlat_H}Add to Battle Survival Chance (not die after battle) this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Battle")]
        public float BattleSurvivalBonusFlat
        {
            get => CurrentEntry?.BattleSurvivalBonusFlat ?? 0.0f;
            set { CurrentEntry.BattleSurvivalBonusFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_FoodMult}Food Consumption Mult", -1f, 5f, Order = 0, HintText = "{=RT_FoodMult_H}Mutliply Food Consumption by (1 + value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Logistics/Food")]
        public float FoodConsumptionBonusMultipler
        {
            get => CurrentEntry?.FoodConsumptionBonusMultipler ?? 0.0f;
            set { CurrentEntry.FoodConsumptionBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_FoodFlat}Food Consumption Flat", -50f, 50f, Order = 1, HintText = "{=RT_FoodFlat_H}Add to Food Consumption this value", RequireRestart = false)]
        [SettingPropertyGroup("General/Logistics/Food")]
        public float FoodConsumptionBonusFlat
        {
            get => CurrentEntry?.FoodConsumptionBonusFlat ?? 0.0f;
            set { CurrentEntry.FoodConsumptionBonusFlat = value; Save(); }
        }

        [SettingPropertyFloatingInteger("{=RT_NightMult}Night Bonuses (Day Minuses) Stats Mult", -1f, 5f, Order = 0, HintText = "{=RT_NightMult_H}Mutliply Stats by: if Night (1 + value), if Day (1 - value)", RequireRestart = false)]
        [SettingPropertyGroup("General/Environment/Night")]
        public float NightStatsBonusMultipler
        {
            get => CurrentEntry?.NightStatsBonusMultipler ?? 0.0f;
            set { CurrentEntry.NightStatsBonusMultipler = value; Save(); }
        }
        [SettingPropertyFloatingInteger("{=RT_NightFlat}Night Bonuses (Day Minuses) Stats Flat", -50f, 50f, Order = 1, HintText = "{=RT_NightFlat_H}Add to Stats this value if Night, if Day substract", RequireRestart = false)]
        [SettingPropertyGroup("General/Environment/Night")]
        public float NightStatsBonusFlat
        {
            get => CurrentEntry?.NightStatsBonusFlat ?? 0.0f;
            set { CurrentEntry.NightStatsBonusFlat = value; Save(); }
        }
    }
}
