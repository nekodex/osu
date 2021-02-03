// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.SoundDesign
{
    public class TestSceneAccuracyCircle : OsuTestScene
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        private Bindable<bool> playTicks = new Bindable<bool>(true);
        private Bindable<bool> playImpact = new Bindable<bool>(true);
        private Bindable<bool> playBadgeSounds = new Bindable<bool>(true);
        private Bindable<bool> playSwooshSound = new Bindable<bool>(true);
        private Bindable<bool> tickIsLoop = new Bindable<bool>(false);
        private Bindable<string> tickSampleName = new Bindable<string>("badge-dink-2");

        private BindableDouble tickPitchFactor = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 3,
            Precision = 0.1,
        };

        private BindableDouble tickDebounceStart = new BindableDouble(10)
        {
            MinValue = 1,
            MaxValue = 100,
        };

        private BindableDouble tickDebounceEnd = new BindableDouble(400)
        {
            MinValue = 100,
            MaxValue = 1000,
        };

        private BindableDouble swooshPreDelay = new BindableDouble(450)
        {
            MinValue = -1000,
            MaxValue = 1000,
        };

        private Bindable<Easing> tickRateEasing = new Bindable<Easing>(Easing.None);
        private Bindable<Easing> tickPitchEasing = new Bindable<Easing>(Easing.None);
        private Bindable<Easing> tickVolumeEasing = new Bindable<Easing>(Easing.OutSine);

        private BindableDouble tickVolumeStart = new BindableDouble(0.6)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        private BindableDouble tickVolumeEnd = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        private BindableDouble impactVolume = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        private BindableDouble badgeDinkVolume = new BindableDouble(0.5)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        private BindableDouble swooshVolume = new BindableDouble(0.5)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        private DrawableSample previewSampleChannel;

        [Test]
        public void TestLowDRank()
        {
            var score = createScore();
            score.Accuracy = 0.2;
            score.Rank = ScoreRank.D;

            addCircleStep(score);
        }

        [Test]
        public void TestDRank()
        {
            var score = createScore();
            score.Accuracy = 0.5;
            score.Rank = ScoreRank.D;

            addCircleStep(score);
        }

        [Test]
        public void TestCRank()
        {
            var score = createScore();
            score.Accuracy = 0.75;
            score.Rank = ScoreRank.C;

            addCircleStep(score);
        }

        [Test]
        public void TestBRank()
        {
            var score = createScore();
            score.Accuracy = 0.85;
            score.Rank = ScoreRank.B;

            addCircleStep(score);
        }

        [Test]
        public void TestARank()
        {
            var score = createScore();
            score.Accuracy = 0.925;
            score.Rank = ScoreRank.A;

            addCircleStep(score);
        }

        [Test]
        public void TestSRank()
        {
            var score = createScore();
            score.Accuracy = 0.975;
            score.Rank = ScoreRank.S;

            addCircleStep(score);
        }

        [Test]
        public void TestAlmostSSRank()
        {
            var score = createScore();
            score.Accuracy = 0.9999;
            score.Rank = ScoreRank.S;

            addCircleStep(score);
        }

        [Test]
        public void TestSSRank()
        {
            var score = createScore();
            score.Accuracy = 1;
            score.Rank = ScoreRank.X;

            addCircleStep(score);
        }

        private void addCircleStep(ScoreInfo score) => AddStep("add panel", () =>
        {
            FileSelector sampleFileSelector;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.25f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("222")
                                },
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarVisible = false,
                                    Child = new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 10),
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play Ticks",
                                                Current = { BindTarget = playTicks }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Tick Volume (Start)",
                                                Current = { BindTarget = tickVolumeStart }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Tick Volume (End)",
                                                Current = { BindTarget = tickVolumeEnd }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play Impact",
                                                Current = { BindTarget = playImpact }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Impact Volume",
                                                Current = { BindTarget = impactVolume }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play BadgeSounds",
                                                Current = { BindTarget = playBadgeSounds }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Badge Dink Volume",
                                                Current = { BindTarget = badgeDinkVolume }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play Swoosh",
                                                Current = { BindTarget = playSwooshSound }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Swoosh Volume",
                                                Current = { BindTarget = swooshVolume }
                                            },

                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Swoosh Pre-Delay",
                                                Current = { BindTarget = swooshPreDelay }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Loop ScoreTick",
                                                Current = { BindTarget = tickIsLoop }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "ScoreTick Start Debounce Rate",
                                                Current = { BindTarget = tickDebounceStart }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "ScoreTick End Debounce Rate",
                                                Current = { BindTarget = tickDebounceEnd }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "ScoreTick Rate Easing:"
                                            },
                                            new SettingsEnumDropdown<Easing>
                                            {
                                                Current = { BindTarget = tickRateEasing }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "ScoreTick Pitch Factor",
                                                Current = { BindTarget = tickPitchFactor }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "Pitch Easing:"
                                            },
                                            new SettingsEnumDropdown<Easing>
                                            {
                                                Current = { BindTarget = tickPitchEasing }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "Volume Easing:"
                                            },
                                            new SettingsEnumDropdown<Easing>
                                            {
                                                Current = { BindTarget = tickVolumeEasing }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "Current Tick Sample:"
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2 },
                                                Current = { BindTarget = tickSampleName }
                                            },
                                            sampleFileSelector = new FileSelector("/Users/jamie/Sandbox/derp/Samples/Results")
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 500,
                                            }
                                        }
                                    },
                                },
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.75f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"))
                                },
                                new AccuracyCircle(score, true)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(230),
                                    DebounceRateStart = { BindTarget = tickDebounceStart },
                                    DebounceRateStop = { BindTarget = tickDebounceEnd },
                                    PlayTicks = { BindTarget = playTicks },
                                    TickVolumeStart = { BindTarget = tickVolumeStart },
                                    TickVolumeEnd = { BindTarget = tickVolumeEnd },
                                    PlayImpact = { BindTarget = playImpact },
                                    ImpactVolume = { BindTarget = impactVolume },
                                    PlayBadgeDinks = { BindTarget = playBadgeSounds },
                                    BadgeDinkVolume = { BindTarget = badgeDinkVolume },
                                    PlaySwoosh = { BindTarget = playSwooshSound },
                                    SwooshVolume = { BindTarget = swooshVolume },
                                    ScoreTickSampleName = { BindTarget = tickSampleName },
                                    TickPitchFactor = { BindTarget = tickPitchFactor },
                                    ScoreTickIsLoop = { BindTarget = tickIsLoop },
                                    ScoreTickPitchEasing = { BindTarget = tickPitchEasing },
                                    ScoreTickRateEasing = { BindTarget = tickRateEasing },
                                    ScoreTickVolumeEasing = { BindTarget = tickVolumeEasing },
                                    SwooshPreDelay = { BindTarget = swooshPreDelay }
                                }
                            }
                        }
                    }
                },
            };

            sampleFileSelector.CurrentFile.ValueChanged += (value) =>
            {
                var sample = Path.GetFileNameWithoutExtension(value.NewValue.Name);

                previewSampleChannel?.Dispose();
                previewSampleChannel = new DrawableSample(audioManager.Samples.Get($"Results/{sample}"));
                previewSampleChannel?.Play();

                tickSampleName.Value = sample;
            };
        });

        private ScoreInfo createScore() => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
            },
            Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
            Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
            TotalScore = 2845370,
            Accuracy = 0.95,
            MaxCombo = 999,
            Rank = ScoreRank.S,
            Date = DateTimeOffset.Now,
            Statistics =
            {
                { HitResult.Miss, 1 },
                { HitResult.Meh, 50 },
                { HitResult.Good, 100 },
                { HitResult.Great, 300 },
            }
        };
    }
}
