// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Select
{
    // TODO: extract functionality to framework as a RearrangableListContainer or something
    public class RearrangableBeatmapListContainer : RearrangableListContainer<BeatmapPlaylistDrawable>
    {
        public void AddItem(PlaylistItem item)
        {
            var drawable = new BeatmapPlaylistDrawable
            {
                PlaylistItem = { Value = item },
            };

            base.AddItem(drawable);
        }
    }
}
