using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace TigerUtilsLib;

public static partial class TigerUtils {
    public static void InitializeTigerUtils(Mod mod) {
        _mod = mod;
        _modName = mod.Name;
    }
    private static Mod? _mod;
    public static Mod ModInstance => _mod!;
    private static string? _modName;
    internal static string ModName => _modName!;
    #region 获取 / 创建 (Mod|Global|Sample)?(Item|Projectile|NPC)
    public static Item NewItem<T>(int stack = 1, int prefix = 0) where T : ModItem => new(ModContent.ItemType<T>(), stack, prefix);
    public static T NewModItem<T>(int stack = 1, int prefix = 0) where T : ModItem => (T)new Item(ModContent.ItemType<T>(), stack, prefix).ModItem;
    public static T NewModItemWithType<T>(int type, int stack = 1, int prefix = 0) where T : ModItem => (T)new Item(type, stack, prefix).ModItem;
    public static T NewGlobalItem<T>(int type, int stack = 1, int prefix = 0) where T : GlobalItem => new Item(type, stack, prefix).GetGlobalItem<T>();
    public static T? NewGlobalItemS<T>(int type, int stack = 1, int prefix = 0) where T : GlobalItem => new Item(type, stack, prefix).TryGetGlobalItem<T>(out var result) ? result : null;
    public static T NewNPCInWorld<T>(IEntitySource source, Vector2 position, int start = 0, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0, int target = 255) where T : ModNPC {
        var npc = NPC.NewNPCDirect(source, position, ModContent.NPCType<T>(), start, ai0, ai1, ai2, ai3, target);
        return (T)npc.ModNPC;
    }
    public static Item SampleItem(int itemID) => ContentSamples.ItemsByType[itemID];
    public static Item SampleItem<T>() where T : ModItem => ContentSamples.ItemsByType[ModContent.ItemType<T>()];
    public static T SampleModItem<T>() where T : ModItem => (T)ContentSamples.ItemsByType[ModContent.ItemType<T>()].ModItem;
    public static Projectile SampleProjectile(int projectileID) => ContentSamples.ProjectilesByType[projectileID];
    public static Projectile SampleModProjectile<T>() where T : ModProjectile => ContentSamples.ProjectilesByType[ModContent.ProjectileType<T>()];
    public static T SampleProjectile<T>() where T : ModProjectile => (T)ContentSamples.ProjectilesByType[ModContent.ProjectileType<T>()].ModProjectile;
    public static NPC SampleNPC(int npcNetID) => ContentSamples.NpcsByNetId[npcNetID];
    public static NPC SampleNPC<T>() where T : ModNPC => ContentSamples.NpcsByNetId[ModContent.NPCType<T>()];
    public static T SampleModNPC<T>() where T : ModNPC => (T)ContentSamples.NpcsByNetId[ModContent.NPCType<T>()].ModNPC;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetGlobalSafe<TGlobal, TResult>(int entityType, ReadOnlySpan<TGlobal> entityGlobals, [NotNullWhen(true)] out TResult? result) where TGlobal : GlobalType<TGlobal> where TResult : TGlobal
        => TryGetGlobalSafe(entityType, entityGlobals, ModContent.GetInstance<TResult>(), out result);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetGlobalSafe<TGlobal, TResult>(int entityType, ReadOnlySpan<TGlobal> entityGlobals, TResult baseInstance, [NotNullWhen(true)] out TResult? result) where TGlobal : GlobalType<TGlobal> where TResult : TGlobal {
        short perEntityIndex = baseInstance.PerEntityIndex;
        //只是加了下面这句中对entityGlobals长度的检查
        //TryGetModPlayer中都有的TryGetGlobal却没有
        if (entityType > 0 && perEntityIndex >= 0 && perEntityIndex < entityGlobals.Length) {
            result = entityGlobals[perEntityIndex] as TResult;
            return result != null;
        }

        if (GlobalTypeLookups<TGlobal>.AppliesToType(baseInstance, entityType)) {
            result = baseInstance;
            return true;
        }

        result = null;
        return false;
    }
    #endregion

