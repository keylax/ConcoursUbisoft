using UnityEditor;
using UnityEngine;

namespace Audio
{
    [System.Serializable]
    public class VoiceLine : System.Object
    {
        public string subtitles = "";
        public AudioClip line = null;
        public int importance = 3;
    }
}