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
    // TODO: extract functionality to framework as a SortableFillFlowContainer or something
    public class BeatmapSortableFlowContainer : FillFlowContainer<BeatmapPlaylistItem>
    {
        public bool IsDragging => draggedItem != null;

        public readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();
        private Vector2 nativeDragPosition;
        private BeatmapPlaylistItem draggedItem;
        private List<Drawable> sortableChildList;
        private int maxLayoutPosition;

        public BeatmapSortableFlowContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            LayoutDuration = 160;
            LayoutEasing = Easing.OutQuint;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(1);

            Playlist.ItemsAdded += itemsAdded;
        }

        public void AddItem(PlaylistItem item)
        {
            Playlist.Add(item);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            nativeDragPosition = e.ScreenSpaceMousePosition;
            draggedItem = this.FirstOrDefault(d => d.IsDraggable);
            sortableChildList = new List<Drawable>(FlowingChildren);
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
            var itemsPos = ToLocalSpace(nativeDragPosition);
            int srcIndex = (int)GetLayoutPosition(draggedItem);

            // Find the last item with position < mouse position. Note we can't directly use
            // the item positions as they are being transformed
            float heightAccumulator = 0;
            int dstIndex = 0;
            for (; dstIndex < Count; dstIndex++)
            {
                // Using BoundingBox here takes care of scale, paddings, etc...
                heightAccumulator += this[dstIndex].BoundingBox.Height + Spacing.Y;
                if (heightAccumulator > itemsPos.Y)
                    break;
            }

            dstIndex = MathHelper.Clamp(dstIndex, 0, Count - 1);

            if (srcIndex == dstIndex)
                return;

            sortableChildList.Remove(draggedItem);

            if (srcIndex < dstIndex - 1)
                dstIndex--;

            sortableChildList.Insert(dstIndex, draggedItem);

            for (int i = 0; i < sortableChildList.Count; i++)
                SetLayoutPosition(sortableChildList[i], i);
        }

        private void itemsAdded(IEnumerable<PlaylistItem> items)
        {
            foreach (var item in items)
            {
                var drawable = new BeatmapPlaylistItem(item);
                drawable.RequestRemoval += handleRemoval;
                Add(drawable);
                SetLayoutPosition(drawable, maxLayoutPosition++);
            }
        }

        private void handleRemoval(BeatmapPlaylistItem item)
        {
            Playlist.Remove(item.PlaylistItem.Value);
            Remove(item);
        }
    }
}
