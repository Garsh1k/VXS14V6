using System.Diagnostics.CodeAnalysis;
using System.Text;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

/// <summary>
/// Requires the character to be or not be one of the specified sexes.
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class SexRequirement : JobRequirement
{
    [DataField(required: true)]
    public HashSet<Sex> Sexes = new();

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null)
            return true;

        var sb = new StringBuilder();
        sb.Append("[color=yellow]");

        foreach (var sex in Sexes)
        {
            sb.Append(sex);
            sb.Append(' ');
        }

        sb.Append("[/color]");

        if (!Inverted)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-whitelisted-sexes")}\n{sb}");

            if (!Sexes.Contains(profile.Sex))
                return false;
        }
        else
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-blacklisted-sexes")}\n{sb}");

            if (Sexes.Contains(profile.Sex))
                return false;
        }

        return true;
    }
}
