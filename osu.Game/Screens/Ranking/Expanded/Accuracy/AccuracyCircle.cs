// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// The component that displays the player's accuracy on the results screen.
    /// </summary>
    public class AccuracyCircle : CompositeDrawable
    {
        /// <summary>
        /// Duration for the transforms causing this component to appear.
        /// </summary>
        public const double APPEAR_DURATION = 200;

        /// <summary>
        /// Delay before the accuracy circle starts filling.
        /// </summary>
        public const double ACCURACY_TRANSFORM_DELAY = 450;

        /// <summary>
        /// Duration for the accuracy circle fill.
        /// </summary>
        public const double ACCURACY_TRANSFORM_DURATION = 3000;

        /// <summary>
        /// Delay after <see cref="ACCURACY_TRANSFORM_DURATION"/> for the rank text (A/B/C/D/S/SS) to appear.
        /// </summary>
        public const double TEXT_APPEAR_DELAY = ACCURACY_TRANSFORM_DURATION / 2;

        /// <summary>
        /// Delay before the rank circles start filling.
        /// </summary>
        public const double RANK_CIRCLE_TRANSFORM_DELAY = 150;

        /// <summary>
        /// Duration for the rank circle fills.
        /// </summary>
        public const double RANK_CIRCLE_TRANSFORM_DURATION = 800;

        /// <summary>
        /// Relative width of the rank circles.
        /// </summary>
        public const float RANK_CIRCLE_RADIUS = 0.06f;

        /// <summary>
        /// Relative width of the circle showing the accuracy.
        /// </summary>
        private const float accuracy_circle_radius = 0.2f;

        /// <summary>
        /// SS is displayed as a 1% region, otherwise it would be invisible.
        /// </summary>
        private const double virtual_ss_percentage = 0.01;

        /// <summary>
        /// The easing for the circle filling transforms.
        /// </summary>
        public static readonly Easing ACCURACY_TRANSFORM_EASING = Easing.OutPow10;

        private readonly ScoreInfo score;

        private readonly bool withFlair;

        private SmoothCircularProgress accuracyCircle;
        private SmoothCircularProgress innerMask;
        private Container<RankBadge> badges;
        private RankText rankText;

        private DrawableSample scoreTickSound;
        private DrawableSample badgeTickSound;
        private DrawableSample badgeMaxSound;
        private DrawableSample rankImpactSound;
        private DrawableSample rankImpactFailSound;
        private DrawableSample swooshUpSound;

        private Bindable<double> tickPlaybackRate;
        private double lastTickPlaybackTime;
        private bool isTicking;

        // TODO: temp TestScene hooks
        public Bindable<double> SwooshPreDelay = new Bindable<double>(2200);
        public Bindable<double> DebounceRateStart = new Bindable<double>(10);
        public Bindable<double> DebounceRateStop = new Bindable<double>(300);
        public Bindable<string> ScoreTickSampleName = new Bindable<string>("badge-dink-2");
        public Bindable<double> TickPitchFactor = new Bindable<double>(0.1);
        public Bindable<bool> PlayTicks = new Bindable<bool>(true);
        public Bindable<bool> PlayImpact = new Bindable<bool>(true);
        public Bindable<bool> PlayBadgeDinks = new Bindable<bool>(true);
        public Bindable<bool> PlaySwoosh = new Bindable<bool>(true);
        public Bindable<bool> ScoreTickIsLoop = new Bindable<bool>(false);

        public Bindable<double> TickVolumeStart = new Bindable<double>(0.6);
        public Bindable<double> TickVolumeEnd = new Bindable<double>(1.0);
        public Bindable<double> ImpactVolume = new Bindable<double>(1.0);
        public Bindable<double> BadgeDinkVolume = new Bindable<double>(0.5);
        public Bindable<double> SwooshVolume = new Bindable<double>(0.5);

        public Bindable<Easing> ScoreTickRateEasing = new Bindable<Easing>(Easing.OutSine);
        public Bindable<Easing> ScoreTickPitchEasing = new Bindable<Easing>(Easing.OutSine);
        public Bindable<Easing> ScoreTickVolumeEasing = new Bindable<Easing>(Easing.OutSine);

        public AccuracyCircle(ScoreInfo score, bool withFlair)
        {
            this.score = score;
            this.withFlair = withFlair;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            InternalChildren = new Drawable[]
            {
                new SmoothCircularProgress
                {
                    Name = "Background circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(47),
                    Alpha = 0.5f,
                    InnerRadius = accuracy_circle_radius + 0.01f, // Extends a little bit into the circle
                    Current = { Value = 1 },
                },
                accuracyCircle = new SmoothCircularProgress
                {
                    Name = "Accuracy circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#BAFFA9")),
                    InnerRadius = accuracy_circle_radius,
                },
                new BufferedContainer
                {
                    Name = "Graded circles",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.8f),
                    Padding = new MarginPadding(2),
                    Children = new Drawable[]
                    {
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.X),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 1 }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.S),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 1 - virtual_ss_percentage }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.A),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.95f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.B),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.9f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.C),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.8f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.D),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.7f }
                        },
                        new RankNotch(0),
                        new RankNotch((float)(1 - virtual_ss_percentage)),
                        new RankNotch(0.95f),
                        new RankNotch(0.9f),
                        new RankNotch(0.8f),
                        new RankNotch(0.7f),
                        new BufferedContainer
                        {
                            Name = "Graded circle mask",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(1),
                            Blending = new BlendingParameters
                            {
                                Source = BlendingType.DstColor,
                                Destination = BlendingType.OneMinusSrcAlpha,
                                SourceAlpha = BlendingType.One,
                                DestinationAlpha = BlendingType.SrcAlpha
                            },
                            Child = innerMask = new SmoothCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                InnerRadius = RANK_CIRCLE_RADIUS - 0.01f,
                            }
                        }
                    }
                },
                badges = new Container<RankBadge>
                {
                    Name = "Rank badges",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Vertical = -15, Horizontal = -20 },
                    Children = new[]
                    {
                        new RankBadge(1f, getRank(ScoreRank.X)),
                        new RankBadge(0.95f, getRank(ScoreRank.S)),
                        new RankBadge(0.9f, getRank(ScoreRank.A)),
                        new RankBadge(0.8f, getRank(ScoreRank.B)),
                        new RankBadge(0.7f, getRank(ScoreRank.C)),
                        new RankBadge(0.35f, getRank(ScoreRank.D)),
                    }
                },
                rankText = new RankText(score.Rank)
            };

            if (withFlair)
            {
                tickPlaybackRate = new Bindable<double>(DebounceRateStart.Value);

                AddInternal(badgeTickSound = new DrawableSample(audio.Samples.Get("Results/badge-dink-3"))
                {
                    Volume = { BindTarget = BadgeDinkVolume }
                });
                AddInternal(badgeMaxSound = new DrawableSample(audio.Samples.Get("Results/badge-dink-8"))
                {
                    Volume = { BindTarget = BadgeDinkVolume }
                });
                AddInternal(rankImpactSound = new DrawableSample(audio.Samples.Get("Results/rank-impact"))
                {
                    Volume = { BindTarget = ImpactVolume }
                });
                AddInternal(swooshUpSound = new DrawableSample(audio.Samples.Get("Results/swoosh-up-2"))
                {
                    Volume = { BindTarget = SwooshVolume }
                });
                AddInternal(rankImpactFailSound = new DrawableSample(audio.Samples.Get("Results/rank-impact-fail-3"))
                {
                    Volume = { BindTarget = ImpactVolume }
                });
            }

            ScoreTickSampleName.BindValueChanged((sample) =>
            {
                if (!withFlair) return;

                scoreTickSound?.Expire();
                var sampleToLoad = sample.NewValue;

                AddInternal(scoreTickSound = new DrawableSample(audio.Samples.Get($"Results/{sampleToLoad}"))
                {
                    Looping = ScoreTickIsLoop.Value,
                    Frequency = { Value = 1.0 }
                });
            }, true);
        }

        private ScoreRank getRank(ScoreRank rank)
        {
            foreach (var mod in score.Mods.OfType<IApplicableToScoreProcessor>())
                rank = mod.AdjustRank(rank, score.Accuracy);

            return rank;
        }

        protected override void Update()
        {
            base.Update();

            if (!PlayTicks.Value || !isTicking || ScoreTickIsLoop.Value) return;

            bool enoughTimePassedSinceLastPlayback = Clock.CurrentTime - lastTickPlaybackTime >= tickPlaybackRate.Value;

            if (!enoughTimePassedSinceLastPlayback) return;

            scoreTickSound?.Play();
            lastTickPlaybackTime = Clock.CurrentTime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0).Then().ScaleTo(1, APPEAR_DURATION, Easing.OutQuint);

            if (PlaySwoosh.Value)
                this.Delay(SwooshPreDelay.Value).Schedule(() => swooshUpSound?.Play());

            using (BeginDelayedSequence(RANK_CIRCLE_TRANSFORM_DELAY, true))
                innerMask.FillTo(1f, RANK_CIRCLE_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

            using (BeginDelayedSequence(ACCURACY_TRANSFORM_DELAY, true))
            {
                double targetAccuracy = score.Rank == ScoreRank.X || score.Rank == ScoreRank.XH ? 1 : Math.Min(1 - virtual_ss_percentage, score.Accuracy);

                accuracyCircle.FillTo(targetAccuracy, ACCURACY_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

                // sound fun
                if (PlayTicks.Value)
                {
                    scoreTickSound?.FrequencyTo(1 + (targetAccuracy * TickPitchFactor.Value), ACCURACY_TRANSFORM_DURATION, ScoreTickPitchEasing.Value);
                    scoreTickSound?.VolumeTo(TickVolumeStart.Value).Then().VolumeTo(TickVolumeEnd.Value, ACCURACY_TRANSFORM_DURATION, ScoreTickVolumeEasing.Value);

                    if (!ScoreTickIsLoop.Value)
                        this.TransformBindableTo(tickPlaybackRate, DebounceRateStop.Value, ACCURACY_TRANSFORM_DURATION, ScoreTickRateEasing.Value);
                }

                Schedule(() =>
                {
                    if (!PlayTicks.Value) return;

                    if (ScoreTickIsLoop.Value)
                        scoreTickSound?.Play();
                    else
                        isTicking = true;
                });

                int badgeNum = 0;

                foreach (var badge in badges)
                {
                    if (badge.Accuracy > score.Accuracy)
                        continue;

                    using (BeginDelayedSequence(inverseEasing(ACCURACY_TRANSFORM_EASING, Math.Min(1 - virtual_ss_percentage, badge.Accuracy) / targetAccuracy) * ACCURACY_TRANSFORM_DURATION, true))
                    {
                        badge.Appear();
                        Schedule(() =>
                        {
                            if (badgeTickSound == null || badgeMaxSound == null || !PlayBadgeDinks.Value) return;

                            if (badgeNum < (badges.Count - 1))
                            {
                                badgeTickSound.Frequency.Value = 1 + (badgeNum++ * 0.05);
                                // badgeTickSound.Volume.Value = 0.5;
                                badgeTickSound?.Play();
                            }
                            else
                            {
                                badgeMaxSound.Frequency.Value = 1 + (badgeNum++ * 0.05);
                                // badgeMaxSound.Volume.Value = 0.6;
                                badgeMaxSound?.Play();

                                if (ScoreTickIsLoop.Value)
                                    scoreTickSound?.Stop();
                                else
                                    isTicking = false;
                            }
                        });
                    }
                }

                using (BeginDelayedSequence(TEXT_APPEAR_DELAY, true))
                {
                    rankText.Appear();
                    Schedule(() =>
                    {
                        if (ScoreTickIsLoop.Value)
                            scoreTickSound?.Stop();
                        else
                            isTicking = false;

                        if (!PlayImpact.Value) return;

                        if (score.Rank >= ScoreRank.A)
                            rankImpactSound?.Play();
                        else
                            rankImpactFailSound?.Play();
                    });
                }
            }
        }

        private double inverseEasing(Easing easing, double targetValue)
        {
            double test = 0;
            double result = 0;
            int count = 2;

            while (Math.Abs(result - targetValue) > 0.005)
            {
                int dir = Math.Sign(targetValue - result);

                test += dir * 1.0 / count;
                result = Interpolation.ApplyEasing(easing, test);

                count++;
            }

            return test;
        }
    }
}
