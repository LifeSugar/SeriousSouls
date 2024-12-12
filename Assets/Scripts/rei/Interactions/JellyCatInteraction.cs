namespace rei
{
    public class JellyCatInteraction : WorldInteraction
    {
        void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }
        public override void InteractActual()
        {
            InputHandler.instance._playerStates.powered = true;
            InputHandler.instance._playerStates.anim.Play("taking");
            UIManager.instance.OpenInteractionInfoCanvas("Gained An Inexplicable Strength In My Heart.");
        }

        void OnDestroy()
        {
            PickableItemsManager.instance.interactions.Remove(this);
        }
    }
}