    #region 物品相关
    /// <summary>
    /// <br/>本地的手持物品
    /// <br/>包含鼠标上的物品的处理
    /// <br/>比起<see cref="Player.HeldItem"/>, 它就算在鼠标上也可以作出修改, 而且额外带有set访问器
    /// <br/>不过在放在鼠标上的物品不能使用或者失去焦点时反而需要修改<see cref="Player.HeldItem"/>
    /// <br/>保险起见推荐在鼠标上有物品时对此(此时此值为<see cref="Main.mouseItem"/>)和<see cref="Player.HeldItem"/>一并作出修改
    /// <br/>注意不要把它设置为<see langword="null"/>
    /// <br/>另外用之前先检查一下<see cref="Item.IsAir"/>
    /// </summary>
    public static Item LocalRealHeldItem {
        get {
            if (Main.mouseItem.IsNotAirS()) {
                return Main.mouseItem;
            }
            return Main.LocalPlayer.HeldItem;
        }
        set {
            int selected = Main.LocalPlayer.selectedItem;
            if (selected == 58) {
                Main.mouseItem = value;
            }
            else {
                Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] = value;
            }
        }
    }
    /// <summary>
    /// 以更安全的方式调用<see cref="LocalRealHeldItem"/><br/>
    /// 即使<see cref="Main.LocalPlayer"/>为<see langword="null"/>也不会报错
    /// </summary>
    public static Item? LocalRealHeldItemSafe {
        get {
            return Main.LocalPlayer == null || !Main.LocalPlayer.active ? null : LocalRealHeldItem;
        }
        set {
            if (Main.LocalPlayer == null) {
                return;
            }
            if (value == null) {
                LocalRealHeldItem = new();
            }
            else {
                LocalRealHeldItem = value;
            }

        }
    }
    /// <summary>
    /// 在使用<see cref="Item.NewItem"/>后调用以同步
    /// </summary>
    /// <param name="itemWai"></param>
    public static void TrySyncItem(int itemWai, bool noGrabDelay = true) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWai, noGrabDelay.ToInt());
        }
    }
    #endregion
    #region 物块相关
    public static void KillWallWithoutItemDrop(int i, int j, bool fail = false, bool sync = false) {
        // 摘自 WorldGen.KillWall
        if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
            return;

        Tile tile = Main.tile[i, j];
        if (tile == null) {
            tile = new Tile();
            Main.tile[i, j] = tile;
        }

        if (tile.wall <= 0)
            return;

        fail = WorldGen.KillWall_CheckFailure(fail, tile);

        WallLoader.KillWall(i, j, tile.wall, ref fail);

        WorldGen.KillWall_PlaySounds(i, j, tile, fail);
        int num = 10;
        if (fail)
            num = 3;

        WallLoader.NumDust(i, j, tile.wall, fail, ref num);

        for (int k = 0; k < num; k++) {
            WorldGen.KillWall_MakeWallDust(i, j, tile);
        }

        if (fail) {
            WorldGen.SquareWallFrame(i, j);
            return;
        }

        // WorldGen.KillWall_DropItems(i, j, tile);
        tile.wall = 0;
        tile.ClearWallPaintAndCoating();
        WorldGen.SquareWallFrame(i, j);
        if (tile.type >= 0 && TileID.Sets.FramesOnKillWall[tile.type])
            WorldGen.TileFrame(i, j);
        if (sync) {
            NetMessage.SendTileSquare(-1, i, j);
        }
    }
    #endregion
    #region 本地化相关
    public static LocalizedText GetModLocalization(string suffix, Func<string>? makeDefaultValue = null)
        => Language.GetOrRegister(string.Join('.', "Mods", ModName, suffix), makeDefaultValue);
    public static string GetModLocalizedText(string suffix, Func<string>? makeDefaultValue = null)
        => GetModLocalization(suffix, makeDefaultValue).Value;
    #endregion
    #region 同步相关
    public static void WriteIntWithNegativeAsN1(BitWriter bitWriter, BinaryWriter binaryWriter, int value) {
        if (value < 0) {
            bitWriter.WriteBit(true);
            return;
        }
        bitWriter.WriteBit(false);
        binaryWriter.Write7BitEncodedInt(value);
    }
    public static int ReadIntWithNegativeAsN1(BitReader bitReader, BinaryReader binaryReader)
       => bitReader.ReadBit() ? -1 : binaryReader.Read7BitEncodedInt();
    #endregion
    #region Random
    public static class MyRandomT {
        private static UnifiedRandom DefaultUnifiedRandomGetter() => Main.rand;
        /// <summary>
        /// 将double转化为int
        /// 其中小数部分按概率转化为0或1
        /// </summary>
        public static int RandomD2I(double x, UnifiedRandom rand) {
            int floor = (int)Math.Floor(x);
            double delta = x - floor;
            if (rand.NextDouble() < delta) {
                return floor + 1;
            }
            return floor;
        }
        /// <summary>
        /// 将double转化为bool
        /// 当大于1时为真, 小于0时为假
        /// 在中间则按概率
        /// </summary>
        /// <param name="x"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static bool RandomD2B(double x, UnifiedRandom rand) {
            return x > 1 - rand.NextDouble();
        }
        #region 随机多个总和固定的非负数 RandomNonnegetivesWithFixedSum
        public static void RandomNonnegetivesWithFixedSum(int[] result, int sum, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            result[0] = rand.Next(int.MaxValue / count);
            for (int i = 1; i < count; ++i)
                result[i] = result[i - 1] + rand.Next(int.MaxValue / count);
            int newSum = result[count - 1];
            if (newSum == 0)
                return;
            double m = (double)sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)Math.Round(result[i] * m);
            result[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, float[] weights, int sum, UnifiedRandom? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            float[] rs = new float[count];
            rs[0] = rand.NextFloat(weights[0]);
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextFloat(weights[i]);
            float newSum = result[count - 1];
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)MathF.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, Func<int, float> weightByIndex, int sum, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            float[] rs = new float[count];
            rs[0] = rand.NextFloat(weightByIndex(0));
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextFloat(weightByIndex(i));
            float newSum = result[count - 1];
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)MathF.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, double[] weights, int sum, UnifiedRandom? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            double[] rs = new double[count];
            rs[0] = rand.NextDouble(weights[0]);
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextDouble(weights[i]);
            double newSum = result[count - 1];
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)Math.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, Func<int, double> weightByIndex, int sum, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            double[] rs = new double[count];
            rs[0] = rand.NextDouble(weightByIndex(0));
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextDouble(weightByIndex(i));
            double newSum = result[count - 1];
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)Math.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(float[] result, float sum = 1f, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            float newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextFloat();
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(float[] result, float[] weights, float sum = 1f, UnifiedRandom? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            float newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextFloat(weights[i]);
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(float[] result, Func<int, float> weightByIndex, float sum = 1f, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            float newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextFloat(weightByIndex(i));
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(double[] result, double sum = 1.0, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            double newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextDouble();
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(double[] result, double[] weights, double sum = 1.0, UnifiedRandom? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            double newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextDouble(weights[i]);
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(double[] result, Func<int, double> weightByIndex, double sum = 1.0, UnifiedRandom? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= DefaultUnifiedRandomGetter();
            double newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextDouble(weightByIndex(i));
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, int sum, UnifiedRandom? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, float[] weights, int sum, UnifiedRandom? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, Func<int, float> weightByIndex, int sum, UnifiedRandom? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, double[] weights, int sum, UnifiedRandom? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, Func<int, double> weightByIndex, int sum, UnifiedRandom? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        public static float[] RandomNonnegetivesWithFixedSum(int count, float sum, UnifiedRandom? rand = null) {
            float[] result = new float[count];
            RandomNonnegetivesWithFixedSum(result, sum, rand);
            return result;
        }
        public static float[] RandomNonnegetivesWithFixedSum(int count, float[] weights, float sum, UnifiedRandom? rand = null) {
            float[] result = new float[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static float[] RandomNonnegetivesWithFixedSum(int count, Func<int, float> weightByIndex, float sum, UnifiedRandom? rand = null) {
            float[] result = new float[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        public static double[] RandomNonnegetivesWithFixedSum(int count, double sum = 1.0, UnifiedRandom? rand = null) {
            double[] result = new double[count];
            RandomNonnegetivesWithFixedSum(result, sum, rand);
            return result;
        }
        public static double[] RandomNonnegetivesWithFixedSum(int count, double[] weights, double sum = 1.0, UnifiedRandom? rand = null) {
            double[] result = new double[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static double[] RandomNonnegetivesWithFixedSum(int count, Func<int, double> weightByIndex, double sum = 1.0, UnifiedRandom? rand = null) {
            double[] result = new double[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        #endregion
    }
    #endregion
    #region 绘制
    #region SwitchRenderTargetTemporarily
    public static SwitchRenderTargetTemporarilyDisposable SwitchRenderTargetTemporarily(RenderTarget2D target, bool clearOldTarget = false) => new(target, clearOldTarget);
    public static SwitchRenderTargetTemporarilyDisposable SwitchRenderTargetTemporarily(bool clearOldTarget, params RenderTargetBinding[] targets) => new(clearOldTarget, targets);
    public static SwitchRenderTargetTemporarilyDisposable SwitchRenderTargetTemporarily(params RenderTargetBinding[] targets) => new(targets);
    public static SwitchRenderTargetTemporarilyDisposable SwitchRenderTargetTemporarily(RenderTarget2D target, Color clearColor, bool clearOldTarget = false) => new(target, clearColor, clearOldTarget);
    public static SwitchRenderTargetTemporarilyDisposable SwitchRenderTargetTemporarily(Color clearColor, bool clearOldTarget, params RenderTargetBinding[] targets) => new(clearColor, clearOldTarget, targets);
    public static SwitchRenderTargetTemporarilyDisposable SwitchRenderTargetTemporarily(Color clearColor, params RenderTargetBinding[] targets) => new(clearColor, targets);
    public readonly record struct SwitchRenderTargetTemporarilyDisposable(RenderTargetBinding[] OldTargets, bool ClearOldTarget) : IDisposable {
        static SwitchRenderTargetTemporarilyDisposable() {
            MonoModHooks.Add(typeof(GraphicsDevice).GetMethod(nameof(GraphicsDevice.Clear), TMLReflection.bfi, [typeof(ClearOptions), typeof(Vector4), typeof(float), typeof(int)]),
                (Action<GraphicsDevice, ClearOptions, Vector4, float, int> orig, GraphicsDevice self, ClearOptions options, Vector4 color, float depth, int stencil) => {
                    if (!InResettleRenderTargets) {
                        orig(self, options, color, depth, stencil);
                    }
                });
        }
        private static bool InResettleRenderTargets { get; set; }

        public SwitchRenderTargetTemporarilyDisposable(RenderTarget2D target, bool clearOldTarget = false) : this(Main.instance.GraphicsDevice.GetRenderTargets(), clearOldTarget) {
            var graphicsDevice = Main.instance.GraphicsDevice;
            graphicsDevice.SetRenderTarget(target);
        }
        public SwitchRenderTargetTemporarilyDisposable(RenderTarget2D target, Color clearColor, bool clearOldTarget = false) : this(Main.instance.GraphicsDevice.GetRenderTargets(), clearOldTarget) {
            var graphicsDevice = Main.instance.GraphicsDevice;
            graphicsDevice.SetRenderTarget(target);
            graphicsDevice.Clear(clearColor);
        }
        public SwitchRenderTargetTemporarilyDisposable(bool clearOldTarget, params RenderTargetBinding[] targets) : this(Main.instance.GraphicsDevice.GetRenderTargets(), clearOldTarget) {
            var graphicsDevice = Main.instance.GraphicsDevice;
            graphicsDevice.SetRenderTargets(targets);
        }
        public SwitchRenderTargetTemporarilyDisposable(Color clearColor, bool clearOldTarget, params RenderTargetBinding[] targets) : this(Main.instance.GraphicsDevice.GetRenderTargets(), clearOldTarget) {
            var graphicsDevice = Main.instance.GraphicsDevice;
            graphicsDevice.SetRenderTargets(targets);
            graphicsDevice.Clear(clearColor);
        }
        public SwitchRenderTargetTemporarilyDisposable(params RenderTargetBinding[] targets) : this(false, targets) { }
        public SwitchRenderTargetTemporarilyDisposable(Color clearColor, params RenderTargetBinding[] targets) : this(clearColor, false, targets) { }
        public void Dispose() {
            var graphicsDevice = Main.instance.GraphicsDevice;
            InResettleRenderTargets = !ClearOldTarget;
            graphicsDevice.SetRenderTargets(OldTargets);
            InResettleRenderTargets = false;

            /*
            // 不用钩子的写法, 但是不能处理 oldTargets[0] 不是 RenderTarget2D 的情况 (oldTargets[0] 一定是 IRenderTarget, 但 IRenderTarget 的 RenderTargetUsage 是只读的)
            var graphicsDevice = Main.instance.GraphicsDevice;
            RenderTargetUsage oldUsage;
            if (oldTargets.Length == 0) {
                oldUsage = graphicsDevice.PresentationParameters.RenderTargetUsage;
                graphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
                graphicsDevice.SetRenderTargets(null);
                graphicsDevice.PresentationParameters.RenderTargetUsage = oldUsage;
                return;
            }
            if (oldTargets[0].RenderTarget is not RenderTarget2D oldTarget) {
                graphicsDevice.SetRenderTargets(oldTargets);
                return;
            }
            oldUsage = oldTarget.RenderTargetUsage;
            oldTarget.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            graphicsDevice.SetRenderTargets(oldTargets);
            oldTarget.RenderTargetUsage = oldUsage;
            */
        }
    }
    #endregion
    #region SetFilterActivity
    public static void SetFilterActivity(string filterName, bool activity) {
        var filter = Filters.Scene[filterName];
        if (activity) {
            if (!filter.IsActive()) {
                Filters.Scene.Activate(filterName);
            }
        }
        else {
            if (filter.IsActive()) {
                Filters.Scene.Deactivate(filterName);
                if (Filters.Scene._activeFilters.Remove(filter)) {
                    Filters.Scene._activeFilterCount -= 1;
                }
            }
        }
    }
    #endregion
    #region ChatManager 优化
    public static class ChatManagerFix {
        #region 原版代码参照
        public static Vector2 GetStringSize_Vanilla(DynamicSpriteFont font, TextSnippet[] snippets, Vector2 baseScale, float maxWidth = -1f) {
            // Vector2 mousePosition = Main.MouseScreen; // vec
            Vector2 position = Vector2.Zero; // vector
            Vector2 result = Vector2.Zero;
            float spaceWidth = font.MeasureString(" ").X; // x
            float snippetScale; // num = 1f
            float maxSnippetScale = 0f; // num2
            foreach (TextSnippet snippet in snippets) {
                snippet.Update();
                snippetScale = snippet.Scale;
                if (snippet.UniqueDraw(justCheckingString: true, out Vector2 size, null, scale: baseScale.X * snippetScale)) {
                    /*
				    vector.X += size.X * baseScale.X * num;
				    */
                    position.X += size.X;

                    result.X = Math.Max(result.X, position.X);
                    result.Y = Math.Max(result.Y, position.Y + size.Y);
                    continue;
                }

                string[] lines = snippet.Text.Split('\n'); // array
                                                           // string[] array2 = array;
                for (int j = 0; j < lines.Length; j++) {
                    string[] words = lines[j].Split(' '); // array3
                    for (int k = 0; k < words.Length; k++) {
                        if (k != 0)
                            position.X += spaceWidth * baseScale.X * snippetScale;

                        if (maxWidth > 0f) {
                            float num3 = font.MeasureString(words[k]).X * baseScale.X * snippetScale;
                            if (position.X + num3 > maxWidth) {
                                position.X = 0;
                                position.Y += font.LineSpacing * maxSnippetScale * baseScale.Y;
                                result.Y = Math.Max(result.Y, position.Y);
                                maxSnippetScale = 0f;
                            }
                        }

                        if (maxSnippetScale < snippetScale)
                            maxSnippetScale = snippetScale;

                        Vector2 vector2 = font.MeasureString(words[k]);
                        // mousePosition.Between(position, position + vector2);
                        position.X += vector2.X * baseScale.X * snippetScale;
                        result.X = Math.Max(result.X, position.X);
                        result.Y = Math.Max(result.Y, position.Y + vector2.Y);
                    }

                    // Fix spacing with \n and chat tags, #2981
                    // Skips last line from counting, so "text\ntext", wouldn't be measured as "text\ntext\n"
                    /*
                    if (array.Length > 1) {
                    */
                    if (lines.Length > 1 && j < lines.Length - 1) {
                        position.X = 0;
                        position.Y += font.LineSpacing * maxSnippetScale * baseScale.Y;
                        result.Y = Math.Max(result.Y, position.Y);
                        maxSnippetScale = 0f;
                    }
                }
            }

            return result;
        }
        [SuppressMessage("Performance", "SYSLIB1045:转换为“GeneratedRegexAttribute”。")]
        public static Vector2 DrawColorCodedString_Vanilla(SpriteBatch spriteBatch, DynamicSpriteFont font, TextSnippet[] snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale, out int hoveredSnippet, float maxWidth, bool ignoreColors = false) {
            hoveredSnippet = -1; // num
            Vector2 mousePosition = Main.MouseScreen; // vec
            Vector2 positionNow = position; // vector
            Vector2 result = positionNow;
            float x = font.MeasureString(" ").X;
            Color color = baseColor;
            float snippetScale; // num2 = 1f
            float maxSnippetScale = 0f; // num3
            for (int i = 0; i < snippets.Length; i++) {
                TextSnippet textSnippet = snippets[i];
                textSnippet.Update();
                if (!ignoreColors)
                    color = textSnippet.GetVisibleColor();

                snippetScale = textSnippet.Scale;

                /*
                if (textSnippet.UniqueDraw(justCheckingString: false, out var size, spriteBatch, vector, color, num2)) {
                */
                if (textSnippet.UniqueDraw(justCheckingString: false, out var size, spriteBatch, positionNow, color, baseScale.X * snippetScale)) {
                    if (mousePosition.Between(positionNow, positionNow + size))
                        hoveredSnippet = i;

                    /*
                    vector.X += size.X * baseScale.X * num2;
                    */
                    positionNow.X += size.X;

                    result.X = Math.Max(result.X, positionNow.X);
                    continue;
                }

                string[] lines = Regex.Split(textSnippet.Text, "(\n)"); // array
                bool flag = true;
                foreach (string text in lines) {
                    string[] words = text.Split(' '); // array2
                    if (text == "\n") {
                        positionNow.Y += font.LineSpacing * maxSnippetScale * baseScale.Y;
                        positionNow.X = position.X;
                        result.Y = Math.Max(result.Y, positionNow.Y);
                        maxSnippetScale = 0f;
                        flag = false;
                        continue;
                    }

                    for (int k = 0; k < words.Length; k++) {
                        if (k != 0)
                            positionNow.X += x * baseScale.X * snippetScale;

                        if (maxWidth > 0f) {
                            float num4 = font.MeasureString(words[k]).X * baseScale.X * snippetScale;
                            if (positionNow.X - position.X + num4 > maxWidth) {
                                positionNow.X = position.X;
                                positionNow.Y += font.LineSpacing * maxSnippetScale * baseScale.Y;
                                result.Y = Math.Max(result.Y, positionNow.Y);
                                maxSnippetScale = 0f;
                            }
                        }

                        if (maxSnippetScale < snippetScale)
                            maxSnippetScale = snippetScale;

                        spriteBatch.DrawString(font, words[k], positionNow, color, rotation, origin, baseScale * textSnippet.Scale * snippetScale, SpriteEffects.None, 0f);
                        Vector2 vector2 = font.MeasureString(words[k]);
                        if (mousePosition.Between(positionNow, positionNow + vector2))
                            hoveredSnippet = i;

                        positionNow.X += vector2.X * baseScale.X * snippetScale;
                        result.X = Math.Max(result.X, positionNow.X);
                    }

                    if (lines.Length > 1 && flag) {
                        positionNow.Y += font.LineSpacing * maxSnippetScale * baseScale.Y;
                        positionNow.X = position.X;
                        result.Y = Math.Max(result.Y, positionNow.Y);
                        maxSnippetScale = 0f;
                    }
                    flag = true;
                }
            }
            return result;
        }
        #endregion

        #region Parse
        public static List<TextSnippet> Parse(string text, Color baseColor, bool convertNormalSnippets = false) {
            var snippets = ChatManager.ParseMessage(text, baseColor);
            if (convertNormalSnippets) {
                ConvertNormalSnippets(snippets);
            }
            return snippets;
        }
        #endregion
        #region ConvertNormalSnippets
        public static T ConvertNormalSnippets<T>(T snippets) where T : IList<TextSnippet> {
            for (int i = 0; i < snippets.Count; i++) {
                TextSnippet snippet = snippets[i];
                if (snippets[i].GetType() == typeof(TextSnippet)) {
                    PlainTagHandler.PlainSnippet plainSnippet = new(snippet.Text, snippet.Color, snippet.Scale);
                    snippets[i] = plainSnippet;
                }
            }
            return snippets;
        }
        #endregion
        #region GetStringSize
        public static Vector2 GetStringSize(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, float maxWidth,
            ref Vector2 positionNow, ref float maxLineHeightNow) {
            #region 局部变量初始化
            Vector2 position = Vector2.Zero;
            Vector2 positionNowInner = positionNow;
            Vector2 result = Vector2.Zero;
            TextSnippet snippet = null!;
            float scale = 1;
            float maxLineHeightInner = maxLineHeightNow;
            bool justNewLine = true;
            int i = 0;
            #endregion
            void NewLine(bool unique) { }
            void DrawString(string str, Vector2 size) { }
            void UniqueDraw(TextSnippet ts, Vector2 size) { }
            void AfterUpdateSnippet(TextSnippet ts) { }
            void PostHandleNonUniqueSnippet(TextSnippet ts) { }
            void FinalProcess() { }
            HandleSnippets(font, snippets, Vector2.Zero, baseScale, maxWidth,
                ref positionNowInner, ref result, ref scale, ref maxLineHeightInner, ref justNewLine, ref snippet, ref i,
                NewLine, DrawString, UniqueDraw, AfterUpdateSnippet, PostHandleNonUniqueSnippet, FinalProcess);
            positionNow = positionNowInner;
            maxLineHeightNow = maxLineHeightInner;
            return result;
        }
        public static Vector2 GetStringSize(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale = 1, float maxWidth = -1f) {
            Vector2 positionNow = Vector2.Zero;
            float maxLineHeightNow = 0;
            return GetStringSize(font, snippets, baseScale, maxWidth, ref positionNow, ref maxLineHeightNow);
        }
        #endregion
        #region DrawString
        /// <summary>
        /// <br/>优化:
        /// <br/>换成 IEnumerable &lt;TextSnippet>
        /// <br/>根据 TextSnippet.UniqueDraw 返回的 Size 调整行高
        /// <br/>处理换行方式修改
        /// <br/><paramref name="maxWidth"/> 为 0 时也视为限宽
        /// <br/><paramref name="baseScae"/> 变为 float
        /// <br/>返回值现在是大小而不是右下角的位置
        /// </summary>
        public static Vector2 DrawColorCodedString(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin, float baseScale, out int hoveredSnippet, float maxWidth, bool ignoreColors,
            ref Vector2 positionNow, ref float maxLineHeightNow) {
            #region 局部变量初始化
            int hovered = -1;
            Vector2 mousePosition = Main.MouseScreen;
            Vector2 positionNowInner = positionNow;
            Vector2 result = positionNowInner;
            Color color = baseColor;
            TextSnippet snippet = null!;
            float scale = 1;
            float maxLineHeightInner = maxLineHeightNow;
            bool justNewLine = true;
            int i = -1;
            #endregion
            void NewLine(bool unique) { }
            void DrawString(string str, Vector2 size) {
                // Terraria 原版原本这里是莫名其妙的 snippet.Scale ^ 2, 这里还是把它修了吧
                // spriteBatch.DrawString(font, str, positionNow, color, rotation, origin, baseScale * snippet.Scale * snippetScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(font, str, positionNowInner, color, rotation, origin, scale, SpriteEffects.None, 0);
            }
            void UniqueDraw(TextSnippet ts, Vector2 size) {
                ts.UniqueDraw(false, out _, spriteBatch, positionNowInner, color, scale);
                if (mousePosition.Between(positionNowInner, positionNowInner + size))
                    hovered = i;
            }
            void AfterUpdateSnippet(TextSnippet ts) {
                if (!ignoreColors)
                    color = ts.GetVisibleColor();
            }
            void PostHandleNonUniqueSnippet(TextSnippet ts) { }
            void FinalProcess() { }
            HandleSnippets(font, snippets, position, baseScale, maxWidth,
                ref positionNowInner, ref result, ref scale, ref maxLineHeightInner, ref justNewLine, ref snippet, ref i,
                NewLine, DrawString, UniqueDraw, AfterUpdateSnippet, PostHandleNonUniqueSnippet, FinalProcess);
            hoveredSnippet = hovered;
            positionNow = positionNowInner;
            maxLineHeightNow = maxLineHeightInner;
            return result;
        }
        public static Vector2 DrawColorCodedString(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin, float baseScale, out int hoveredSnippet, float maxWidth = -1, bool ignoreColors = false) {
            Vector2 positionNow = position;
            float maxLineHeightNow = 0;
            return DrawColorCodedString(spriteBatch, font, snippets, position, baseColor, rotation, origin, baseScale, out hoveredSnippet, maxWidth, ignoreColors, ref positionNow, ref maxLineHeightNow);
        }
        #endregion
        #region DrawStringShadow
        public static void DrawColorCodedStringShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin, float baseScale, float maxWidth = -1, float spread = 2) {
            for (int i = 0; i < ChatManager.ShadowDirections.Length; i++) {
                DrawColorCodedString(spriteBatch, font, snippets, position + ChatManager.ShadowDirections[i] * spread, baseColor, rotation, origin, baseScale, out var _, maxWidth, ignoreColors: true);
            }
        }
        #endregion
        #region DrawStringWithShadow
        // 不带 Color 的版本, 默认前景白色, 阴影黑色
        public static Vector2 DrawColorCodedStringWithShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, float rotation, Vector2 origin, float baseScale, out int hoveredSnippet, float maxWidth = -1, float spread = 2) {
            DrawColorCodedStringShadow(spriteBatch, font, snippets, position, Color.Black, rotation, origin, baseScale, maxWidth, spread);
            return DrawColorCodedString(spriteBatch, font, snippets, position, Color.White, rotation, origin, baseScale, out hoveredSnippet, maxWidth);
        }
        // 带有前景颜色的版本, 默认阴影黑色
        public static Vector2 DrawColorCodedStringWithShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, float rotation, Color color, Vector2 origin, float baseScale, out int hoveredSnippet, float maxWidth = -1, float spread = 2) {
            DrawColorCodedStringShadow(spriteBatch, font, snippets, position, Color.Black, rotation, origin, baseScale, maxWidth, spread);
            return DrawColorCodedString(spriteBatch, font, snippets, position, color, rotation, origin, baseScale, out hoveredSnippet, maxWidth /*, ignoreColors: true*/);
        }
        // 带有前景和后景颜色的版本
        public static Vector2 DrawColorCodedStringWithShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, float rotation, Color color, Color shadowColor, Vector2 origin, float baseScale, out int hoveredSnippet, float maxWidth = -1, float spread = 2) {
            DrawColorCodedStringShadow(spriteBatch, font, snippets, position, shadowColor, rotation, origin, baseScale, maxWidth, spread);
            return DrawColorCodedString(spriteBatch, font, snippets, position, color, rotation, origin, baseScale, out hoveredSnippet, maxWidth);
        }
        // 带有前景和后景颜色和 ref 参数的版本
        public static Vector2 DrawColorCodedStringWithShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, float rotation, Color color, Color shadowColor, Vector2 origin, float baseScale, out int hoveredSnippet, float maxWidth, float spread, ref Vector2 positionNow, ref float maxLineHeightNow) {
            DrawColorCodedStringShadow(spriteBatch, font, snippets, position, shadowColor, rotation, origin, baseScale, maxWidth, spread);
            return DrawColorCodedString(spriteBatch, font, snippets, position, color, rotation, origin, baseScale, out hoveredSnippet, maxWidth, false, ref positionNow, ref maxLineHeightNow);
        }
        #endregion
        #region CreateWrappedText
        /// <summary>
        /// 使用了 object.MemberwiseClone 克隆
        /// 如果想让特定 TextSnippet 实现深拷贝需实现 <see cref="ICloneable"/>
        /// </summary>
        public static List<TextSnippet> CreateWrappedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, float maxWidth,
            out Vector2 textSize, ref Vector2 positionNow, ref float maxLineHeightNow) {
            if (maxWidth < 0) {
                textSize = GetStringSize(font, snippets, baseScale);
                return snippets.ToList();
            }
            #region 局部变量初始化
            List<TextSnippet> wrapped = [];
            Vector2 position = Vector2.Zero;
            Vector2 positionNowInner = positionNow;
            Vector2 result = positionNowInner;
            TextSnippet? lastSnippetAddedNonUnique = null; // 如果最后一个加入的 snippet 是 unique 的话那么它会是 null
            TextSnippet snippet = null!;
            float scale = 1;
            float maxLineHeightInner = maxLineHeightNow;
            bool justNewLine = true;
            int i = 0;
            StringBuilder wrappedTextBuilder = new();
            #endregion
            void NewLine(bool unique) {
                wrappedTextBuilder.Append('\n');
            }
            void DrawString(string str, Vector2 size) {
                wrappedTextBuilder.Append(str);
            }
            void UniqueDraw(TextSnippet ts, Vector2 size) {
                if (wrappedTextBuilder.Length != 0) {
                    // 应该来说这种情况只会在 UniqueDraw 之前调用了 NewLine 时存在
                    // 即 wrappedTextBuilder 的内容只会是 "\n"
                    Debug.Assert(wrappedTextBuilder.ToString() == "\n");
                    if (lastSnippetAddedNonUnique == null) {
                        lastSnippetAddedNonUnique = new TextSnippet();
                        wrapped.Add(lastSnippetAddedNonUnique);
                    }
                    lastSnippetAddedNonUnique.Text += wrappedTextBuilder.ToString();
                    wrappedTextBuilder.Clear();
                }
                wrapped.Add(ShallowClone(ts));
                lastSnippetAddedNonUnique = null;
            }
            void AfterUpdateSnippet(TextSnippet ts) { }
            void PostHandleNonUniqueSnippet(TextSnippet ts) {
                #region 将 snippet 克隆到新数组中
                lastSnippetAddedNonUnique = ShallowClone(ts);
                lastSnippetAddedNonUnique.Text = wrappedTextBuilder.ToString();
                wrappedTextBuilder.Clear();
                wrapped.Add(lastSnippetAddedNonUnique);
                #endregion
            }
            void FinalProcess() { }
            HandleSnippets(font, snippets, position, baseScale, maxWidth,
                ref positionNowInner, ref result, ref scale, ref maxLineHeightInner, ref justNewLine, ref snippet, ref i,
                NewLine, DrawString, UniqueDraw, AfterUpdateSnippet, PostHandleNonUniqueSnippet, FinalProcess);
            positionNow = positionNowInner;
            maxLineHeightNow = maxLineHeightInner;
            textSize = result;
            return wrapped;
        }
        /// <inheritdoc cref="CreateWrappedText(DynamicSpriteFont, IEnumerable{TextSnippet}, float, float, out Vector2)"/>
        public static List<TextSnippet> CreateWrappedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, float maxWidth, out Vector2 textSize) {
            Vector2 positionNow = Vector2.Zero;
            float maxLineHeightNow = 0;
            return CreateWrappedText(font, snippets, baseScale, maxWidth, out textSize, ref positionNow, ref maxLineHeightNow);
        }
        /// <inheritdoc cref="CreateWrappedText(DynamicSpriteFont, IEnumerable{TextSnippet}, float, float, out Vector2)"/>
        public static List<TextSnippet> CreateWrappedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float maxWidth, out Vector2 textSize) {
            return CreateWrappedText(font, snippets, 1, maxWidth, out textSize);
        }
        /// <inheritdoc cref="CreateWrappedText(DynamicSpriteFont, IEnumerable{TextSnippet}, float, float, out Vector2)"/>
        public static List<TextSnippet> CreateWrappedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, float maxWidth) {
            return CreateWrappedText(font, snippets, baseScale, maxWidth, out _);
        }
        /// <inheritdoc cref="CreateWrappedText(DynamicSpriteFont, IEnumerable{TextSnippet}, float, float, out Vector2)"/>
        public static List<TextSnippet> CreateWrappedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float maxWidth) {
            return CreateWrappedText(font, snippets, 1, maxWidth, out _);
        }
        #endregion
        #region CreatePagedText
        // 用 DEBUG 改着改着就改的稀烂了, 但起码能用了
        public static List<List<TextSnippet>> CreatePagedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, float maxWidth, float maxHeight,
            ref Vector2 positionNow, ref float maxLineHeightNow, ref Vector2 result) {
            if (maxHeight < 0) {
                return [CreateWrappedText(font, snippets, baseScale, maxWidth)];
            }
            #region 局部变量初始化
            List<List<TextSnippet>> pages = []; // 所有页
            List<TextSnippet> page = []; // 页, 原 wrapped
            List<TextSnippet> lineSnippets = []; // 行, 跨行的 snippet 属于最后一行
            Vector2 position = Vector2.Zero;
            Vector2 positionNowInner = positionNow;
            Vector2 resultInner = result;
            TextSnippet? lastSnippetAddedToPageNonUnique = null; // 如果最后一个加入的 snippet 是 unique 的话那么它会是 null
            bool isLastAddedToLineUnique = false;
            bool isFirstAddedToLineUnique = false;
            TextSnippet snippet = null!;
            float scale = 1;
            float maxLineHeightInner = maxLineHeightNow;
            bool justNewLine = true;
            int i = 0;
            bool hasPageText = false;
            StringBuilder pageTextBuilder = new(); // 页内容
            StringBuilder lineTextBuilder = new(); // 行内容
            StringBuilder lineStartTextBuilder = new(); // 行首内容, 只在行首的多行 snippet 结束于此行时有内容
            // 将其设置为 false 需同时保证 pageTextBuilder 长度为 0
            bool inLineStartText = true;
            // 表示处于页的开头行, 用于判断是否要在换行时是否要在将行添加到页前是否添加换行符
            bool inPageStart = true;
            #endregion
            void AddLineSnippet(TextSnippet snippet, bool unique) {
                if (lineSnippets.Count == 0) {
                    isFirstAddedToLineUnique = unique;
                }
                lineSnippets.Add(snippet);
                isLastAddedToLineUnique = unique;
            }
            void AddPageSnippet(TextSnippet snippet, bool unique) {
                page.Add(snippet);
                lastSnippetAddedToPageNonUnique = unique ? null : snippet;
            }
            // 需要 snippets 长度非 0
            void AddPageSnippets(List<TextSnippet> snippets, bool lastUnique) {
                page.AddRange(snippets);
                lastSnippetAddedToPageNonUnique = lastUnique ? null : snippets[^1];
            }
            // 保证 pageTextBuilder 长度为 0
            void NewPage() {
                // 将页内容放入 snippet 中
                if (pageTextBuilder.Length != 0) {
                    var snippetToAdd = ShallowClone(snippet);
                    AddPageSnippet(snippetToAdd, false);
                    snippetToAdd.Text = pageTextBuilder.ToString();
                    pageTextBuilder.Clear();
                }
                pages.Add(page);
                page = [];
                positionNowInner = position;
                resultInner = position;
                inPageStart = true;
            }
            // 保证 lineTextBuilder 长度为 0
            // 保证 lineStartTextBuilder 长度为 0
            // 保证 inLineStartText 为 true
            // 保证 lineSnippets 长度为 0
            void NewLine(bool unique) {
                // 若超高, 则先换页
                if (positionNowInner.Y + maxLineHeightInner > position.Y + maxHeight) {
                    if (positionNowInner.Y != position.Y) {
                        NewPage();
                    }
                }
                bool noPageText = pageTextBuilder.Length == 0;
                // 若没有换页, 则先在页内容中添加一个换行符
                if (!inPageStart) {
                    pageTextBuilder.Append('\n');
                }
                // 如果此行开始有字符, 将其添加到上一个添加到页中的普通 snippet, 若无则添加一个新 snippet 到页
                if (lineStartTextBuilder.Length != 0) {
                    pageTextBuilder.Append(lineStartTextBuilder);
                    lineStartTextBuilder.Clear();
                    if (lastSnippetAddedToPageNonUnique != null) {
                        if (page.Count == 0) {
                            var snippetToAdd = ShallowClone(lastSnippetAddedToPageNonUnique);
                            snippetToAdd.Text = pageTextBuilder.ToString();
                            AddPageSnippet(snippetToAdd, false);
                        }
                        else {
                            lastSnippetAddedToPageNonUnique.Text += pageTextBuilder.ToString();
                        }
                    }
                    else {
                        TextSnippet snippetToAdd = new(pageTextBuilder.ToString());
                        AddPageSnippet(snippetToAdd, false);
                    }
                    pageTextBuilder.Clear();
                }
                Debug.Assert(!unique || lineTextBuilder.Length == 0);
                if (unique || lineSnippets.Count != 0) {
                    Debug.Assert(noPageText);
                    if (hasPageText || pageTextBuilder.Length != 0) {
                        hasPageText = false;
                        if (page.Count != 0 && lastSnippetAddedToPageNonUnique != null) {
                            lastSnippetAddedToPageNonUnique.Text += pageTextBuilder.ToString();
                        }
                        else if (lineSnippets.Count != 0 && !isFirstAddedToLineUnique) {
                            lineSnippets[0].Text = pageTextBuilder.ToString() + lineSnippets[0].Text;
                        }
                        else {
                            TextSnippet snippetToAdd = new(pageTextBuilder.ToString());
                            AddPageSnippet(snippetToAdd, false);
                        }
                        pageTextBuilder.Clear();
                    }
                }
                // 将此行添加到页中
                if (lineSnippets.Count != 0) {
                    AddPageSnippets(lineSnippets, isLastAddedToLineUnique);
                    lineSnippets.Clear();
                }
                // 将行内容添加到页内容中
                pageTextBuilder.Append(lineTextBuilder);
                lineTextBuilder.Clear();
                if (!unique) {
                    hasPageText = true;
                }
                inLineStartText = true;
                inPageStart = false;
            }
            // 字符串默认写入行内容中
            void DrawString(string str, Vector2 size) {
                lineTextBuilder.Append(str);
            }
            // 保证 inLineStartText 为 false (即保证 pageTextBuilder 长度为 0)
            // 保证 lineTextBuilder 长度为 0
            void UniqueDraw(TextSnippet snippet, Vector2 size) {
                // 由于在 NewLine 和 PostHandleNonUniqueSnippet 中都保证了 lineTextBuilder 的长度为 0, 所以此处它的长度应保持为 0
                Debug.Assert(lineTextBuilder.Length == 0);
                // 由于此方法不然就在 PostHandleNonUnique 后, 不然就在 UniqueDraw 后, 不然就在它们接着 NewLine 后
                // 所以 pageTextBuilder 应该只会是 ""
                Debug.Assert(pageTextBuilder.Length == 0);
                inLineStartText = false;
                AddLineSnippet(ShallowClone(snippet), true);
            }
            void AfterUpdateSnippet(TextSnippet snippet) { }
            // 保证处理完后 lineTextBuilder 长度为 0
            // 保证 inLineStartText 为 false (即保证 pageTextBuilder 长度为 0)
            // 可能会在 lineStartTextBuilder 中保存信息
            void PostHandleNonUniqueSnippet(TextSnippet snippet) {
                TextSnippet snippetToAdd = ShallowClone(snippet);
                // 若不是开头的, 则直接放入行中
                if (!inLineStartText) {
                    Debug.Assert(pageTextBuilder.Length == 0);
                    snippet.Text = lineTextBuilder.ToString();
                    lineTextBuilder.Clear();
                    AddLineSnippet(snippetToAdd, false);
                    return;
                }
                // 如果是开头的普通 snippet...
                inLineStartText = false;
                // 若没有页内容...
                if (pageTextBuilder.Length == 0) {
                    // 将行内容转换为此 snippet 放入行中
                    snippetToAdd.Text = lineTextBuilder.ToString();
                    lineTextBuilder.Clear();
                    AddLineSnippet(snippetToAdd, false);
                }
                // 若有页内容...
                else {
                    Debug.Assert(lineSnippets.Count == 0);
                    // 将其取出转换为此 snippet 放入页中
                    snippetToAdd.Text = pageTextBuilder.ToString();
                    pageTextBuilder.Clear();
                    AddPageSnippet(snippetToAdd, false);
                    // 行内容放入行首内容
                    lineStartTextBuilder.Append(lineTextBuilder);
                    lineTextBuilder.Clear();
                }

            }
            void FinalProcess() {
                // 若超高, 则先换页
                if (positionNowInner.Y + maxLineHeightInner > position.Y + maxHeight) {
                    if (positionNowInner.Y != position.Y) {
                        NewPage();
                    }
                }
                Debug.Assert(pageTextBuilder.Length == 0);
                Debug.Assert(lineTextBuilder.Length == 0);
                // 若没有换页, 则先在页内容中添加一个换行符
                if (!inPageStart) {
                    pageTextBuilder.Append('\n');
                }
                // 如果此行开始有字符, 将其添加到上一个添加到页中的普通 snippet, 若无则添加一个新 snippet 到页
                if (lineStartTextBuilder.Length != 0) {
                    pageTextBuilder.Append(lineStartTextBuilder);
                    lineStartTextBuilder.Clear();
                    if (lastSnippetAddedToPageNonUnique != null) {
                        lastSnippetAddedToPageNonUnique.Text += pageTextBuilder.ToString();
                    }
                    else {
                        TextSnippet snippetToAdd = new(pageTextBuilder.ToString());
                        AddPageSnippet(snippetToAdd, false);
                    }
                    pageTextBuilder.Clear();
                }
                if (lineSnippets.Count != 0) {
                    if (hasPageText || pageTextBuilder.Length != 0) {
                        hasPageText = false;
                        if (lastSnippetAddedToPageNonUnique != null) {
                            lastSnippetAddedToPageNonUnique.Text += pageTextBuilder.ToString();
                        }
                        else if (lineSnippets.Count != 0 && !isFirstAddedToLineUnique) {
                            lineSnippets[0].Text = pageTextBuilder.ToString() + lineSnippets[0].Text;
                        }
                        else {
                            TextSnippet snippetToAdd = new(pageTextBuilder.ToString());
                            AddPageSnippet(snippetToAdd, false);
                        }
                        pageTextBuilder.Clear();
                    }
                    AddPageSnippets(lineSnippets, isLastAddedToLineUnique);
                    lineSnippets.Clear();
                }
                pages.Add(page);
            }
            HandleSnippets(font, snippets, position, baseScale, maxWidth,
                ref positionNowInner, ref resultInner, ref scale, ref maxLineHeightInner, ref justNewLine, ref snippet, ref i,
                NewLine, DrawString, UniqueDraw, AfterUpdateSnippet, PostHandleNonUniqueSnippet, FinalProcess);
            positionNow = positionNowInner;
            result = resultInner;
            return pages;
        }
        public static List<List<TextSnippet>> CreatePagedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, float maxWidth, float maxHeight) {
            Vector2 startPosition = Vector2.Zero;
            float maxLineHeight = 0;
            Vector2 result = Vector2.Zero;
            return CreatePagedText(font, snippets, baseScale, maxWidth, maxHeight,
                ref startPosition, ref maxLineHeight, ref result);
        }
        public static List<List<TextSnippet>> CreatePagedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float baseScale, Vector2 pageSize) {
            return CreatePagedText(font, snippets, baseScale, pageSize.X, pageSize.Y);
        }
        public static List<List<TextSnippet>> CreatePagedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, float maxWidth, float maxHeight) {
            return CreatePagedText(font, snippets, 1, maxWidth, maxHeight);
        }
        public static List<List<TextSnippet>> CreatePagedText(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 pageSize) {
            return CreatePagedText(font, snippets, 1, pageSize.X, pageSize.Y);
        }
        #endregion

        #region 一切的根基 HandleSnippets
        private static void HandleSnippets(DynamicSpriteFont font, IEnumerable<TextSnippet> snippets, Vector2 position, float baseScale, float maxWidth,
            ref Vector2 positionNow, ref Vector2 result, ref float scale, ref float maxLineHeight, ref bool justNewLine, ref TextSnippet snippet, ref int i,
            Action<bool> NewLine, Action<string, Vector2> DrawString, Action<TextSnippet, Vector2> UniqueDraw, Action<TextSnippet> AfterUpdateSnippet, Action<TextSnippet> PostHandleNonUniqueSnippet, Action FinalProcess
        ) {
            #region 局部变量初始化
            i = -1;
            float scaleInner;
            #endregion
            #region 本地函数
            void NewLineInner(ref Vector2 positionNow, ref Vector2 result, ref float maxLineHeight, ref bool justNewLine, bool unique = false) {
                if (justNewLine) {
                    maxLineHeight.ClampMinTo(font.LineSpacing);
                }
                NewLine(unique);
                positionNow.Y += maxLineHeight;
                result.X.ClampMinTo(positionNow.X);
                maxLineHeight = 0;
                positionNow.X = position.X;
                justNewLine = true;
            }
            Vector2 MeasureString(string str) {
                return font.MeasureString(str) * scaleInner;
            }
            void DrawStringSimple(string str, ref Vector2 positionNow, ref float maxLineHeight, ref bool justNewLine) {
                var size = MeasureString(str);
                DrawStringInner(str, size, ref positionNow, ref maxLineHeight, ref justNewLine);
            }
            void DrawStringInner(string str, Vector2 size, ref Vector2 positionNow, ref float maxLineHeight, ref bool justNewLine) {
                if (str.Length == 0) {
                    return;
                }
                justNewLine = false;
                DrawString(str, size);
                positionNow.X += size.X;
                Debug.Assert(size.Y >= font.LineSpacing * scaleInner);
                maxLineHeight.ClampMinTo(size.Y);

            }
            #endregion
            foreach (var snip in snippets) {
                snippet = snip;
                i += 1;
                snippet.Update();
                AfterUpdateSnippet(snippet);
                scaleInner = scale = snippet.Scale * baseScale;
                #region 处理有独特绘制的 Snippet (UniqueDraw)
                if (snippet.UniqueDraw(true, out var size, null, scale: scale)) {
                    // 若超限, 则先换行
                    if (maxWidth >= 0 && positionNow.X != position.X && positionNow.X + size.X > position.X + maxWidth) {
                        NewLineInner(ref positionNow, ref result, ref maxLineHeight, ref justNewLine, unique: true);
                    }
                    justNewLine = false;
                    UniqueDraw(snippet, size);
                    positionNow.X += size.X;
                    maxLineHeight.ClampMinTo(size.Y);
                    continue;
                }
                #endregion

                string[] lines = snippet.Text.Split('\n');
                Debug.Assert(lines.Length != 0);
                #region 不限宽的处理
                if (maxWidth < 0) {
                    DrawStringSimple(lines[0], ref positionNow, ref maxLineHeight, ref justNewLine);
                    for (int j = 1; j < lines.Length; ++j) {
                        NewLineInner(ref positionNow, ref result, ref maxLineHeight, ref justNewLine);
                        DrawStringSimple(lines[j], ref positionNow, ref maxLineHeight, ref justNewLine);
                    }
                    PostHandleNonUniqueSnippet(snippet);
                    continue;
                }
                #endregion
                #region 限宽的处理
                for (int j = 0; j < lines.Length; ++j) {
                    if (j != 0) {
                        NewLineInner(ref positionNow, ref result, ref maxLineHeight, ref justNewLine);
                    }
                    var line = lines[j];
                    StringBuilder wordBuilder = new();
                    string? word = null;
                    Vector2 wordSize = Vector2.Zero;
                    for (int k = 0; k < line.Length; ++k) {
                        char c = line[k];
                        // 如果此字符属于一个单词...
                        if ('A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '_') {
                            wordBuilder.Append(c);
                            var lastWord = word;
                            word = wordBuilder.ToString();
                            wordSize = MeasureString(word);

                        CheckOverWidth:
                            // 如果超限...
                            if (positionNow.X + wordSize.X >= position.X + maxWidth) {
                                // 如果不处于开头, 则先换行, 再检查是否超限
                                if (positionNow.X != position.X) {
                                    NewLineInner(ref positionNow, ref result, ref maxLineHeight, ref justNewLine);
                                    goto CheckOverWidth;
                                }
                                // 如果此单词只有这个字母, 则画出它, 并清空单词
                                if (lastWord == null) {
                                    DrawStringInner(word, wordSize, ref positionNow, ref maxLineHeight, ref justNewLine);
                                    wordBuilder.Clear();
                                    word = null;
                                }
                                // 否则画出没有这个字母的单词, 并换行, 清空单词, 放入这个字母
                                else {
                                    DrawStringSimple(lastWord, ref positionNow, ref maxLineHeight, ref justNewLine);
                                    NewLineInner(ref positionNow, ref result, ref maxLineHeight, ref justNewLine);
                                    wordBuilder.Clear();
                                    wordBuilder.Append(c);
                                    word = null;
                                }
                            }
                            continue;
                        }
                        // 否则...
                        // 若有单词则先画出单词
                        if (word != null) {
                            DrawStringInner(word, wordSize, ref positionNow, ref maxLineHeight, ref justNewLine);
                            word = null;
                            wordBuilder.Clear();
                        }
                        #region 画出字母
                        string s = new([c]);
                        var charSize = MeasureString(s);
                        // 若没有超限, 则直接画出并返回
                        if (positionNow.X + charSize.X <= position.X + maxWidth) {
                            DrawStringInner(s, charSize, ref positionNow, ref maxLineHeight, ref justNewLine);
                            continue;
                        }
                        // 若不处于开头, 则先换行
                        if (positionNow.X != position.X) {
                            NewLineInner(ref positionNow, ref result, ref maxLineHeight, ref justNewLine);
                        }
                        // 直接画出
                        DrawStringInner(s, charSize, ref positionNow, ref maxLineHeight, ref justNewLine);
                        #endregion
                    }
                    // 如果有残存单词则画出它
                    if (word != null) {
                        DrawStringInner(word, wordSize, ref positionNow, ref maxLineHeight, ref justNewLine);
                    }
                }
                PostHandleNonUniqueSnippet(snippet);
                #endregion
            }
            FinalProcess();
            result.X.ClampMinTo(positionNow.X);
            if (justNewLine) {
                maxLineHeight.ClampMinTo(font.LineSpacing);
            }
            result.Y.ClampMinTo(positionNow.Y + maxLineHeight);
            result -= position;
        }
        #endregion
    }
    #endregion
    #endregion
    #region 杂项

    #region 获取是否是开发者模式
    /// <summary>
    /// 获取是否是开发者模式
    /// </summary>
    public static bool IsTMLInDeveloperMode => ModCompile.DeveloperMode;
    #endregion
    public static T TMLInstance<T>() where T : class => ContentInstance<T>.Instance; // ModContent.GetInstance<T>()
    #region TextureFromColors
    public static Texture2D TextureFromColors(int width, int height, Color[] colors) {
        return DoOnMainThread(() => TextureFromColorsInner(width, height, colors));
    }
    public static Task<Texture2D> TextureFromColorsAsync(int width, int height, Color[] colors) {
        return DoOnMainThreadAsync(() => TextureFromColorsInner(width, height, colors));
    }
    private static Texture2D TextureFromColorsInner(int width, int height, Color[] colors) {
        Texture2D result = new(Main.instance.GraphicsDevice, width, height);
        result.SetData(colors);
        return result;
    }
    public static Asset<Texture2D> AssetTextureFromColors(int width, int height, Color[] colors, bool immediate = false) {
        return TextureFromColorsAsync(width, height, colors).ToAsset("TextureFromColors", immediate);
    }
    #endregion
    #region DoOnMainThread
    /// <summary>
    /// 若在主线程则直接执行, 否则安排到主线程执行
    /// </summary>
    /// <param name="wait">在非主线程时是否等待到任务完成时</param>
    public static void DoOnMainThread(Action action, bool wait = true) {
        if (ThreadCheck.IsMainThread) {
            action();
            return;
        }
        if (wait) {
            Main.RunOnMainThread(action).GetAwaiter().GetResult();
            return;
        }
        Main.RunOnMainThread(action);
    }
    /// <summary>
    /// 若在主线程则直接执行, 否则安排到主线程执行并等待到任务执行结束, 然后获得返回值
    /// </summary>
    public static T DoOnMainThread<T>(Func<T> func) {
        if (ThreadCheck.IsMainThread) {
            return func();
        }
        return Main.RunOnMainThread(func).GetAwaiter().GetResult();
    }
    /// <summary>
    /// 若在主线程则直接执行, 否则等待到主线程执行
    /// </summary>
    public static Task DoOnMainThreadAsync(Action action) {
        if (ThreadCheck.IsMainThread) {
            action();
            return Task.CompletedTask;
        }
        return Main.RunOnMainThread(action);
    }
    /// <summary>
    /// 若在主线程则直接执行, 否则等待到主线程执行
    /// </summary>
    public static Task<T> DoOnMainThreadAsync<T>(Func<T> func) {
        if (ThreadCheck.IsMainThread) {
            return Task.FromResult(func());
        }
        return Main.RunOnMainThread(func);
    }
    #endregion
    /// <summary>
    /// 在游戏时调用, 用以直接保存并退出
    /// </summary>
    public static void SaveAndQuit() {
        // 摘自 Terraria.IngameOptions.Draw 中 Lang.inter[35] ("保存并退出") 相关的部分
        SteamedWraps.StopPlaytimeTracking();
        SystemLoader.PreSaveAndQuit();
        IngameOptions.Close();
        Main.menuMode = 10;
        Main.gameMenu = true;
        WorldGen.SaveAndQuit();
    }
    #region Main.SetMouseWorld
    public static void Main_SetMouseWorld(Vector2 mouseWorld) {
        Main.mouseX = (int)(mouseWorld.X - Main.screenPosition.X);
        Main.mouseY = (int)(mouseWorld.Y - Main.screenPosition.Y);
    }
    public static void Main_SetMouseWorldX(float mouseWorldX) {
        Main.mouseX = (int)(mouseWorldX - Main.screenPosition.X);
    }
    public static void Main_SetMouseWorldY(float mouseWorldY) {
        Main.mouseY = (int)(mouseWorldY - Main.screenPosition.Y);
    }
    #endregion

    #endregion
}

