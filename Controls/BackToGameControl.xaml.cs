using System.Collections.Generic;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using BackToGame.Locator;

namespace BackToGame.Controls
{

    public partial class BackToGameControl : PluginUserControl
    {
        static IPlayniteAPI PlayniteAPI;
        static readonly private Dictionary<string, int> ProcessIds = new Dictionary<string, int>();

        public BackToGameControl(IPlayniteAPI api)
        {
            PlayniteAPI = api;
            InitializeComponent();
            DataContext = this;
        }

        public RelayCommand<object> ActivateGame  { get => new RelayCommand<object>(ActivateGameCommand); }

        private Game ControlGame => GameContext ?? PlayniteAPI.MainView.SelectedGames.FirstOrDefault();

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

            var trackingMode = game.GameActions.FirstOrDefault().TrackingMode;
            if (SupportedTrackingMode(trackingMode))
            {
                ProcessTreeLocator.BringProcessToFront(processId);
            }
        }

        private bool SupportedTrackingMode( TrackingMode mode )
        {
            return mode == TrackingMode.Default
                || mode == TrackingMode.Process
                || mode == TrackingMode.OriginalProcess;
        }
        public bool IsRunning {
            get => ControlGame.IsRunning
                    && SupportedTrackingMode(ControlGame.GameActions.FirstOrDefault().TrackingMode)
                    && ProcessIds.ContainsKey(ControlGame.Id.ToString());
        }

        static public void OnGameStarted( Game game, int processId)
        {
            ProcessIds[game.Id.ToString()] = processId;
        }

        static public void OnGameStopped( Game game)
        {
            ProcessIds.Remove(game.Id.ToString());
        }
    }
}