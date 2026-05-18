using System.Windows.Input;

namespace Lab_06_OOP.Commands
{
    public static class ConfectioneryCommands
    {
        public static readonly RoutedUICommand CreateTastingSet = new RoutedUICommand(
            "Создать дегустационный набор",
            nameof(CreateTastingSet),
            typeof(ConfectioneryCommands),
            new InputGestureCollection
            {
                new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift)
            });
    }
}
