using AgeRegression.Data;
using AgeRegression.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Dialogue;

/// <summary>
/// Maps an NPC name to its <see cref="NpcReactionProfileData"/> and
/// the appropriate dialogue pack.
///
/// <para>
/// Falls back to the <c>generic_fallback</c> pack when no NPC-specific
/// pack exists. Expansion mod authors add entries to
/// <c>assets/data/npc-reactions.json</c> via Content Patcher
/// <c>EditData</c> to register their NPCs.
/// </para>
/// </summary>
public sealed class NpcReactionRouter
{
    private const string FallbackPackId = "generic_fallback";

    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;
    private readonly Dictionary<string, NpcReactionProfileData?>
        _profileCache = new();

    public NpcReactionRouter(DataLoader dataLoader, LogHelper log)
    {
        _dataLoader = dataLoader;
        _log        = log;
    }

    public NpcReactionProfileData? GetProfile(string npcName)
    {
        if (_profileCache.TryGetValue(npcName, out var cached))
            return cached;

        var profile = _dataLoader.NpcProfiles
            .FirstOrDefault(p => string.Equals(
                p.NpcName, npcName, StringComparison.OrdinalIgnoreCase));

        _profileCache[npcName] = profile;
        return profile;
    }

    public DialoguePackData? GetDialoguePack(string npcName)
    {
        var profile = GetProfile(npcName);
        var packKey = profile?.DialoguePackKey ?? npcName;

        var pack = _dataLoader.GetDialoguePack(packKey);
        if (pack is not null) return pack;

        if (!string.Equals(packKey, npcName,
                StringComparison.OrdinalIgnoreCase))
            pack = _dataLoader.GetDialoguePack(npcName);

        if (pack is not null) return pack;

        _log.Trace("No dialogue pack for '{0}'. Using generic fallback.",
            npcName);
        return _dataLoader.GetDialoguePack(FallbackPackId);
    }

    public void InvalidateCache()
    {
        _profileCache.Clear();
    }
}
