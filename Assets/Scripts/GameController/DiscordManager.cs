using System.Collections;
using UnityEngine;
using System;

public class DiscordManager : MonoBehaviour
{
    public Discord.Discord discord;
    public Discord.ActivityManager activityManager;

    DateTime _start;

    void Start()
    {
        print(DateTime.Today.Ticks);

        // Check if in Editor, if true, stop this script
        if (Application.isEditor)
        {
            this.enabled = false;
            return;
        }   
        
        // Setting up Vars  
        _start = DateTime.Now;
        discord = new Discord.Discord(1018838439396900894, (System.UInt64)Discord.CreateFlags.Default);
        activityManager = discord.GetActivityManager();

        // Start Loop
        StartCoroutine(UpdateLoop());
    }

    void LateUpdate()
    {
        discord.RunCallbacks();   
    }

    IEnumerator UpdateLoop()
    {
        try
        {
            // Update Discord Status
            TimeSpan elapsed = DateTime.Now - _start;

            var activity = new Discord.Activity
            {
                Details = "Playing for " + elapsed.TotalMinutes.ToString("0.0") + " Minutes",
                State = "In Match ",
                Assets = {
                    LargeImage = "gameicon",
                },
                Instance = true,
            };
            activity.ApplicationId = 1018838439396900894;

            activityManager.UpdateActivity(activity, (res) => {
                if (res == Discord.Result.Ok)
                    Debug.Log("Discord Status Set");
                else
                    Debug.LogError("Discord Status failed");
            });           
        }
        catch { Debug.LogError("Error with Discord Status Update!"); }

        yield return new WaitForSecondsRealtime(6f);
        StartCoroutine(UpdateLoop());
    }
}
