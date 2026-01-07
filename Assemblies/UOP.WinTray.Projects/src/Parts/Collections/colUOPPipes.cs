using System;
using System.Collections.Generic;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Structures;

/// <summary>
/// A collection of uopPipes
///a simple collection class with some extended adding and retrieving functionality
/// </summary>
public class colUOPPipes :ICloneable
{

    private TPIPES tStruc;

    #region Constructors

    public colUOPPipes() { tStruc = new TPIPES(); }

    internal colUOPPipes(TPIPES aStructure) { tStruc = aStructure; }


    #endregion

    public colUOPPipes Clone() => new colUOPPipes(tStruc.Clone());

    object ICloneable.Clone() => (object)this.Clone();

    internal TPIPES Structure { get =>  tStruc; set => tStruc = value; }

    /// <summary>
    ///#1the item to add to the collection
    ///^used to add an item to the collection
    ///~won't add "Nothing" (no error raised).
    /// </summary>
    /// <param name="aPipe"></param>
    public void Add(uopPipe aPipe) { if (aPipe == null) tStruc.Add(aPipe.Structure); }


    /// <summary>
    /// Collection Count
    /// </summary>
    /// <returns>returns the number of items in the collection</returns>
    public int Count() => tStruc.Count;
    
    /// <summary>
    /// #1the nozzle name to search for
    /// </summary>
    /// <param name="aName"></param>
    /// <returns>returns the nozzle whose name matches the passed string</returns>
    public uopPipe GetPipeByName(string aName)
    {
        TPIPE aMem = tStruc.GetPipeByName(aName);
        return (aMem.Index <= 0) ? null : new uopPipe(aMem);
    }
    /// <summary>
    /// 1the nozzle descriptor to search for
    /// </summary>
    /// <param name="aDescriptor"></param>
    /// <returns>returns the nozzle whose descriptor matches the passed string</returns>
    public uopPipe GetByDescriptor(string aDescriptor)
    {
        TPIPE aMem = tStruc.GetByDescriptor(aDescriptor);
        return (aMem.Index <= 0) ? null : new uopPipe(aMem);
    }

    //#1the nozzle descriptor to search for
    //^returns the nozzle whose descriptor matches the passed string
    public uopPipe GetBySizeDescriptor(string aDescriptor)
    {
        TPIPE aMem = tStruc.GetBySizeDescriptor(aDescriptor);
        return (aMem.Index <= 0) ? null : new uopPipe(aMem);

    }

    //#1the nozzle size to search for
    //#1the nozzle schedule to search for
    //^returns the nozzle whose descriptor matches the passed string
    public uopPipe GetBySizeAndSchedule(string aSize, string aSchedule)
    {
        TPIPE aMem = tStruc.GetBySizeAndSchedule(aSize,aSchedule);
        return (aMem.Index <= 0) ? null : new uopPipe(aMem);

    }
    /// <summary>
    ///returns the unique schedules available in the collection
    /// </summary>
    /// <returns></returns>
    public List<string> GetSchedules() => tStruc.GetSchedules();

    public colUOPPipes GetSubSet(string aSchedule, string aSpecName = "") => new colUOPPipes(tStruc.GetSubSet(aSchedule, aSpecName));
    
   
    /// <summary>
    /// Item
    /// </summary>
    /// <param name="aIndex"></param>
    /// <returns>returns the item from the collection at the requested index</returns>
    public uopPipe Item(int aIndex) => (aIndex > 0 & aIndex <= tStruc.Count) ? new uopPipe(tStruc.Item(aIndex)) : null;
   
}