public static partial class TigerClasses {
#if MOUSE_MANAGER
    public sealed class MouseManager : ModSystem {
        public static bool OldMouseLeft { get; private set; }
        public static bool MouseLeft { get; private set; }
        public static bool MouseLeftDown => MouseLeft && !OldMouseLeft;
        public event Action? OnMouseLeftDown;
        public override void PostUpdateInput() {
            OldMouseLeft = MouseLeft;
            MouseLeft = Main.mouseLeft;
            if (MouseLeftDown) {
                OnMouseLeftDown?.Invoke();
            }
        }
    }
#endif
#if TIME_MANAGER
    public class TimeManager : ModSystem
    {
        public static UncheckedUlongTime TimeNow { get; private set; }
        static readonly Dictionary<UncheckedUlongTime, List<Action>> events = new();
        public override void PostUpdateTime()
        {
            TimeNow += 1ul;
            if (events.ContainsKey(TimeNow))
            {
                foreach (Action e in events[TimeNow])
                {
                    e.Invoke();
                }
                events.Remove(TimeNow);
            }
        }
        public static UncheckedUlongTime RegisterEvent(Action e, ulong timeDelay)
        {
            UncheckedUlongTime time = TimeNow + timeDelay;
            if (events.ContainsKey(time))
            {
                events[time].Add(e);
            }
            else
            {
                events.Add(time, new() { e });
            }
            return time;
        }
        public static UncheckedUlongTime RegisterEvent(Action e, UncheckedUlongTime time)
        {
            if (events.ContainsKey(time))
            {
                events[time].Add(e);
            }
            else
            {
                events.Add(time, new() { e });
            }
            return time;
        }
        public static bool CancelEvent(Action e, UncheckedUlongTime time)
        {
            if (!events.ContainsKey(time))
            {
                return false;
            }
            return events[time].Remove(e);
        }
    }
#endif
    /// <summary>
    /// <br/>代表获取文本的方式
    /// <br/>可以为 <see cref="string"/>, <see cref="LocalizedText"/> 或 委托
    /// <br/>如果要使用委托, 可以使用 new(delegate) 的形式获得
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct TextGetter {
        #region 构造函数
        public TextGetter(string stringValue) => StringValue = stringValue;
        public TextGetter(LocalizedText localizedTextValue) => LocalizedTextValue = localizedTextValue;
        public TextGetter(Func<string> stringGetterValue) => StringGetterValue = stringGetterValue;
        #endregion
        #region Vars
        enum TextGetterType {
            None,
            String,
            LocalizedText,
            StringGetter
        }
        [FieldOffset(8)]
        TextGetterType Type;
        [FieldOffset(0)]
        string? stringValue;
        [FieldOffset(0)]
        LocalizedText? localizedTextValue;
        [FieldOffset(0)]
        Func<string>? stringGetterValue;
        #endregion
        #region 设置与获取值
        public readonly bool IsNone => Type == TextGetterType.None;
        public readonly string? Value => Type switch {
            TextGetterType.String => stringValue,
            TextGetterType.LocalizedText => localizedTextValue?.Value,
            TextGetterType.StringGetter => stringGetterValue?.Invoke(),
            _ => null
        };
        public string? StringValue {
            readonly get => Type == TextGetterType.String ? stringValue : null;
            set => (Type, stringValue) = (TextGetterType.String, value);
        }
        public LocalizedText? LocalizedTextValue {
            readonly get => Type == TextGetterType.LocalizedText ? localizedTextValue : null;
            set => (Type, localizedTextValue) = (TextGetterType.LocalizedText, value);
        }
        public Func<string>? StringGetterValue {
            readonly get => Type == TextGetterType.StringGetter ? stringGetterValue : null;
            set => (Type, stringGetterValue) = (TextGetterType.StringGetter, value);
        }
        public readonly static TextGetter Default = default;
        public readonly TextGetter WithDefault(TextGetter defaultValue) => IsNone ? defaultValue : this;
        #endregion
        #region 类型转换
        public static implicit operator TextGetter(string? stringValue) => stringValue == null ? Default : new(stringValue);
        public static implicit operator TextGetter(LocalizedText? localizedTextValue) => localizedTextValue == null ? Default : new(localizedTextValue);
        public static implicit operator TextGetter(Func<string>? stringGetterValue) => stringGetterValue == null ? Default : new(stringGetterValue);
        public override readonly string ToString() => Value ?? string.Empty;
        #endregion
        #region 运算符重载
        public static TextGetter operator |(TextGetter left, TextGetter right) => left.IsNone ? right : left;
        #endregion
    }
    /// <summary>
    /// <br/>代表 <see cref="Texture2D"/> 的获取方式
    /// <br/>可以为 <see cref="Texture2D"/>, <see cref="Asset{T}"/>, 委托, 或者图片的路径 (会转换为 <see cref="Asset{T}"/>)
    /// <br/>如果要使用委托, 可以使用 new(delegate) 的形式获得
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct Texture2DGetter {
        #region 构造函数
        public Texture2DGetter(string texturePath) => SetTexturePath(texturePath);
        public Texture2DGetter(Texture2D texture2DValue) => Texture2DValue = texture2DValue;
        public Texture2DGetter(Asset<Texture2D> assetOfTexture2DValue) => AssetOfTexture2DValue = assetOfTexture2DValue;
        public Texture2DGetter(Func<Texture2D?> texture2DGetterValue) => Texture2DGetterValue = texture2DGetterValue;
        #endregion
        #region Vars
        enum Texture2DGetterType {
            None,
            Texture2D,
            AssetOfTexture2D,
            Texture2DGetter
        }
        [FieldOffset(16)]
        Texture2DGetterType Type;
        [FieldOffset(0)]
        Texture2D? texture2DValue;
        [FieldOffset(0)]
        Asset<Texture2D>? assetOfTexture2DValue;
        [FieldOffset(0)]
        Func<Texture2D?>? texture2DGetterValue;
        [FieldOffset(8)]
        string? assetPath;
        #endregion
        #region 设置与获取值
        public readonly bool IsNone => Type == Texture2DGetterType.None;
        public readonly Texture2D? Value => Type switch {
            Texture2DGetterType.Texture2D => texture2DValue,
            Texture2DGetterType.AssetOfTexture2D => assetOfTexture2DValue?.Value,
            Texture2DGetterType.Texture2DGetter => texture2DGetterValue?.Invoke(),
            _ => null
        };
        public void SetTexturePath(string? texturePath) {
            if (texturePath != null && ModContent.RequestIfExists<Texture2D>(texturePath, out var texture)) {
                AssetOfTexture2DValue = texture;
                assetPath = texturePath;
            }
            else {
                Type = Texture2DGetterType.None;
            }
        }
        public Texture2D? Texture2DValue {
            readonly get => Type == Texture2DGetterType.Texture2D ? texture2DValue : null;
            set => (Type, texture2DValue) = (Texture2DGetterType.Texture2D, value);
        }
        public string? AssetPath {
            readonly get => Type == Texture2DGetterType.AssetOfTexture2D ? assetPath : null;
            set => SetTexturePath(value);
        }
        public Asset<Texture2D>? AssetOfTexture2DValue {
            readonly get => Type == Texture2DGetterType.AssetOfTexture2D ? assetOfTexture2DValue : null;
            set => (Type, assetOfTexture2DValue, assetPath) = (Texture2DGetterType.AssetOfTexture2D, value, null);
        }
        public Func<Texture2D?>? Texture2DGetterValue {
            readonly get => Type == Texture2DGetterType.Texture2DGetter ? texture2DGetterValue : null;
            set => (Type, texture2DGetterValue) = (Texture2DGetterType.Texture2DGetter, value);
        }
        public static readonly Texture2DGetter Default = default;
        public readonly Texture2DGetter WithDefault(Texture2DGetter defaultValue) => IsNone ? defaultValue : this;
        #endregion
        #region 类型转换
        public static implicit operator Texture2DGetter(string? texturePath) => texturePath == null ? default : new(texturePath);
        public static implicit operator Texture2DGetter(Texture2D? texture2DValue) => texture2DValue == null ? default : new(texture2DValue);
        public static implicit operator Texture2DGetter(Asset<Texture2D>? assetOfTexture2DValue) => assetOfTexture2DValue == null ? default : new(assetOfTexture2DValue);
        public static implicit operator Texture2DGetter(Func<Texture2D?>? texture2DGetterValue) => texture2DGetterValue == null ? default : new(texture2DGetterValue);
        public override readonly string ToString() => Value?.ToString() ?? string.Empty;
        #endregion
        #region 运算符重载
        public static Texture2DGetter operator |(Texture2DGetter left, Texture2DGetter right) => left.IsNone ? right : left;
        #endregion
    }

