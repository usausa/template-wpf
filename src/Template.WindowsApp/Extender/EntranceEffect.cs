namespace Template.WindowsApp.Extender;

using System.Windows.Media.Animation;

public sealed class EntranceEffect : IWindowsNavigationEffect
{
    public const string Name = "Entrance";

    private const double Offset = 24;

    private static readonly Duration AnimationDuration = new(TimeSpan.FromMilliseconds(250));

    public Task EffectAsync(WindowsNavigationEffectContext context)
    {
        if (context.Phase is not (WindowsNavigationEffectPhase.Open or WindowsNavigationEffectPhase.Activate))
        {
            return Task.CompletedTask;
        }

        var view = context.View;
        var transform = new TranslateTransform(0, Offset);
        view.RenderTransform = transform;
        view.Opacity = 0;

        var completion = new TaskCompletionSource();
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var fade = new DoubleAnimation(0, 1, AnimationDuration) { EasingFunction = easing };
        fade.Completed += (_, _) =>
        {
            view.BeginAnimation(UIElement.OpacityProperty, null);
            view.Opacity = 1;
            view.RenderTransform = null;
            completion.TrySetResult();
        };
        transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(Offset, 0, AnimationDuration) { EasingFunction = easing });
        view.BeginAnimation(UIElement.OpacityProperty, fade);
        return completion.Task;
    }
}
