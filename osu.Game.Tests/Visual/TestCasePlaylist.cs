// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePlaylist : ManualInputManagerTestCase
    {
        private RulesetStore rulesets;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapPlaylist),
            typeof(BeatmapPlaylistItem),
            typeof(BeatmapSortableFlowContainer),
        };

        private BeatmapPlaylist playlist;

        private int lastInsert;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
            Add(playlist = new BeatmapPlaylist());
        }

        [SetUp]
        public void SetUp()
            {
            lastInsert = 0;
            playlist.ClearItems();
            for (int i = 0; i < 4; i++)
                playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4)));
            }

        [Test]
        public void AddRemoveTests()
            {
            AddStep("Hover Remove Button", () => { InputManager.MoveMouseTo(getFirstChild().ToScreenSpace(getFirstChild().DrawSize + new Vector2(-20, -getChildDrawableSize().Y * 0.5f))); });
            AddStep("RemoveItem", () => InputManager.Click(MouseButton.Left));
            AddAssert("Ensure correct child count", () => getChildCount() == 3);
            AddStep("AddItem", () => { playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4))); });
            AddAssert("Ensure correct child count", () => getChildCount() == 4);
        }

        [Test]
        public void SortingTests()
        {
            AddStep("Hover drag handle", () => { InputManager.MoveMouseTo(getFirstChild().ToScreenSpace(new Vector2(10, getChildDrawableSize().Y * 0.5f))); });
            AddStep("Click", () => { InputManager.PressButton(MouseButton.Left); });
            AddStep("Drag downward", () => { InputManager.MoveMouseTo(getFirstChild().ToScreenSpace(new Vector2(10, getChildDrawableSize().Y * 2.5f))); });
            AddStep("Release", () => { InputManager.ReleaseButton(MouseButton.Left); });
            AddAssert("Ensure item is now third", () => playlist.Child.GetLayoutPosition(getFirstChild()) == 2);

            AddStep("Hover drag handle", () => { InputManager.MoveMouseTo(getFirstChild().ToScreenSpace(new Vector2(10, getChildDrawableSize().Y * 0.5f))); });
            AddStep("Click", () => { InputManager.PressButton(MouseButton.Left); });
            AddStep("Drag upward", () => { InputManager.MoveMouseTo(getFirstChild().ToScreenSpace(new Vector2(10, -getChildDrawableSize().Y * 1.5f))); });
            AddStep("Release", () => { InputManager.ReleaseButton(MouseButton.Left); });
            AddAssert("Ensure item is now first again", () => playlist.Child.GetLayoutPosition(getFirstChild()) == 0);
        }

        private int getChildCount()
            {
            return playlist.Child.Children.Count;
        }

        private BeatmapPlaylistItem getFirstChild()
                {
            return playlist.Child.Children.First();
                }

        private Vector2 getChildDrawableSize()
        {
            return getFirstChild().DrawSize;
        }

        private PlaylistItem generatePlaylistItem(RulesetInfo ruleset)
        {
            var beatmap = new TestBeatmap(ruleset);
            var playlistItem = new PlaylistItem
            {
                Beatmap = beatmap.BeatmapInfo,
                Ruleset = beatmap.BeatmapInfo.Ruleset,
                RulesetID = beatmap.BeatmapInfo.Ruleset?.ID ?? 0
            };

            var instance = ruleset.CreateInstance();
            playlistItem.RequiredMods.AddRange(instance.GetAllMods());

            return playlistItem;
        }
    }
}
