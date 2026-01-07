
using System;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
/// <summary>
///  UOP Utilities object that is used for inter-object connectivity and awareness
/// Option Explicit
/// </summary>
public class uopEventHandler
{
    //public delegate void ProjectRequestHandler(string aHandle,ref uopProject aProject);
    //public static event ProjectRequestHandler eventProjectRequest;
    //public delegate void RangeRequestHandler(string aGUID, ref uopTrayRange aRange);
    //public static event RangeRequestHandler eventRangeRequest;
    //public delegate void ProjectTerminationHandler(string aProjectHandle);
    //public event ProjectTerminationHandler eventProjectTermination;

    
    public void RegisterProject(uopProject aProject)
    {
        if (aProject == null) return;

        uopGlobals.glProjCount += 1;

        aProject.Index = uopGlobals.glProjCount;

        string prefix = aProject.ProjectType switch
        {
            uppProjectTypes.MDSpout => "MD",
            uppProjectTypes.MDDraw => "MDD",
            uppProjectTypes.CrossFlow => "XF",
            _ => "UK"
        };
        aProject.Handle = prefix + uopGlobals.glProjCount;

    }


    public dynamic RetrieveBeam(string aRangeGUID, uppBeamTypes aBeamType, int aBeamIndex, uopTrayAssembly rAssy = null)
    {
        dynamic RetrieveBeam = null;
        try
        {
            uopProject aProject = null;
            dynamic Panels = null;
            dynamic aPanel = null;
            //xfReceivingPan axfPan = null;

            //xfDowncomer xfDC = null;
              rAssy ??= uopEvents.RetrieveTrayAssembly(aRangeGUID);
            
            if (rAssy == null) return null;

            aProject = rAssy.Project;

        
                switch (aBeamType)
                {
                    case uppBeamTypes.Deck:
                        if (aBeamIndex == 0)
                        {
                            RetrieveBeam = rAssy.DeckBeamObj;
                        }
                        else
                        {
                            RetrieveBeam = rAssy.DeckBeams.Item(mzUtils.VarToInteger(aBeamIndex));
                        }
                        break;
                    case uppBeamTypes.Downcomer:
                        if (aProject.ProjectType == uppProjectTypes.CrossFlow)
                        {
                            //if (aBeamIndex == 0)
                            //{
                            //    xfDC = rAssy.Downcomer;
                            //    RetrieveBeam = xfDC.get_Beam();
                            //}
                            //else
                            //{
                            //    Beams = rAssy.Downcomers;
                            //    if (Beams != null)
                            //    {
                            //        xfDC = Beams.Item(Convert.ToInt32(aBeamIndex));
                            //    }
                            //    if (xfDC != null)
                            //    {
                            //        RetrieveBeam = xfDC.get_Beam();
                            //    }
                            //}
                        }

                        break;
                 
                    case uppBeamTypes.IntegralPan:

                        throw new NotImplementedException();
                        //break;
                        //if (aProject.ProjectType == uppProjectTypes.CrossFlow)
                        //{
                        //    if (aBeamIndex == 0)
                        //    {
                        //        aPan = rAssy.ReceivingPan;
                        //    }
                        //    else
                        //    {
                        //        Pans = rAssy.ReceivingPans;
                        //        if (Pans != null)
                        //        {
                        //            aPan = Pans.Item(Convert.ToInt32(aBeamIndex));
                        //        }
                        //    }
                        //    //if (aPan != null)
                            //{
                            //    axfPan = aPan;
                            //    //RetrieveBeam = axfPan.IntegralBeam;//TODO-Verify IntegralBeam
                            //}

                        //}
                        //break;
                    case uppBeamTypes.IntegralDeck:
                        if (aProject.ProjectType == uppProjectTypes.CrossFlow)
                        {
                            Panels = rAssy.DeckPanelsObj;
                            if (Panels != null)
                            {
                                aPanel = Panels.Item(aBeamIndex);
                            }
                            if (aPanel != null)
                            {
                                RetrieveBeam = aPanel.IntegralBeam;
                            }
                        }
                        break;
              

            }
        }
        catch (Exception)
        {

        }
        return RetrieveBeam;
    }
    /// <summary>
    /// Retrieve Chimney Tray
    /// </summary>
    /// <param name="aProjectHandle"></param>
    /// <param name="aPartIndex"></param>
    /// <returns>returns the indicated chimney tray from the chimney trays collection of the indicated project</returns>
    public static uopPart RetrieveChimneyTray(string aProjectHandle, int aPartIndex)
    {
        if (String.IsNullOrWhiteSpace(aProjectHandle) || aPartIndex <= 0 ) return null;
        uopProject Proj = uopEvents.RetrieveProject(aProjectHandle);
        if (Proj == null) return null;

        uopParts CTrays = Proj.ChimneyTrays;

        if (CTrays == null) return null;
        return (aPartIndex <= CTrays.Count) ? CTrays.Item(aPartIndex) : null;
       
    }
    /// <summary>
    /// Retrieve Chimney Trays
    /// </summary>
    /// <param name="aProjectHandle"></param>
    /// <returns>returns the chimney trays collection of the indicated project</returns>
    public static colMDChimneyTrays RetrieveChimneyTrays(string aProjectHandle)
    {
        uopProject Proj = uopEvents.RetrieveProject(aProjectHandle);

        return Proj?.ChimneyTrays;
    }

    public static uopSheetMetal RetrieveFirstDeckMaterial(string aProjectHandle)
    {
        uopProject Proj = uopEvents.RetrieveProject(aProjectHandle);

        return Proj?.FirstDeckMaterial;
    }


    
    public static uopPart RetrieveReceivingPan(string aProjectHandle, int aColumnIndex, string aRangeGUID, int aPanIndex)
    {
        dynamic _rVal = null;
        dynamic Pans = null;
        uopTrayAssembly rAssy = uopEvents.RetrieveTrayAssembly(aRangeGUID);
        if (rAssy == null) return null;

     
        if (aPanIndex <= 0)
        {
            _rVal = rAssy.ReceivingPan;
        }
        else
        {
            Pans = null; //rAssy.ReceivingPans;
            if (Pans != null)
            {
                _rVal = Pans.Item(Convert.ToDouble(aPanIndex));
            }
        }
        return _rVal;
    }




    public void UnRegisterProject(uopProject aProject)
    {
        if (aProject == null)
        {
            //TODO: Need to implement. Just return null as to continue integrtaion
            return ;
        }
        uopGlobals.glProjCount --;

        //eventProjectTermination?.Invoke(aProject.Handle);
    }
}