using System;
using UnityEngine;
public static class Game {
    //C# mod is not too useful. This one acts identically to the python one (and the math one)
    public static int mod(int a, int n) {
        return ((a % n) + n) % n;
    }
    public static Action<string> elapsedTimeLogger(string logname) {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        return (message) => {
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;
            Debug.Log(String.Format("{0}: {1} took {2} millis",logname,message,elapsed));
            watch.Restart();
        };
    }
}