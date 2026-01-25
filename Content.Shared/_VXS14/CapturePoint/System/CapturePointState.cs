using System;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.GG.CapturePoint;

namespace Content.Server.GG.CapturePoint
{
    /// <summary>
    /// State for networked syncing of the CapturePoint
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GGCapturePointComponentState : ComponentState
    {
        public float CurrentPointProgression { get; }

        public string Leader { get; }

        public string Team { get; }

        public GGCapturePointComponentState(float cur, string lead, string team)
        {
            CurrentPointProgression = cur;
            Leader = lead;
            Team = team;
        }
    }
}
