using log4net;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using TMLConfigManager = Terraria.ModLoader.Config.ConfigManager;
using TMLDelegate = System.Delegate;
using TMLItem = Terraria.Item;
using TMLItemTagHandler = Terraria.GameContent.UI.Chat.ItemTagHandler;
using TMLLogging = Terraria.ModLoader.Logging;
using TMLMain = Terraria.Main;
using TMLMod = Terraria.ModLoader.Mod;
using TMLModConfig = Terraria.ModLoader.Config.ModConfig;
using TMLModLoader = Terraria.ModLoader.ModLoader;
using TMLMonoModHooks = Terraria.ModLoader.MonoModHooks;
using TMLPlayer = Terraria.Player;
using TMLProjectileLoader = Terraria.ModLoader.ProjectileLoader;
using TMLPropertyFieldWrapper = Terraria.ModLoader.Config.UI.PropertyFieldWrapper;
using TMLSamplerState = Microsoft.Xna.Framework.Graphics.SamplerState;
using TMLSpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using TMLTextSnippet = Terraria.UI.Chat.TextSnippet;
using TMLUIModConfig = Terraria.ModLoader.Config.UI.UIModConfig;

namespace TigerUtilsLib;

public static partial class TigerUtils {
    public static class TMLPublicization {
        public static class ConfigManager {
            public static void Save(TMLModConfig config) => TMLConfigManager.Save(config);
            public static IDictionary<TMLMod, List<TMLModConfig>> Configs => TMLConfigManager.Configs;
            public static string GetLocalizedTooltip(TMLPropertyFieldWrapper member) => TMLConfigManager.GetLocalizedTooltip(member);
        }
        public static class ItemTagHandler {
            public static TMLTextSnippet NewItemSnippet(TMLItem item) => new TMLItemTagHandler.ItemSnippet(item);
        }
        public static class Logging {
            public static ILog TML => TMLLogging.tML;
            public static ILog Terraria => TMLLogging.Terraria;
        }
        public static class ModLoader {
            public static bool IsLoading => TMLModLoader.isLoading;
        }
        public static class MonoModHooks {
            public static string StringRep(MethodBase m) => TMLMonoModHooks.StringRep(m);
            /// <summary>
            /// <br/>由于使用了 TML 的私有类作为返回值所以在不公开 TML 的库中无法使用,
            /// <br/>请使用 <see cref="GetDetours(Assembly)"/> 和 <see cref="GetIlHooks(Assembly)"/> 代替
            /// </summary>
            public static TMLMonoModHooks.DetourList GetDetourList(Assembly asm) => TMLMonoModHooks.GetDetourList(asm);
            public static List<DetourInfo> GetDetours(Assembly asm) {
                return GetDetourList(asm).detours;
            }
            public static List<ILHookInfo> GetIlHooks(Assembly asm) {
                return GetDetourList(asm).ilHooks;
            }
        }
        public static class Player {
            public static void ItemCheck_Shoot(TMLPlayer player, int whoAmI, TMLItem sItem, int weaponDamage) => player.ItemCheck_Shoot(whoAmI, sItem, weaponDamage);
        }
        public static class SpriteBatch {
            public static TMLSamplerState GetSamplerState(TMLSpriteBatch spriteBatch) => spriteBatch.samplerState;
            /// <summary>
            /// 默认是 <see cref="TMLSamplerState.LinearClamp"/>
            /// 将它设置为 <see cref="TMLSamplerState.PointClamp"/> 可以让像素不再模糊
            /// </summary>
            public static void SetSamplerState(TMLSpriteBatch spriteBatch, TMLSamplerState state) => spriteBatch.samplerState = state;
            public static unsafe void FlushBatch(TMLSpriteBatch spriteBatch) => spriteBatch.FlushBatch();
            public static void PrepRenderState(TMLSpriteBatch spriteBatch) => spriteBatch.PrepRenderState();
        }
        public static class UIModConfig {
            public static string Tooltip {
                get => TMLUIModConfig.Tooltip;
                set => TMLUIModConfig.Tooltip = value;
            }
        }
    }
    public static class TMLReflection {
        #region consts
        public const BindingFlags bfall = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public const BindingFlags bfpi = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags bfps = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags bfni = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags bfns = BindingFlags.NonPublic | BindingFlags.Static;

        public const BindingFlags bfp = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        public const BindingFlags bfn = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public const BindingFlags bfi = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags bfs = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        #endregion

