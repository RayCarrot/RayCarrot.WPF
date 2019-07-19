using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.WPF
{
    /// <summary>
    /// The behavior to use for <see cref="UserLevelTag"/>
    /// </summary>
    public enum UserLevelTagBehavior
    {
        /// <summary>
        /// The element should be collapsed when not meeting the <see cref="UserLevel"/> requirement
        /// </summary>
        Collapse = 0,

        /// <summary>
        /// The element should be disabled when not meeting the <see cref="UserLevel"/> requirement
        /// </summary>
        Disable = 1
    }
}