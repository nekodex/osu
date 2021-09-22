// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class BeatmapDeleteDialog : PopupDialog
    {
        private BeatmapManager manager;
        private AudioMixer mixer;
        private BQFParameters filter;

        private readonly Bindable<float> filterFreq = new Bindable<float>(22000);

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager, AudioManager audio)
        {
            manager = beatmapManager;
            mixer = audio.TrackMixer;

            filter = new BQFParameters
            {
                lFilter = BQFType.LowPass,
                fCenter = filterFreq.Value,
                fQ = 1
            };

            mixer.Effects.Add(filter);

            filterFreq.ValueChanged += e =>
            {
                filter.fCenter = e.NewValue;
                mixer.Effects[0] = filter;
            };
        }

        public BeatmapDeleteDialog(BeatmapSetInfo beatmap)
        {
            BodyText = $@"{beatmap.Metadata?.Artist} - {beatmap.Metadata?.Title}";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = @"Confirm deletion of";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Totally. Delete it.",
                    Action = () => manager.Delete(beatmap),
                },
                new PopupDialogCancelButton
                {
                    Text = @"Firetruck, I didn't mean to!",
                },
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.TransformBindableTo(filterFreq, 150, 1000);
        }

        protected override void PopOut()
        {
            base.PopOut();

            this.TransformBindableTo(filterFreq, 22000, 1000);
        }
    }
}