    public static partial class Textures {
        public static ColorsClass Colors { get; } = new();
        public class ColorsClass {
            public readonly Asset<Texture2D> White  = GetColorTexture(Color.White );
            public readonly Asset<Texture2D> Black  = GetColorTexture(Color.Black );
            public readonly Asset<Texture2D> Gray   = GetColorTexture(Color.Gray  );
            public readonly Asset<Texture2D> Red    = GetColorTexture(Color.Red   );
            public readonly Asset<Texture2D> Orange = GetColorTexture(Color.Orange);
            public readonly Asset<Texture2D> Yellow = GetColorTexture(Color.Yellow);
            public readonly Asset<Texture2D> Green  = GetColorTexture(Color.Green );
            public readonly Asset<Texture2D> Blue   = GetColorTexture(Color.Blue  );
            public readonly Asset<Texture2D> Purple = GetColorTexture(Color.Purple);
            private static readonly Dictionary<Color, Asset<Texture2D>> colorsCache = [];
            public Asset<Texture2D> this[Color color] => GetColorTexture(color);
            private static Asset<Texture2D> GetColorTexture(Color color) {
                if (colorsCache.TryGetValue(color, out var value)) {
                    return value;
                }
                value = AssetTextureFromColors(1, 1, [color]);
                colorsCache.Add(color, value);
                return value;
            }
            public static void ClearCache() => colorsCache.Clear();
        }
    }

    /// <summary>
    /// 顶点信息
    /// </summary>
    /// <param name="position">屏幕位置</param>
    /// <param name="texCoord">纹理坐标, 三维分别是x, y, 透明度, 都介于 0 - 1 之间</param>
    /// <param name="color">颜色</param>
    public struct Vertex(Vector2 position, Vector3 texCoord, Color color) : IVertexType {
        public readonly VertexDeclaration VertexDeclaration => _vertexDeclaration;
        private readonly static VertexDeclaration _vertexDeclaration = new([
            new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        ]);
        public Vector2 Position { get; set; } = position;
        public Color Color { get; set; } = color;
        public Vector3 TexCoord { get; set; } = texCoord;
    }
}

public static partial class TigerExtensions {
    #region TagCompound 拓展
    /// <summary>
    /// 若不为默认值则将值保存到 <paramref name="tag"/> 中
    /// </summary>
    public static void SetWithDefault<T>(this TagCompound tag, string key, T? value, T? defaultValue = default, bool replace = false) where T : IEquatable<T> {
        if (value == null && defaultValue == null || value?.Equals(defaultValue) == true) {
            return;
        }
        tag.Set(key, value, replace);
    }
    /// <summary>
    /// 若不为默认值则将值保存到 <paramref name="tag"/> 中
    /// </summary>
    /// <param name="checkDefault">检查值是否是默认值</param>
    public static void SetWithDefault<T>(this TagCompound tag, string key, T? value, Func<T?, bool> checkDefault, bool replace = false) {
        if (!checkDefault(value)) {
            tag.Set(key, value, replace);
        }
    }
    /// <summary>
    /// 若不为默认值 ( ! <paramref name="isDefault"/> ) 则将值保存到 <paramref name="tag"/> 中
    /// </summary>
    public static void SetWithDefault<T>(this TagCompound tag, string key, T? value, bool isDefault, bool replace = false) {
        if (!isDefault) {
            tag.Set(key, value, replace);
        }
    }
    /// <summary>
    /// 若不为默认值则将值保存到 <paramref name="tag"/> 中
    /// </summary>
    public static void SetWithDefaultN<T>(this TagCompound tag, string key, T value, T defaultValue = default, bool replace = false) where T : struct {
        if (value.Equals(defaultValue) != true) {
            tag.Set(key, value, replace);
        }
    }
    public static void SetWithNullDefault<T>(this TagCompound tag, string key, T value, bool replace = false) {
        if (value != null) {
            tag.Set(key, value, replace);
        }
    }

