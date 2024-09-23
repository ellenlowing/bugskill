using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TutorialVideoPlayer : MonoBehaviour
{
    public List<VideoPlayer> VideoPlayers;

    void Start()
    {
        foreach (var videoPlayer in VideoPlayers)
        {
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += source =>
            {
                source.Play();
            };
        }
    }

    public void PlayVideos()
    {
        foreach (var videoPlayer in VideoPlayers)
        {
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
    }

    public void StopVideos()
    {
        foreach (var videoPlayer in VideoPlayers)
        {
            videoPlayer.isLooping = false;
            videoPlayer.Stop();
        }
    }
}
