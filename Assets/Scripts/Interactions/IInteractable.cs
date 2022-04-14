public interface IInteractable
{
    /// <summary>
    /// Returns the text that will be showed on the UI when this object if targeted.
    /// </summary>
    /// <returns></returns>
    public string GetInteractText();

    /// <summary>
    /// Returns the text that will be showed on the UI when this object can not be interacted with.
    /// </summary>
    /// <returns></returns>
    //public string GetInteractFailedText();
    
    /// <summary>
    /// Called by InteractionManager.cs when the user presses the Interact key.
    /// </summary>
    public void Interact();

    /// <summary>
    /// Can this object be interacted with right now?
    /// </summary>
    /// <returns></returns>
    public bool CanBeInteractedWith();
}