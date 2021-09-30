// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// using ManagedBass.Fx;

using ManagedBass.Fx;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.Audio
{
    public class TestSceneFilter : OsuTestScene
    {
        // [Resolved]
        // private AudioManager audio { get; set; }

        private WorkingBeatmap testBeatmap;

        private OsuSpriteText lowpassText;
        private OsuSpriteText highpassText;
        private OsuSpriteText bandpassText;

        private Filter lowpassFilter;
        private Filter highpassFilter;
        private Filter bandpassFilter;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            testBeatmap = new WaveformTestBeatmap(audio);
            lowpassFilter = new Filter(audio.TrackMixer);
            highpassFilter = new Filter(audio.TrackMixer, BQFType.HighPass);
            bandpassFilter = new Filter(audio.TrackMixer, BQFType.BandPass);
            Add(new FillFlowContainer
            {
                Children = new Drawable[]
                {
                    lowpassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = "Low Pass: OFF",
                        Font = new FontUsage(size: 40),
                    },
                    new OsuSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = { BindTarget = lowpassFilter.Cutoff }
                    },
                    highpassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = "High Pass: OFF",
                        Font = new FontUsage(size: 40),
                    },
                    new OsuSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = { BindTarget = highpassFilter.Cutoff }
                    },
                    bandpassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = "Band Pass: OFF",
                        Font = new FontUsage(size: 40),
                    },
                    new OsuSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = { BindTarget = bandpassFilter.Cutoff }
                    },
                }
            });
            lowpassFilter.Cutoff.ValueChanged += e => lowpassText.Text = $"Low Pass: {e.NewValue}hz";
            highpassFilter.Cutoff.ValueChanged += e => highpassText.Text = $"High Pass: {e.NewValue}hz";
            bandpassFilter.Cutoff.ValueChanged += e => bandpassText.Text = $"Band Pass: {e.NewValue}hz";
        }

        [Test]
        public void TestLowPass()
        {
            testFilter(lowpassFilter, 21968, 0);
        }

        [Test]
        public void TestHighPass()
        {
            testFilter(highpassFilter, 21968, 0);
        }

        [Test]
        public void TestBandPass()
        {
            testFilter(bandpassFilter, 21968, 0);
        }

        private void testFilter(Filter filter, int cutoffFrom, int cutoffTo)
        {
            Add(filter);
            AddStep("Prepare Track", () =>
            {
                testBeatmap.LoadTrack();
            });
            AddStep("Play Track", () =>
            {
                testBeatmap.Track.Start();
            });
            AddWaitStep("Let track play", 10);
            AddStep("Filter Down", () => filter.CutoffTo(cutoffFrom).Then().CutoffTo(cutoffTo, 5000, Easing.OutCubic));
            AddWaitStep("Let track play", 10);
            AddStep("Filter Up", () => filter.CutoffTo(cutoffTo).Then().CutoffTo(cutoffFrom, 5000, Easing.InCubic));
            AddWaitStep("Let track play", 10);
            AddStep("Filter ZFUK (21967)", () => filter.CutoffTo(0).Then().CutoffTo(21967));
            AddStep("Filter FUK (21968)", () => filter.CutoffTo(0).Then().CutoffTo(21968));
            AddStep("Filter FUK2 (21969)", () => filter.CutoffTo(0).Then().CutoffTo(21969));
            AddStep("Filter Off", () => filter.CutoffTo(0).Then().CutoffTo(22050));
            AddStep("Stop track", () =>
            {
                testBeatmap.Track.Stop();
            });
        }
    }
}
