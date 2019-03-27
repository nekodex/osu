// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class RearrangableListContainer<T> : CompositeDrawable where T : Drawable, IRearrangableDrawable<T>
    {
        public readonly BindableList<T> ListItems = new BindableList<T>();

        public float GetLayoutPosition(T d) => listContainer.GetLayoutPosition(d);

        public IReadOnlyList<T> Children => listContainer.Children;

        public int Count => listContainer.Count;

        private int maxLayoutPosition;
        private readonly BeatmapFillFlowContainer listContainer;

        public RearrangableListContainer()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new BeatmapScrollContainer
            {
                Child = listContainer = new BeatmapFillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    LayoutDuration = 160,
                    LayoutEasing = Easing.OutQuint,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(1),
                }
            };

            ListItems.ItemsAdded += itemsAdded;
        }

        public void AddItem(T item)
        {
            ListItems.Add(item);
        }

        public void ClearItems()
        {
            ListItems.Clear();
            listContainer.Clear();
        }

        private void itemsAdded(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                item.RequestRemoval += handleRemoval;
                listContainer.Add(item);
                listContainer.SetLayoutPosition(item, maxLayoutPosition++);
            }
        }

        private void handleRemoval(T item)
        {
            // ListItems.Remove(item.Model);
            listContainer.Remove(item);
        }

        private class BeatmapScrollContainer : ScrollContainer<BeatmapFillFlowContainer>
        {
            public BeatmapScrollContainer()
            {
                RelativeSizeAxes = Axes.Both;
                ScrollbarOverlapsContent = false;
                Padding = new MarginPadding(5);
            }

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

        private class BeatmapFillFlowContainer : FillFlowContainer<T>
        {
            public bool IsDragging => draggedItem != null;

            private T draggedItem;
            private Vector2 nativeDragPosition;
            private List<Drawable> sortableChildList;

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
        }
    }

    public interface IRearrangableDrawable<T>
    {
        event Action<T> RequestRemoval;
        bool IsDraggable { get; }
    }
}
