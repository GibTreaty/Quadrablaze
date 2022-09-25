using UnityEngine;
using UnityEngine.UI;

namespace UIWidgets
{
	/// <summary>
	/// AudioPlayer.
	/// Play single AudioClip.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class AudioPlayer : MonoBehaviour
	{
		[SerializeField]
		Slider progress;
		
		[SerializeField]
		Button playButton;
		
		[SerializeField]
		Button pauseButton;
		
		[SerializeField]
		Button stopButton;
		
		[SerializeField]
		Button toggleButton;
		
		AudioSource source;
		
		/// <summary>
		/// AudioSource to play AudioClip.
		/// </summary>
		public AudioSource Source {
			get {
				if (source==null)
				{
					source = GetComponent<AudioSource>();
				}
				return source;
			}
		}

		bool isStarted = false;

		/// <summary>
		/// Init AudioPlayer and attach listeners.
		/// </summary>
		public virtual void Start()
		{
			if (isStarted)
			{
				return ;
			}
			isStarted = true;

			if (playButton!=null)
			{
				playButton.onClick.AddListener(Play);
			}
			if (pauseButton!=null)
			{
				pauseButton.onClick.AddListener(Pause);
			}
			if (stopButton!=null)
			{
				stopButton.onClick.AddListener(Stop);
			}
			if (toggleButton!=null)
			{
				toggleButton.onClick.AddListener(Toggle);
			}

			if (progress!=null)
			{
				progress.onValueChanged.AddListener(SetCurrentTimeSample);
			}

		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (playButton!=null)
			{
				playButton.onClick.RemoveListener(Play);
			}
			if (pauseButton!=null)
			{
				pauseButton.onClick.RemoveListener(Pause);
			}
			if (stopButton!=null)
			{
				stopButton.onClick.RemoveListener(Stop);
			}
			if (toggleButton!=null)
			{
				toggleButton.onClick.RemoveListener(Toggle);
			}
			if (progress!=null)
			{
				progress.onValueChanged.RemoveListener(SetCurrentTimeSample);
			}
		}

		/// <summary>
		/// Set playback position in seconds.
		/// </summary>
		/// <param name="time">Playback position in seconds.</param>
		public virtual void SetTime(float time)
		{
			if (Source.clip!=null)
			{
				Source.time = time;
			}
		}

		/// <summary>
		/// Set playback position in PCM samples.
		/// </summary>
		/// <param name="timesample">Playback position in PCM samples.</param>
		protected virtual void SetCurrentTimeSample(float timesample)
		{
			SetCurrentTimeSample(Mathf.RoundToInt(timesample));
		}

		/// <summary>
		/// Set playback position in PCM samples.
		/// </summary>
		/// <param name="timesample">Playback position in PCM samples.</param>
		public virtual void SetCurrentTimeSample(int timesample)
		{
			if (Source.clip!=null)
			{
				Source.timeSamples = timesample;
			}
		}

		/// <summary>
		/// Set AudioClip to play.
		/// </summary>
		/// <param name="clip">AudioClip to play.</param>
		public virtual void SetAudioClip(AudioClip clip)
		{
			if (Source.isPlaying)
			{
				Source.Stop();
				Source.clip = clip;
				Source.Play();
			}
			else
			{
				Source.clip = clip;
			}
			Update();
		}

		/// <summary>
		/// Play specified AudioClip.
		/// </summary>
		/// <param name="clip">AudioClip to play.</param>
		public virtual void Play(AudioClip clip)
		{
			Source.Stop();
			Source.clip = clip;
			Source.Play();
		}

		/// <summary>
		/// Play current AudioClip.
		/// </summary>
		public virtual void Play()
		{
			Source.Play();
			Update();
		}

		/// <summary>
		/// Pauses playing current AudioClip.
		/// </summary>
		public virtual void Pause()
		{
			Source.Pause();
			Update();
		}

		/// <summary>
		/// Stops playing current AudioClip.
		/// </summary>
		public virtual void Stop()
		{
			Source.Stop();
			Update();
		}

		/// <summary>
		/// Pauses current AudioClip, if it's playing, otherwise unpaused.
		/// </summary>
		public virtual void Toggle()
		{
			if (Source.isPlaying)
			{
				Source.Pause();
			}
			else
			{
				Source.Play();
			}
			Update();
		}

		/// <summary>
		/// Update buttons state and playing progress.
		/// </summary>
		protected virtual void Update()
		{
			if (playButton!=null)
			{
				playButton.gameObject.SetActive(!Source.isPlaying);
			}
			if (pauseButton!=null)
			{
				pauseButton.gameObject.SetActive(Source.isPlaying);
			}
			if (stopButton!=null)
			{
				stopButton.gameObject.SetActive(Source.isPlaying);
			}

			if (progress!=null)
			{
				progress.wholeNumbers = true;
				progress.minValue = 0;
				if (Source.clip!=null)
				{
					progress.maxValue = Source.clip.samples;
					progress.value = Source.timeSamples;
				}
				else
				{
					progress.maxValue = 100;
					progress.value = 0;
				}
			}
		}
	}
}