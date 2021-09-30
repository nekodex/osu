// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;

namespace osu.Game.Audio.Effects
{
    public class Filter : Component, IFilterableAudioComponent
    {
        private const int max_cutoff = 22049;
        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;

        public BindableNumber<int> Cutoff { get; set; } = new BindableNumber<int>(max_cutoff)
        {
            MinValue = 0,
            MaxValue = max_cutoff
        };

        /// <summary>
        /// A BiQuad filter that performs a filter-sweep when toggled on or off.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be attached to.</param>
        /// <param name="filterType">The type of filter to employ (e.g. LowPass, HighPass, etc)</param>
        public Filter(AudioMixer mixer, BQFType filterType = BQFType.LowPass)
        {
            this.mixer = mixer;
            filter = new BQFParameters
            {
                lFilter = filterType,
                fCenter = max_cutoff
            };
            attachFilter();
        }

        private void attachFilter()
        {
            Logger.Log("===== ATTACH =====");
            mixer.Effects.Add(filter);
            Cutoff.ValueChanged += updateFilter;
        }

        private void detachFilter()
        {
            Logger.Log("===== DETACH =====");
            Cutoff.ValueChanged -= updateFilter;
            mixer.Effects.Remove(filter);
        }

        private void updateFilter(ValueChangedEvent<int> cutoff)
        {
            var filterIndex = mixer.Effects.IndexOf(filter);
            if (filterIndex < 0) return;

            var existingFilter = mixer.Effects[filterIndex] as BQFParameters;
            if (existingFilter == null) return;

            existingFilter.fCenter = cutoff.NewValue;
            mixer.Effects[filterIndex] = existingFilter;

            // Logger.Log($"===== FILTER: {cutoff.NewValue} =====");
            // var filterIndex2 = mixer.Effects.IndexOf(filter);
            // if (filterIndex2 < 0) return;
            //
            // var existingFilter2 = mixer.Effects[filterIndex2] as BQFParameters;
            // if (existingFilter2 == null) return;
            //
            // Logger.Log($"===== FILTER ACTUAL: {existingFilter2.fCenter} =====");
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            detachFilter();
        }
    }
}
