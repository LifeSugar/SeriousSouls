using DG.Tweening;
using UnityEngine;

namespace rei
{
    public class SwitchButton : WorldInteraction
    {
        void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }
        
        public GameObject lift;

        public override void InteractActual()
        {
            Vector3 targetPosition = new Vector3(lift.transform.localPosition.x, -2.3f, lift.transform.localPosition.z);

            lift.transform.DOLocalMove(targetPosition, 1.0f).OnComplete(()=>UIManager.instance.OpenInteractionInfoCanvas("Something happened"));
        }
    }
}