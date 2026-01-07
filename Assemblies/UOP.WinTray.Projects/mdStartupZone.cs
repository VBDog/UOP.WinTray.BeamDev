using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

namespace UOP.WinTray.Projects
{
    public class mdStartupZone : mdSpoutArea
    {

        #region Constructors

        public mdStartupZone() => Init();

        public mdStartupZone(mdSpoutArea aSpoutArea, mdTrayAssembly aAssy = null)
        {
            Init(aSpoutArea, aAssy);
          
        }


        private void Init(mdSpoutArea aSpoutArea, mdTrayAssembly aAssy = null)
        {
            _ProjectType = uppProjectTypes.MDSpout;
            TrayAssembly = aAssy;   
            if( aSpoutArea != null)
            {
               DefineBySpoutArea(aSpoutArea);

            } 

        }
        
        
        #endregion Constructors

        #region Properties

        private uppProjectTypes _ProjectType;
        public uppProjectTypes ProjectType { get => _ProjectType; set => _ProjectType = value == uppProjectTypes.MDSpout || value == uppProjectTypes.MDDraw ? value : uppProjectTypes.MDSpout ; }

        private WeakReference<mdTrayAssembly> _AssyRef;
        public mdTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out mdTrayAssembly _rVal)) _AssyRef = null;
                return _rVal;
            }
            set
            {

                _AssyRef = value != null ? new WeakReference<mdTrayAssembly>(value) : null;
                if (value != null) 
                { 
                    ProjectType = value.ProjectType;
                }
            }
        }

       
        private mdStartupLine _TopLeftLine;

        public mdStartupLine TopLeftLine { get => _TopLeftLine; set => _TopLeftLine = value; }

        private mdStartupLine _TopRightLine;

        public mdStartupLine TopRighLine { get => _TopRightLine; set => _TopRightLine = value; }

        private mdStartupLine _BottomLeftLine;

        public mdStartupLine BottomLeftLine { get => _BottomLeftLine; set => _BottomLeftLine = value; }

        private mdStartupLine _BottomRightLine;

        public mdStartupLine BottomRighLine { get => _BottomRightLine; set => _BottomRightLine = value; }

        #endregion Properties

        #region Methods

        public void DefineBySpoutArea(mdSpoutArea aArea)
        {
            if (aArea == null) return;
            base.Copy(aArea);
             
        }
        #endregion Methods
    }
}
