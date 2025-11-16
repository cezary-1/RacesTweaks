using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;
using System;
using SandBox.GameComponents;
using TaleWorlds.Library;
using TaleWorlds.Core;
using System.Linq;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using System.Collections.Generic;

namespace RacesTweaks
{
    //growth age mult
    public static class AgingTracker
    {
        private static readonly HashSet<Hero> heroes = new HashSet<Hero>();
        private static int timestamp;

        public static void Clear()
        {
            heroes.Clear();
            timestamp = 0;
        }

        public static void AddHero(Hero hero)
        {
            var s = RaceTweaksGlobalSettings.Instance;
            if (s == null) return;
            if (hero.Age >= s.AgeForGrowth)
                heroes.Add(hero);
        }

        public static void DailyUpdate()
        {
            int newTimestamp = (int)CampaignTime.Now.ToDays;
            if (newTimestamp == timestamp) return;
            timestamp = newTimestamp;

            // compute the base multiplier from compatibility setting:
            // R_other = CampaignTime.DaysInYear / DaysPerHeroYearForCompatibility
            var s = RaceTweaksGlobalSettings.Instance;
            if (s == null) return;
            float daysPerYearCompat = s.DaysPerHeroYearForCompatibility;
            if (daysPerYearCompat <= 0f) daysPerYearCompat = (float)CampaignTime.DaysInYear;
            float R_other = (float)CampaignTime.DaysInYear / daysPerYearCompat;

            foreach (var hero in heroes)
            {
                if (hero.Age < s.AgeForGrowth) continue;
                ApplyRaceAging(hero, R_other);
            }
        }

        private static void ApplyRaceAging(Hero hero, float baseMultiplier /* = R_other */)
        {
            if (hero == null) return;
            var faceGen = HarmonyPatches.RaceTweaksFaceGenHolder.Instance;
            if (faceGen is null) return;   // sanity

            // now pull the private field off *that* instance:
            var raceNamesDictionary = AccessTools
                         .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                         .GetValue(faceGen)
                       as Dictionary<string, int>;
            if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;
            // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
            var raceId = hero.CharacterObject?.Race;

            if (raceId == null || raceId < 0) return;
            // pull that race’s settings from MCM
            var raceString = raceNamesDictionary
              .Where(kv => kv.Value == raceId)
              .Select(kv => kv.Key)
              .FirstOrDefault();
            if (raceString == null) return;
            var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

            if (s == null) return;

            float R_race = s.AgeGrowthMultiplier;
            if (Math.Abs(R_race - 1f) < 0.00001f && Math.Abs(baseMultiplier - 1f) < 0.00001f)
                return; // no change

            // compute the required per-day shift for this race so final R_total = R_other * R_race
            // S_race = R_other * (R_race - 1)
            float S_race = baseMultiplier * (R_race - 1f);

            if (Math.Abs(S_race) < 0.00001f) return;

            // LifeIsShort subtracts S (makes birth earlier for S>0)
            var newBirthDays = (float)hero.BirthDay.ToDays - S_race;
            hero.SetBirthDay(CampaignTime.Days(newBirthDays));
        }
    }

    public static class HarmonyPatches
    {
        // 1) A place to hold the single FaceGen instance:
        internal static class RaceTweaksFaceGenHolder
        {
            public static TaleWorlds.MountAndBlade.FaceGen Instance;
        }

        public static class RaceTweaksHelpers
        {
            public static int GetHeroTier(CharacterObject character)
            {
                return MathF.Min(MathF.Max(MathF.Ceiling(((float)character.Level - 5f) / 5f), 0), Campaign.Current.Models.CharacterStatsModel.MaxCharacterTier);
            }
        }

