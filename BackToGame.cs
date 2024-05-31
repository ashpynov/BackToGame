using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.IO;
using System.Reflection;
using BackToGame.Controls;


namespace BackToGame
{
    public class BackToGame : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static public BackToGameControl Control { get; private set; }
        public override Guid Id { get; } = Guid.Parse("c05dfa72-e302-44cf-9612-af1f7caa07f7");

        static public List<BackToGameControl> controls = new List<BackToGameControl>();

        static IPlayniteAPI PlayniteAPI;

        public BackToGame(IPlayniteAPI api) : base(api)
        {
            PlayniteAPI = api;
            Control = new BackToGameControl(PlayniteAPI);
            Injector.InjectBackToGameCommmand(api, Control);

            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
            Localization.SetPluginLanguage(PluginFolder, PlayniteAPI.ApplicationSettings.Language);

            controls.Add(Control);

            #region Control constructor

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "BackToGame",
                ElementList = new List<string> { "Control" }
            });
            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "BackToGame",
                SettingsRoot = $"{nameof(Control)}"
            });

            #endregion
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            var strArgs = args.Name.Split('_');

            var controlType = strArgs[0];

            switch (controlType)
            {
                case "Control":
                    controls.Add(new BackToGameControl(PlayniteAPI));
                    return controls.Last();
                default:
                    throw new ArgumentException($"Unrecognized controlType '{controlType}' for request '{args.Name}'");
            }
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            foreach (BackToGameControl c in controls) c.OnGameStarted(args.Game, args.StartedProcessId);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            foreach (BackToGameControl c in controls) c.OnGameStopped(args.Game);
        }
   }
}