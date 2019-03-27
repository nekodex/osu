// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePlaylist : OsuTestCase
    {
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
            Add(playlist = new BeatmapPlaylist());

            for (int i = 0; i < 4; i++)
            {
                playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4)));
            }

            AddStep("AddItem", () =>
            {
                playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4)));
            });

            AddStep("AddItem x 25", () =>
            {
                for (int i = 0; i < 25; i++)
                {
                    playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4)));
                }
            });
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
