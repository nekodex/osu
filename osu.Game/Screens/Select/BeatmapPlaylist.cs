// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylist : CompositeDrawable
    {
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();
        private readonly FillFlowContainer<BeatmapPlaylistItem> playlistFlowContainer;
        private Vector2 nativeDragPosition;
        private BeatmapPlaylistItem draggedItem;
        private List<Drawable> sortableChildList;
        private int maxLayoutPosition;

        public BeatmapPlaylist()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                // ScrollbarOverlapsContent = false,
                Padding = new MarginPadding(5),
                Child = playlistFlowContainer = new FillFlowContainer<BeatmapPlaylistItem>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    LayoutDuration = 160,
                    LayoutEasing = Easing.OutQuint,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(1),
                }
            };

            playlist.ItemsAdded += itemsAdded;
        }

        public void AddItem(PlaylistItem item)
        {
            playlist.Add(item);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            nativeDragPosition = e.ScreenSpaceMousePosition;
            draggedItem = playlistFlowContainer.FirstOrDefault(d => d.IsDraggable);
            sortableChildList = new List<Drawable>(playlistFlowContainer.FlowingChildren);
            return draggedItem != null || base.OnDragStart(e);
        }

        protected override bool OnDrag(DragEvent e)
        {
            nativeDragPosition = e.ScreenSpaceMousePosition;
            return draggedItem != null || base.OnDrag(e);
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            nativeDragPosition = e.ScreenSpaceMousePosition;
            var handled = draggedItem != null || base.OnDragEnd(e);
            draggedItem = null;
            sortableChildList = new List<Drawable>();
            return handled;
        }

        protected override void Update()
        {
            base.Update();

            if (draggedItem == null)
                return;

            updateDragPosition();
        }

        private void updateDragPosition()
        {
            var itemsPos = playlistFlowContainer.ToLocalSpace(nativeDragPosition);
            int srcIndex = (int)playlistFlowContainer.GetLayoutPosition(draggedItem);

            // Find the last item with position < mouse position. Note we can't directly use
            // the item positions as they are being transformed
            float heightAccumulator = 0;
            int dstIndex = 0;
            for (; dstIndex < playlistFlowContainer.Count; dstIndex++)
            {
                // Using BoundingBox here takes care of scale, paddings, etc...
                heightAccumulator += playlistFlowContainer[dstIndex].BoundingBox.Height;
                if (heightAccumulator > itemsPos.Y)
                    break;
            }

            dstIndex = MathHelper.Clamp(dstIndex, 0, playlistFlowContainer.Count - 1);

            if (srcIndex == dstIndex)
                return;

            sortableChildList.Remove(draggedItem);

            if (srcIndex < dstIndex - 1)
                dstIndex--;

            sortableChildList.Insert(dstIndex, draggedItem);

            for (int i = 0; i < sortableChildList.Count; i++)
                playlistFlowContainer.SetLayoutPosition(sortableChildList[i], i);
        }

        private void itemsAdded(IEnumerable<PlaylistItem> items)
        {
            foreach (var item in items)
            {
                var drawable = new BeatmapPlaylistItem(item);
                drawable.RequestRemoval += handleRemoval;
                playlistFlowContainer.Add(drawable);
                playlistFlowContainer.SetLayoutPosition(drawable, maxLayoutPosition++);
            }
        }

        private void handleRemoval(BeatmapPlaylistItem item)
        {
            playlist.Remove(item.PlaylistItem.Value);
            playlistFlowContainer.Remove(item);
        }
    }
}
