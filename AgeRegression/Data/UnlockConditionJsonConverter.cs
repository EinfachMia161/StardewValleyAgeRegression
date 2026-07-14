using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AgeRegression.Data;

/// <summary>
/// Polymorphic JSON converter for <see cref="IUnlockCondition"/>.
///
/// <para>
/// Each condition object carries a <c>Type</c> discriminator that selects
/// the concrete condition class:
/// <list type="bullet">
///   <item><description><c>Year</c> → <see cref="YearCondition"/> using <c>Value</c> (int)</description></item>
///   <item><description><c>Season</c> → <see cref="SeasonCondition"/> using <c>Value</c> (string)</description></item>
///   <item><description><c>MailFlag</c> → <see cref="MailFlagCondition"/> using <c>Flag</c> (string)</description></item>
///   <item><description><c>Friendship</c> → <see cref="FriendshipCondition"/> using <c>Npc</c> + <c>Points</c></description></item>
/// </list>
/// The discriminator is matched case-insensitively.
/// </para>
///
/// <para>
/// This converter is registered in <see cref="AssetJson.Settings"/> so the
/// asset pipeline can deserialize the new <c>Conditions</c> array format.
/// </para>
/// </summary>
public sealed class UnlockConditionJsonConverter : JsonConverter<IUnlockCondition>
{
    public override IUnlockCondition? ReadJson(
        JsonReader reader,
        Type objectType,
        IUnlockCondition? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var obj = JObject.Load(reader);

        var typeToken = obj["Type"] ?? obj["type"];
        if (typeToken is null)
            throw new JsonSerializationException(
                "Unlock condition is missing a \"Type\" discriminator.");

        return typeToken.ToString().Trim().ToLowerInvariant() switch
        {
            "year" => new YearCondition(obj.Value<int>("Value")),
            "season" => new SeasonCondition(
                obj.Value<string>("Value")
                ?? obj.Value<string>("Season")
                ?? string.Empty),
            "mailflag" or "mail" => new MailFlagCondition(
                obj.Value<string>("Flag")
                ?? obj.Value<string>("MailFlag")
                ?? string.Empty),
            "friendship" => new FriendshipCondition(
                obj.Value<string>("Npc")
                ?? obj.Value<string>("FriendshipNpc")
                ?? string.Empty,
                obj.Value<int>("Points")),
            _ => throw new JsonSerializationException(
                $"Unknown unlock condition type '{typeToken}'.")
        };
    }

    public override void WriteJson(
        JsonWriter writer, IUnlockCondition? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var obj = value switch
        {
            YearCondition c => new JObject
            {
                ["Type"]  = "Year",
                ["Value"] = c.RequiredYear
            },
            SeasonCondition c => new JObject
            {
                ["Type"]  = "Season",
                ["Value"] = c.RequiredSeason
            },
            MailFlagCondition c => new JObject
            {
                ["Type"] = "MailFlag",
                ["Flag"] = c.MailFlag
            },
            FriendshipCondition c => new JObject
            {
                ["Type"]  = "Friendship",
                ["Npc"]   = c.NpcName,
                ["Points"] = c.RequiredPoints
            },
            _ => throw new JsonSerializationException(
                $"Cannot serialize unlock condition type '{value.GetType().Name}'.")
        };

        obj.WriteTo(writer);
    }
}