        private static Assembly? assembly;
        public static Assembly Assembly => assembly ??= typeof(Main).Assembly;

        public static class Types {
            static Types() {
                AllTypes = [];
                foreach (var type in typeof(TMLMain).Assembly.GetTypes()) {
                    if (type.FullName != null) {
                        AllTypes.Add(type.FullName, type);
                    }
                }
                UIModConfig = AllTypes["Terraria.ModLoader.Config.UI.UIModConfig"];
                ModCompile = AllTypes["Terraria.ModLoader.Core.ModCompile"];
            }
            public static Dictionary<string, Type> AllTypes { get; private set; }
            public static Type UIModConfig { get; private set; }
            public static Type ModCompile { get; private set; }
        }

        public static class ConfigManager {
            public static readonly Type Type = typeof(TMLConfigManager);
            public static readonly MethodInfo Save = Type.GetMethod(nameof(TMLConfigManager.Save), bfns)!;
        }
        public static class Delegate {
            public static readonly Type Type = typeof(TMLDelegate);
            // _invocationList ?

        }
        public static class Item {
            public static readonly Type Type = typeof(TMLItem);
            public static readonly MethodInfo Clone = Type.GetMethod(nameof(TMLItem.Clone), bfpi)!;
        }
        public static class Main {
            public static readonly Type Type = typeof(TMLMain);
            public static readonly FieldInfo MouseItem = Type.GetField(nameof(TMLMain.mouseItem), bfps)!;
        }
        public static class ModCompile {
            public static readonly Type Type = Types.ModCompile;
            public static readonly PropertyInfo DeveloperMode = Type.GetProperty("DeveloperMode", bfps)!;
            public static readonly MethodInfo FindModSources = Type.GetMethod("FindModSources", bfns)!;
        }
        public static class Player {
            static Player() {
                #region Fields
                AllFields = [];
                foreach (var field in Type.GetFields(bfall)) {
                    AllFields.Add(field.Name, field);
                }
                Inventory = AllFields[nameof(TMLPlayer.inventory)];
                ManaRegen = AllFields[nameof(TMLPlayer.manaRegen)];
                ManaRegenCount = AllFields[nameof(TMLPlayer.manaRegenCount)];
                NebulaLevelMana = AllFields[nameof(TMLPlayer.nebulaLevelMana)];
                NebulaManaCounter = AllFields[nameof(TMLPlayer.nebulaManaCounter)];
                StatMana = AllFields[nameof(TMLPlayer.statMana)];
                StatManaMax = AllFields[nameof(TMLPlayer.statManaMax)];
                StatManaMax2 = AllFields[nameof(TMLPlayer.statManaMax2)];
                #endregion
                #region Methods
                AllMethods = [];
                foreach (var method in Type.GetMethods(bfall)) {
                    AllMethods.AddElement(method.Name, method);
                }
                DropItemCheck = AllMethods[nameof(TMLPlayer.dropItemCheck)][0];
                ItemCheck_Shoot = AllMethods[nameof(TMLPlayer.ItemCheck_Shoot)][0];
                #endregion
            }
            public static readonly Type Type = typeof(TMLPlayer);
            public static readonly Dictionary<string, FieldInfo> AllFields;
            public static readonly Dictionary<string, List<MethodInfo>> AllMethods;
            public static readonly FieldInfo Inventory;
            public static readonly FieldInfo ManaRegen;
            public static readonly FieldInfo ManaRegenCount;
            public static readonly FieldInfo NebulaLevelMana;
            public static readonly FieldInfo NebulaManaCounter;
            public static readonly FieldInfo StatMana;
            public static readonly FieldInfo StatManaMax;
            public static readonly FieldInfo StatManaMax2;
            public static readonly MethodInfo DropItemCheck;
            public static readonly MethodInfo ItemCheck_Shoot;
        }
        public static class ProjectileLoader {
            public static readonly Type Type = typeof(TMLProjectileLoader);
            public static readonly MethodInfo OnSpawn = Type.GetMethod(nameof(TMLProjectileLoader.OnSpawn), bfns)!;
            public delegate void OnSpawnDelegate(Projectile projectile, IEntitySource source);
        }
        public static class UIModConfig {
            public static readonly Type Type = typeof(TMLUIModConfig);
            public static readonly PropertyInfo Tooltip = Type.GetProperty(nameof(TMLUIModConfig.Tooltip), bfps)!;
        }
    }
}