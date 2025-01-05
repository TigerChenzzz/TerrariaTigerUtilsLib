#nullable enable

using System;
using System.Reflection;

namespace TigerUtilsLib.Reflections {
    public static partial class SystemReflections {
        public static partial class System {
            public static class SpanHelpers {
                public static Type Type { get; } = Type.GetType("System.SpanHelpers", true) ?? throw new NullReferenceException();
                #region SequenceEqual
                //!! TODO: 这里的 ulong 实际上是 [NativeInteger] UIntPtr, 对于 64 位系统来说就是 ulong
                private delegate bool Delegate_SequenceEqualWithByte(ref byte first, ref byte second, ulong length);
                public static MethodInfo Method_SequenceEqual_Byte { get; } = Type.GetMethod<Delegate_SequenceEqualWithByte>("SequenceEqual") ?? throw new NullReferenceException();
                private static readonly Delegate_SequenceEqualWithByte sequenceEqualWithByte = Method_SequenceEqual_Byte.CreateDelegate<Delegate_SequenceEqualWithByte>();
                public static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length) => sequenceEqualWithByte(ref first, ref second, length);
                #endregion
            }
        }
    }
}
