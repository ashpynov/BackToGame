using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using BackToGame.Locator;
using System.Windows;
namespace BackToGame.Controls
{

    public partial class BackToGameControl : PluginUserControl, INotifyPropertyChanged
    {
        static IPlayniteAPI PlayniteAPI;
        readonly private Dictionary<string, int> ProcessIds = new Dictionary<string, int>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public BackToGameControl(IPlayniteAPI api): base()
        {
            PlayniteAPI = api;
            InitializeComponent();
            DataContext = this;
        }

        public RelayCommand<object> ActivateGame  { get => new RelayCommand<object>(ActivateGameCommand); }

        private Game ControlGame
        {
            get
            {
                if (GameContext is Game) return GameContext;

                dynamic ctx = Application.Current.MainWindow.DataContext;
                Game game = ctx.GameStatusVisible ? game = ctx.GameStatusView?.Game?.Game as Game : null;

                return game ?? ctx.SelectedGameDetails?.Game?.Game as Game ?? PlayniteAPI.MainView?.SelectedGames?.FirstOrDefault();
            }
        }
        private int GetProcessId(Game game)
        {
            try     { return ProcessIds[game.Id.ToString()]; }
            catch   { return -1; }
        }

        private void ActivateGameCommand(object obj)
        {
            var game = obj as Game ?? ControlGame;
            var processId = GetProcessId(game);
            if (processId==-1) return;

            if (SupportedTrackingMode(game))
            {
                ProcessTreeLocator.BringProcessToFront(processId);
            }
        }

        private bool SupportedTrackingMode(Game game)
        {
            var mode = ControlGame?.GameActions?.FirstOrDefault()?.TrackingMode ?? TrackingMode.Default;
            return mode == TrackingMode.Default
                || mode == TrackingMode.Process
                || mode == TrackingMode.OriginalProcess;
        }

        public bool IsRunning
        {
            get =>  (ControlGame?.IsRunning ?? false)
                    && SupportedTrackingMode(ControlGame)
                    && ProcessIds.ContainsKey(ControlGame.Id.ToString());
        }
        public bool GameIsRunning(Game game)
        {
            Game g = game ?? ControlGame;
            return g?.IsRunning ?? false
                    && SupportedTrackingMode(g)
                    && ProcessIds.ContainsKey(g.Id.ToString());
        }

        public void OnGameStarted(Game game, int processId)
        {
            if (GameContext != null && GameContext.Id != game.Id)
                return;

            ProcessIds[game.Id.ToString()] = processId;
            (GameContext ?? game).PropertyChanged += OnGameChanged;
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("TestRunning");
        }

        public void OnGameStopped(Game game)
        {
            if (GameContext != null && GameContext.Id != game.Id)
                return;

            (GameContext ?? game).PropertyChanged -= OnGameChanged;
            ProcessIds.Remove(game.Id.ToString());
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("TestRunning");
        }

        public void OnGameChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("TestRunning");
            OnPropertyChanged(args.PropertyName);
        }
    }
}