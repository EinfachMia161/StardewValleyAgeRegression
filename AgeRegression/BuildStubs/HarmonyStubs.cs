using System;

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HarmonyPatchAttribute : Attribute
    {
        public HarmonyPatchAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HarmonyPrefixAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HarmonyPostfixAttribute : Attribute { }

    public enum MethodType { Normal }

    public sealed class HarmonyMethodAttribute : Attribute { }

    public class Harmony
    {
        public Harmony(string id) { }
        public void PatchAll() { }
    }
}
