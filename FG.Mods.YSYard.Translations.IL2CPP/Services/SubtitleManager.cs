using RenderHeads.Media.AVProVideo;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace FG.Mods.YSYard.Translations.Services;

internal static class SubtitleManager
{
    private static readonly Dictionary<string, string> _srtPaths
        = new()
        {
            ["Video/End.mp4"] = "End.mp4.srt"
        };

    private static Action _resetListener;

    internal static void OnVideoPlayerInit(VideoPlayerWnd vpw)
    {
        var bg = vpw.mGameObject.transform.Find("BG");
        var textRef = bg.Find("Text").GetComponent<Text>();

        var goSt = new GameObject("Subtitle");
        var rtSt = goSt.AddComponent<RectTransform>();
        goSt.AddComponent<CanvasRenderer>();

        var textSt = goSt.AddComponent<Text>();
        textSt.font = textRef.font;
        textSt.fontSize = textRef.fontSize;
        textSt.material = textRef.material;
        textSt.alignment = TextAnchor.MiddleCenter;
        textSt.horizontalOverflow = HorizontalWrapMode.Overflow;

        var subtitle = goSt.AddComponent<SubtitlesUGUI>();
        _resetListener = () =>
        {
            subtitle.Text = textSt;
            subtitle.Player = vpw.mMediaPlayer;
        };

        goSt.transform.SetParent(bg, false);
        rtSt.localPosition -= new Vector3(0f, 285f, 0f);
    }

    internal static void SetSrt(VideoPlayerWnd vpw, string videoPath)
    {
        if (!_srtPaths.TryGetValue(videoPath, out var srtPath))
        {
            return;
        }
        if (_resetListener == null)
        {
            throw new InvalidOperationException(
                $"{nameof(SubtitleManager)} not initialized.");
        }

        var fullSrtPath = Path.Combine(
            PathProvider.PathDef.PluginRootPath, srtPath);
        if (!File.Exists(fullSrtPath))
        {
            Plugin.Log.LogError($"\"{fullSrtPath}\" does not exist.");
            return;
        }

        _resetListener?.Invoke();

        var player = vpw.mMediaPlayer;
        player.SideloadSubtitles = true;
        player.EnableSubtitles(
            new(fullSrtPath, MediaPathType.AbsolutePathOrURL));
    }

    internal static void ClearSrt(VideoPlayerWnd vpw)
        => vpw.mMediaPlayer.DisableSubtitles();
}
