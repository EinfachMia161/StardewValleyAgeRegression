// Minimal compile-time stubs for SMAPI/Stardew types so the project can build
// without referencing the real game assemblies. These are only for local test builds.
using System;
using System.Collections.Generic;

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
    }
}

namespace StardewValley
{
    public class DataLoader { }

    public class ObjectData { }

    public class ObjectDataDictionary : Dictionary<int, ObjectData> { }

    public class NPC
    {
        // Placeholder for dialogue-related compatibility
        public static void loadCurrentDialogue() { }
    }

    public class GameData { }
}
