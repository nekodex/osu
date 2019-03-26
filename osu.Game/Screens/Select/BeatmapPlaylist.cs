// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylist : ScrollContainer<BeatmapSortableFlowContainer>
    {
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();

        public BeatmapPlaylist()
        {
            BeatmapSortableFlowContainer sortableFlowContainer;
            RelativeSizeAxes = Axes.Both;
            ScrollbarOverlapsContent = false;
            Padding = new MarginPadding(5);
            Child = sortableFlowContainer = new BeatmapSortableFlowContainer();
            playlist.BindTo(sortableFlowContainer.Playlist);
        }

        public void AddItem(PlaylistItem item) => playlist.Add(item);

        protected override void Update()
        {
            base.Update();
            updateScrollPosition();
        }

        private void updateScrollPosition()
        {
            const float scroll_trigger_distance = 10;
            const double max_power = 50;
            const double exp_base = 1.05;

            var mouse = GetContainingInputManager().CurrentState.Mouse;

            if (!mouse.IsPressed(MouseButton.Left) || !Child.IsDragging)
                return;

            var localPos = ToLocalSpace(mouse.Position);

            if (localPos.Y < scroll_trigger_distance)
            {
                if (Current <= 0)
                    return;

                var power = Math.Min(max_power, Math.Abs(scroll_trigger_distance - localPos.Y));
                ScrollBy(-(float)Math.Pow(exp_base, power));
            }
            else if (localPos.Y > DrawHeight - scroll_trigger_distance)
            {
                if (IsScrolledToEnd())
                    return;

                var power = Math.Min(max_power, Math.Abs(DrawHeight - scroll_trigger_distance - localPos.Y));
                ScrollBy((float)Math.Pow(exp_base, power));
            }
        }
    }
}
