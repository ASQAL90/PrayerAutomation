// Auto-hide behavior for overlay scrollbars: shows the thumb while it is in use or hovered, fades it out when idle.
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using Thumb = System.Windows.Controls.Primitives.Thumb;

namespace PrayerAutomation.UI;

/// <summary>Shows a scrollbar thumb while it is in use or hovered and fades it out when idle, like macOS overlay scrollbars.</summary>
internal static class AutoHideScrollBar
{
    private static readonly TimeSpan HideDelay = TimeSpan.FromMilliseconds(700);
    private static readonly Duration FadeDuration = new(TimeSpan.FromMilliseconds(180));
    private static readonly ConditionalWeakTable<ScrollBar, ScrollBarAutoHide> Bars = new();

    /// <summary>Attaches the auto-hide behavior to the scrollbar; safe to call more than once.</summary>
    /// <param name="scrollBar">The scrollbar to track.</param>
    internal static void Attach(ScrollBar scrollBar)
    {
        if (scrollBar is null || Bars.TryGetValue(scrollBar, out _)) return;
        Bars.Add(scrollBar, new ScrollBarAutoHide(scrollBar));
    }

    /// <summary>Holds the per-scrollbar state for auto-hiding.</summary>
    private sealed class ScrollBarAutoHide
    {
        private readonly ScrollBar _scrollBar;
        private readonly DispatcherTimer _hideTimer;
        private Thumb? _thumb;
        private bool _attached;

        internal ScrollBarAutoHide(ScrollBar scrollBar)
        {
            _scrollBar = scrollBar;
            _hideTimer = new DispatcherTimer { Interval = HideDelay };
            _hideTimer.Tick += (_, _) => { _hideTimer.Stop(); FadeThumb(0); };
            _scrollBar.Loaded += (_, _) => AttachTemplate();
            _scrollBar.Unloaded += (_, _) => _hideTimer.Stop();
            _scrollBar.Scroll += (_, _) => Reveal();
            _scrollBar.MouseEnter += (_, _) => Reveal();
            _scrollBar.MouseLeave += (_, _) => ScheduleHide();
        }

        /// <summary>Finds the PART_Thumb part of the template and hides it until the scrollbar is used.</summary>
        private void AttachTemplate()
        {
            if (_attached) return;
            _attached = true;
            _scrollBar.ApplyTemplate();
            _thumb = _scrollBar.Template.FindName("PART_Thumb", _scrollBar) as Thumb;
            if (_thumb is null) return;
            _thumb.DragStarted += (_, _) => Reveal();
            _thumb.DragCompleted += (_, _) => ScheduleHide();
            _thumb.Opacity = 0;
        }

        /// <summary>Shows the thumb and cancels the pending hide.</summary>
        private void Reveal()
        {
            _hideTimer.Stop();
            FadeThumb(0.75);
        }

        /// <summary>Schedules fading the thumb out after the idle delay.</summary>
        private void ScheduleHide()
        {
            _hideTimer.Stop();
            _hideTimer.Start();
        }

        /// <summary>Fades the thumb to the given opacity.</summary>
        /// <param name="opacity">Target opacity, 0 to hide.</param>
        private void FadeThumb(double opacity)
        {
            if (_thumb is null) return;
            _thumb.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(opacity, FadeDuration) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
        }
    }
}
