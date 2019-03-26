// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylistItem : Container
    {
        public readonly Bindable<PlaylistItem> PlaylistItem = new Bindable<PlaylistItem>();

        public event Action<BeatmapPlaylistItem> RequestRemoval;

        public bool IsDraggable { get; private set; }

        private const int fade_duration = 60;
        private readonly DragHandle dragHandle;
        private readonly RemoveButton removeButton;
        private readonly Box background;
        private readonly Box gradient;
        private bool isHovered;
        private bool isDragged;
        private readonly Color4 backgroundColour = Color4.Black;
        private readonly Color4 selectedColour = new Color4(0.1f, 0.1f, 0.1f, 1f);

        public BeatmapPlaylistItem(PlaylistItem item)
        {
            UpdateableBeatmapBackgroundSprite cover;
            Height = 50;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = 25,
                    },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = backgroundColour.Opacity(40),
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    background = new Box
                                    {
                                        Colour = backgroundColour,
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 0.5f,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 0.5f,
                                        Children = new Drawable[]
                                        {
                                            cover = new UpdateableBeatmapBackgroundSprite
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                FillMode = FillMode.Stretch
                                            },
                                            gradient = new Box
                                            {
                                                Colour = ColourInfo.GradientHorizontal(backgroundColour, backgroundColour.Opacity(0.5f)),
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                        },
                                    },
                                }
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Padding = new MarginPadding(7),
                                Children = new Drawable[]
                                {
                                    new DifficultyIcon(item.Beatmap)
                                    {
                                        Scale = new Vector2(1.5f)
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Padding = new MarginPadding
                                        {
                                            Left = 10,
                                        },
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Spacing = new Vector2(5),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = new LocalisedString((item.Beatmap?.BeatmapSet?.Metadata.ArtistUnicode, item.Beatmap?.BeatmapSet?.Metadata.Artist)),
                                                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = "-",
                                                        Font = OsuFont.GetFont()
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = new LocalisedString((item.Beatmap?.Metadata.TitleUnicode, item.Beatmap?.Metadata.Title)),
                                                        Font = OsuFont.GetFont()
                                                    },
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Spacing = new Vector2(10),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = item.Beatmap?.Version,
                                                        Font = OsuFont.GetFont(size: 12)
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = $"mapped by {item.Beatmap?.BeatmapSet?.Metadata.Author.Username}",
                                                        Font = OsuFont.GetFont(size: 12, italics: true),
                                                        Colour = Color4.Violet,
                                                    }
                                                }
                                            },
                                        }
                                    },
                                }
                            },
                        },
                    }
                },
                new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Position = new Vector2(-20, 0),
                    Child = removeButton = new RemoveButton
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => RequestRemoval?.Invoke(this)
                    }
                },
                dragHandle = new DragHandle()
            };

            PlaylistItem.ValueChanged += change => cover.Beatmap.Value = change.NewValue.Beatmap;
            PlaylistItem.Value = item;
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (e.IsPressed(MouseButton.Left))
            {
                if (isDragged)
                    isHovered = true;

                return true;
            }

            isHovered = true;

            showHoverElements(true);
            setHighlighted(true);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            isHovered = false;

            if (isDragged)
                return;

            showHoverElements(false);
            setHighlighted(false);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!e.IsPressed(MouseButton.Left))
                return false;

            // Manually track dragging status as to not capture dragging events (so we don't interfere with the scrolling behaviour of our parent)
            isDragged = true;
            IsDraggable = dragHandle.IsHovered;

            if (IsDraggable)
                removeButton.Hide();

            return false;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            isDragged = false;
            IsDraggable = false;
            setHighlighted(false);

            if (!isHovered)
                showHoverElements(false);
            else
                removeButton.Show();

            return base.OnMouseUp(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // This is to show the current item's buttons after having dragged a different item and landing here (i.e. the OnHover was prevented from being fired)
            if (!isHovered && !e.IsPressed(MouseButton.Left))
                showHoverElements(true);

            return base.OnMouseMove(e);
        }

        private void showHoverElements(bool show)
        {
            if (show)
            {
                removeButton.Show();
                dragHandle.Show();
            }
            else
            {
                removeButton.Hide();
                dragHandle.Hide();
            }
        }

        private void setHighlighted(bool selected)
        {
            var colour = selected ? selectedColour : backgroundColour;
            background.Colour = colour;
            gradient.Colour = ColourInfo.GradientHorizontal(colour, colour.Opacity(0.5f));
        }

        private class DragHandle : Container
        {
            public DragHandle()
            {
                RelativeSizeAxes = Axes.Y;
                Width = 25;
                Alpha = 0;
                Child = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(12),
                    Icon = FontAwesome.fa_bars,
                    Margin = new MarginPadding { Left = 5, Top = 2 }
                };
            }

            public override bool HandlePositionalInput => IsPresent;

            public override void Show()
            {
                this.FadeIn(fade_duration);
            }

            public override void Hide()
            {
                this.FadeOut(fade_duration);
            }
        }

        private class RemoveButton : OsuClickableContainer
        {
            public RemoveButton()
            {
                Alpha = 0;
                Child = new SpriteIcon
                {
                    Colour = Color4.White,
                    Icon = FontAwesome.fa_minus_square,
                    Size = new Vector2(14),
                };
            }

            public override void Show()
            {
                this.FadeIn(fade_duration);
            }

            public override void Hide()
            {
                this.FadeOut(fade_duration);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                Content.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                Content.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(e);
            }
        }
    }
}
