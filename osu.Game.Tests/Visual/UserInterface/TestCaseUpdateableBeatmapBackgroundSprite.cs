// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestCaseUpdateableBeatmapBackgroundSprite : OsuTestCase
    {
        private TestUpdateableBeatmapBackgroundSprite backgroundSprite;
        private readonly Bindable<BeatmapInfo> beatmapBindable = new Bindable<BeatmapInfo>();
        private BeatmapSetInfo testBeatmap;
        private IAPIProvider api;
        private RulesetStore rulesets;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu, IAPIProvider api, RulesetStore rulesets)
        {
            testBeatmap = ImportBeatmapTest.LoadOszIntoOsu(osu);
            this.api = api;
            this.rulesets = rulesets;
        }

        [SetUp]
        public virtual void SetUp() => Schedule(() =>
        {
            RelativeSizeAxes = Axes.Both;
            Child = backgroundSprite = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };
            backgroundSprite.Beatmap.BindTo(beatmapBindable);
        });

        [Test]
        public void BasicFunctionalityTests()
        {
            AddStep("load null beatmap", () => beatmapBindable.Value = null);
            AddUntilStep("wait for cleanup...", () => backgroundSprite.ChildCount == 1);
            AddStep("load testBeatmap beatmap", () => beatmapBindable.Value = testBeatmap.Beatmaps.First());
            AddUntilStep("wait for cleanup...", () => backgroundSprite.ChildCount == 1);

            if (api.IsLoggedIn)
            {
                var req = new GetBeatmapSetRequest(1);
                api.Queue(req);

                AddUntilStep("wait for api response", () => req.Result != null);
                AddStep("load online beatmap", () => beatmapBindable.Value = new BeatmapInfo
                {
                    BeatmapSet = req.Result?.ToBeatmapSet(rulesets)
                });
                AddUntilStep("wait for cleanup...", () => backgroundSprite.ChildCount == 1);
            }
            else
            {
                AddStep("online (login first)", () => { });
            }
        }

        [Test]
        public void ReloadingTests()
        {
            FillFlowContainer flowContainer;
            var spriteContainer = new Container();

            AddStep("load null beatmap", () => beatmapBindable.Value = null);

            AddStep("init testing layout", () =>
            {
                Child = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),
                    Child = flowContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                    }
                };

                flowContainer.Add(
                    spriteContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
                        Masking = true,
                        Child = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both },
                    }
                );

                flowContainer.Add(
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
                        Masking = true,
                        Child = backgroundSprite = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both },
                    }
                );

                backgroundSprite.Beatmap.BindTo(beatmapBindable);
            });

            AddStep("push element offscreen", () =>
            {
                spriteContainer.ResizeHeightTo(1000, 300);
            });

            AddUntilStep("wait for child disposal...", () => backgroundSprite.ChildDisposed);

            AddStep("bring element back onscreen", () =>
            {
                spriteContainer.ResizeHeightTo(100, 300);
            });
        }

        private class TestUpdateableBeatmapBackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            public int ChildCount => InternalChildren.Count;
            public bool ChildDisposed => !((DelayedLoadWrapper)InternalChild).Content.IsAlive;
        }
    }
}