        // 2) Harmony patch on its ctor to grab it:
        [HarmonyPatch(typeof(TaleWorlds.MountAndBlade.FaceGen))]
        class FaceGenConstructorPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(MethodType.Constructor)]
            public static void CtorPostfix(TaleWorlds.MountAndBlade.FaceGen __instance)
            {
                RaceTweaksFaceGenHolder.Instance = __instance;
            }
        }


            private static Dictionary<string, double> ParseLenientMap(string raw)
        {
            var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(raw))
                return dict;

            // 1) split on commas
            var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                // 2) find the first colon
                var idx = part.IndexOf(':');
                if (idx < 0)
                    continue; // no colon, skip

                // 3) trim key and value
                var key = part.Substring(0, idx).Trim();
                var value = part.Substring(idx + 1).Trim();

                if (key.Length == 0)
                    continue;

                // 4) parse the number
                if (double.TryParse(value, out var d))
                    dict[key] = d;
            }

            return dict;
        }


        //MaxHitpoints for characters
        [HarmonyPatch(typeof(DefaultCharacterStatsModel))]
        class DefaultCharacterStatsModelPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(DefaultCharacterStatsModel.MaxHitpoints))]
            public static void MaxHitpoints(
                ref ExplainedNumber __result,
                CharacterObject character,
                bool includeDescriptions = false)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;
                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = character?.Race;

                
                if (raceId < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r=> r.RaceId == raceString).FirstOrDefault();
                
                if (s != null)
                {
                    if (s.MaxHitpointsMultipler != 0f)
                    {
                        // never go negative:
                        var ratio = MathF.Max(-1f, s.MaxHitpointsMultipler);
                        // add percent
                        var factor = __result.ResultNumber * ratio;
                        __result.Add((float)factor,
                            new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));
                        /*
                        if (s.Debug)
                            InformationManager.DisplayMessage(
                                new InformationMessage(
                                    $"RaceTweaks hp applied ×{ratio:P0} for {character.Name}"));
                        */
                    }
                    if (s.MaxHitpointsFlat != 0f)
                    {
                        __result.Add((float)s.MaxHitpointsFlat,
                            new TextObject("{=RaceTweaksFlat}RaceTweaks Flat:"));
                        /*
                        if (s.Debug)
                            InformationManager.DisplayMessage(
                                new InformationMessage(
                                    $"RaceTweaks hp applied +{s.MaxHitpointsFlat:P0} for {character.Name}"));
                        */
                    }
                    __result.LimitMin(1f);
                }

            }
        }


        //Abilities

        [HarmonyPatch(typeof(SandboxAgentStatCalculateModel))]
        class AgentsStatsPatch
        {

            [HarmonyPostfix]
            [HarmonyPatch(nameof(SandboxAgentStatCalculateModel.UpdateAgentStats))]
            public static void Postfix_UpdateAgentSpeed(
                Agent agent,
                AgentDrivenProperties agentDrivenProperties)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    if (agent is null)
                        return;

                    if (!agent.IsHuman)
                        return;

                    if (!agent.IsHero)
                        return;
                    var character = agent.Character;

                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = character?.Race;


                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    //Speed in battle
                    if (character != null
                     && s != null)
                    {
                        //Multipler

                        if (s.MovementSpeedMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.MovementSpeedMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.MaxSpeedMultiplier *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: MovementSpeed patch: {s.MovementSpeedMultipler}"));
                        }

                        if (s.HandlingMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.HandlingMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.HandlingMultiplier *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: Handling patch: {s.HandlingMultipler}"));
                        }

                        if (s.AccuracyMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.AccuracyMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.WeaponInaccuracy /= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: AccuracyMultipler patch: {s.AccuracyMultipler}"));
                        }

                        if (s.ReloadMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.ReloadMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.ReloadSpeed *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: Reload patch: {s.ReloadMultipler}"));
                        }

                        if (s.SwingMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.SwingMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.SwingSpeedMultiplier *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: Swing patch: {s.SwingMultipler}"));
                        }

                        if (s.ArmorEncumbranceMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.ArmorEncumbranceMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.ArmorEncumbrance *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: ArmorEncumbrance patch: {s.ArmorEncumbranceMultipler}"));
                        }

                        if (s.WeaponsEncumbranceMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.WeaponsEncumbranceMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.WeaponsEncumbrance *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: WeaponsEncumbrance patch: {s.WeaponsEncumbranceMultipler}"));
                        }

                        if (s.ArmorArmsMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.ArmorArmsMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.ArmorArms *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor arms patch: {s.ArmorArmsMultipler}"));
                        }

                        if (s.ArmorHeadMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.ArmorHeadMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.ArmorHead *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor head patch: {s.ArmorHeadMultipler}"));
                        }

                        if (s.ArmorLegsMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.ArmorLegsMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.ArmorLegs *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor legs patch: {s.ArmorLegsMultipler}"));

                        }

                        if (s.ArmorTorsoMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.ArmorTorsoMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.ArmorTorso *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor torso patch: {s.ArmorTorsoMultipler}"));
                        }

                        //Flat

                        if (s.MovementSpeedFlat != 0f)
                        {
                            agentDrivenProperties.MaxSpeedMultiplier = (float)MathF.Max(0f, agentDrivenProperties.MaxSpeedMultiplier + s.MovementSpeedFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: MovementSpeed patch: {s.MovementSpeedFlat}"));
                        }

                        if (s.HandlingFlat != 0f)
                        {
                            agentDrivenProperties.HandlingMultiplier = (float)MathF.Max(0f, agentDrivenProperties.HandlingMultiplier + s.HandlingFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: HandlingFlat patch: {s.HandlingFlat}"));
                        }

                        if (s.AccuracyFlat != 0f)
                        {
                            agentDrivenProperties.WeaponInaccuracy = (float)MathF.Max(0f, agentDrivenProperties.WeaponInaccuracy - s.AccuracyFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: AccuracyFlat patch: {s.AccuracyFlat}"));
                        }

                        if (s.ReloadFlat != 0f)
                        {
                            agentDrivenProperties.ReloadSpeed = (float)MathF.Max(0f, agentDrivenProperties.ReloadSpeed + s.ReloadFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: ReloadFlat patch: {s.ReloadFlat}"));
                        }

                        if (s.SwingFlat != 0f)
                        {
                            agentDrivenProperties.SwingSpeedMultiplier = (float)MathF.Max(0f, agentDrivenProperties.SwingSpeedMultiplier + s.SwingFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: SwingFlat patch: {s.SwingFlat}"));
                        }

                        if (s.ArmorEncumbranceFlat != 0f)
                        {

                            agentDrivenProperties.ArmorEncumbrance = (float)MathF.Max(0f, agentDrivenProperties.ArmorEncumbrance + s.ArmorEncumbranceFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: ArmorEncumbrance patch: {s.ArmorEncumbranceFlat}"));
                        }

                        if (s.WeaponsEncumbranceFlat != 0f)
                        {

                            agentDrivenProperties.WeaponsEncumbrance = (float)MathF.Max(0f, agentDrivenProperties.WeaponsEncumbrance + s.WeaponsEncumbranceFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: WeaponsEncumbrance patch: {s.WeaponsEncumbranceFlat}"));
                        }

                        if (s.ArmorArmsFlat != 0f)
                        {

                            agentDrivenProperties.ArmorArms = (float)MathF.Max(0f, agentDrivenProperties.ArmorArms + s.ArmorArmsFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor arms patch: {s.ArmorArmsFlat}"));
                        }

                        if (s.ArmorHeadFlat != 0f)
                        {

                            agentDrivenProperties.ArmorHead = (float)MathF.Max(0f, agentDrivenProperties.ArmorHead + s.ArmorHeadFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor head patch: {s.ArmorHeadFlat}"));
                        }

                        if (s.ArmorLegsFlat != 0f)
                        {

                            agentDrivenProperties.ArmorLegs = (float)MathF.Max(0f, agentDrivenProperties.ArmorLegs + s.ArmorLegsFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor legs patch: {s.ArmorLegsFlat}"));

                        }

                        if (s.ArmorTorsoFlat != 0f)
                        {

                            agentDrivenProperties.ArmorTorso = (float)MathF.Max(0f, agentDrivenProperties.ArmorTorso + s.ArmorTorsoFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: armor torso patch: {s.ArmorTorsoFlat}"));
                        }

                        //Night/Day patches
                        bool isNight = Campaign.Current?.IsNight ?? false;
                        if (isNight)
                        {
                            if (s.NightStatsBonusMultipler != 0f)
                            {
                                agentDrivenProperties.MaxSpeedMultiplier *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.HandlingMultiplier *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponInaccuracy *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ReloadSpeed *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.SwingSpeedMultiplier *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorEncumbrance *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponsEncumbrance *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorArms *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorHead *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorLegs *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorTorso *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                            }

                            if (s.NightStatsBonusFlat != 0f)
                            {
                                agentDrivenProperties.MaxSpeedMultiplier = (float)MathF.Max(0f, agentDrivenProperties.MaxSpeedMultiplier + s.NightStatsBonusMultipler);
                                agentDrivenProperties.HandlingMultiplier = (float)MathF.Max(0f, agentDrivenProperties.HandlingMultiplier + s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponInaccuracy = (float)MathF.Max(0f, agentDrivenProperties.WeaponInaccuracy - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ReloadSpeed = (float)MathF.Max(0f, agentDrivenProperties.ReloadSpeed + s.NightStatsBonusMultipler);
                                agentDrivenProperties.SwingSpeedMultiplier = (float)MathF.Max(0f, agentDrivenProperties.SwingSpeedMultiplier + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorEncumbrance = (float)MathF.Max(0f, agentDrivenProperties.ArmorEncumbrance - s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponsEncumbrance = (float)MathF.Max(0f, agentDrivenProperties.WeaponsEncumbrance - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorArms = (float)MathF.Max(0f, agentDrivenProperties.ArmorArms + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorHead = (float)MathF.Max(0f, agentDrivenProperties.ArmorHead + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorLegs = (float)MathF.Max(0f, agentDrivenProperties.ArmorLegs + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorTorso = (float)MathF.Max(0f, agentDrivenProperties.ArmorTorso + s.NightStatsBonusMultipler);
                            }
                        }
                        else
                        {
                            if (s.NightStatsBonusMultipler != 0f)
                            {
                                agentDrivenProperties.MaxSpeedMultiplier *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.HandlingMultiplier *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponInaccuracy *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ReloadSpeed *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.SwingSpeedMultiplier *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorEncumbrance *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponsEncumbrance *= (float)MathF.Max(0f, 1f + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorArms *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorHead *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorLegs *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorTorso *= (float)MathF.Max(0f, 1f - s.NightStatsBonusMultipler);
                            }

                            if (s.NightStatsBonusFlat != 0f)
                            {
                                agentDrivenProperties.MaxSpeedMultiplier = (float)MathF.Max(0f, agentDrivenProperties.MaxSpeedMultiplier - s.NightStatsBonusMultipler);
                                agentDrivenProperties.HandlingMultiplier = (float)MathF.Max(0f, agentDrivenProperties.HandlingMultiplier - s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponInaccuracy = (float)MathF.Max(0f, agentDrivenProperties.WeaponInaccuracy + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ReloadSpeed = (float)MathF.Max(0f, agentDrivenProperties.ReloadSpeed - s.NightStatsBonusMultipler);
                                agentDrivenProperties.SwingSpeedMultiplier = (float)MathF.Max(0f, agentDrivenProperties.SwingSpeedMultiplier - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorEncumbrance = (float)MathF.Max(0f, agentDrivenProperties.ArmorEncumbrance + s.NightStatsBonusMultipler);
                                agentDrivenProperties.WeaponsEncumbrance = (float)MathF.Max(0f, agentDrivenProperties.WeaponsEncumbrance + s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorArms = (float)MathF.Max(0f, agentDrivenProperties.ArmorArms - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorHead = (float)MathF.Max(0f, agentDrivenProperties.ArmorHead - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorLegs = (float)MathF.Max(0f, agentDrivenProperties.ArmorLegs - s.NightStatsBonusMultipler);
                                agentDrivenProperties.ArmorTorso = (float)MathF.Max(0f, agentDrivenProperties.ArmorTorso - s.NightStatsBonusMultipler);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage($"RaceTweaks error in battle patch: {ex.Message}"));
                }
            }

        }

        // Health Regeneration Patch
        [HarmonyPatch(typeof(DefaultPartyHealingModel))]
        class PartyHealingPatch
        {
            // Heroes (lord & companions)
            [HarmonyPostfix]
            [HarmonyPatch(nameof(DefaultPartyHealingModel.GetDailyHealingHpForHeroes))]
            public static void Postfix_GetDailyHealingHpForHeroes(
                ref ExplainedNumber __result,
                MobileParty party,
                bool includeDescriptions = false)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                var leader = party.LeaderHero?.CharacterObject;
                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = leader?.Race;


                if (raceId < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                if (leader != null
                 && s != null)
                {
                    if (s.HealthRegenMultipler != 0f)
                    {
                        // never go negative:
                        var ratio = MathF.Max(-1f, s.HealthRegenMultipler);
                        // add percent
                        var factor = __result.ResultNumber * ratio;
                        __result.Add((float)factor,
                            new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));
                    }
                    if (s.HealthRegenFlat != 0f)
                    {
                        __result.Add((float)s.HealthRegenFlat,
                            new TextObject("{=RaceTweaksFlat}RaceTweaks Flat:"));
                    }

                    /*
                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage($"RaceTweaks: health regen patch: {s.HealthRegenFlat}"));
                    */
                    //Night/Day patches
                    bool isNight = Campaign.Current?.IsNight ?? false;
                    if (isNight)
                    {
                        if (s.NightStatsBonusMultipler != 0f)
                        {
                            __result.Add(__result.ResultNumber * (float)MathF.Max(-1f, s.NightStatsBonusMultipler),
                                new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));
                        }
                        if (s.NightStatsBonusFlat != 0f)
                        {
                            __result.Add((float)s.NightStatsBonusFlat,
                                new TextObject("{=RaceTweaksFlat}RaceTweaks Flat:"));
                        }
                    }
                    else
                    {
                        if (s.NightStatsBonusMultipler != 0f)
                        {
                            __result.Add(__result.ResultNumber * -(float)MathF.Max(-1f, s.NightStatsBonusMultipler),
                                new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));
                        }
                        if (s.NightStatsBonusFlat != 0f)
                        {
                            __result.Add(-(float)s.NightStatsBonusFlat,
                                new TextObject("{=RaceTweaksFlat}RaceTweaks Flat:"));
                        }
                    }

                }
            }

            // Regular troops
            [HarmonyPostfix]
            [HarmonyPatch(nameof(DefaultPartyHealingModel.GetDailyHealingForRegulars))]
            public static void Postfix_GetDailyHealingForRegulars(
                ref ExplainedNumber __result,
                MobileParty party,
                bool includeDescriptions = false)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                var leader = party.LeaderHero?.CharacterObject;
                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = leader?.Race;


                if (raceId < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                if (leader != null
                 && s != null)
                {
                    if (s.HealthRegenMultipler != 0f)
                    {
                        // never go negative:
                        var ratio = MathF.Max(-1f, s.HealthRegenMultipler);
                        // add percent
                        var factor = __result.ResultNumber * ratio;
                        __result.Add((float)factor,
                            new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));
                    }
                    if (s.HealthRegenFlat != 0f)
                    {
                        __result.Add((float)s.HealthRegenFlat,
                            new TextObject("{=RaceTweaksFlat}RaceTweaks Flat:"));
                    }
                    /*
                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage($"RaceTweaks: health regen patch: {s.HealthRegenFlat}"));
                    */
                }
            }
        }


        //Damage Patches

        // 1) Damage Absorption (defender)
        [HarmonyPatch(typeof(SandboxAgentApplyDamageModel))]
        static class DamageAbsorbPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(SandboxAgentApplyDamageModel.CalculateDamage))]
            public static void Postfix(
                ref float __result,
                in AttackInformation attackInformation,
                in AttackCollisionData collisionData,
                in MissionWeapon weapon,
                float baseDamage)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    var defender = attackInformation.VictimAgent;
                    var co = defender?.Character;
                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = co?.Race;


                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    if (co != null
                        && s != null)
                    {

                        if (s.DamageAbsorptionMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.DamageAbsorptionMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result /= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                              new InformationMessage(
                                $"RaceTweaks: {co.Name} absorbed {s.DamageAbsorptionMultipler:F1}"));
                        }

                        if (s.DamageAbsorptionFlat != 0f)
                        {
                            __result = (float)MathF.Max(0f, __result - s.DamageAbsorptionFlat);

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                              new InformationMessage(
                                $"RaceTweaks: {co.Name} absorbed {s.DamageAbsorptionFlat:F1}"));
                        }

                    }
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                      new InformationMessage($"[RaceTweaks] Absorb patch error: {ex.Message}"));
                }
            }
        }

        // 2) Weapon-class bonus (attacker)
        [HarmonyPatch(typeof(SandboxAgentApplyDamageModel))]
        static class DamageWeaponPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(SandboxAgentApplyDamageModel.CalculateDamage))]
            public static void Postfix(
                ref float __result,
                in AttackInformation attackInformation,
                in AttackCollisionData collisionData,
                in MissionWeapon weapon,
                float baseDamage)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                // 3) Attacker race settings
                var attackerRace = attackInformation.AttackerAgent?.Character?.Race;;


                if (attackerRace < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == attackerRace)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var settAtk = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                if (attackerRace == null
                 || settAtk == null)
                    return;

                // 3) Fetch WeaponComponentData from MissionWeapon (private field)
                var wcd = weapon.CurrentUsageItem;
                if (wcd == null)
                {
                    if (settAtk.UnarmedAttackDamageMultipler != 0f)
                    {
                        // Multiply the existing speed factor by (1 + delta)
                        var ratio = 1f + settAtk.UnarmedAttackDamageMultipler;
                        // never go negative:
                        ratio = MathF.Max(0f, ratio);
                        __result *= (float)ratio;

                        if (settAtk.Debug)
                            InformationManager.DisplayMessage(new InformationMessage(
                            $"RaceTweaks: applied unarmed bonus: {settAtk.UnarmedAttackDamageMultipler}"));

                        return;
                    }

                    if (settAtk.UnarmedAttackDamageFlat != 0f)
                    {


                        __result += (float)settAtk.UnarmedAttackDamageFlat;
                        __result = MathF.Max(0f, __result);

                        if (settAtk.Debug)
                            InformationManager.DisplayMessage(new InformationMessage(
                            $"RaceTweaks: applied unarmed bonus: {settAtk.UnarmedAttackDamageFlat}"));

                        return;
                    }
                }
                else
                {
                    if (settAtk.Debug)
                        InformationManager.DisplayMessage(new InformationMessage(
                        $"RaceTweaks: applied for {wcd.WeaponClass} bonus"));

                    double ratio;
                    // 5) Apply per‑weapon‐class bonus
                    switch (wcd.WeaponClass)
                    {

                        case WeaponClass.Bow:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.BowBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.BowBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);

                            break;
                        case WeaponClass.Crossbow:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.CrossbowBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.CrossbowBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);


                            break;
                        case WeaponClass.OneHandedSword:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.SwordBonusMultipler + settAtk.OneHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.SwordBonusFlat + (float)settAtk.OneHandedBonusFlat;
                            __result = MathF.Max(0f, __result);

                            break;
                        case WeaponClass.OneHandedAxe:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.AxeBonusMultipler + settAtk.OneHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.AxeBonusFlat + (float)settAtk.OneHandedBonusFlat;
                            __result = MathF.Max(0f, __result);

                            break;
                        case WeaponClass.OneHandedPolearm:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.PolearmBonusMultipler + settAtk.OneHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.PolearmBonusFlat + (float)settAtk.OneHandedBonusFlat;
                            __result = MathF.Max(0f, __result);

                            break;
                        case WeaponClass.Mace:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.MaceBonusMultipler + settAtk.OneHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.MaceBonusFlat + (float)settAtk.OneHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.Dagger:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.KnifeBonusMultipler + settAtk.OneHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.KnifeBonusFlat + (float)settAtk.OneHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.TwoHandedPolearm:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.PolearmBonusMultipler + settAtk.TwoHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.PolearmBonusFlat + (float)settAtk.TwoHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.TwoHandedSword:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.SwordBonusMultipler + settAtk.TwoHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.SwordBonusFlat + (float)settAtk.TwoHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.TwoHandedMace:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.MaceBonusMultipler + settAtk.TwoHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.MaceBonusFlat + (float)settAtk.TwoHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.TwoHandedAxe:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.AxeBonusMultipler + settAtk.TwoHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.AxeBonusFlat + (float)settAtk.TwoHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.Boulder:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.StoneBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.StoneBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.ThrowingAxe:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.ThrowingAxeBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.ThrowingAxeBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.ThrowingKnife:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.ThrowingKnifeBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.ThrowingKnifeBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.Stone:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.StoneBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.StoneBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.Javelin:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.JavelinBonusMultipler + settAtk.RangedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.JavelinBonusFlat + (float)settAtk.RangedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;
                        case WeaponClass.LargeShield:
                        case WeaponClass.SmallShield:
                            // Multiply the existing speed factor by (1 + delta)
                            ratio = 1f + settAtk.ShieldBonusMultipler + settAtk.OneHandedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            __result += (float)settAtk.ShieldBonusFlat + (float)settAtk.OneHandedBonusFlat;
                            __result = MathF.Max(0f, __result);
                            break;

                        default:
                            // no extra bonus
                            break;
                    }
                }





            }

        }

        // 3) Flat bonus to every attack (attacker)
        [HarmonyPatch(typeof(SandboxAgentApplyDamageModel))]
        static class FlatDamagePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(SandboxAgentApplyDamageModel.CalculateDamage))]
            public static void Postfix(
                ref float __result,
                in AttackInformation attackInformation,
                in AttackCollisionData collisionData,
                in MissionWeapon weapon,
                float baseDamage)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                // Attacker race → flat bonus damage:
                var attackerRace = attackInformation.AttackerAgent?.Character?.Race;
                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)


                if (attackerRace < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == attackerRace)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                if (attackerRace != null
                 && s != null)
                {

                    if (s.TotalDamageBonusMultipler != 0f)
                    {
                        // Multiply the existing speed factor by (1 + delta)
                        var ratio = 1f + s.TotalDamageBonusMultipler;
                        // never go negative:
                        ratio = MathF.Max(0f, ratio);
                        __result *= (float)ratio;

                        if (s.Debug)
                            InformationManager.DisplayMessage(
                          new InformationMessage(
                            $"RaceTweaks: x{s.TotalDamageBonusMultipler} damage"));
                    }

                    if (s.TotalDamageBonusFlat != 0f)
                    {
                        __result += (float)s.TotalDamageBonusFlat;

                        if (s.Debug)
                            InformationManager.DisplayMessage(
                          new InformationMessage(
                            $"RaceTweaks: +{s.TotalDamageBonusFlat} damage"));
                    }
                    __result = MathF.Max(0f, __result);

                }
            }
        }


        // Persuasion
        [HarmonyPatch(
          typeof(DefaultPersuasionModel),
          "GetDefaultSuccessChance",
          new Type[] { typeof(PersuasionOptionArgs), typeof(float) }
        )]
        static class DefaultPersuasionModel_Patch
        {
            // Postfix gets the original return in __result:
            static void Postfix(
                ref float __result,
                PersuasionOptionArgs optionArgs,
                float difficultyMultiplier)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    // Who’s persuading?
                    // In this private method Hero.MainHero is used, so we’ll, too:
                    var hero = Hero.MainHero;
                    if (hero?.CharacterObject == null) return;
                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = hero?.CharacterObject?.Race;


                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    // Lookup your config
                    if (s != null)
                    {
                        if (s.PersuasionBonusMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.PersuasionBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);

                            __result = MathF.Clamp(
                                __result * (float)ratio,
                                0.1f,   // same lower bound as original
                                1.0f    // same upper bound
                            );

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage(
                                    $"RaceTweaks: Persuasion x {s.PersuasionBonusMultipler:P0}"));
                        }

                        if (s.PersuasionBonusFlat != 0f)
                        {
                            __result = MathF.Clamp(
                                __result + (float)s.PersuasionBonusFlat,
                                0.1f,   // same lower bound as original
                                1.0f    // same upper bound
                            );

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage(
                                    $"RaceTweaks: Persuasion + {s.PersuasionBonusFlat:P0}"));
                        }


                    }
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks Persuasion patch error: {ex.Message}"));
                }
            }
        }



        // Age

        // Patch BecomeOldAge getter



        // Patch Age Death
        public static class AgeOverrideContext
        {
            // Holds the Hero whose age‐check is in flight. Null otherwise.
            public static Hero CurrentHero;
        }

        // 1) Wrap DailyTickHero so that AgeOverrideContext.CurrentHero is set for its duration
        [HarmonyPatch(
            typeof(AgingCampaignBehavior),
            "DailyTickHero",
            new Type[] { typeof(Hero) }
        )]
        static class DailyTickHero_Wrapper
        {
            [HarmonyPrefix]
            public static void Prefix_SetCurrentHero(Hero hero)
            {
                AgeOverrideContext.CurrentHero = hero;
            }

            [HarmonyPostfix]
            public static void Postfix_ClearCurrentHero(Hero hero)
            {
                // Clear unconditionally (even if hero is null or missing in JSON)
                AgeOverrideContext.CurrentHero = null;
            }
        }


        // 2) Patch DefaultAgeModel.get_BecomeOldAge to return race‐specific thresholds when CurrentHero is set
        [HarmonyPatch(
            typeof(DefaultAgeModel),
            "get_BecomeOldAge"   // the getter for the BecomeOldAge property
        )]
        static class BecomeOldAge_GetterPatch
        {
            [HarmonyPrefix]
            public static bool Prefix_ReturnRaceValue(ref int __result)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return true;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return true;

                var hero = AgeOverrideContext.CurrentHero;
                if (hero == null)
                    return true; // no override; run original getter

                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = hero?.CharacterObject?.Race;


                if (raceId < 0) return true;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return true;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

                if (s != null
                    && s.BecomeOldAge > 18)
                {
                    __result = s.BecomeOldAge;

                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks: BecomeOldAge = {s.BecomeOldAge}"));

                    return false;  // skip original; return our per‐race value
                }

                return true; // no settings for this race; run original
            }
        }
        // 2) Patch DefaultAgeModel.get_MaxAge to return race‐specific thresholds when CurrentHero is set
        [HarmonyPatch(
            typeof(DefaultAgeModel),
            "get_MaxAge"   // the getter for the BecomeOldAge property
        )]
        static class MaxAge_GetterPatch
        {
            [HarmonyPrefix]
            public static bool Prefix_ReturnRaceValue(ref int __result)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return true;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return true;

                var hero = AgeOverrideContext.CurrentHero;
                if (hero == null)
                    return true; // no override; run original getter

                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = hero?.CharacterObject?.Race;
                if (raceId < 0) return true;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return true;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

                if (s != null
                    && s.MaxAge > 18)
                {
                    __result = s.MaxAge;

                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks: BecomeOldAge = {s.MaxAge}"));

                    return false;  // skip original; return our per‐race value
                }

                return true; // no settings for this race; run original
            }
        }

        [HarmonyPatch(typeof(DefaultPregnancyModel))]
        static class PregnancyAgeWindowPatch
        {
            // Signature from Bannerlord 1.2.10:
            // public override float GetDailyChanceOfPregnancyForHero(Hero hero)
            [HarmonyPostfix]
            [HarmonyPatch(nameof(DefaultPregnancyModel.GetDailyChanceOfPregnancyForHero))]
            public static void Postfix_GetDailyChanceOfPregnancyForHero(
                Hero hero,
                ref float __result)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                if (hero?.CharacterObject == null)
                    return; // vanilla

                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = hero?.CharacterObject?.Race;


                if (raceId < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

                if (s == null
                    || s.MinPregnancyAge <= 0
                    || s.MaxPregnancyAge <= 0
                    || s.MinPregnancyAge >= s.MaxPregnancyAge)
                {
                    return; // vanilla
                }

                // children + clan factors (same as vanilla)
                var clanTier = hero?.Clan?.Tier ?? RaceTweaksHelpers.GetHeroTier(hero.CharacterObject);

                int numChildren = hero.Children.Count + 1;
                float clanFactor = 4f + 4f * clanTier;
                int livingLords = hero?.Clan?.Lords?.Count(h => h.IsAlive) ?? 2 + hero.Children.Where(h => h.Clan == hero.Clan).ToList().Count;
                float spouseFactor =
                    (hero != Hero.MainHero && hero.Spouse != Hero.MainHero)
                    ? Math.Min(1f, (2f * clanFactor - livingLords) / clanFactor)
                    : 1f;

                // Dynamic slope so age 18→1000 maps 1.2→0
                float slope = 1.2f / (s.MaxPregnancyAge - s.MinPregnancyAge);
                float slopeFact = 1.2f - (hero.Age - s.MinPregnancyAge) * slope;

                // If outside [Min,Max], zero chance
                if (hero.Spouse == null
                    || slopeFact <= 0f)
                {
                    __result = 0f;
                    return;
                }

                // Compute baseChance exactly as vanilla does:
                float baseChance = slopeFact
                    / (numChildren * numChildren)
                    * 0.12f
                    * spouseFactor;

                // Apply “Virile” perk if present:
                var explained = new ExplainedNumber(baseChance, false, null);
                if (hero.GetPerkValue(DefaultPerks.Charm.Virile)
                    || hero.Spouse.GetPerkValue(DefaultPerks.Charm.Virile))
                {
                    explained.AddFactor(
                        DefaultPerks.Charm.Virile.PrimaryBonus,
                        DefaultPerks.Charm.Virile.Name);
                }

                if (s.Debug)
                    InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"RaceTweaks: Hero = {hero.Name} MinPregnancyAge = {s.MinPregnancyAge} MaxPregnancyAge = {s.MaxPregnancyAge}"));

                __result = explained.ResultNumber;
                __result = MathF.Max(0f, __result);

            }
        }

        // Pregnancy Chance Patch
        // Postfix‑patch DefaultPregnancyModel.GetDailyChanceOfPregnancyForHero
        [HarmonyPatch(typeof(DefaultPregnancyModel), nameof(DefaultPregnancyModel.GetDailyChanceOfPregnancyForHero))]
        static class PregnancyChancePostfix
        {
            public static void Postfix(Hero hero, ref float __result)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                if (hero?.CharacterObject != null)
                {
                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = hero?.CharacterObject?.Race;


                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    // look up your per‐race settings
                    if (s != null)
                    {

                        if (s.PregnancyChanceMultipler != 0f)
                        {
                            // override whatever the base model computed
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + s.PregnancyChanceMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                              new InformationMessage($"[RaceTweaks] -> pregnancy chance x {s.PregnancyChanceMultipler} = {__result}"));
                        }

                        if (s.PregnancyChanceFlat != 0f)
                        {
                            __result += (float)s.PregnancyChanceFlat;

                            if (s.Debug)
                                InformationManager.DisplayMessage(
                              new InformationMessage($"[RaceTweaks] -> pregnancy chance + {s.PregnancyChanceFlat} = {__result}"));
                        }


                    }
                }

 

                if (hero?.Spouse?.CharacterObject != null)
                {
                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = hero.Spouse.CharacterObject?.Race;


                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var d = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    // look up spouse per‐race settings
                    if (d != null)
                    {

                        if (d.PregnancyChanceMultipler != 0f)
                        {
                            // override whatever the base model computed
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + d.PregnancyChanceMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            if (d.Debug)
                                InformationManager.DisplayMessage(
                              new InformationMessage($"[RaceTweaks] -> pregnancy chance x {d.PregnancyChanceMultipler} = {__result}"));
                        }

                        if (d.PregnancyChanceFlat != 0f)
                        {
                            __result += (float)d.PregnancyChanceFlat;

                            if (d.Debug)
                                InformationManager.DisplayMessage(
                              new InformationMessage($"[RaceTweaks] -> pregnancy chance + {d.PregnancyChanceFlat} = {__result}"));
                        }


                    }
                }

                __result = MathF.Max(0f, __result);
            }
        }


        // Skills
        //Hero for skills
        public static class SkillRateContext
        {
            public static Hero CurrentHero;
        }

        [HarmonyPatch(
            typeof(DefaultCharacterDevelopmentModel),
            nameof(DefaultCharacterDevelopmentModel.CalculateLearningRate),
            new Type[]{
            typeof(Hero),
            typeof(SkillObject)
            }
        )]
        static class SkillRateHero
        {
            [HarmonyPrefix]
            public static void Prefix_SetCurrentHero(Hero hero, SkillObject skill)
            {
                SkillRateContext.CurrentHero = hero;
            }

            [HarmonyPostfix]
            public static void Postfix_ClearCurrentHero(Hero hero, SkillObject skill)
            {
                // Clear unconditionally (even if hero is null or missing in JSON)
                SkillRateContext.CurrentHero = null;
            }
        }


        // 1) XP–Gain Rate (Hero + Skill) override
        [HarmonyPatch(
            typeof(DefaultCharacterDevelopmentModel),
            nameof(DefaultCharacterDevelopmentModel.CalculateLearningRate),
            new Type[]{
            typeof(int),      // attributeValue
            typeof(int),      // focusValue
            typeof(int),      // skillValue
            typeof(int),      // characterLevel
            typeof(TextObject),
            typeof(bool)      // includeDescriptions
            }
        )]
        static class DetailedLearningRateFlatPatch
        {
            [HarmonyPostfix]
            public static void Postfix(
                int attributeValue,
                int focusValue,
                int skillValue,
                int characterLevel,
                TextObject attributeName,
                bool includeDescriptions,
                ref ExplainedNumber __result)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    if (attributeName == null) return;

                    var hero = SkillRateContext.CurrentHero;

                    if (hero?.CharacterObject == null) hero = Hero.MainHero;

                    if (hero?.CharacterObject == null) return;

                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = hero.CharacterObject?.Race;
                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

                    if (s == null)
                        return;


                    var key = attributeName.ToString(); // e.g. "Vigor", "Control"

                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"Attribute chosen: {key}"));

                    var skillMulti = ParseLenientMap(s.SkillLearningRateBonusMultiplerRaw);

                    if (skillMulti.TryGetValue(key, out var multiBonus)
                        )
                    {
                        // never go negative:
                        var ratio = MathF.Max(-1f, multiBonus);
                        // add percent
                        var factor = __result.ResultNumber * ratio;

                        // Add a flat bonus to the explained number:
                        __result.Add(
                            (float)factor,
                            new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler: "
                                           ));

                        // Optional: clamp so rate never goes below zero
                        __result.LimitMin(0f);

                        if (s.Debug)
                            InformationManager.DisplayMessage(
                            new InformationMessage(
                                $"RaceTweaks: {key} learn-rate {multiBonus:F1}"));
                    }

                    var skillFlat = ParseLenientMap(s.SkillLearningRateBonusFlatRaw);
                    if (skillFlat.TryGetValue(key, out var flatBonus)
                        && flatBonus != 0)
                    {
                        // Add a flat bonus to the explained number:
                        __result.Add(
                            (float)flatBonus,
                            new TextObject("{=RaceTweaksFlat}RaceTweaks Flat: "
                                           ));

                        // Optional: clamp so rate never goes below zero
                        __result.LimitMin(0f);

                        if (s.Debug)
                            InformationManager.DisplayMessage(
                            new InformationMessage(
                                $"RaceTweaks: {key} learn-rate {flatBonus:F1}"));
                    }
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks LearnRate patch error: {ex.Message}"));
                }
            }
        }

        [HarmonyPatch(
            typeof(DefaultCharacterDevelopmentModel),
            nameof(DefaultCharacterDevelopmentModel.CalculateLearningLimit),
            new Type[] { typeof(int), typeof(int), typeof(TextObject), typeof(bool) }
        )]
        static class LearningLimitPatch
        {
            public static void Postfix(
                int attributeValue,
                int focusValue,
                TextObject attributeName,
                bool includeDescriptions,
                ref ExplainedNumber __result)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    // only patch when the game is actually building a tooltip
                    if (!includeDescriptions || attributeName == null)
                        return;

                    var hero = SkillRateContext.CurrentHero;
                    if (hero?.CharacterObject == null) hero = Hero.MainHero;

                    if (hero?.CharacterObject == null) return;

                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = hero.CharacterObject?.Race;
                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

                    if (s == null)
                        return;

                    // attributeName.ToString() gives the un-localized key, e.g. "Vigor"
                    var attrKey = attributeName.ToString();
                    var skillMulti = ParseLenientMap(s.SkillLearningLimitBonusMultiplerRaw);
                    if (skillMulti.TryGetValue(attrKey, out var multibonus) && multibonus != 0)
                    {
                        // never go negative:
                        var ratio = MathF.Max(-1f, multibonus);
                        // add percent
                        var factor = __result.ResultNumber * ratio;

                        __result.Add(
                            (float)factor,
                            new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));

                        if (s.Debug)
                            InformationManager.DisplayMessage(
                            new InformationMessage(
                                $"RaceTweaks: {attrKey} learning-cap {multibonus}"));
                    }

                    var skillFlat = ParseLenientMap(s.SkillLearningRateBonusFlatRaw);
                    if (skillFlat.TryGetValue(attrKey, out var bonus) && bonus != 0)
                    {
                        __result.Add(
                            (float)bonus,
                            new TextObject("{=RaceTweaksFlat}RaceTweaks Flat: "
                                           + attrKey + " cap "));

                        if (s.Debug)
                            InformationManager.DisplayMessage(
                            new InformationMessage(
                                $"RaceTweaks: {attrKey} learning-cap {bonus}"));
                    }
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks LearningLimit patch error: {ex.Message}"));
                }
            }
        }

        //Mount Bonuses

        [HarmonyPatch(typeof(SandboxAgentStatCalculateModel))]
        class MountPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(SandboxAgentStatCalculateModel.UpdateAgentStats))]
            public static void Postfix_UpdateAgentStats(
                Agent agent,
                AgentDrivenProperties agentDrivenProperties)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    if (agent is null)
                        return;

                    if (!agent.IsMount)
                        return;

                    if (agent.RiderAgent is null)
                        return;

                    if (!agent.RiderAgent.IsHuman)
                        return;

                    if (!agent.RiderAgent.IsHero)
                        return;

                    var character = agent.RiderAgent.Character;

                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = character?.Race;
                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var m = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    if (character != null
                     && m != null)
                    {
                        //Multi
                        //Manuver in battle
                        if (m.MountManeuverBonusMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + m.MountManeuverBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.MountManeuver *= (float)ratio;

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount charge: {m.MountManeuverBonusMultipler}"));
                        }
                        //Speed in battle
                        if (m.MountSpeedBonusMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + m.MountSpeedBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.MountSpeed *= (float)ratio;

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount speed: {m.MountSpeedBonusMultipler}"));
                        }
                        //Charge in battle
                        if (m.MountChargeBonusMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + m.MountChargeBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.MountChargeDamage *= (float)ratio;

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount charge: {m.MountChargeBonusMultipler}"));
                        }
                        //Dash in battle
                        if (m.MountDashBonusMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + m.MountDashBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.MountDashAccelerationMultiplier *= (float)ratio;

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount dash: {m.MountDashBonusMultipler}"));
                        }
                        //Difficulty in battle
                        if (m.MountDifficultyBonusMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + m.MountDifficultyBonusMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            agentDrivenProperties.MountDifficulty *= (float)ratio;

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount Difficulty: {m.MountDifficultyBonusMultipler}"));
                        }

                        //Flat
                        //Manuver in battle
                        if (m.MountManeuverBonusFlat != 0f)
                        {
                            agentDrivenProperties.MountManeuver += (float)m.MountManeuverBonusFlat;
                            agentDrivenProperties.MountManeuver = MathF.Max(0f, agentDrivenProperties.MountManeuver);

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount manuver: {m.MountManeuverBonusFlat}"));
                        }
                        //Speed in battle
                        if (m.MountSpeedBonusFlat != 0f)
                        {
                            agentDrivenProperties.MountSpeed += (float)m.MountSpeedBonusFlat;
                            agentDrivenProperties.MountSpeed = MathF.Max(0f, agentDrivenProperties.MountSpeed);

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount speed: {m.MountSpeedBonusFlat}"));
                        }
                        //Charge in battle
                        if (m.MountChargeBonusFlat != 0f)
                        {
                            agentDrivenProperties.MountChargeDamage += (float)m.MountChargeBonusFlat;
                            agentDrivenProperties.MountChargeDamage = MathF.Max(0f, agentDrivenProperties.MountChargeDamage);

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount charge: {m.MountChargeBonusFlat}"));
                        }
                        //Dash in battle
                        if (m.MountDashBonusFlat != 0f)
                        {
                            agentDrivenProperties.MountDashAccelerationMultiplier += (float)m.MountDashBonusFlat;
                            agentDrivenProperties.MountDashAccelerationMultiplier = MathF.Max(0f, agentDrivenProperties.MountDashAccelerationMultiplier);

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount dash: {m.MountDashBonusFlat}"));
                        }
                        //Difficulty in battle
                        if (m.MountDifficultyBonusFlat != 0f)
                        {
                            agentDrivenProperties.MountDifficulty += (float)m.MountDifficultyBonusFlat;
                            agentDrivenProperties.MountDifficulty = MathF.Max(0f, agentDrivenProperties.MountDifficulty);

                            if (m.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount Difficulty: {m.MountDifficultyBonusFlat}"));
                        }
                    }

                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage($"RaceTweaks error in mount patch: {ex.Message}"));
                }
            }


            [HarmonyPostfix]
            [HarmonyPatch(nameof(SandboxAgentStatCalculateModel.GetEffectiveMaxHealth))]
            public static void Postfix_UpdateAgentHealth(
                ref float __result,
                Agent agent)
            {
                try
                {
                    var faceGen = RaceTweaksFaceGenHolder.Instance;
                    if (faceGen is null) return;   // sanity

                    // now pull the private field off *that* instance:
                    var raceNamesDictionary = AccessTools
                                 .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                                 .GetValue(faceGen)
                               as Dictionary<string, int>;
                    if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;

                    if (agent is null)
                        return;

                    if (!agent.IsMount)
                        return;

                    if (agent.RiderAgent is null)
                        return;

                    if (!agent.RiderAgent.IsHuman)
                        return;

                    if (!agent.RiderAgent.IsHero)
                        return;

                    var character = agent.RiderAgent.Character;
                    // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                    var raceId = character?.Race;
                    if (raceId < 0) return;
                    // pull that race’s settings from MCM
                    var raceString = raceNamesDictionary
                      .Where(kv => kv.Value == raceId)
                      .Select(kv => kv.Key)
                      .FirstOrDefault();
                    if (raceString == null) return;
                    var h = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                    if (character != null
                     && h != null)
                    {
                        if (h.MountExtraHitpointsMultipler != 0f)
                        {
                            // Multiply the existing speed factor by (1 + delta)
                            var ratio = 1f + h.MountExtraHitpointsMultipler;
                            // never go negative:
                            ratio = MathF.Max(0f, ratio);
                            __result *= (float)ratio;

                            if (h.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount hitpoints: {h.MountDifficultyBonusMultipler}"));
                        }

                        if (h.MountExtraHitpointsFlat != 0f)
                        {
                            __result += (float)h.MountExtraHitpointsFlat;

                            if (h.Debug)
                                InformationManager.DisplayMessage(
                                new InformationMessage($"RaceTweaks: mount hitpoints: {h.MountExtraHitpointsFlat}"));
                        }
                        __result = MathF.Max(0.1f, __result);

                    }

                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage($"RaceTweaks error in mount patch: {ex.Message}"));
                }
            }
        }

        //Survival chance patch

        [HarmonyPatch(typeof(DefaultPartyHealingModel))]
        class SurvivalChancePatch
        {

            [HarmonyPostfix]
            [HarmonyPatch(nameof(DefaultPartyHealingModel.GetSurvivalChance))]
            static void Postfix(
            ref float __result,
            PartyBase party,
            CharacterObject character,
            DamageTypes damageType,
            bool canDamageKillEvenIfBlunt,
            PartyBase enemyParty = null)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;
                // only patch if we have a character and a race entry
                if (character == null) return;
                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = character?.Race;
                if (raceId < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();
                if (s == null)
                    return;

                if (s.BattleSurvivalBonusMultipler != 0f)
                {
                    // Multiply the existing speed factor by (1 + delta)
                    var ratio = 1f + s.BattleSurvivalBonusMultipler;
                    // never go negative:
                    ratio = MathF.Max(0f, ratio);

                    // add it, then clamp 0–1
                    __result = (float)MathF.Min(1f, __result * ratio);

                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks: {character.Name} survival chance x{s.BattleSurvivalBonusMultipler:P0} => {__result:P0}"));
                }

                // your new per‑race float in JSON (e.g. 0.25 for +25% survival chance)
                if (s.BattleSurvivalBonusFlat != 0f)
                {
                    // add it, then clamp 0–1
                    __result = (float)MathF.Min(1f, __result + s.BattleSurvivalBonusFlat);

                    if (s.Debug)
                        InformationManager.DisplayMessage(
                        new InformationMessage(
                            $"RaceTweaks: {character.Name} survival chance +{s.BattleSurvivalBonusFlat:P0} => {__result:P0}"));
                }



            }
        }

        //Food Consumption
        [HarmonyPatch(typeof(DefaultMobilePartyFoodConsumptionModel))]
        static class FoodConsumption_PerksPatch
        {
            // Patch the private CalculatePerkEffects(MobileParty, ref ExplainedNumber) method:
            [HarmonyPostfix]
            [HarmonyPatch("CalculatePerkEffects")]
            static void Postfix_CalculatePerkEffects(
                DefaultMobilePartyFoodConsumptionModel __instance,
                MobileParty party,
                ref ExplainedNumber result)
            {
                var faceGen = RaceTweaksFaceGenHolder.Instance;
                if (faceGen is null) return;   // sanity

                // now pull the private field off *that* instance:
                var raceNamesDictionary = AccessTools
                             .Field(typeof(TaleWorlds.MountAndBlade.FaceGen), "_raceNamesDictionary")
                             .GetValue(faceGen)
                           as Dictionary<string, int>;
                if (raceNamesDictionary == null || raceNamesDictionary.Count == 0) return;
                // only patch if this party actually consumes food
                if (!__instance.DoesPartyConsumeFood(party))
                    return;

                var leader = party.LeaderHero?.CharacterObject;
                if (leader == null) return;

                // get the integer ID of the race (you used "0"=Human, "1"=Giant, etc)
                var raceId = leader?.Race;
                if (raceId < 0) return;
                // pull that race’s settings from MCM
                var raceString = raceNamesDictionary
                  .Where(kv => kv.Value == raceId)
                  .Select(kv => kv.Key)
                  .FirstOrDefault();
                if (raceString == null) return;
                var s = RaceTweaksGlobalSettings.Instance?.EntriesList.Where(r => r.RaceId == raceString).FirstOrDefault();

                if (s == null)
                    return;

                if (s.FoodConsumptionBonusMultipler != 0f)
                {
                    // never go negative:
                    var ratio = s.FoodConsumptionBonusMultipler;
                    // add percent
                    var factor = result.ResultNumber * ratio;

                    result.Add((float)factor, new TextObject("{=RaceTweaksMultipler}RaceTweaks Multipler:"));

                }

                if (s.FoodConsumptionBonusFlat != 0f)
                {
                    result.Add((float)s.FoodConsumptionBonusFlat, new TextObject("{=RaceTweaksFlat}RaceTweaks Flat:"));

                }



            }
        }


        //aging growth
        [HarmonyPatch(typeof(Hero), "Init")]
        static class HeroInitAgingPatch
        {
            static void Postfix(Hero __instance)
                => AgingTracker.AddHero(__instance);
        }
        [HarmonyPatch(typeof(Hero), "AfterLoad")]
        static class HeroLoadAgingPatch
        {
            static void Postfix(Hero __instance)
                => AgingTracker.AddHero(__instance);
        }

        [HarmonyPatch(typeof(AgingCampaignBehavior), "DailyTickHero")]
        static class DailyTickAgingPrefix
        {
            static void Prefix(Hero hero)
            {
                // ensure our global clear happened at game start
                AgingTracker.DailyUpdate();
            }
        }
        [HarmonyPatch(typeof(GameManagerBase), "Initialize")]
        static class GameStartPatch
        {
            static void Postfix() => AgingTracker.Clear();
        }

    }
}
