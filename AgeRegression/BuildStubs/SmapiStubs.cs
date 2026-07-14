// Minimal compile-time stubs for SMAPI/Stardew types so the project can build
// without referencing the real game assemblies. These are only for local test builds.
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI
{
    public interface IModHelper { }
    public class VerboseLogStringHandler { }
    public interface IMonitor 
    { 
        void Log(string message); 
        void Log(string message, LogLevel level);
        void LogOnce(string message);
        void LogOnce(string message, LogLevel level);
        void VerboseLog(string message);
        bool IsVerbose { get; set; }
    }
    public interface IManifest { string Name { get; } }
    public class ModDataDictionary : Dictionary<string, string> { }

    public class AssetRequestedEventArgs : EventArgs
    {
        public string? AssetName { get; set; }
        public IAssetName NameWithoutLocale { get; set; } = null!;
    }

    public interface IAssetName
    {
        string Name { get; }
        bool IsEquivalentTo(string name);
    }
}

namespace StardewValley.GameData.Objects
{
    public class ObjectData
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Category { get; set; }
        public int Price { get; set; }
        public string Texture { get; set; } = string.Empty;
        public int SpriteIndex { get; set; }
        public int Edibility { get; set; }
        public bool CanBeGivenAsGift { get; set; } = true;
        public bool CanBeTrashed { get; set; } = true;
        public bool ExcludeFromShippingCollection { get; set; } = false;
        public bool ExcludeFromRandomSale { get; set; } = false;
        public List<string> ContextTags { get; set; } = new();
    }
}

namespace StardewValley
{
    public class Object
    {
        public Object(string id, int count) { }
        public Dictionary<string, string> modData { get; set; } = new();
        public string QualifiedItemId { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public virtual void draw(
            SpriteBatch spriteBatch,
            int x,
            int y,
            float alpha) { }
        public virtual void drawInMenu(
            SpriteBatch spriteBatch,
            Vector2 location,
            float scale) { }
        public virtual void drawWhenHeld(
            SpriteBatch spriteBatch) { }
    }

    public class NPC
    {
        public static void loadCurrentDialogue() { }
    }

    public class GameData { }
    
    public static class Game1
    {
        public static object graphics { get; set; } = null!;
        public static object content { get; set; } = null!;
        public static int tileSize { get; set; } = 16;
        public static object staminaRect { get; set; } = null!;
        public static object smallFont { get; set; } = null!;
    }
}
