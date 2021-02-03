// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using Newtonsoft.Json;
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
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
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
    [Serializable]
    public class TestSceneAccuracyCircle : OsuTestScene
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        private DrawableSample previewSampleChannel;
        private AccuracyCircleAdjustments settings = new AccuracyCircleAdjustments();

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
                                                Current = { BindTarget = settings.PlayTicks }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Tick Volume (Start)",
                                                Current = { BindTarget = settings.TickVolumeStart }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Tick Volume (End)",
                                                Current = { BindTarget = settings.TickVolumeEnd }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play Impact",
                                                Current = { BindTarget = settings.PlayImpact }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Impact Volume",
                                                Current = { BindTarget = settings.ImpactVolume }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play BadgeSounds",
                                                Current = { BindTarget = settings.PlayBadgeSounds }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Badge Dink Volume",
                                                Current = { BindTarget = settings.BadgeDinkVolume }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Play Swoosh",
                                                Current = { BindTarget = settings.PlaySwooshSound }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Swoosh Volume",
                                                Current = { BindTarget = settings.SwooshVolume }
                                            },

                                            new SettingsSlider<double>
                                            {
                                                LabelText = "Swoosh Pre-Delay",
                                                Current = { BindTarget = settings.SwooshPreDelay }
                                            },
                                            new SettingsCheckbox
                                            {
                                                LabelText = "Loop ScoreTick",
                                                Current = { BindTarget = settings.TickIsLoop }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "ScoreTick Start Debounce Rate",
                                                Current = { BindTarget = settings.TickDebounceStart }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "ScoreTick End Debounce Rate",
                                                Current = { BindTarget = settings.TickDebounceEnd }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "ScoreTick Rate Easing:"
                                            },
                                            new SettingsEnumDropdown<Easing>
                                            {
                                                Current = { BindTarget = settings.TickRateEasing }
                                            },
                                            new SettingsSlider<double>
                                            {
                                                LabelText = "ScoreTick Pitch Factor",
                                                Current = { BindTarget = settings.TickPitchFactor }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "Pitch Easing:"
                                            },
                                            new SettingsEnumDropdown<Easing>
                                            {
                                                Current = { BindTarget = settings.TickPitchEasing }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "Volume Easing:"
                                            },
                                            new SettingsEnumDropdown<Easing>
                                            {
                                                Current = { BindTarget = settings.TickVolumeEasing }
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                Text = "Current Tick Sample:"
                                            },
                                            new OsuSpriteText
                                            {
                                                Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2 },
                                                Current = { BindTarget = settings.TickSampleName }
                                            },
                                            sampleFileSelector = new FileSelector("/Users/jamie/Sandbox/derp/Samples/Results")
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 500,
                                            },
                                            new TriangleButton
                                            {
                                                Text = "Save",
                                                Action = save,
                                                RelativeSizeAxes = Axes.X,
                                            },
                                            new TriangleButton
                                            {
                                                Text = "Load",
                                                Action = load,
                                                RelativeSizeAxes = Axes.X,
                                            },
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
                                    DebounceRateStart = { BindTarget = settings.TickDebounceStart },
                                    DebounceRateStop = { BindTarget = settings.TickDebounceEnd },
                                    PlayTicks = { BindTarget = settings.PlayTicks },
                                    TickVolumeStart = { BindTarget = settings.TickVolumeStart },
                                    TickVolumeEnd = { BindTarget = settings.TickVolumeEnd },
                                    PlayImpact = { BindTarget = settings.PlayImpact },
                                    ImpactVolume = { BindTarget = settings.ImpactVolume },
                                    PlayBadgeDinks = { BindTarget = settings.PlayBadgeSounds },
                                    BadgeDinkVolume = { BindTarget = settings.BadgeDinkVolume },
                                    PlaySwoosh = { BindTarget = settings.PlaySwooshSound },
                                    SwooshVolume = { BindTarget = settings.SwooshVolume },
                                    ScoreTickSampleName = { BindTarget = settings.TickSampleName },
                                    TickPitchFactor = { BindTarget = settings.TickPitchFactor },
                                    ScoreTickIsLoop = { BindTarget = settings.TickIsLoop },
                                    ScoreTickPitchEasing = { BindTarget = settings.TickPitchEasing },
                                    ScoreTickRateEasing = { BindTarget = settings.TickRateEasing },
                                    ScoreTickVolumeEasing = { BindTarget = settings.TickVolumeEasing },
                                    SwooshPreDelay = { BindTarget = settings.SwooshPreDelay }
                                }
                            }
                        }
                    }
                },
            };

            sampleFileSelector.CurrentFile.ValueChanged += value =>
            {
                var sample = Path.GetFileNameWithoutExtension(value.NewValue.Name);

                previewSampleChannel?.Dispose();
                previewSampleChannel = new DrawableSample(audioManager.Samples.Get($"Results/{sample}"));
                previewSampleChannel?.Play();

                settings.TickSampleName.Value = sample;
            };
        });

        [Resolved]
        private GameHost host { get; set; }

        private void save()
        {
            File.WriteAllText(host.Storage.GetFullPath("out.json"), JsonConvert.SerializeObject(settings));
        }

        private void load()
        {
            var saved = JsonConvert.DeserializeObject<AccuracyCircleAdjustments>(File.ReadAllText(host.Storage.GetFullPath("out.json")));

            foreach (var (_, prop) in saved.GetSettingsSourceProperties())
            {
                var targetBindable = (IBindable)prop.GetValue(settings);
                var sourceBindable = (IBindable)prop.GetValue(saved);

                targetBindable?.Parse(sourceBindable);
            }
        }

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

    public class AccuracyCircleAdjustments
    {
        [SettingSource("setting")]
        public Bindable<bool> PlayTicks { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<bool> PlayImpact { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<bool> PlayBadgeSounds { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<bool> PlaySwooshSound { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<bool> TickIsLoop { get; } = new Bindable<bool>(false);

        [SettingSource("setting")]
        public Bindable<string> TickSampleName { get; } = new Bindable<string>("badge-dink-2");

        [SettingSource("setting")]
        public BindableDouble TickPitchFactor { get; } = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 3,
            Precision = 0.1,
        };

        [SettingSource("setting")]
        public BindableDouble TickDebounceStart { get; } = new BindableDouble(10)
        {
            MinValue = 1,
            MaxValue = 100,
        };

        [SettingSource("setting")]
        public BindableDouble TickDebounceEnd { get; } = new BindableDouble(400)
        {
            MinValue = 100,
            MaxValue = 1000,
        };

        [SettingSource("setting")]
        public BindableDouble SwooshPreDelay { get; } = new BindableDouble(450)
        {
            MinValue = -1000,
            MaxValue = 1000,
        };

        [SettingSource("setting")]
        public Bindable<Easing> TickRateEasing { get; } = new Bindable<Easing>(Easing.None);

        [SettingSource("setting")]
        public Bindable<Easing> TickPitchEasing { get; } = new Bindable<Easing>(Easing.None);

        [SettingSource("setting")]
        public Bindable<Easing> TickVolumeEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public BindableDouble TickVolumeStart { get; } = new BindableDouble(0.6)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        [SettingSource("setting")]
        public BindableDouble TickVolumeEnd { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        [SettingSource("setting")]
        public BindableDouble ImpactVolume { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        [SettingSource("setting")]
        public BindableDouble BadgeDinkVolume { get; } = new BindableDouble(0.5)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };

        [SettingSource("setting")]
        public BindableDouble SwooshVolume { get; } = new BindableDouble(0.5)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1,
        };
    }
}
