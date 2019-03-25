// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylist : CompositeDrawable
    {
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();

        public BeatmapPlaylist()
        {
            BeatmapSortableFlowContainer sortableFlowContainer;
            RelativeSizeAxes = Axes.Both;
            InternalChild = new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarOverlapsContent = false,
                Padding = new MarginPadding(5),
                Child = sortableFlowContainer = new BeatmapSortableFlowContainer()
            };

            playlist.BindTo(sortableFlowContainer.Playlist);
        }

        public void AddItem(PlaylistItem item) => playlist.Add(item);
    }
}
