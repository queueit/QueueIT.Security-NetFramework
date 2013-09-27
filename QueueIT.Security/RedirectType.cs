namespace QueueIT.Security
{
    /// <summary>
    /// The way a user has been redirected to the target URL
    /// </summary>
    public enum RedirectType
    {
        /// <summary>
        /// Unable to determine the redirect type
        /// </summary>
        Unknown, 
        /// <summary>
        /// User has been redirected to the target URL by the queue
        /// </summary>
        Queue, 
        /// <summary>
        /// User has been redirected to the target URL by the SafetyNet
        /// </summary>
        Safetynet, 
        /// <summary>
        /// User has been redirected to the target URL after the event has ended
        /// </summary>
        AfterEvent, 
        /// <summary>
        /// User has been redirected to the target URL while the queue was disabled
        /// </summary>
        Disabled, 
        /// <summary>
        /// User has been redirected to the target URL using a direct link and has not been through the queue
        /// </summary>
        DirectLink
    }
}