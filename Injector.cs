using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using BackToGame.Controls;

namespace BackToGame
{
    static public class Injector
    {

        static IPlayniteAPI PlayniteAPI;
        private static readonly ILogger logger = LogManager.GetLogger();
        static private BackToGameControl Control;

        static readonly private Dictionary<string, dynamic> proxiedCommands = new Dictionary<string, dynamic>();

        static readonly private RelayCommand<object> ProxyActivateSelectedCommand =
            new RelayCommand<object>((a) => StartOrBackToGame(a, "ActivateSelectedCommand"));

        static readonly private RelayCommand<object> ProxyStartSelectedGameCommand =
            new RelayCommand<object>((a) => StartOrBackToGame(a, "StartSelectedGameCommand"));

        static readonly private RelayCommand<Game> ProxyStartGameFromTrayCommand =
            new RelayCommand<Game>((a) => StartOrBackToGame(a, "StartGameFromTrayCommand"));

        static readonly private RelayCommand<Game> ProxyStartGameCommand =
            new RelayCommand<Game>((a) => StartOrBackToGame(a, "StartGameCommand"));

        static public void InjectBackToGameCommmand( IPlayniteAPI api, BackToGameControl control )
        {
            PlayniteAPI = api;
            Control = control;

            foreach (var command in new List<string> {
                "ActivateSelectedCommand",
                "StartSelectedGameCommand",
                "StartGameFromTrayCommand",
                "StartGameCommand"
            }) InjectProxy(command);

            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(Window_Loaded));
        }

        static private void StartOrBackToGame<T>( T game, string commandName)
        {
            dynamic originalCommand = proxiedCommands[commandName];

            if (Control.GameIsRunning(game as Game))
            {
                Control.ActivateGame.Execute(game);
            }
            else if (originalCommand != null && originalCommand.CanExecute(game))
            {
                originalCommand.Execute(game);
            }
        }

        static void InjectProxy(string name)
        {
            try
            {
                object mainModel = PlayniteAPI.MainView
                    .GetType()
                    .GetField("mainModel", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(PlayniteAPI.MainView);

                logger.Info($"Injecting BackToGame into {name}");

                var property = mainModel
                    .GetType()
                    .GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (property == null) return;

                dynamic command = property.GetGetMethod()?.Invoke(mainModel, null);
                proxiedCommands[name] = command;
                if (command == null) return;

                dynamic proxy = typeof(Injector).GetField($"Proxy{name}", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                proxy.Gesture = command.Gesture;

                property.GetSetMethod(true)?.Invoke(mainModel, new[] { proxy });
                return;
            }
            catch
            {
                logger.Error($"Cannot inject BackToGame into {name} connamd");
            }
        }

        static private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Control.GameIsRunning(null)) return;
            if (sender.GetType().Name != "GameMenuWindow") return;

            try
            {
                IEnumerable<DependencyObject> buttons = (sender as DependencyObject).FindVisualChildren<DependencyObject>("Playnite.FullscreenApp.Controls.ButtonEx");
                var name = ResourceProvider.GetString("LOCPlayGame");
                dynamic target = buttons.FirstOrDefault(w => (w as dynamic)?.DataContext?.Title == name);
                target.Command = new RelayCommandExt<Window, object>(sender as Window, (dialog, obj) =>
                {
                    dialog.Close();
                    Control.ActivateGame.Execute(null);
                });
            }
            catch { };
        }
    }
}