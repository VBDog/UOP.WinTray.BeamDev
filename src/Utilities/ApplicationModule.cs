using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using Unity;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Interfaces;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// Singleton class to hold the _Container, register and resolve the types to/from the _Container.
    /// </summary>
    public class ApplicationModule
    {
        #region Variables

        private readonly IUnityContainer _Container;
        private static readonly ApplicationModule _ApplicationModule = new();
   
    
        #endregion

        #region Properties

        public static ApplicationModule Instance
        {
            get
            {
                return _ApplicationModule;
            }
        }

        #endregion Properties

        #region Constructor

        private ApplicationModule()
        {
            _Container = new UnityContainer();
             _Container.RegisterSingleton<IDialogService, DialogService>();
            _Container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _Container.RegisterType<IMaterialHelper, MaterialHelper>();
            _Container.RegisterType<IReportWriter, ReportWriter>();
      
         
            _Container.RegisterInstance(System.Threading.SynchronizationContext.Current);
         
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Resolve method with Type as Parameter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return _Container.Resolve(type);
        }

        /// <summary>
        /// Resolve method with Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return (T)_Container.Resolve(typeof(T));
        }

        /// <summary>
        /// Resolve method with parameter override option
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterOverride"></param>
        /// <returns></returns>
        public T Resolve<T>(Unity.Resolution.ParameterOverride parameterOverride)
        {
            return (T)_Container.Resolve(typeof(T), parameterOverride);
        }

        /// <summary>
        /// Resolve method with parameter override option
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterOverride"></param>
        /// <returns></returns>
        public T Resolve<T>(Unity.Resolution.ResolverOverride[] resolverOverride)
        {
            return (T)_Container.Resolve(typeof(T), resolverOverride);
        }

        #endregion
    }
}
