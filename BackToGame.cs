using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using BackToGame.Controls;
using System.Security.Cryptography.X509Certificates;

using System.Reflection;

using System.Windows.Media;
using System.Media;
using System.ComponentModel;

using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;
using System.IO.Compression;
using System.Threading;

namespace BackToGame
{
    public class BackToGame : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public BackToGameControl Control { get; }
        public override Guid Id { get; } = Guid.Parse("c05dfa72-e302-44cf-9612-af1f7caa07f7");

        IPlayniteAPI PlayniteAPI;
        public BackToGame(IPlayniteAPI api) : base(api)
        {
            PlayniteAPI = api;
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
            Localization.SetPluginLanguage(PluginFolder, PlayniteAPI.ApplicationSettings.Language);
            Control = new BackToGameControl(PlayniteAPI);

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
                case "BackToGame":
                    return new BackToGameControl(PlayniteAPI);
                default:
                    throw new ArgumentException($"Unrecognized controlType '{controlType}' for request '{args.Name}'");
            }
        }

        public override void OnGameStarted(OnGameStartedEventArgs args) => BackToGameControl.OnGameStarted(args.Game, args.StartedProcessId);

        public override void OnGameStopped(OnGameStoppedEventArgs args) => BackToGameControl.OnGameStopped(args.Game);
    }
}