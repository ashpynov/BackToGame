using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using BackToGame.Locator;
namespace BackToGame.Controls
{

    public partial class BackToGameControl : PluginUserControl, INotifyPropertyChanged
    {
        static IPlayniteAPI PlayniteAPI;
        static readonly private Dictionary<string, int> ProcessIds = new Dictionary<string, int>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public BackToGameControl(IPlayniteAPI api)
        {
            PlayniteAPI = api;
            InitializeComponent();
            DataContext = this;
        }

        public RelayCommand<object> ActivateGame  { get => new RelayCommand<object>(ActivateGameCommand); }

        private Game ControlGame => GameContext ?? PlayniteAPI.MainView?.SelectedGames?.FirstOrDefault();

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
            get =>  ControlGame?.IsRunning ?? false
                    && SupportedTrackingMode(ControlGame)
                    && ProcessIds.ContainsKey(ControlGame.Id.ToString());
        }

        public void OnGameStarted(Game game, int processId)
        {
            ProcessIds[game.Id.ToString()] = processId;
            game.PropertyChanged += OnGameChanged;
            OnPropertyChanged("IsRunning");
        }

        public void OnGameStopped(Game game)
        {
            game.PropertyChanged -= OnGameChanged;
            ProcessIds.Remove(game.Id.ToString());
            OnPropertyChanged("IsRunning");
        }

        public void OnGameChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged("IsRunning");
            OnPropertyChanged(args.PropertyName);
        }
    }
}