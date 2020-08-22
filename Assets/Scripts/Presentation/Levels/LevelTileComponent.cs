using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Presentation.Levels
{
	public class LevelTileComponent : MonoBehaviour
	{
		public GameObject BreadCrumb;

		public void ShowBreadCrumb(bool state, float delay = 0)
		{
			BreadCrumb.gameObject.SetActive(state);

			if (state && !DOTween.IsTweening(BreadCrumb.transform))
			{
				BreadCrumb.transform.DOJump(BreadCrumb.transform.position, 0.15f, 1, 0.175f).SetDelay(delay);
			}
		}
	}
}