    public static Func<Item?, bool> ItemCheckDefault => i => i == null || i.IsAir;
    /// <summary>
    /// <br/>获得此值, 若不存在则返回默认值
    /// <br/>若类型不正确会报错
    /// </summary>
    public static T? GetWithDefault<T>(this TagCompound tag, string key) {
        return tag.TryGet(key, out T value) ? value : default;
    }
    /// <summary>
    /// <br/>获得此值, 若不存在则返回默认值(<paramref name="defaultValue"/>)
    /// <br/>若类型不正确会报错
    /// </summary>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetWithDefault<T>(this TagCompound tag, string key, T? defaultValue) {
        return tag.TryGet(key, out T value) ? value : defaultValue;
    }
    /// <summary>
    /// <br/>返回是否成功得到值, 返回假时得到的是默认值(返回真时也可能得到默认值(若保存的为默认值的话))
    /// <br/>若类型不正确会报错
    /// </summary>
    public static bool GetWithDefault<T>(this TagCompound tag, string key, [NotNullWhen(true)] out T? value) {
        if (tag.TryGet(key, out value) && value is not null) {
            return true;
        }
        value = default;
        return false;
    }
    /// <summary>
    /// <br/>返回是否成功得到值, 返回假时得到的是默认值(返回真时也可能得到默认值(若保存的为默认值的话))
    /// <br/>若类型不正确会报错
    /// </summary>
    public static bool GetWithDefault<T>(this TagCompound tag, string key, [NotNullWhen(true)][NotNullIfNotNull(nameof(defaultValue))] out T value, T defaultValue) {
        if (tag.TryGet(key, out value) && value is not null) {
            return true;
        }
        value = defaultValue;
        return false;
    }
    public static void SetWithDefaultN<T>(this TagCompound tag, string key, T? value, bool replace = false) where T : struct {
        if (!value.HasValue) {
            return;
        }
        tag.Set(key, value.Value, replace);
    }
    public static bool GetWithDefaultN<T>(this TagCompound tag, string key, [NotNullWhen(true)] out T? value) where T : struct {
        if (tag.TryGet(key, out T val)) {
            value = val;
            return true;
        }
        value = null;
        return false;
    }
    public static T? GetWithDefaultN<T>(this TagCompound tag, string key) where T : struct {
        return tag.TryGet(key, out T val) ? val : null;
    }

    public static bool Replace<TOld, TNew>(this TagCompound tag, string oldKey, string newKey, Func<TOld?, TNew?> convert, TOld? oldDefaultValue = default, TNew? newDefaultValue = default, bool removeOldKey = true) where TNew : IEquatable<TNew> {
        bool result = tag.GetWithDefault(oldKey, out TOld? oldValue, oldDefaultValue);
        if (removeOldKey) {
            tag.Remove(oldKey);
        }
        tag.SetWithDefault(newKey, convert(oldValue), newDefaultValue, replace: true);
        return result;
    }
    public static bool Replace<T>(this TagCompound tag, string oldKey, string newKey, T? defaultValue = default, bool removeOldKey = true) where T : IEquatable<T> {
        bool result = tag.GetWithDefault(oldKey, out T? value, defaultValue);
        if (removeOldKey) {
            tag.Remove(oldKey);
        }
        tag.SetWithDefault(newKey, value, defaultValue);
        return result;
    }
    public static bool Replace(this TagCompound tag, string oldKey, string newKey, bool removeOldKey = true) {
        if (!tag.ContainsKey(oldKey)) {
            return false;
        }
        tag[newKey] = tag[oldKey];
        if (removeOldKey) {
            tag.Remove(oldKey);
        }
        return true;
    }
    /// <summary>
    /// 需要 <paramref name="tag"/>[<paramref name="key"/>] 中是 List&lt;<typeparamref name="TElement"/>&gt;
    /// </summary>
    public static void AddElement<TElement>(this TagCompound tag, string key, TElement element) {
        if (tag.ContainsKey(key)) {
            tag.Get<List<TElement>>(key).Add(element);
        }
        else {
            tag[key] = new List<TElement>() { element };
        }
    }
    /// <summary>
    /// 需要 <paramref name="tag"/>[<paramref name="key"/>] 中是 List&lt;<typeparamref name="TElement"/>&gt;
    /// </summary>
    public static void AddElementRange<TElement>(this TagCompound tag, string key, IEnumerable<TElement> elements) {
        if (tag.ContainsKey(key)) {
            tag.Get<List<TElement>>(key).AddRange(elements);
        }
        else {
            tag.Add(key, new List<TElement>([.. elements]));
        }
    }
    /// <summary>
    /// 需要 <paramref name="tag"/>[<paramref name="key"/>] 中是 List&lt;<typeparamref name="TElement"/>&gt;
    /// </summary>
    public static void AddElementRange<TElement>(this TagCompound tag, string key, List<TElement> elementList) {
        if (tag.ContainsKey(key)) {
            tag.Get<List<TElement>>(key).AddRange(elementList);
        }
        else {
            tag.Add(key, elementList);
        }
    }

    #region SaveDictionaryData
    public static void SaveDictionaryData<T>(this TagCompound tag, string key, Dictionary<string, T> dictionary) {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            data[k] = v;
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    public static void SaveDictionaryData<T>(this TagCompound tag, string key, Dictionary<string, T> dictionary, Func<T, object> valueSelector) {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            data[k] = valueSelector(v);
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    public static void SaveDictionaryData<T>(this TagCompound tag, string key, Dictionary<string, T> dictionary, Func<string, T, object> pairSelector) {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            data[k] = pairSelector(k, v);
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    public static void SaveDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<TKey, string> keySelector) where TKey : notnull {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            data[keySelector(k)] = v;
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    public static void SaveDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<TKey, string> keySelector, Func<TValue, object> valueSelector) where TKey : notnull {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            data[keySelector(k)] = valueSelector(v);
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    public static void SaveDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<TKey, string> keySelector, Func<TKey, TValue, object> pairSelector) where TKey : notnull {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            data[keySelector(k)] = pairSelector(k, v);
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    public static void SaveDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<TKey, string> keySelector, Func<string, TValue, object> pairSelector) where TKey : notnull {
        TagCompound data = [];
        foreach (var (k, v) in dictionary) {
            var ks = keySelector(k);
            data[ks] = pairSelector(ks, v);
        }
        if (data.Count == 0) {
            return;
        }
        tag[key] = data;
    }
    #endregion
    #region LoadDictionaryData
    #region 字典作返回值
    public static Dictionary<string, T> LoadDictionaryData<T>(this TagCompound tag, string key) {
        Dictionary<string, T> result = [];
        LoadDictionaryData(tag, key, result);
        return result;
    }
    public static Dictionary<string, T> LoadDictionaryData<T>(this TagCompound tag, string key, Func<object, T> valueSelector) {
        Dictionary<string, T> result = [];
        LoadDictionaryData(tag, key, result, valueSelector);
        return result;
    }
    public static Dictionary<string, T> LoadDictionaryData<T>(this TagCompound tag, string key, Func<string, object, T> pairSelector) {
        Dictionary<string, T> result = [];
        LoadDictionaryData(tag, key, result, pairSelector);
        return result;
    }
    public static Dictionary<TKey, TValue> LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Func<string, TKey> keySelector) where TKey : notnull {
        Dictionary<TKey, TValue> result = [];
        LoadDictionaryData(tag, key, result, keySelector);
        return result;
    }
    public static Dictionary<TKey, TValue> LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Func<string, TKey> keySelector, Func<object, TValue> valueSelector) where TKey : notnull {
        Dictionary<TKey, TValue> result = [];
        LoadDictionaryData(tag, key, result, keySelector, valueSelector);
        return result;
    }
    public static Dictionary<TKey, TValue> LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Func<string, TKey> keySelector, Func<string, object, TValue> pairSelector) where TKey : notnull {
        Dictionary<TKey, TValue> result = [];
        LoadDictionaryData(tag, key, result, keySelector, pairSelector);
        return result;
    }
    public static Dictionary<TKey, TValue> LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Func<string, TKey> keySelector, Func<TKey, object, TValue> pairSelector) where TKey : notnull {
        Dictionary<TKey, TValue> result = [];
        LoadDictionaryData(tag, key, result, keySelector, pairSelector);
        return result;
    }
    #endregion
    #region 字典作参数
    public static void LoadDictionaryData<T>(this TagCompound tag, string key, Dictionary<string, T> dictionary) {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var k in data.dict.Keys) {
            dictionary[k] = data.Get<T>(k);
        }
    }
    public static void LoadDictionaryData<T>(this TagCompound tag, string key, Dictionary<string, T> dictionary, Func<object, T> valueSelector) {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var (k, v) in data) {
            dictionary[k] = valueSelector(v);
        }
    }
    public static void LoadDictionaryData<T>(this TagCompound tag, string key, Dictionary<string, T> dictionary, Func<string, object, T> pairSelector) {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var (k, v) in data) {
            dictionary[k] = pairSelector(k, v);
        }
    }
    public static void LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<string, TKey> keySelector) where TKey : notnull {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var k in data.dict.Keys) {
            dictionary[keySelector(k)] = data.Get<TValue>(k);
        }
    }
    public static void LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<string, TKey> keySelector, Func<object, TValue> valueSelector) where TKey : notnull {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var (k, v) in data) {
            dictionary[keySelector(k)] = valueSelector(v);
        }
    }
    public static void LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<string, TKey> keySelector, Func<string, object, TValue> pairSelector) where TKey : notnull {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var (k, v) in data) {
            dictionary[keySelector(k)] = pairSelector(k, v);
        }
    }
    public static void LoadDictionaryData<TKey, TValue>(this TagCompound tag, string key, Dictionary<TKey, TValue> dictionary, Func<string, TKey> keySelector, Func<TKey, object, TValue> pairSelector) where TKey : notnull {
        if (!tag.TryGet(key, out TagCompound data)) {
            return;
        }
        foreach (var (k, v) in data) {
            TKey ks = keySelector(k);
            dictionary[ks] = pairSelector(ks, v);
        }
    }
    #endregion
    #endregion
    #endregion
    #region BinaryWriter/Reader 拓展
    /*
    //渣, 不要用, 没测试过, 用了概不负责
    /// <summary>
    /// 支持类型: 原生, Color, Vector2, 及其构成的数组或列表或字典
    /// (<see cref="List{T}"/>, <see cref="Dictionary{TKey, TValue}"/>)
    /// </summary>
    public static void WriteObj<T>(this BinaryWriter bw, T obj) {
        if (obj is ulong @ulong) { bw.Write(@ulong); }
        else if (obj is uint @uint) { bw.Write(@uint); }
        else if (obj is ushort @ushort) { bw.Write(@ushort); }
        else if (obj is string @string) { bw.Write(@string); }
        else if (obj is float @float) { bw.Write(@float); }
        else if (obj is sbyte @sbyte) { bw.Write(@sbyte); }
        else if (obj is long @long) { bw.Write(@long); }
        else if (obj is int @int) { bw.Write(@int); }
        else if (obj is Half @Half) { bw.Write(@Half); }
        else if (obj is double @double) { bw.Write(@double); }
        else if (obj is decimal @decimal) { bw.Write(@decimal); }
        else if (obj is char @char) { bw.Write(@char); }
        else if (obj is byte @byte) { bw.Write(@byte); }
        else if (obj is bool @bool) { bw.Write(@bool); }
        else if (obj is short @short) { bw.Write(@short); }
        else if (obj is byte[] buffer) { bw.Write(buffer.Length); bw.Write(buffer); }
        else if (obj is char[] chars) { bw.Write(chars.Length); bw.Write(chars); }
        else if (obj is Color @color) { bw.WriteRGB(@color); }
        else if (obj is Vector2 @vector2) { bw.WritePackedVector2(@vector2); }
        else if (obj is Array array) { bw.Write(array.Length); foreach (var o in array) { bw.WriteObj(o); } }
        else if (obj is List<object> list) { bw.Write(list.Count); foreach (int i in Range(list.Count)) { bw.WriteObj(list[i]); } }
        else if (obj is Dictionary<object, object> dict) { bw.Write(dict.Count); foreach (var pair in dict) { bw.WriteObj(pair.Key); bw.WriteObj(pair.Value); } }
        else
            throw new Exception("type not suppoerted for type " + obj?.GetType().ToString() ?? "null");
    }
    public static void WriteArray<T>(this BinaryWriter bw, T[] array) {
        bw.Write(array.Length);
        foreach (int i in Range(array.Length)) {
            bw.WriteObj(array[i]);
        }
    }
    public static void WriteList<T>(this BinaryWriter bw, List<T> array) {
        bw.Write(array.Count);
        foreach (int i in Range(array.Count)) {
            bw.WriteObj(array[i]);
        }
    }
    public static void WriteDict<TKey, TValue>(this BinaryWriter bw, Dictionary<TKey, TValue> dict) where TKey : notnull {
        bw.Write(dict.Count);
        foreach (var pair in dict) {
            bw.WriteObj(pair.Key);
            bw.WriteObj(pair.Value);
        }
    }
    /// <summary>
    /// 支持类型: 原生, Color, Vector2
    /// </summary>
    public static void ReadObj<T>(this BinaryReader br, out T obj) {
        Type type = typeof(T);
        if (false) { }
        else if (type == typeof(ulong)) { obj = (T)(object)br.ReadUInt64(); }
        else if (type == typeof(uint)) { obj = (T)(object)br.ReadUInt32(); }
        else if (type == typeof(ushort)) { obj = (T)(object)br.ReadUInt16(); }
        else if (type == typeof(string)) { obj = (T)(object)br.ReadString(); }
        else if (type == typeof(float)) { obj = (T)(object)br.ReadSingle(); }
        else if (type == typeof(sbyte)) { obj = (T)(object)br.ReadSByte(); }
        else if (type == typeof(long)) { obj = (T)(object)br.ReadInt64(); }
        else if (type == typeof(int)) { obj = (T)(object)br.ReadInt32(); }
        else if (type == typeof(Half)) { obj = (T)(object)br.ReadHalf(); }
        else if (type == typeof(double)) { obj = (T)(object)br.ReadDouble(); }
        else if (type == typeof(decimal)) { obj = (T)(object)br.ReadDecimal(); }
        else if (type == typeof(char)) { obj = (T)(object)br.ReadChar(); }
        else if (type == typeof(byte)) { obj = (T)(object)br.ReadByte(); }
        else if (type == typeof(bool)) { obj = (T)(object)br.ReadBoolean(); }
        else if (type == typeof(short)) { obj = (T)(object)br.ReadInt16(); }
        else if (type == typeof(byte[])) { int length = br.ReadInt32(); obj = (T)(object)br.ReadBytes(length); }
        else if (type == typeof(char[])) { int length = br.ReadInt32(); obj = (T)(object)br.ReadChars(length); }
        else if (type == typeof(Color)) { obj = (T)(object)br.ReadRGB(); }
        else if (type == typeof(Vector2)) { obj = (T)(object)br.ReadPackedVector2(); }
        else
            throw new Exception("type not suppoerted for type " + type.ToString());
    }
    /// <summary>
    /// 支持<see cref="ReadObj"/>所支持类型的数组
    /// </summary>
    public static void ReadArray<T>(this BinaryReader br, out T[] array) {
        int length = br.ReadInt32();
        array = new T[length];
        foreach (int i in Range(length)) {
            br.ReadObj(out array[i]);
        }
    }
    /// <summary>
    /// 支持<see cref="ReadObj"/>所支持类型的列表
    /// </summary>
    public static void ReadList<T>(this BinaryReader br, ref List<T> list) {
        int count = br.ReadInt32();
        if (list == null) {
            list = new(count);
        }
        else {
            list.Clear();
        }
        foreach (int i in Range(count)) {
            br.ReadObj(out T element);
            list[i] = element;
        }
    }
    public static void ReadDict<TKey, TValue>(this BinaryReader br, ref Dictionary<TKey, TValue> dict) where TKey : notnull {
        int count = br.ReadInt32();
        dict = [];
        foreach (int _ in Range(count)) {
            br.ReadObj(out TKey key);
            br.ReadObj(out TValue value);
            dict.Add(key, value);
        }
    }
    */
    #endregion
    #region AppendItem
    public static StringBuilder AppendItem(this StringBuilder stringBuilder, Item item) =>
        stringBuilder.Append(ItemTagHandler.GenerateTag(item));
    public static StringBuilder AppendItem(this StringBuilder stringBuilder, int itemID) =>
        stringBuilder.Append(ItemTagHandler.GenerateTag(ContentSamples.ItemsByType[itemID]));
    #endregion
    #region 关于Tooltips
    #region AddIf
    public static bool AddIf(this List<TooltipLine> tooltips, bool condition, string name, string text, Color? overrideColor = null) {
        if (condition) {
            TooltipLine line = new(ModInstance, name, text);
            if (overrideColor != null) {
                line.OverrideColor = overrideColor;
            }
            tooltips.Add(line);
            return true;
        }
        return false;
    }
    public static bool AddIf(this List<TooltipLine> tooltips, bool condition, Func<string> nameDelegate, Func<string> textDelegate, Color? overrideColor = null) {
        if (condition) {
            TooltipLine line = new(ModInstance, nameDelegate?.Invoke(), textDelegate?.Invoke());
            if (overrideColor != null) {
                line.OverrideColor = overrideColor;
            }
            tooltips.Add(line);
            return true;
        }
        return false;
    }
    public static bool AddIf(this List<TooltipLine> tooltips, bool condition, string name, Func<string> textDelegate, Color? overrideColor = null) {
        if (condition) {
            TooltipLine line = new(ModInstance, name, textDelegate?.Invoke());
            if (overrideColor != null) {
                line.OverrideColor = overrideColor;
            }
            tooltips.Add(line);
            return true;
        }
        return false;
    }
    #endregion
    #region InsertIf
    public static bool InsertIf(this List<TooltipLine> tooltips, bool condition, int index, string name, string text, Color? overrideColor = null) {
        if (condition) {
            TooltipLine line = new(ModInstance, name, text);
            if (overrideColor != null) {
                line.OverrideColor = overrideColor;
            }
            tooltips.Insert(index, line);
            return true;
        }
        return false;
    }
    public static bool InsertIf(this List<TooltipLine> tooltips, bool condition, int index, Func<string> nameDelegate, Func<string> textDelegate, Color? overrideColor = null) {
        if (condition) {
            TooltipLine line = new(ModInstance, nameDelegate?.Invoke(), textDelegate?.Invoke());
            if (overrideColor != null) {
                line.OverrideColor = overrideColor;
            }
            tooltips.Insert(index, line);
            return true;
        }
        return false;
    }
    public static bool InsertIf(this List<TooltipLine> tooltips, bool condition, int index, string name, Func<string> textDelegate, Color? overrideColor = null) {
        if (condition) {
            TooltipLine line = new(ModInstance, name, textDelegate?.Invoke());
            if (overrideColor != null) {
                line.OverrideColor = overrideColor;
            }
            tooltips.Insert(index, line);
            return true;
        }
        return false;
    }
    #endregion
    public static List<TooltipLine> GetTooltips(this Item item) {
        //摘自Main.MouseText_DrawItemTooltip
        float num = 1f;
        if (item.DamageType == DamageClass.Melee && Main.player[Main.myPlayer].kbGlove) {
            num += 1f;
        }
        if (Main.player[Main.myPlayer].kbBuff) {
            num += 0.5f;
        }
        if (num != 1f) {
            item.knockBack *= num;
        }
        if (item.DamageType == DamageClass.Ranged && Main.player[Main.myPlayer].shroomiteStealth) {
            item.knockBack *= 1f + (1f - Main.player[Main.myPlayer].stealth) * 0.5f;
        }
        int num2 = 30;
        int oneDropLogo = -1;
        int researchLine = -1;
        float knockBack = item.knockBack;
        int numTooltips = 1;
        string[] texts = new string[num2];
        bool[] modifier = new bool[num2];
        bool[] badModifier = new bool[num2];
        for (int m = 0; m < num2; m++) {
            modifier[m] = false;
            badModifier[m] = false;
        }
        string[] names = new string[num2];
        Main.MouseText_DrawItemTooltip_GetLinesInfo(item, ref oneDropLogo, ref researchLine, knockBack, ref numTooltips, texts, modifier, badModifier, names, out int prefixlineIndex);
        if (Main.npcShop > 0 && item.value >= 0 && (item.type < ItemID.CopperCoin || item.type > ItemID.PlatinumCoin)) {
            Main.LocalPlayer.GetItemExpectedPrice(item, out long calcForSelling, out long calcForBuying);
            long price = (item.isAShopItem || item.buyOnce) ? calcForBuying : calcForSelling;
            if (item.shopSpecialCurrency != -1) {
                names[numTooltips] = "SpecialPrice";
                CustomCurrencyManager.GetPriceText(item.shopSpecialCurrency, texts, ref numTooltips, price);
            }
            else if (price > 0L) {
                string text = "";
                long platinum = 0L;
                long gold = 0L;
                long silver = 0L;
                long copper = 0L;
                price *= item.stack;
                if (!item.buy) {
                    price /= 5L;
                    if (price < 1L) {
                        price = 1L;
                    }
                    long singlePrice = price;
                    price *= item.stack;
                    int amount = Main.shopSellbackHelper.GetAmount(item);
                    if (amount > 0) {
                        price += (-singlePrice + calcForBuying) * Math.Min(amount, item.stack);
                    }
                }
                if (price < 1L) {
                    price = 1L;
                }
                if (price >= 1000000L) {
                    platinum = price / 1000000L;
                    price -= platinum * 1000000L;
                }
                if (price >= 10000L) {
                    gold = price / 10000L;
                    price -= gold * 10000L;
                }
                if (price >= 100L) {
                    silver = price / 100L;
                    price -= silver * 100L;
                }
                if (price >= 1L) {
                    copper = price;
                }
                if (platinum > 0L) {
                    text = string.Concat(
                    [
                    text,
                        platinum.ToString(),
                        " ",
                        Lang.inter[15].Value,
                        " "
                    ]);
                }
                if (gold > 0L) {
                    text = string.Concat(
                    [
                    text,
                        gold.ToString(),
                        " ",
                        Lang.inter[16].Value,
                        " "
                    ]);
                }
                if (silver > 0L) {
                    text = string.Concat(
                    [
                    text,
                        silver.ToString(),
                        " ",
                        Lang.inter[17].Value,
                        " "
                    ]);
                }
                if (copper > 0L) {
                    text = string.Concat(
                    [
                    text,
                        copper.ToString(),
                        " ",
                        Lang.inter[18].Value,
                        " "
                    ]);
                }
                if (!item.buy) {
                    texts[numTooltips] = Lang.tip[49].Value + " " + text;
                }
                else {
                    texts[numTooltips] = Lang.tip[50].Value + " " + text;
                }
                names[numTooltips] = "Price";
                numTooltips++;
            }
            else if (item.type != ItemID.DefenderMedal) {
                texts[numTooltips] = Lang.tip[51].Value;
                names[numTooltips] = "Price";
                numTooltips++;
            }
        }

        //摘自ItemLoader.ModifyTooltips
        List<TooltipLine> tooltips = [];
        for (int i = 0; i < numTooltips; i++) {
            tooltips.Add(new TooltipLine(ModInstance, names[i], texts[i]) {
                IsModifier = modifier[i],
                IsModifierBad = badModifier[i]
            });
        }
        if (item.prefix >= PrefixID.Count && prefixlineIndex != -1) {
            ModPrefix prefix = PrefixLoader.GetPrefix(item.prefix);
            IEnumerable<TooltipLine>? tooltipLines = prefix?.GetTooltipLines(item);
            if (tooltipLines != null) {
                foreach (TooltipLine line in tooltipLines) {
                    tooltips.Insert(prefixlineIndex, line);
                    prefixlineIndex++;
                }
            }
        }
        item.ModItem?.ModifyTooltips(tooltips);
        if (!item.IsAir) {
            foreach (GlobalItem globalItem in item.Globals) {
                globalItem.ModifyTooltips(item, tooltips);
            }
        }
        return tooltips;
    }
    #endregion
    #region Player
    public static bool ConsumeItem<T>(this Player player, bool reverseOrder = false, bool includeVoidBag = false) where T : ModItem
        => player.ConsumeItem(ModContent.ItemType<T>(), reverseOrder, includeVoidBag);
    #endregion
    #region Mod
    /// <summary>
    /// Retrieves the text value for a localization key belonging to this piece of content with the given suffix.<br/>
    /// The text returned will be for the currently selected language.
    /// </summary>
    public static string GetLocalizedValue(this Mod mod, string suffix, Func<string>? makeDefaultValue = null) => mod.GetLocalization(suffix, makeDefaultValue).Value;
    #endregion
    #region Item
    /// <summary>
    /// 以更安全的方式调用<see cref="Item.TryGetGlobalItem{T}(out T)"/>
    /// </summary>
    public static bool TryGetGlobalItemSafe<T>(this Item item, [NotNullWhen(true)] out T? result) where T : GlobalItem
        => TryGetGlobalSafe<GlobalItem, T>(item.type, item.EntityGlobals, out result);
    /// <summary>
    /// <br/>以更安全的方式调用<see cref="Item.TryGetGlobalItem{T}(T, out T)"/>
    /// <br/><paramref name="baseInstance"/>默认由<see cref="ModContent.GetInstance{T}"/>获得
    /// </summary>
    public static bool TryGetGlobalItemSafe<T>(this Item item, T baseInstance, [NotNullWhen(true)] out T? result) where T : GlobalItem
        => TryGetGlobalSafe<GlobalItem, T>(item.type, item.EntityGlobals, baseInstance, out result);
    public static bool IsNotAir(this Item item) => !item.IsAir;
    /// <summary>
    /// 在<paramref name="item"/>为空时也返回<see langword="true"/>
    /// </summary>
    public static bool IsAirS([NotNullWhen(false)] this Item? item) => item == null || item.IsAir;
    /// <summary>
    /// 在<paramref name="item"/>为空时返回<see langword="false"/>
    /// </summary>
    public static bool IsNotAirS([NotNullWhen(true)] this Item? item) => item != null && !item.IsAir;
    #endregion
    #region NPC
    public static bool TryGetGlobalNPCSafe<T>(this NPC npc, [NotNullWhen(true)] out T? result) where T : GlobalNPC
        => TryGetGlobalSafe<GlobalNPC, T>(npc.type, npc.EntityGlobals, out result);
    public static bool TryGetGlobalNPCSafe<T>(this NPC npc, T baseInstance, [NotNullWhen(true)] out T? result) where T : GlobalNPC
        => TryGetGlobalSafe<GlobalNPC, T>(npc.type, npc.EntityGlobals, baseInstance, out result);
    #endregion
    #region Projectile
    public static bool TryGetGlobalProjectileSafe<T>(this Projectile projectile, [NotNullWhen(true)] out T? result) where T : GlobalProjectile
        => TryGetGlobalSafe<GlobalProjectile, T>(projectile.type, projectile.EntityGlobals, out result);
    public static bool TryGetGlobalProjectileSafe<T>(this Projectile projectile, T baseInstance, [NotNullWhen(true)] out T? result) where T : GlobalProjectile
        => TryGetGlobalSafe<GlobalProjectile, T>(projectile.type, projectile.EntityGlobals, baseInstance, out result);

    #region FullyHostile和FullyFriendly
    public static bool IsFullyHostile(this Projectile projectile)
        => projectile.hostile && !projectile.friendly;
    public static bool IsFullyFriendly(this Projectile projectile)
        => projectile.friendly && !projectile.hostile;
    public static void SetFullyHostile(this Projectile projectile) {
        projectile.hostile = true;
        projectile.friendly = false;
    }
    public static void SetFullyFriendly(this Projectile projectile) {
        projectile.friendly = true;
        projectile.hostile = false;
    }
    #endregion
    #endregion
    #region Tile
    /// <summary>
    /// 返回 <see cref="Tile.HasTile"/> &amp;&amp; !<see cref="Tile.IsActuated"/>
    /// </summary>
    public static bool IsCollidable(this Tile tile) => tile.HasTile && !tile.IsActuated;
    /// <summary>
    /// 不算 SolidTop
    /// </summary>
    public static bool IsSolid(this Tile tile) {
        return tile.IsCollidable() && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
    }
    public static bool IsSolidTop(this Tile tile) {
        return tile.IsCollidable() && Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0;
    }
    #endregion
    #region UnifiedRandom
    public static double NextDouble(this UnifiedRandom rand, double maxValue) => rand.NextDouble() * maxValue;
    public static double NextDouble(this UnifiedRandom rand, double minValue, double maxValue) => rand.NextDouble() * (maxValue - minValue) + minValue;
    /// <summary>
    /// 会有<paramref name="p"/>的概率得到<see langword="true"/>
    /// </summary>
    public static bool NextBool(this UnifiedRandom rand, float p) => rand.NextFloat() < p;
    /// <summary>
    /// 会有<paramref name="p"/>的概率得到<see langword="true"/>
    /// </summary>
    public static bool NextBool(this UnifiedRandom rand, double p) => rand.NextDouble() < p;
    /// <summary>
    /// 会有<paramref name="p"/>的概率得到<see langword="true"/>
    /// 当<paramref name="p"/>不在 (0, 1) 区间时不会取随机数
    /// </summary>
    public static bool NextBoolS(this UnifiedRandom rand, float p) => p > 0 && (p >= 1 || rand.NextFloat() < p);
    /// <summary>
    /// 会有<paramref name="p"/>的概率得到<see langword="true"/>
    /// 当<paramref name="p"/>不在 (0, 1) 区间时不会取随机数
    /// </summary>
    public static bool NextBoolS(this UnifiedRandom rand, double p) => p > 0 && (p >= 1 || rand.NextDouble() < p);
    #endregion
    #region 几何
    public static Vector2? GetCollidePositionWithTile(this DirectedLine line, bool ignoreSolidTop = false) {
        var delta = line.Delta;
        foreach (var (tileX, tileY) in line.GetPassingTilesCoordinate()) {
            var tileRect = Main.tile[tileX, tileY].GetRect(tileX, tileY, delta, ignoreSolidTop);
            if (tileRect == null) {
                continue;
            }
            var collidePosition = line.GetCollidePosition(tileRect.Value);
            if (collidePosition != null) {
                return collidePosition.Value;
            }
        }
        return null;
    }
    #region Tile
    #region GetPassingTiles
    #region DirectedLine.GetPassingTiles
    public static IEnumerable<Tile> GetPassingTiles(this DirectedLine line) {
        foreach (var (tileX, tileY) in GetPassingTilesCoordinate(line)) {
            yield return Main.tile[tileX, tileY];
        }
    }
    public static IEnumerable<(int tileX, int tileY)> GetPassingTilesCoordinate(this DirectedLine line) {
        DirectedLine? cutQ = line.CutByRect(new Rect(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16));
        if (cutQ == null) {
            yield break;
        }
        line = cutQ.Value;
        bool lineGoRight = line.Start.X <= line.End.X;
        bool lineGoDown = line.Start.Y <= line.End.Y;
        int startTileX, endTileX, startTileY, endTileY;

        static int GetTilePosition(float p, bool forward, int tileMax) {
            if (p % 16 != 0) {
                return (int)(p / 16);
            }
            if (forward) {
                return ((int)(p / 16) - 1).WithMin(0);
            }
            return ((int)(p / 16)).WithMax(tileMax);
        }
        startTileX = GetTilePosition(line.Start.X, lineGoRight, Main.maxTilesX);
        endTileX = GetTilePosition(line.End.X, !lineGoRight, Main.maxTilesX);
        int lineGoRightIndicator = lineGoRight ? 1 : -1;
        int lineGoDownIndicator = lineGoDown ? 1 : -1;
        foreach (int tileX in Range(startTileX, endTileX + lineGoRightIndicator, RangeType.Automatic)) {
            DirectedLine? cut2Q = line.CutByXF(tileX * 16, tileX * 16 + 16);
            if (cut2Q == null) {
                // 不应存在这种情况, 不过先不处理它
                continue;
            }
            DirectedLine cut2 = cut2Q.Value;
            startTileY = GetTilePosition(cut2.Start.Y, lineGoDown, Main.maxTilesY);
            endTileY = GetTilePosition(cut2.End.Y, !lineGoDown, Main.maxTilesY);
            foreach (int tileY in Range(startTileY, endTileY + lineGoDownIndicator, RangeType.Automatic)) {
                yield return (tileX, tileY);
            }
        }
    }
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingTilesWithCoordinate(this DirectedLine line) {
        foreach (var (tileX, tileY) in GetPassingTilesCoordinate(line)) {
            yield return (Main.tile[tileX, tileY], tileX, tileY);
        }
    }
    #endregion
    #region Rect.GetPassingTiles
    public static IEnumerable<Tile> GetPassingTilesI(this Rect rect) => GetPassingTilesIF(rect.MakePositiveL());
    public static IEnumerable<Tile> GetPassingTilesIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                yield return Main.tile[i, j];
            }
        }
    }
    public static IEnumerable<(int tileX, int tileY)> GetPassingTilesCoordinateI(this Rect rect) => GetPassingTilesCoordinateIF(rect.MakePositiveL());
    public static IEnumerable<(int tileX, int tileY)> GetPassingTilesCoordinateIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                yield return (i, j);
            }
        }
    }
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingTilesWithCoordinateI(this Rect rect) => GetPassingTilesWithCoordinateIF(rect.MakePositiveL());
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingTilesWithCoordinateIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                yield return (Main.tile[i, j], i, j);
            }
        }
    }
    
    public static IEnumerable<Tile> GetPassingSolidTilesI(this Rect rect) => GetPassingSolidTilesIF(rect.MakePositiveL());
    public static IEnumerable<Tile> GetPassingSolidTilesIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                var tile = Main.tile[i, j];
                if (tile.IsSolid() && rect.CollideSolidTileIF(tile.BlockType, i, j, tileLeft, tileRight, tileTop, tileBottom)) {
                    yield return tile;
                }
            }
        }
    }
    public static IEnumerable<(int tileX, int tileY)> GetPassingSolidTilesCoordinateI(this Rect rect) => GetPassingSolidTilesCoordinateIF(rect.MakePositiveL());
    public static IEnumerable<(int tileX, int tileY)> GetPassingSolidTilesCoordinateIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                var tile = Main.tile[i, j];
                if (tile.IsSolid() && rect.CollideSolidTileIF(tile.BlockType, i, j, tileLeft, tileRight, tileTop, tileBottom)) {
                    yield return (i, j);
                }
            }
        }
    }
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingSolidTilesWithCoordinateI(this Rect rect) => GetPassingSolidTilesWithCoordinateIF(rect.MakePositiveL());
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingSolidTilesWithCoordinateIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                var tile = Main.tile[i, j];
                if (tile.IsSolid() && rect.CollideSolidTileIF(tile.BlockType, i, j, tileLeft, tileRight, tileTop, tileBottom)) {
                    yield return (tile, i, j);
                }
            }
        }
    }
    
    public static IEnumerable<Tile> GetPassingSolidTopTilesI(this Rect rect) => GetPassingSolidTopTilesIF(rect.MakePositiveL());
    public static IEnumerable<Tile> GetPassingSolidTopTilesIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                var tile = Main.tile[i, j];
                if (tile.IsSolidTop() && rect.CollideSolidTopTileIF(tile.BlockType, i, j, tileLeft, tileRight, tileTop, tileBottom)) {
                    yield return tile;
                }
            }
        }
    }
    public static IEnumerable<(int tileX, int tileY)> GetPassingSolidTopTilesCoordinateI(this Rect rect) => GetPassingSolidTopTilesCoordinateIF(rect.MakePositiveL());
    public static IEnumerable<(int tileX, int tileY)> GetPassingSolidTopTilesCoordinateIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                var tile = Main.tile[i, j];
                if (tile.IsSolidTop() && rect.CollideSolidTopTileIF(tile.BlockType, i, j, tileLeft, tileRight, tileTop, tileBottom)) {
                    yield return (i, j);
                }
            }
        }
    }
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingSolidTopTilesWithCoordinateI(this Rect rect) => GetPassingSolidTopTilesWithCoordinateIF(rect.MakePositiveL());
    public static IEnumerable<(Tile tile, int tileX, int tileY)> GetPassingSolidTopTilesWithCoordinateIF(this Rect rect) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        for (int j = tileTop; j <= tileBottom; ++j) {
            for (int i = tileLeft; i <= tileRight; ++i) {
                var tile = Main.tile[i, j];
                if (tile.IsSolidTop() && rect.CollideSolidTopTileIF(tile.BlockType, i, j, tileLeft, tileRight, tileTop, tileBottom)) {
                    yield return (tile, i, j);
                }
            }
        }
    }
    #endregion
    #endregion
    #region CollideTile
    #region Rect.CollideTile
    public static bool CollideSolidTileI(this Rect rect, BlockType blockType, int tileX, int tileY) {
        rect.MakePositive();
        return CollideSolidTileIF(rect, blockType, tileX, tileY);
    }
    /// <summary>
    /// 需要 <paramref name="rect"/> 的长宽非负
    /// </summary>
    public static bool CollideSolidTileIF(this Rect rect, BlockType blockType, int tileX, int tileY) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        if (tileX < tileLeft || tileX > tileRight || tileY < tileTop || tileY > tileBottom) {
            return false;
        }
        return CollideSolidTileIF(rect, blockType, tileX, tileY, tileLeft, tileRight, tileTop, tileBottom);
    }
    /// <summary>
    /// 需要 <paramref name="rect"/> 的长宽非负, 且 <paramref name="tileX"/> 和 <paramref name="tileY"/> 在范围内
    /// </summary>
    public static bool CollideSolidTileIF(this Rect rect, BlockType blockType, int tileX, int tileY, int tileLeft, int tileRight, int tileTop, int tileBottom) {
        return blockType switch {
            BlockType.Solid => true,
            BlockType.HalfBlock => tileY != tileBottom || rect.Bottom - tileY * 16 > 8,
            BlockType.SlopeDownLeft => tileX != tileLeft || tileY != tileBottom || rect.Bottom - tileY * 16 > rect.Left - tileX * 16,
            BlockType.SlopeDownRight => tileX != tileRight || tileY != tileBottom || rect.Bottom - tileY * 16 + rect.Right - tileX * 16 > 16,
            BlockType.SlopeUpLeft => tileX != tileLeft || tileY != tileTop || rect.Top - tileY * 16 + rect.Left - tileX * 16 < 16,
            BlockType.SlopeUpRight => tileX != tileRight || tileY != tileTop || rect.Top - tileY * 16 < rect.Right - tileX * 16,
            _ => true,
        };
    }
    public static bool CollideSolidTopTileI(this Rect rect, BlockType blockType, int tileX, int tileY) {
        rect.MakePositive();
        return CollideSolidTopTileIF(rect, blockType, tileX, tileY);
    }
    /// <summary>
    /// 需要 <paramref name="rect"/> 的长宽非负
    /// </summary>
    public static bool CollideSolidTopTileIF(this Rect rect, BlockType blockType, int tileX, int tileY) {
        rect.GetTileRangeIF(out int tileLeft, out int tileRight, out int tileTop, out int tileBottom);
        if (tileX < tileLeft || tileX > tileRight || tileY < tileTop || tileY > tileBottom) {
            return false;
        }
        return CollideSolidTopTileIF(rect, blockType, tileX, tileY, tileLeft, tileRight, tileTop, tileBottom);
    }
    /// <summary>
    /// 需要 <paramref name="rect"/> 的长宽非负, 且 <paramref name="tileX"/> 和 <paramref name="tileY"/> 在范围内
    /// </summary>
    public static bool CollideSolidTopTileIF(this Rect rect, BlockType blockType, int tileX, int tileY, int tileLeft, int tileRight, int tileTop, int tileBottom) {
        return blockType switch {
            BlockType.Solid => tileY != tileTop,
            BlockType.HalfBlock => (tileY != tileTop || rect.Top - tileY * 16 < 8) && (tileY != tileBottom || rect.Bottom - tileY * 16 > 8),
            BlockType.SlopeDownLeft => (tileX != tileLeft || tileY != tileBottom || rect.Bottom - tileY * 16 > rect.Left - tileX * 16)
                &&(tileX != tileRight || tileY != tileTop || rect.Top - tileY * 16 < rect.Right - tileX * 16),
            BlockType.SlopeDownRight => (tileX != tileRight || tileY != tileBottom || rect.Bottom - tileY * 16 + rect.Right - tileX * 16 > 16)
                && (tileX != tileLeft || tileY != tileTop || rect.Top - tileY * 16 + rect.Left - tileX * 16 < 16),
            _ => tileY != tileTop,
        };
    }
    #endregion
    public static bool CollideSolidTile(this Vector2 point, BlockType blockType, int tileX, int tileY) {
        if (point.X < tileX * 16 || point.X > (tileX + 1) * 16 || point.Y < tileY * 16 || point.Y > (tileY + 1) * 16) {
            return false;
        }
        return blockType switch {
            BlockType.Solid => true,
            BlockType.HalfBlock => point.Y % 16 >= 8,
            BlockType.SlopeDownLeft => point.Y % 16 >= point.X % 16,
            BlockType.SlopeDownRight => point.Y % 16 + point.X % 16 >= 16,
            BlockType.SlopeUpLeft => point.Y % 16 + point.X % 16 <= 16,
            BlockType.SlopeUpRight => point.Y % 16 <= point.X % 16,
            _ => true,
        };
    }
    public static bool CollideSolidTileI(this Vector2 point, BlockType blockType, int tileX, int tileY) {
        if (point.X <= tileX * 16 || point.X >= (tileX + 1) * 16 || point.Y <= tileY * 16 || point.Y >= (tileY + 1) * 16) {
            return false;
        }
        return blockType switch {
            BlockType.Solid => true,
            BlockType.HalfBlock => point.Y - tileY * 16 > 8,
            BlockType.SlopeDownLeft => point.Y - tileY * 16 > point.X - tileX * 16,
            BlockType.SlopeDownRight => point.Y - tileY * 16 + point.X - tileX * 16 > 16,
            BlockType.SlopeUpLeft => point.Y - tileY * 16 + point.X - tileX * 16 < 16,
            BlockType.SlopeUpRight => point.Y - tileY * 16 < point.X - tileX * 16,
            _ => true,
        };
    }
    #endregion
    #region GetTileRange
    /// <summary>
    /// <br/>获得矩形对应的物块边界, 全包含
    /// <br/>会限制在世界范围内
    /// </summary>
    public static void GetTileRangeI(this Rect rect, out int tileLeft, out int tileRight, out int tileTop, out int tileBottom) {
        rect.MakePositive();
        GetTileRangeIF(rect, out tileLeft, out tileRight, out tileTop, out tileBottom);
    }
    /// <summary>
    /// <br/>获得矩形对应的物块边界, 全包含
    /// <br/>会限制在世界范围内
    /// <br/>需保证 <paramref name="rect"/> 长宽非负
    /// </summary>
    public static void GetTileRangeIF(this Rect rect, out int tileLeft, out int tileRight, out int tileTop, out int tileBottom) {
        tileTop = ((int)(rect.Top / 16)).WithMin(0);
        tileLeft = ((int)(rect.Left / 16)).WithMin(0);
        tileRight = ((int)MathF.Ceiling(rect.Right / 16) - 1).WithMax(Main.maxTilesX - 1);
        tileBottom = ((int)MathF.Ceiling(rect.Bottom / 16) - 1).WithMax(Main.maxTilesY - 1);
    }
    #endregion
    #endregion
    public static Rect? GetRect(this Tile tile, int tileX, int tileY) {
        // 参考 Collision.TileCollision
        if (tile == null || !tile.HasTile || tile.IsActuated || !Main.tileSolid[tile.TileType]) {
            return null;
        }
        if (tile.IsHalfBlock) {
            return new Rect(tileX * 16, tileY * 16 + 8, 16, 8);
        }
        return new Rect(tileX * 16, tileY * 16, 16, 16);
    }
    public static Rect? GetRect(this Tile tile, int tileX, int tileY, Vector2 direction, bool ignoreSolidTop = false) {
        if (tile == null || !tile.HasTile || tile.IsActuated) {
            return null;
        }
        if (Main.tileSolidTop[tile.TileType]) {
            if (ignoreSolidTop) {
                return null;
            }
            if (direction.Y <= 0) {
                return null;
            }
            if (tile.TileFrameY != 0) {
                return null;
            }
            return new Rect(tileX * 16, tileY * 16, 16, 0);
        }
        else if (!Main.tileSolid[tile.TileType]) {
            return null;
        }
        if (tile.IsHalfBlock) {
            return new Rect(tileX * 16, tileY * 16 + 8, 16, 8);
        }
        return new Rect(tileX * 16, tileY * 16, 16, 16);
    }
    #endregion
    #region 解构拓展
    public static void Deconstruct(this CalculatedStyle self, out float x, out float y, out float width, out float height) {
        x = self.X;
        y = self.Y;
        width = self.Width;
        height = self.Height;
    }
    #endregion
    #region 绘制相关
    #region SpriteBatch 拓展
    #region Draw9Piece
    /// <summary>
    /// 按九宫格的形式画出图片
    /// </summary>
    /// <param name="destination">目标矩形</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9Piece(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false)
        => Draw9Piece(spriteBatch, texture, 0, 0, texture.Width, texture.Height, destination.X, destination.Y, destination.Width, destination.Height, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    /// <summary>
    /// 按九宫格的形式画出图片
    /// </summary>
    /// <param name="source">原图的裁剪矩形</param>
    /// <param name="destination">目标矩形</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9Piece(this SpriteBatch spriteBatch, Texture2D texture, Rectangle? source, Rectangle destination, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false) {
        #region 解构
        destination.Deconstruct(out int dx, out int dy, out int dw, out int dh);
        int sx, sy, sw, sh;
        if (source != null)
            source.Value.Deconstruct(out sx, out sy, out sw, out sh);
        else {
            sx = sy = 0;
            sw = texture.Width;
            sh = texture.Height;
        }
        #endregion
        Draw9Piece(spriteBatch, texture, sx, sy, sw, sh, dx, dy, dw, dh, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    }
    /// <summary>
    /// 按九宫格的形式画出图片
    /// </summary>
    /// <param name="dx">目标矩形的 x 坐标</param>
    /// <param name="dy">目标矩形的 y 坐标</param>
    /// <param name="dw">目标矩形的宽度</param>
    /// <param name="dh">目标矩形的宽度</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9Piece(this SpriteBatch spriteBatch, Texture2D texture, int dx, int dy, int dw, int dh, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false)
        => Draw9Piece(spriteBatch, texture, 0, 0, texture.Width, texture.Height, dx, dy, dw, dh, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    /// <summary>
    /// 按九宫格的形式画出图片
    /// </summary>
    /// <param name="sx">原图的裁剪矩形的 x 坐标</param>
    /// <param name="sy">原图的裁剪矩形的 y 坐标</param>
    /// <param name="sw">原图的裁剪矩形的宽度</param>
    /// <param name="sh">原图的裁剪矩形的宽度</param>
    /// <param name="dx">目标矩形的 x 坐标</param>
    /// <param name="dy">目标矩形的 y 坐标</param>
    /// <param name="dw">目标矩形的宽度</param>
    /// <param name="dh">目标矩形的宽度</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9Piece(this SpriteBatch spriteBatch, Texture2D texture, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false) {
        #region 保险措施
        if (texture.Width <= 0 || texture.Height <= 0 || dw <= 0 || dh <= 0) {
            return;
        }
        if (sw <= 0 || sh <= 0 || sx >= texture.Width || sy >= texture.Height || sx + sw <= 0 || sy + sh <= 0) {
            return;
        }
        sx.ClampMinTo(0);
        sy.ClampMinTo(0);
        sw.ClampMaxTo(texture.Width - sx);
        sh.ClampMaxTo(texture.Height - sy);

        borderX.ClampMinTo(0);
        borderY.ClampMinTo(0);
        borderRight.ClampMinTo(0);
        borderBottom.ClampMinTo(0);
        destinationLeft.ClampMinTo(0);
        destinationTop.ClampMinTo(0);
        destinationRight.ClampMinTo(0);
        destinationBottom.ClampMinTo(0);
        #endregion
        Draw9PieceF(spriteBatch, texture, sx, sy, sw, sh, dx, dy, dw, dh, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    }
    /// <summary>
    /// <br/>按九宫格的形式画出图片
    /// <br/>需确保:
    /// <br/>图片的宽度和高度大于0
    /// <br/>目标矩形的宽度和高度大于0
    /// <br/>若有原裁剪矩形, 则它需要在图片范围内且宽高都大于0
    /// <br/>边框参数不为空时都不小于0
    /// </summary>
    /// <param name="destination">目标矩形</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9PieceF(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false)
        => Draw9PieceF(spriteBatch, texture, 0, 0, texture.Width, texture.Height, destination.X, destination.Y, destination.Width, destination.Height, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    /// <summary>
    /// <br/>按九宫格的形式画出图片
    /// <br/>需确保:
    /// <br/>图片的宽度和高度大于0
    /// <br/>目标矩形的宽度和高度大于0
    /// <br/>若有原裁剪矩形, 则它需要在图片范围内且宽高都大于0
    /// <br/>边框参数不为空时都不小于0
    /// </summary>
    /// <param name="source">原图的裁剪矩形</param>
    /// <param name="destination">目标矩形</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9PieceF(this SpriteBatch spriteBatch, Texture2D texture, Rectangle? source, Rectangle destination, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false) {
        #region 解构
        destination.Deconstruct(out int dx, out int dy, out int dw, out int dh);
        int sx, sy, sw, sh;
        if (source != null) {
            source.Value.Deconstruct(out sx, out sy, out sw, out sh);
        }
        else {
            sx = sy = 0;
            sw = texture.Width;
            sh = texture.Height;
        }
        #endregion
        Draw9PieceF(spriteBatch, texture, sx, sy, sw, sh, dx, dy, dw, dh, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    }
    /// <summary>
    /// <br/>按九宫格的形式画出图片
    /// <br/>需确保:
    /// <br/>图片的宽度和高度大于0
    /// <br/>目标矩形的宽度和高度大于0
    /// <br/>若有原裁剪矩形, 则它需要在图片范围内且宽高都大于0
    /// <br/>边框参数不为空时都不小于0
    /// </summary>
    /// <param name="dx">目标矩形的 x 坐标</param>
    /// <param name="dy">目标矩形的 y 坐标</param>
    /// <param name="dw">目标矩形的宽度</param>
    /// <param name="dh">目标矩形的宽度</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9PieceF(this SpriteBatch spriteBatch, Texture2D texture, int dx, int dy, int dw, int dh, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false)
        => Draw9PieceF(spriteBatch, texture, 0, 0, texture.Width, texture.Height, dx, dy, dw, dh, color, borderX, borderY, borderRight, borderBottom, destinationLeft, destinationTop, destinationRight, destinationBottom, forceDrawCenter);
    /// <summary>
    /// <br/>按九宫格的形式画出图片
    /// <br/>需确保:
    /// <br/>图片的宽度和高度大于0
    /// <br/>目标矩形的宽度和高度大于0
    /// <br/>若有原裁剪矩形, 则它需要在图片范围内且宽高都大于0
    /// <br/>边框参数不为空时都不小于0
    /// </summary>
    /// <param name="sx">原图的裁剪矩形的 x 坐标</param>
    /// <param name="sy">原图的裁剪矩形的 y 坐标</param>
    /// <param name="sw">原图的裁剪矩形的宽度</param>
    /// <param name="sh">原图的裁剪矩形的宽度</param>
    /// <param name="dx">目标矩形的 x 坐标</param>
    /// <param name="dy">目标矩形的 y 坐标</param>
    /// <param name="dw">目标矩形的宽度</param>
    /// <param name="dh">目标矩形的宽度</param>
    /// <param name="borderX">左边框(在原图)的宽度</param>
    /// <param name="borderY">
    /// <br/>上边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// </param>
    /// <param name="borderRight">
    /// <br/>右边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderX"/>相同
    /// <br/>若左右边框之和大于原图宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="borderBottom">
    /// <br/>下边框(在原图)的宽度
    /// <br/>若为空, 则与<paramref name="borderY"/>相同
    /// <br/>若上下边框之和大于原图高度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationLeft">
    /// <br/>目标左边框的宽度
    /// <br/>若为空则与最终原图左边框的宽度相同
    /// </param>
    /// <param name="destinationTop">
    /// <br/>目标上边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图上左边框的比例
    /// </param>
    /// <param name="destinationRight">
    /// <br/>目标右边框的宽度
    /// <br/>若为空则与设置为与目标左边框成原图右左边框的比例
    /// <br/>若左右边框之和大于目标宽度 - 1, 则按比例缩小
    /// </param>
    /// <param name="destinationBottom">
    /// <br/>目标下边框的宽度
    /// <br/>若为空则与设置为与目标上边框成原图下上边框的比例
    /// <br/>若上下边框之和大于目标高度 - 1, 则按比例缩小
    /// </param>
    public static void Draw9PieceF(this SpriteBatch spriteBatch, Texture2D texture, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, Color color, int borderX, int? borderY = null, int? borderRight = null, int? borderBottom = null, int? destinationLeft = null, int? destinationTop = null, int? destinationRight = null, int? destinationBottom = null, bool forceDrawCenter = false) {
        int fdc = forceDrawCenter.ToInt();
        #region 设置sdx1, sdy1, sdx3, sdy3 (bl, bt, br, bb) (border left, border top, border right, border bottom)
        int sdx1, sdy1, sdx3, sdy3;
        int by = borderY ?? borderX;
        if (borderRight == null) {
            sdx1 = sdx3 = borderX.WithMax((texture.Width - fdc) / 2);
        }
        else {
            int idealWidth = borderX + (int)borderRight;
            if (idealWidth < texture.Width) {
                sdx1 = borderX;
                sdx3 = (int)borderRight;
            }
            else {
                sdx1 = (texture.Width - fdc) * borderX / idealWidth;
                sdx3 = (texture.Width - fdc) * (int)borderRight / idealWidth;
            }
        }
        if (borderBottom == null) {
            sdy1 = sdy3 = by.WithMax((texture.Height - fdc) / 2);
        }
        else {
            int idealHeight = by + (int)borderBottom;
            if (idealHeight < texture.Height) {
                sdy1 = by;
                sdy3 = (int)borderBottom;
            }
            else {
                sdy1 = (texture.Height - fdc) * by / idealHeight;
                sdy3 = (texture.Height - fdc) * (int)borderBottom / idealHeight;
            }
        }
        #endregion
        #region 设置 ddx1, ddy1, ddx3, ddy3 (dl, dt, dr, db)
        int ddx1, ddy1, ddx3, ddy3;
        destinationBottom ??= destinationTop;
        ddx1 = destinationLeft ?? sdx1;
        ddx3 = destinationRight ?? (ddx1 * sdx3 / sdx1);
        ddy1 = destinationTop ?? (ddx1 * sdy1 / sdx1);
        ddy3 = destinationBottom ?? (ddy1 * sdy3 / sdy1);
        int dlr = ddx1 + ddx3;
        if (dlr >= dw) {
            ddx1 = (dw - fdc) * ddx1 / dlr;
            ddx3 = (dw - fdc) * ddx3 / dlr;
        }
        int dtb = ddy1 + ddy3;
        if (ddy1 + ddy3 >= dh) {
            ddy1 = (dh - fdc) * ddy1 / dtb;
            ddy3 = (dh - fdc) * ddy3 / dtb;
        }
        #endregion
        //四角
        int dx1 = dx, dy1 = dy, sx1 = sx, sy1 = sy;
        int sx3 = sx + sw - sdx3;
        int sy3 = sy + sh - sdy3;
        int dx3 = dx + dw - ddx3;
        int dy3 = dy + dh - ddy3;
        if (ddx1 > 0 && ddy1 > 0)
            spriteBatch.Draw(texture, dx1, dy1, ddx1, ddy1, sx1, sy1, sdx1, sdy1, color);
        if (ddx3 > 0 && ddy1 > 0)
            spriteBatch.Draw(texture, dx3, dy1, ddx3, ddy1, sx3, sy1, sdx3, sdy1, color);
        if (ddx1 > 0 && ddy3 > 0)
            spriteBatch.Draw(texture, dx1, dy3, ddx1, ddy3, sx1, sy3, sdx1, sdy3, color);
        if (ddx3 > 0 && ddy3 > 0)
            spriteBatch.Draw(texture, dx3, dy3, ddx3, ddy3, sx3, sy3, sdx3, sdy3, color);
        //四边
        int sx2 = sx + sdx1;
        int sy2 = sy + sdy1;
        int dx2 = dx + ddx1;
        int dy2 = dy + ddy1;
        int sdx2 = sw - sdx1 - sdx3;
        int sdy2 = sh - sdy1 - sdy3;
        int ddx2 = dw - ddx1 - ddx3;
        int ddy2 = dh - ddy1 - ddy3;
        if (ddy1 > 0 && ddx2 > 0)
            spriteBatch.Draw(texture, dx2, dy1, ddx2, ddy1, sx2, sy1, sdx2, sdy1, color);
        if (ddy3 > 0 && ddx2 > 0)
            spriteBatch.Draw(texture, dx2, dy3, ddx2, ddy3, sx2, sy3, sdx2, sdy3, color);
        if (ddx1 > 0 && ddy2 > 0)
            spriteBatch.Draw(texture, dx1, dy2, ddx1, ddy2, sx1, sy2, sdx1, sdy2, color);
        if (ddx3 > 0 && ddy2 > 0)
            spriteBatch.Draw(texture, dx3, dy2, ddx3, ddy2, sx3, sy2, sdx3, sdy2, color);
        //中心
        if (ddx2 > 0 && ddy2 > 0)
            spriteBatch.Draw(texture, new Rectangle(dx2, dy2, ddx2, ddy2), new Rectangle(sx2, sy2, sdx2, sdy2), color);
    }
    #endregion
    public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, int dx, int dy, int dw, int dh, int sx, int sy, int sw, int sh, Color color)
        => spriteBatch.Draw(texture, new Rectangle(dx, dy, dw, dh), new Rectangle(sx, sy, sw, sh), color);
    public static void DrawBox(this SpriteBatch self, Rectangle destination, Color color, int width = 1) => DrawBox(self, destination, color, Color.Transparent, width);
    public static void DrawBox(this SpriteBatch self, Rectangle destination, Color color, Color innerColor, int width = 1) {
        var texture = Textures.Colors.White.Value;
        int size = Math.Min(destination.Width, destination.Height);
        if (width < 0 || (size + 1) / 2 <= width) {
            self.Draw(texture, destination, color);
            return;
        }
        int dx1 = destination.X, dx2 = dx1 + width, dx3 = dx1 + destination.Width - width;
        int dy1 = destination.Y, dy2 = dy1 + width, dy3 = dy1 + destination.Height - width;
        int dw = destination.Width, ddx = dw - 2 * width, ddy = destination.Height - 2 * width;
        self.Draw(texture, new Rectangle(dx1, dy1, dw, width), color);
        self.Draw(texture, new Rectangle(dx1, dy3, dw, width), color);
        self.Draw(texture, new Rectangle(dx1, dy2, width, ddy), color);
        self.Draw(texture, new Rectangle(dx3, dy2, width, ddy), color);
        if (innerColor != default)
            self.Draw(texture, new Rectangle(dx2, dy2, ddx, ddy), innerColor);
    }
    public static void DrawInvBG(this SpriteBatch spriteBatch, int x, int y, int w, int h, int border = 10, Color? color = null) {
        color ??= new Color(63, 65, 151, 255) * 0.785f;
        spriteBatch.Draw9PieceF(TextureAssets.InventoryBack13.Value, x, y, w, h, color.Value, 10, destinationLeft: border);
    }
    /// <summary>
    /// 自定透明度
    /// </summary>
    public static void DrawInvBGT(this SpriteBatch spriteBatch, int x, int y, int w, int h, int border = 10, float transparency = 0.785f) {
        spriteBatch.Draw9PieceF(TextureAssets.InventoryBack13.Value, x, y, w, h, new Color(63, 65, 151, 255) * transparency, 10, destinationLeft: border);
    }
    #region DrawLine
    /// <summary>
    /// 依据屏幕坐标画线
    /// </summary>
    /// <param name="start">起始位置, 屏幕坐标</param>
    /// <param name="end">结束位置, 屏幕坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="color">颜色, 默认白</param>
    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, float width = 1, Color? color = null) {
        color ??= Color.White;
        float distance = Vector2.Distance(start, end);
        Vector2 scale = new(distance, width);
        float rotation = (end - start).ToRotation();
        Vector2 origin = new(0, 0.5f);
        spriteBatch.Draw(Textures.Colors.White.Value, start, null, color.Value, rotation, origin, scale, SpriteEffects.None, 0);
    }
    /// <summary>
    /// 依据世界坐标画线
    /// </summary>
    /// <param name="start">起始位置, 世界坐标</param>
    /// <param name="end">结束位置, 世界坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="color">颜色, 默认白</param>
    public static void DrawLineInWorld(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, float width = 1, Color? color = null) {
        DrawLine(spriteBatch, start - Main.screenPosition, end - Main.screenPosition, width, color);
    }
    /// <summary>
    /// 画矩形
    /// </summary>
    /// <param name="center">矩形的中心, 世界坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="rotation">旋转值, 弧度制</param>
    /// <param name="color">颜色, 默认白</param>
    public static void DrawRectWithCenter(this SpriteBatch spriteBatch, Vector2 center, float width = 1, float height = 1, float rotation = 0f, Color? color = null) {
        color ??= Color.White;
        spriteBatch.Draw(Textures.Colors.White.Value, center, null, color.Value, rotation, new Vector2(0.5f), new Vector2(width, height), SpriteEffects.None, 0);
    }
    #endregion
    #region RebeginTemporarily, EndTemporarily
    public static SpriteBatchRebeginTemporarilyDisposable RebeginTemporarily(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null)
        => new(spriteBatch, sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.identity);
    public static SpriteBatchRebeginTemporarilyDisposable RebeginTemporarily(this SpriteBatch spriteBatch, SpriteSortMode sortMode, BlendState? blendState, SamplerState? samplerState, DepthStencilState? depthStencilState, RasterizerState? rasterizerState, Effect? effect, Matrix transformMatrix)
        => new(spriteBatch, sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

    public static SpriteBatchRebeginTemporarilyDisposable RebeginTemporarilyStill(this SpriteBatch spriteBatch, SpriteSortMode? sortMode = null, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformMatrix = null)
        => new(spriteBatch, sortMode ?? spriteBatch.sortMode, blendState ?? spriteBatch.blendState, samplerState ?? spriteBatch.samplerState, depthStencilState ?? spriteBatch.depthStencilState, rasterizerState ?? spriteBatch.rasterizerState, effect ?? spriteBatch.customEffect, transformMatrix ?? spriteBatch.transformMatrix);
    public static SpriteBatchRebeginTemporarilyDisposable RebeginTemporarilyStillWithNullEffect(this SpriteBatch spriteBatch, SpriteSortMode? sortMode = null, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Matrix? transformMatrix = null)
        => new(spriteBatch, sortMode ?? spriteBatch.sortMode, blendState ?? spriteBatch.blendState, samplerState ?? spriteBatch.samplerState, depthStencilState ?? spriteBatch.depthStencilState, rasterizerState ?? spriteBatch.rasterizerState, null, transformMatrix ?? spriteBatch.transformMatrix);


    public readonly record struct SpriteBatchRebeginTemporarilyDisposable : IDisposable {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteSortMode _sortMode;
        private readonly BlendState _blendState;
        private readonly SamplerState _samplerState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;
        private readonly Effect _effect;
        private readonly Matrix _transformMatrix;

        /// <summary></summary>
        /// <param name="blendState">默认值 <see cref="BlendState.AlphaBlend"/></param>
        /// <param name="samplerState">默认值 <see cref="SamplerState.LinearClamp"/></param>
        /// <param name="depthStencilState">默认值 <see cref="DepthStencilState.None"/></param>
        /// <param name="rasterizerState">默认值 <see cref="RasterizerState.CullCounterClockwise"/></param>
        public SpriteBatchRebeginTemporarilyDisposable(SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null)
            : this(spriteBatch, sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.identity) { }
        public SpriteBatchRebeginTemporarilyDisposable(SpriteBatch spriteBatch, SpriteSortMode sortMode, BlendState? blendState, SamplerState? samplerState, DepthStencilState? depthStencilState, RasterizerState? rasterizerState, Effect? effect, Matrix transformMatrix) {
            _spriteBatch = spriteBatch;
            _sortMode          = spriteBatch.sortMode         ;
            _blendState        = spriteBatch.blendState       ;
            _samplerState      = spriteBatch.samplerState     ;
            _depthStencilState = spriteBatch.depthStencilState;
            _rasterizerState   = spriteBatch.rasterizerState  ;
            _effect            = spriteBatch.customEffect     ;
            _transformMatrix   = spriteBatch.transformMatrix  ;

            _spriteBatch.End();
            _spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        }
        public void Dispose() {
            // GC.SuppressFinalize(this);
            _spriteBatch.End();
            _spriteBatch.Begin(_sortMode, _blendState, _samplerState, _depthStencilState, _rasterizerState, _effect, _transformMatrix);
        }
    }

    public static SpriteBatchEndTemporarilyDisposable EndTemporarily(this SpriteBatch spriteBatch) => new(spriteBatch);
    public readonly record struct SpriteBatchEndTemporarilyDisposable : IDisposable {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteSortMode _sortMode;
        private readonly BlendState _blendState;
        private readonly SamplerState _samplerState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;
        private readonly Effect _effect;
        private readonly Matrix _transformMatrix;
        public SpriteBatchEndTemporarilyDisposable(SpriteBatch spriteBatch) {
            _spriteBatch = spriteBatch;
            _sortMode          = spriteBatch.sortMode         ;
            _blendState        = spriteBatch.blendState       ;
            _samplerState      = spriteBatch.samplerState     ;
            _depthStencilState = spriteBatch.depthStencilState;
            _rasterizerState   = spriteBatch.rasterizerState  ;
            _effect            = spriteBatch.customEffect     ;
            _transformMatrix   = spriteBatch.transformMatrix  ;

            _spriteBatch.End();
        }
        public void Dispose() {
            // GC.SuppressFinalize(this);
            _spriteBatch.Begin(_sortMode, _blendState, _samplerState, _depthStencilState, _rasterizerState, _effect, _transformMatrix);
        }
    }
    #endregion

    public static BlendState GetBlendState(this SpriteBatch spriteBatch) => spriteBatch.blendState;
    public static void SetBlendState(this SpriteBatch spriteBatch, BlendState state) => spriteBatch.blendState = state;

    public static SamplerState GetSamplerState(this SpriteBatch spriteBatch) => spriteBatch.samplerState;
    public static void SetSamplerState(this SpriteBatch spriteBatch, SamplerState state) => spriteBatch.samplerState = state;

    public static SpriteSortMode GetSortMode(this SpriteBatch spriteBatch) => spriteBatch.sortMode;
    public static void SetSortMode(this SpriteBatch spriteBatch, SpriteSortMode sortMode) => spriteBatch.sortMode = sortMode;

    public static Effect? GetCusomEffect(this SpriteBatch spriteBatch) => spriteBatch.customEffect;
    public static void SetCusomEffect(this SpriteBatch spriteBatch, Effect? effect) => spriteBatch.customEffect = effect;

    public static Matrix GetTransformMatrix(this SpriteBatch spriteBatch) => spriteBatch.transformMatrix;
    public static void SetTransformMatrix(this SpriteBatch spriteBatch, Matrix matrix) => spriteBatch.transformMatrix = matrix;

    public static bool BeginCalled(this SpriteBatch spriteBatch) => spriteBatch.beginCalled;
    public static void SetBeginCalled(this SpriteBatch spriteBatch, bool beginCalled) => spriteBatch.beginCalled = beginCalled;
    #endregion
    #region Texture2D 拓展
    public static void SaveAsPng(this Texture2D self, string path) {
        using var file = File.Open(path, FileMode.Create);
        self.SaveAsPng(file, self.Width, self.Height);
    }
    #endregion
    #region UI

    #region SetPositionAndSize
    public static void SetPositionAndSize(this UIElement self, float leftPixels, float leftPercent, float topPixels, float topPercent, float widthPixels, float widthPercent, float heightPixels, float heightPercent) {
        self.Top.Set(topPixels, topPercent);
        self.Left.Set(leftPixels, leftPercent);
        self.Width.Set(widthPixels, widthPercent);
        self.Height.Set(heightPixels, heightPercent);
    }
    public static void SetPosition(this UIElement self, float leftPixels, float leftPercent, float topPixels, float topPercent) {
        self.Left.Set(leftPixels, leftPercent);
        self.Top.Set(topPixels, topPercent);
    }
    public static void SetPosition(this UIElement self, float leftPixels, float topPixels) {
        self.Left.Pixels = leftPixels;
        self.Top.Pixels = topPixels;
    }
    public static void SetSize(this UIElement self, float widthPixels, float widthPercent, float heightPixels, float heightPercent) {
        self.Width.Set(widthPixels, widthPercent);
        self.Height.Set(heightPixels, heightPercent);
    }
    public static void SetSize(this UIElement self, float widthPixels, float heightPixels) {
        self.Width.Pixels = widthPixels;
        self.Height.Pixels = heightPixels;
    }
    public static void SetX(this UIElement self, float leftPixels, float leftPercent, float widthPixels, float widthPercent) {
        self.Left.Set(leftPixels, leftPercent);
        self.Width.Set(widthPixels, widthPercent);
    }
    public static void SetX(this UIElement self, float leftPixels, float widthPixels) {
        self.Left.Pixels = leftPixels;
        self.Width.Pixels = widthPixels;
    }
    public static void SetY(this UIElement self, float topPixels, float topPercent, float heightPixels, float heightPercent) {
        self.Top.Set(topPixels, topPercent);
        self.Height.Set(heightPixels, heightPercent);
    }
    public static void SetY(this UIElement self, float topPixels, float heightPixels) {
        self.Top.Pixels = topPixels;
        self.Height.Pixels = heightPixels;
    }
    public static void SetLeft(this UIElement self, float leftPixels, float leftPercent) => self.Left.Set(leftPixels, leftPercent);
    public static void SetLeft(this UIElement self, float leftPixels) => self.Left.Pixels = leftPixels;
    public static void SetTop(this UIElement self, float topPixels, float topPercent) => self.Top.Set(topPixels, topPercent);
    public static void SetTop(this UIElement self, float topPixels) => self.Top.Pixels = topPixels;
    public static void SetWidth(this UIElement self, float widthPixels, float widthPercent) => self.Width.Set(widthPixels, widthPercent);
    public static void SetWidth(this UIElement self, float widthPixels) => self.Width.Pixels = widthPixels;
    public static void SetHeight(this UIElement self, float heightPixels, float heightPercent) => self.Height.Set(heightPixels, heightPercent);
    public static void SetHeight(this UIElement self, float heightPixels) => self.Height.Pixels = heightPixels;
    public static void SetRight(this UIElement self, float rightPixels, float rightPercent) => self.Left.Set(rightPixels - self.Width.Pixels, rightPercent - self.Width.Percent);
    public static void SetRight(this UIElement self, float rightPixels) => self.Left.Pixels = rightPixels - self.Width.Pixels;
    public static void SetBottom(this UIElement self, float bottomPixels, float bottomPercent) => self.Top.Set(bottomPixels - self.Height.Pixels, bottomPercent - self.Height.Percent);
    public static void SetBottom(this UIElement self, float bottomPixels) => self.Top.Pixels = bottomPixels - self.Height.Pixels;
    #endregion
    #region Expand
    public static void ExpandLeft(this UIElement self, float leftPixels) => self.SetX(leftPixels, self.Left.Pixels + self.Width.Pixels - leftPixels);
    public static void ExpandTop(this UIElement self, float topPixels) => self.SetY(topPixels, self.Top.Pixels + self.Height.Pixels - topPixels);
    public static void ExpandRight(this UIElement self, float rightPixels) => self.Width.Pixels = rightPixels - self.Left.Pixels;
    public static void ExpandBottom(this UIElement self, float bottomPixels) => self.Height.Pixels = bottomPixels - self.Top.Pixels;
    public static void ExpandRightWithLeft(this UIElement self, float leftPixels) => self.Width.Pixels += leftPixels - self.Left.Pixels;
    public static void ExpandBottomWithTop(this UIElement self, float topPixels) => self.Height.Pixels += topPixels - self.Top.Pixels;
    /// <summary>
    /// 朝左右伸展
    /// </summary>
    public static void ExpandSizeX(this UIElement self, float size) {
        self.Left.Pixels -= size;
        self.Width.Pixels += 2 * size;
    }
    /// <summary>
    /// 朝上下伸展
    /// </summary>
    public static void ExpandSizeY(this UIElement self, float size) {
        self.Top.Pixels -= size;
        self.Height.Pixels += 2 * size;
    }
    /// <summary>
    /// 朝四周伸展
    /// </summary>
    public static void ExpandSize(this UIElement self, float size) {
        ExpandSizeX(self, size);
        ExpandSizeY(self, size);
    }
    #region ExpandWithMin
    public static void ExpandLeftWithMinWidth(this UIElement self, float leftPixels, float minWidthPixels) {
        float rightPixels = self.Left.Pixels + self.Width.Pixels;
        float widthToSet = (rightPixels - leftPixels).ClampMin(minWidthPixels);
        self.SetX(rightPixels - widthToSet, widthToSet);
    }
    public static void ExpandTopWithMinHeight(this UIElement self, float topPixels, float minHeightPixels) {
        float bottomPixels = self.Top.Pixels + self.Height.Pixels;
        float heightToSet = (bottomPixels - topPixels).ClampMin(minHeightPixels);
        self.SetY(bottomPixels - heightToSet, heightToSet);
    }
    public static void ExpandRightWithMinWidth(this UIElement self, float rightPixels, float minWidthPixels) => self.Width.Pixels = (rightPixels - self.Left.Pixels).ClampMin(minWidthPixels);
    public static void ExpandBottomWithMinHeight(this UIElement self, float bottomPixels, float minHeightPixels) => self.Height.Pixels = (bottomPixels - self.Top.Pixels).ClampMin(minHeightPixels);
    public static void ExpandRightWithLeftWithMinWidth(this UIElement self, float leftPixels, float minWidthPixels) => self.Width.Pixels = (self.Width.Pixels + leftPixels - self.Left.Pixels).ClampMin(minWidthPixels);
    public static void ExpandBottomWidthTopWithMinHeight(this UIElement self, float topPixels, float minHeightPixels) => self.Height.Pixels = (self.Height.Pixels + topPixels - self.Top.Pixels).ClampMin(minHeightPixels);
    #endregion
    #region ExpandWithMax
    public static void ExpandLeftWithMaxWidth(this UIElement self, float leftPixels, float maxWidthPixels) {
        float rightPixels = self.Left.Pixels + self.Width.Pixels;
        float widthToSet = (rightPixels - leftPixels).ClampMax(maxWidthPixels);
        self.SetX(rightPixels - widthToSet, widthToSet);
    }
    public static void ExpandTopWithMaxHeight(this UIElement self, float topPixels, float maxHeightPixels) {
        float bottomPixels = self.Top.Pixels + self.Height.Pixels;
        float heightToSet = (bottomPixels - topPixels).ClampMax(maxHeightPixels);
        self.SetY(bottomPixels - heightToSet, heightToSet);
    }
    public static void ExpandRightWithMaxWidth(this UIElement self, float rightPixels, float maxWidthPixels) => self.Width.Pixels = (rightPixels - self.Left.Pixels).ClampMax(maxWidthPixels);
    public static void ExpandBottomWithMaxHeight(this UIElement self, float bottomPixels, float maxHeightPixels) => self.Height.Pixels = (bottomPixels - self.Top.Pixels).ClampMax(maxHeightPixels);
    public static void ExpandRightWithLeftWithMaxWidth(this UIElement self, float leftPixels, float maxWidthPixels) => self.Width.Pixels = (self.Width.Pixels + leftPixels - self.Left.Pixels).ClampMax(maxWidthPixels);
    public static void ExpandBottomWidthTopWithMaxHeight(this UIElement self, float topPixels, float maxHeightPixels) => self.Height.Pixels = (self.Height.Pixels + topPixels - self.Top.Pixels).ClampMax(maxHeightPixels);
    #endregion
    #region ExpandWithRange
    public static void ExpandLeftWithRange(this UIElement self, float leftPixels, float minWidthPixels, float maxWidthPixels) {
        float rightPixels = self.Left.Pixels + self.Width.Pixels;
        float widthToSet = (rightPixels - leftPixels).Clamp(minWidthPixels, maxWidthPixels);
        self.SetX(rightPixels - widthToSet, widthToSet);
    }
    public static void ExpandTopWithRange(this UIElement self, float topPixels, float minHeightPixels, float maxHeightPixels) {
        float bottomPixels = self.Top.Pixels + self.Height.Pixels;
        float heightToSet = (bottomPixels - topPixels).Clamp(minHeightPixels, maxHeightPixels);
        self.SetY(bottomPixels - heightToSet, heightToSet);
    }
    public static void ExpandRightWithRange(this UIElement self, float rightPixels, float minWidthPixels, float maxWidthPixels) => self.Width.Pixels = (rightPixels - self.Left.Pixels).Clamp(minWidthPixels, maxWidthPixels);
    public static void ExpandBottomWithRange(this UIElement self, float bottomPixels, float minHeightPixels, float maxHeightPixels) => self.Height.Pixels = (bottomPixels - self.Top.Pixels).Clamp(minHeightPixels, maxHeightPixels);
    public static void ExpandRightWithLeftWithRange(this UIElement self, float leftPixels, float minWidthPixels, float maxWidthPixels) => self.Width.Pixels = (self.Width.Pixels + leftPixels - self.Left.Pixels).Clamp(minWidthPixels, maxWidthPixels);
    public static void ExpandBottomWidthTopWithRange(this UIElement self, float topPixels, float minHeightPixels, float maxHeightPixels) => self.Height.Pixels = (self.Height.Pixels + topPixels - self.Top.Pixels).Clamp(minHeightPixels, maxHeightPixels);
    #endregion
    #endregion
    #region SetDimensions
    public static void SetDimensions(this UIElement self, CalculatedStyle dimensions) => self._dimensions = dimensions;
    public static void SetInnerDimensions(this UIElement self, CalculatedStyle dimensions) => self._innerDimensions = dimensions;
    public static void SetOuterDimensions(this UIElement self, CalculatedStyle dimensions) => self._outerDimensions = dimensions;
    public static ref CalculatedStyle GetDimensionsRef(this UIElement self) => ref self._dimensions;
    public static ref CalculatedStyle GetInnerDimensionsRef(this UIElement self) => ref self._innerDimensions;
    public static ref CalculatedStyle GetOuterDimensionsRef(this UIElement self) => ref self._outerDimensions;
    #endregion
    #region SetParent
    public static void SetParent(this UIElement self, UIElement? parent) => self.Parent = parent;
    #endregion
    #region 获取一些属性
    public static bool IsDragging(this UIScrollbar self) => self._isDragging;
    public static List<UIElement> GetElements(this UIElement self) => self.Elements;
    public static StyleDimension GetRight(this UIElement self) => new(self.Left.Pixels + self.Width.Pixels, self.Left.Percent + self.Width.Percent);
    public static StyleDimension GetBottom(this UIElement self) => new(self.Top.Pixels + self.Height.Pixels, self.Top.Percent + self.Height.Percent);
    public static float GetRightPixels(this UIElement self) => self.Left.Pixels + self.Width.Pixels;
    public static float GetRightPercent(this UIElement self) => self.Left.Percent + self.Width.Percent;
    public static float GetBottomPixels(this UIElement self) => self.Top.Pixels + self.Height.Pixels;
    public static float GetBottomPercent(this UIElement self) => self.Top.Percent + self.Height.Percent;
    // 由于把 TigerUtils 中的 GetRight 覆盖了所以这里重新写一份
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TRight GetRight<TLeft, TRight>(TLeft left, TRight right) => TigerUtils.GetRight(left, right);
    #endregion
    #region DragOutline
    /// <summary>
    /// <br/>创造一个用以改变边框大小的外框
    /// <br/>会将 <paramref name="self"/> 设置为 <paramref name="outline"/> 的子元素
    /// <br/>并根据 <paramref name="size"/> 自动设置 <paramref name="self"/> 的大小
    /// <br/>当 <paramref name="outline"/> 的 Width.Percent 或 Height.Percent 非 0 时大小限制会出现问题
    /// </summary>
    /// <param name="size">
    /// <br/>边框大小
    /// <br/>只用以设置 <paramref name="self"/> 的大小与位置
    /// <br/>也可以通过在此方法后设置 <paramref name="outline"/> 的内边距
    /// <br/>或 <paramref name="self"/> 的位置来达成一样的效果
    /// </param>
    /// <param name="minWidthPixels"><paramref name="outline"/> 的最小宽度</param>
    /// <param name="minHeightPixels"><paramref name="outline"/> 的最小高度</param>
    /// <param name="maxWidthPixels"><paramref name="outline"/> 的最大宽度</param>
    /// <param name="maxHeightPixels"><paramref name="outline"/> 的最大高度</param>
    public static void CreateDragOutlineOn(this UIElement self, UIElement outline, float size,
        float minWidthPixels = 0, float minHeightPixels = 0, float maxWidthPixels = float.MaxValue, float maxHeightPixels = float.MaxValue,
        Action? onSizeChanged = null) {
        #region Consts
        const int StateNone = 4;
        const int StateLeft = 3;
        const int StateRight = 5;
        const int StateTop = 1;
        const int StateBottom = 7;
        const int StateLeftTop = 0;
        const int StateLeftBottom = 6;
        const int StateRightTop = 2;
        const int StateRightBottom = 8;
        #endregion
        int state = StateNone;
        Vector2 mouseDelta = default;
        outline.Append(self);
        self.SetPositionAndSize(size, 0, size, 0, -size * 2, 1, -size * 2, 1);
        outline.MinWidth.Pixels = minWidthPixels;
        outline.MinHeight.Pixels = minHeightPixels;
        outline.MaxWidth.Pixels = maxWidthPixels;
        outline.MaxHeight.Pixels = maxHeightPixels;
        outline.OnLeftMouseDown += (evt, element) => {
            var mouse = Main.MouseScreen;
            var dim = self.GetDimensions();
            int x = mouse.X <= dim.X ? 0 : mouse.X >= dim.X + dim.Width ? 2 : 1;
            int y = mouse.Y <= dim.Y ? 0 : mouse.Y >= dim.Y + dim.Height ? 2 : 1;
            state = x + y * 3;
            mouseDelta = mouse - new Vector2(outline.Left.Pixels + outline.Width.Pixels * (x / 2), outline.Top.Pixels + outline.Height.Pixels * (y / 2));
        };
        outline.OnLeftMouseUp += (evt, element) => state = StateNone;
        outline.OnUpdate += element => {
            if (state == StateNone) {
                return;
            }
            Vector2 newPosition = Main.MouseScreen - mouseDelta;
            switch (state) {
            case StateLeft:
                outline.ExpandLeftWithRange(newPosition.X, minWidthPixels, maxWidthPixels);
                break;
            case StateRight:
                outline.ExpandRightWithRange(newPosition.X, minWidthPixels, maxWidthPixels);
                break;
            case StateTop:
                outline.ExpandTopWithRange(newPosition.Y, minHeightPixels, maxHeightPixels);
                break;
            case StateBottom:
                outline.ExpandBottomWithRange(newPosition.Y, minHeightPixels, maxHeightPixels);
                break;
            case StateLeftTop:
                outline.ExpandLeftWithRange(newPosition.X, minWidthPixels, maxWidthPixels);
                outline.ExpandTopWithRange(newPosition.Y, minHeightPixels, maxHeightPixels);
                break;
            case StateLeftBottom:
                outline.ExpandLeftWithRange(newPosition.X, minWidthPixels, maxWidthPixels);
                outline.ExpandBottomWithRange(newPosition.Y, minHeightPixels, maxHeightPixels);
                break;
            case StateRightTop:
                outline.ExpandRightWithRange(newPosition.X, minWidthPixels, maxWidthPixels);
                outline.ExpandTopWithRange(newPosition.Y, minHeightPixels, maxHeightPixels);
                break;
            case StateRightBottom:
                outline.ExpandRightWithRange(newPosition.X, minWidthPixels, maxWidthPixels);
                outline.ExpandBottomWithRange(newPosition.Y, minHeightPixels, maxHeightPixels);
                break;
            }
            onSizeChanged?.Invoke();
        };
    }
    #endregion
    #region 操作 UIElement 的 Elements
    public static void RemoveAt(this UIElement self, int index) {
        self.Elements.RemoveAt(index);
        self.Elements[index].Parent = null;
    }
    public static void RemoveRange(this UIElement self, int index, int count) {
        self.Elements.RemoveRange(index, count);
        for (int i = index; i < index + count; ++i) {
            self.Elements[i].Parent = null;
        }
    }
    public static void Insert(this UIElement self, int index, UIElement child) {
        child.Remove();
        child.Parent = self;
        self.Elements.Insert(index, child);
        child.Recalculate();
    }
    public static void InsertRange(this UIElement self, int index, IEnumerable<UIElement> children) {
        foreach (var child in children) {
            child.Remove();
            child.Parent = self;
        }
        self.Elements.InsertRange(index, children);
        foreach (var child in children) {
            child.Recalculate();
        }
    }
    #endregion
    #region UI杂项
    public static bool ContainsPoint(this CalculatedStyle dimensions, Vector2 point) {
        return point.X >= dimensions.X && point.X <= dimensions.X + dimensions.Width && point.Y >= dimensions.Y && point.Y <= dimensions.Y + dimensions.Height;
    }
    public static void RecalculateSelf(this UIElement self) {
        CalculatedStyle parentDimensions = ((self.Parent == null) ? UserInterface.ActiveInstance.GetDimensions() : self.Parent.GetInnerDimensions());
        if (self.Parent != null && self.Parent is UIList)
            parentDimensions.Height = float.MaxValue;

        CalculatedStyle calculatedStyle = (self._outerDimensions = self.GetDimensionsBasedOnParentDimensions(parentDimensions));
        calculatedStyle.X += self.MarginLeft;
        calculatedStyle.Y += self.MarginTop;
        calculatedStyle.Width -= self.MarginLeft + self.MarginRight;
        calculatedStyle.Height -= self.MarginTop + self.MarginBottom;
        self._dimensions = calculatedStyle;
        calculatedStyle.X += self.PaddingLeft;
        calculatedStyle.Y += self.PaddingTop;
        calculatedStyle.Width -= self.PaddingLeft + self.PaddingRight;
        calculatedStyle.Height -= self.PaddingTop + self.PaddingBottom;
        self._innerDimensions = calculatedStyle;
    }
    #endregion

    #endregion
    #endregion
    #region 杂项
    public static Asset<T> ToAsset<T>(this Task<T> self, string assetName, bool immediate = false) where T : class {
        Asset<T> asset = new(assetName);
        if (self.IsCompleted) {
            Debug.Assert(self.IsCompletedSuccessfully, "Task not completed successfully");
            asset.SubmitLoadedContent(self.Result, null);
            return asset;
        }
        if (immediate) {
            self.Wait();
            Debug.Assert(self.IsCompletedSuccessfully, "Task not completed successfully");
            asset.SubmitLoadedContent(self.Result, null);
            return asset;
        }
        asset.SetToLoadingState();
        var task = self.ContinueWith(s => {
            Debug.Assert(s.IsCompletedSuccessfully, "Task not completed successfully");
            asset.SubmitLoadedContent(s.Result, null);
        });
        asset.Wait = task.Wait;
        return asset;
    }
    /// <summary>
    /// 让 TML 的自动卸载不会自动卸载它, 返回是否成功
    /// </summary>
    public static bool MakeNotAutoUnload(this Hook hook) {
        var asm = hook.Target.DeclaringType?.Assembly;
        if (asm == null) {
            return false;
        }
        MonoModHooks.GetDetourList(asm).detours.Remove(hook.DetourInfo);
        return true;
    }
    /// <summary>
    /// 让 TML 的自动卸载不会自动卸载它, 返回是否成功
    /// </summary>
    public static bool MakeNotAutoUnload(this ILHook ilHook) {
        var asm = ilHook.Manipulator.Method.DeclaringType?.Assembly;
        if (asm == null) {
            return false;
        }
        MonoModHooks.GetDetourList(asm).ilHooks.Remove(ilHook.HookInfo);
        return true;
    }
    #endregion
}
