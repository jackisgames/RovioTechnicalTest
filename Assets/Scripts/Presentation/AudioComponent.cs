using UnityEngine;

namespace Assets.Scripts.Presentation
{
	public class AudioComponent : MonoBehaviour
	{
		public AudioSource Source;

		public AudioClip[] Select;
		public AudioClip[] SelectTarget;
		public AudioClip[] Move;
		public AudioClip[] TakeDamage;
		public AudioClip[] Death;
		public AudioClip[] Quake;

		public void PlaySelect()
		{
			var sound = Select[Random.Range(0, Select.Length)];
			PlaySound(sound);
		}

		public void PlaySelectTarget()
		{
			var sound = SelectTarget[Random.Range(0, SelectTarget.Length)];
			PlaySound(sound);
		}

		public void PlayMove()
		{
			var sound = Move[Random.Range(0, Move.Length)];
			PlaySound(sound);
		}

		public void PlayTakeDamage()
		{
			var sound = TakeDamage[Random.Range(0, TakeDamage.Length)];
			PlaySound(sound);
		}

		public void PlayDeath()
		{
			var sound = Death[Random.Range(0, Death.Length)];
			PlaySound(sound);
		}

		public void PlayQuake()
		{
			var sound = Quake[Random.Range(0, Quake.Length)];
			PlaySound(sound);
		}

		private void PlaySound(AudioClip clip)
		{
			Source.PlayOneShot(clip);
		}
	}
}