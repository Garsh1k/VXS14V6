using Content.Server.GG.CapturePoint;
using Content.Shared.GG.CapturePoint;
using Robust.Client.GameObjects;
using Robust.Client.GameStates;
using Robust.Shared.GameStates;

namespace Content.Client.GG.CapturePoint
{
    public sealed class GGCapturePointSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GGCapturePointComponent, ComponentHandleState>(OnCapturePointStateUpdated);
        }

        private void OnCapturePointStateUpdated(EntityUid uid, GGCapturePointComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not GGCapturePointComponentState state)
            {
                return;
            }

            component.CurrentPointProgression = state.CurrentPointProgression;
            component.Leader = state.Leader;
            component.Team = state.Team;


            if (!EntityManager.TryGetComponent(uid, out SpriteComponent? sprite))
            {
                return;
            }

            // sprite.Color = component.PointColor;

        }


        public override void FrameUpdate(float frameTime)
        {
            foreach (var (capturePoint, sprite) in EntityQuery<GGCapturePointComponent, SpriteComponent>())
            {
                //Logger.Debug($"capturePoint.CurrentPointProgression: {capturePoint.CurrentPointProgression}");
                UpdatePointColor(capturePoint, sprite);

            }
        }

        private void UpdatePointColor(GGCapturePointComponent capturePoint, SpriteComponent sprite)
        {
            float progress = capturePoint.CurrentPointProgression / 100f;

            if (capturePoint.Leader == "Syndy")
            {

                sprite.Color = LerpColor(Color.White, Color.Red, progress);
            }
            else if (capturePoint.Leader == "Solfed")
            {
                sprite.Color = LerpColor(Color.White, Color.Blue, progress);
            }
            else
            {
                if (capturePoint.Team == "Syndy")
                {
                    sprite.Color = LerpColor(Color.White, Color.Red, progress);
                }
                else if (capturePoint.Team == "Solfed")
                {
                    sprite.Color = LerpColor(Color.White, Color.Blue, progress);

                }
                else
                    sprite.Color = Color.White;
            }
        }

        private Color LerpColor(Color from, Color to, float progress)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);

            return new Color(
                MathHelper.Lerp(from.R, to.R, progress),
                MathHelper.Lerp(from.G, to.G, progress),
                MathHelper.Lerp(from.B, to.B, progress),
                MathHelper.Lerp(from.A, to.A, progress)
            );
        }


    }
}
