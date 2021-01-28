
using System;
namespace MacacaGames.GameSystem
{
    /// <summary>
    /// Mark a Property or Field inside a class that can be Injected by ApplicationController
    /// Remember the member needs to be accessable to make the Injection work.
    /// </summary>
    /// <example>
    /// In the case while trying to inject ChildClass the inject will has below result 
    /// <code>
    /// ChildClass childClass = new ChildClass();
    /// ApplicationController.Instance.ResolveInjection(childClass);
    ///     
    /// Result:
    /// privateItem => noValue
    /// protectedItem => hasValue
    /// publicItem => hasValue
    /// 
    /// BaseClass baseClass = new BaseClass();
    /// ApplicationController.Instance.ResolveInjection(baseClass);
    /// 
    /// Result:
    /// privateItem => hasValue
    /// protectedItem => hasValue
    /// publicItem => hasValue
    /// 
    /// class ChildClass : BaseClass{
    ///     
    /// }
    /// class BaseClass {
    ///     privete SomeClass privateItem;
    ///     protected SomeClass protectedItem;
    ///     public SomeClass publicItem;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : Attribute { }

    /// <summary>
    /// Mark a class the can be inject by ApplicationController inject system
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResolveTargetAttribute : Attribute
    {
        /// <summary>
        /// Mark a class the can be inject by ApplicationController inject system
        /// </summary>
        /// <param name="order">The order, smaller is earier</param>
        public ResolveTargetAttribute(int order)
        {
            this.order = order;
        }

        /// <summary>
        /// The order to CreateInstance and inject
        /// </summary>
        public int order;
    }
}