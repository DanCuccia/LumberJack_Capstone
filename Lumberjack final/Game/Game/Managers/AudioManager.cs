using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace Game.Managers
{
    /// <summary>A Single soundclip that is currently audible</summary>
    public class SoundClip
    {
        public string name;
        public bool is3D = true;
        public SoundEffectInstance soundInstance;
        public AudioEmitter emitter = new AudioEmitter();
    }

    /// <summary>The main 3D audio managing class,
    /// note: much of this is taken from the ms example,
    /// is a singleton, created once, accessable anywhere</summary>
    public class AudioManager
    {
        private static AudioManager myInstance;
        public static AudioManager getInstance()
        {
            if (myInstance == null)
                myInstance = new AudioManager();
            return myInstance;
        }


        #region Member Variables

        ContentManager content; //ref

        AudioListener listener;
        //AudioEmitter emitter;

        Dictionary<string, SoundEffect> soundList;
        List<SoundClip> activeSounds;

        List<string> musicIdList;
        const int musicWaitTime = 210000; //3.5min
        int currentMusicTime = 0;
        short trackIndex = 0;
        bool musicIsPlaying = false;

        // --- hardcodes ---
        public const float BtnVolume = .7f;
        public const float WindVolume = .2f;
        public const float BirdsVolume = .05f;
        public const float MusicVolume = .6f;
        public const float PlingVolume = .5f;
        public const float RiverVolume = .85f;
        public const float DammedRiverVolume = .65f;
        public const float WaterfallVolume = .7f;
        // ---

        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR - private, is singleton</summary>
        private AudioManager()
        { }

        /// <summary>Initialize members</summary>
        public void Initialize(ContentManager content)
        {
            this.content = content;

            SoundEffect.DistanceScale = 2500;
            SoundEffect.DopplerScale = 0.15f;

            listener = new AudioListener();
            //emitter = new AudioEmitter();

            musicIdList = new List<string>();
            soundList = new Dictionary<string, SoundEffect>();
            activeSounds = new List<SoundClip>();

            loadBtnSounds();
            loadMusicIDList();

            currentMusicTime = 0;
        }

        private void loadMusicIDList()
        {
            musicIdList.Add("audio\\Coronation");
            musicIdList.Add("audio\\Saint_Louis_Blues");
            musicIdList.Add("audio\\Bridge_to_Bama");
        }

        private void loadBtnSounds()
        {
            SoundEffect snd = content.Load<SoundEffect>("audio\\btn_up");
            soundList.Add("btn_up", snd);
            snd = content.Load<SoundEffect>("audio\\btn_down");
            soundList.Add("btn_down", snd);
        }

        /// <summary>clear out any loaded content</summary>
        public void ClearAll()
        {
            foreach (SoundClip clip in activeSounds)
                clip.soundInstance.Stop(true);
            soundList.Clear();

            loadBtnSounds();
        }

        /// <summary>Load content into the manager, ready to be played</summary>
        /// <param name="filepath">where the audio file is loacted</param>
        /// <param name="idName">the id name of your loaded audio file</param>
        public void LoadSound(string filepath, string idName)
        {
            SoundEffect snd = content.Load<SoundEffect>(filepath);
            if (snd != null)
                soundList.Add(idName, snd);
        }

        #endregion Initialization


        #region API

        /// <summary>Main update call</summary>
        public void Update(GameTime time)
        {
            //active sounds
            int index = 0;
            List<int> remInd = new List<int>();
            foreach (SoundClip sound in activeSounds)
            {
                if (sound.soundInstance.State == SoundState.Stopped)
                {
                //    if (sound.soundInstance.IsLooped)
                //    {
                //        sound.soundInstance.Play();
                //    }
                //    else
                //    {
                        remInd.Add(index);
                    //}

                    if (sound.name == "music")
                    {
                        soundList.Remove("music");
                        musicIsPlaying = false;
                        currentMusicTime = 0;
                    }
                }
                else
                {
                    if (sound.is3D)
                    {
                        edit3DSound(sound);
                    }
                }
                index++;
            }
            for (int i = remInd.Count - 1; i >= 0; i--)
            {
                activeSounds.RemoveAt(i);
            }

            //music update
            currentMusicTime += time.ElapsedGameTime.Milliseconds;
            if (currentMusicTime >= AudioManager.musicWaitTime)
            {
                currentMusicTime = 0;
                if (musicIsPlaying == false)
                {
                    SoundEffect music = content.Load<SoundEffect>(musicIdList[trackIndex]);
                    soundList.Add("music", music);

                    musicIsPlaying = true;

                    SoundClip track = this.Play2DSound("music", false);
                    track.soundInstance.Volume = AudioManager.MusicVolume;

                    trackIndex++;
                    if (trackIndex >= musicIdList.Count)
                    {
                        trackIndex = 0;
                    }
                }
            }
        }

        /// <summary>The main play a sound call</summary>
        /// <param name="soundName">id in which you gave the sound during load</param>
        /// <param name="looping">where you want it to loop or not</param>
        /// <returns>a pointer to the playing sound instance</returns>
        public SoundClip Play3DSound(string soundName, Vector3 position, bool looping = true)
        {
            if (soundName == "btn_down" || soundName == "btn_up")
                return playBtnSound(soundName);

            SoundClip clip = new SoundClip();
            clip.soundInstance = soundList[soundName].CreateInstance();
            clip.soundInstance.IsLooped = looping;
            clip.emitter.Position = position;
            clip.name = soundName;
            clip.is3D = true;

            edit3DSound(clip);

            clip.soundInstance.Play();
            activeSounds.Add(clip);

            return clip;
        }

        /// <summary>play an audio clip, without being effected by 3D space</summary>
        /// <param name="soundName">name of the clip you wish to play</param>
        /// <param name="looping">looping or not</param>
        /// <returns>the new running SoundClip, you may edit this</returns>
        public SoundClip Play2DSound(string soundName, bool looping = true)
        {
            if (soundName == "btn_down" || soundName == "btn_up")
                return playBtnSound(soundName);

            SoundClip clip = new SoundClip();
            clip.soundInstance = soundList[soundName].CreateInstance();
            clip.soundInstance.IsLooped = looping;
            clip.name = soundName;
            clip.is3D = false;

            clip.soundInstance.Play();
            activeSounds.Add(clip);

            return clip;
        }

        /// <summary>test if a sound is playing</summary>
        /// <param name="soundName">name of the sound to test</param>
        /// <returns>yay/nay is playing</returns>
        public bool IsPlaying(string soundName)
        {
            foreach (SoundClip clip in activeSounds)
            {
                if (clip.name == soundName)
                    return clip.soundInstance.State == SoundState.Playing;
            }
            return false;
        }

        private SoundClip playBtnSound(string soundName)
        {
            SoundClip clip = null;
            if (IsPlaying(soundName))
                return null;
            else
            {
                clip = new SoundClip();
                clip.soundInstance = soundList[soundName].CreateInstance();
                clip.soundInstance.Volume = AudioManager.BtnVolume;
                clip.soundInstance.IsLooped = false;
                //clip.emitter = this.emitter;
                clip.name = soundName;
                clip.is3D = false;

                clip.soundInstance.Play();
                activeSounds.Add(clip);
            }

            return clip;
        }

        /// <summary>apply 3D effects to any playing sounds</summary>
        private void edit3DSound(SoundClip clip)
        {
            //this.emitter.Position = clip.emitter.Position;
            //this.emitter.Forward = clip.emitter.Forward;
            //this.emitter.Up = clip.emitter.Up;
            //this.emitter.Velocity = clip.emitter.Velocity;
            clip.soundInstance.Apply3D(listener, clip.emitter);
        }

        /// <summary>stop all sounds from playing</summary>
        public void StopAll()
        {
            foreach (SoundClip clip in this.activeSounds)
            {
                clip.soundInstance.Stop();
                clip.soundInstance.Dispose();
            }
            activeSounds.Clear();
        }

        /// <summary>Stop and remove a single clip currently playing</summary>
        /// <param name="name">the name of the playing clip you wish to stop</param>
        public void StopSound(string name)
        {
            int index = 0;
            bool testTrue = false;
            foreach (SoundClip clip in activeSounds)
            {
                if (clip.name == name)
                {
                    clip.soundInstance.Stop();
                    testTrue = true;
                    break;
                }
                index++;
            }
            if (testTrue)
            {
                activeSounds.RemoveAt(index);
            }
        }

        /// <summary>
        /// retreive a sound clip from the active sounds list
        /// </summary>
        /// <param name="name">nane of the clip you want to receive</param>
        /// <returns>the playing soundClip, or null</returns>
        public SoundClip GetSoundClip(string name)
        {
            SoundClip output = null;

            foreach (SoundClip clip in activeSounds)
            {
                if (clip.name == name)
                {
                    output = clip;
                    break;
                }
            }

            return output;
        }

        #endregion API


        #region Mutators

        public AudioListener Listener
        {
            get { return this.listener; }
        }

        #endregion Mutators
    }
}
