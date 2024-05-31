# Back to Game Extension for Playnite
![DownloadCountTotal](https://img.shields.io/github/downloads/ashpynov/BackToGame/total?label=total%20downloads&style=plastic) ![DownloadCountLatest](https://img.shields.io/github/downloads/ashpynov/BackToGame/latest/total?style=plastic) ![LatestVersion](https://img.shields.io/github/v/tag/ashpynov/BackToGame?label=Latest%20version&style=plastic) ![License](https://img.shields.io/github/license/ashpynov/BackToGame?style=plastic)

Back to Game is an extension fro Playnite game manager and launcher to switch back to game if it was minimized, for example by pressing XBox button to activate Playnite. Main goal is to help to gamers who are using Fullsreen theme and controllers on theirs sofa.

Warning: it is required Theme side support to show button on game status screen.


[Latest Release](https://github.com/ashpynov/BackToGame/releases/latest)

## If you feel like supporting
I do everything in my spare time for free, if you feel something aided you and you want to support me, you can always buy me a "koffie" as we say in dutch, no obligations whatsoever...

<a href='https://ko-fi.com/ashpynov' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi2.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

If you may not use Ko-Fi in you country, it should not stop you! On [boosty](https://boosty.to/ashpynov/donate) you may support me and other creators.


# Some technical stuff
## Theme Integration
To use this extension it is require to have theme support.
One of simpliest approach is to modify customice theme Views/GameStatus.xaml file.
You should make modification under ```PART_PanelActionButtons``` section and add new button element like:

```xml
    <StackPanel Name="PART_PanelActionButtons" Orientation="Horizontal"
        HorizontalAlignment="Center" Margin="0,540,0,20" >

        <ButtonEx
            Visibility="{PluginSettings Plugin=BackToGame, Path=IsRunning, Converter={StaticResource BooleanToVisibilityConverter}}"
            Style="{StaticResource ButtonGameStatusAction}"
            Content="{DynamicResource LOCBackToGame}"
            Command="{PluginSettings Plugin=BackToGame, Path=ActivateGame}"
        />
    </StackPanel>
```
- ```Path=IsRunning``` return true if extension is active and game is running. So use it to manage Visibility
- Style - Up to theme, I recomend to use same as for 'Cancel' button.
- ```LOCBackToGame``` - is translatable resource to show 'Back to game' text
- ```Path=ActivateGame``` - handler to call game activation.

Banner/Badge 'Back to game' on came list cards may be implemented in this way:
```xml
    <TextBlock x:Name="GameIsRunningBadge"
        Visibility="{Binding ElementName=BackToGame_Control, Path=Content.IsRunning, Converter={StaticResource BooleanToVisibilityConverter}}"
        Background="{DynamicResource GlyphTransparentBrush}"
        Text="{DynamicResource LOCGameRunning}"
        VerticalAlignment="Bottom"
        TextAlignment="Center"
        Margin="0,0,0,15"
        Padding="0,0,0,5"
        FontSize="{DynamicResource ThemeFontSmallSize}"
        Style="{DynamicResource ButtonTextBlockBoldStyle}" />
...
    <DataTrigger Binding="{Binding ElementName=BackToGame_Control, Path=Content.IsRunning}" Value="True" >
        <Setter Property="Text" Value="{DynamicResource LOCBackToGame}" TargetName="GameIsRunningBadge" />
    </DataTrigger>

```

## Background

As soon as ```ActivateGame``` command is invoked - extension will try to detect game window basing on process Id remembered on game launch.

After window is found it will issue Minimize and then Restore command to reactivate game window.

### Technical problems and limitation
There are many ways how exacly game is executed and managed. So the processId of started event may not belong to exact game window. For examle:
- Pid is game.
- Pid is Launcher, that exacute Game. In this case game Pid is one of children.
- Pid is some process, but game is one of binary from game directory

The Only windows searching by pid modes (exact and tree) are supported.

The Next problem that pid (or list of pids from processes tree) may contain multiple windows. So detection of which is exact game - the point to improve.