using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// simple object used to carry customer info for a project
    /// </summary>
    public class uopCustomer : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Customer;
      


        public delegate void CustomerPropertyChange(uopProperty aProperty);
        public event CustomerPropertyChange eventCustomerPropertyChange;

        #region Constructors


        public uopCustomer() :base(uppPartTypes.Customer, uppProjectFamilies.uopFamMD, "","",false)
        {
            InitializeProperties();
        }


        internal uopCustomer(uopCustomer aPartToCopy) : base(uppPartTypes.Customer, uppProjectFamilies.uopFamMD, "", "", false)
        {
            if (aPartToCopy == null) InitializeProperties(); else Copy(aPartToCopy);
        }

        private void InitializeProperties()
        {
            base.ActiveProps = new TPROPERTIES();  
            AddProperty("Name", "", aDisplayName:"Customer");
            AddProperty("Service", "");
            AddProperty("Location", "");
            AddProperty("Item", "");
            AddProperty("PO", "", aDisplayName: "PO No.");
            AddProperty("ForExport", true);
            
        }


        #endregion


        /// <summary>
        ///returns the objects properties in a collection
        /// </summary>
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        public override void UpdatePartProperties() { }

        /// <summary>
        /// flag indicating if the shipping product to the customer represents a US export
        /// </summary>
        public bool ForExport { get => PropValB("ForExport"); set => Notify(PropValSet("ForExport", value));  }

        /// <summary>
        /// the item number string assigned to the customer
        /// </summary>
        public string Item { get => PropValS("Item"); set => Notify(PropValSet("Item", value.Trim())); }

        /// <summary>
        /// the location string assigned to the customer
        /// </summary>
        public string Location { get => PropValS("Location"); set => Notify(PropValSet("Location", value.Trim())); }

        /// <summary>
        /// the name string assigned to the customer
        /// </summary>
        public override string Name { get => PropValS("Name");  set => Notify(PropValSet("Name", value.Trim())); }

        /// <summary>
        /// the PO number string assigned to the customer
        /// </summary>
        public string PO { get => PropValS("PO"); set => Notify(PropValSet("PO", value.Trim())); }


        public override string INIPath => "CUSTOMER";
        
        /// <summary>
        ///returns the properties required to save the object to file
        /// signatures like "COLOR=RED"
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
            return base.SaveProperties(aHeading);
        }

        /// <summary>
        /// the service string assigned to the customer
        /// </summary>
        public string Service { get => PropValS("Service"); set => Notify(PropValSet("Service", value.Trim())); }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        public uopCustomer Clone() => new uopCustomer(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();
        

        /// <summary>
        /// the file name to read the customer properties from
        /// reads the properties of the customer from a text file in INI file format
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aFileSpec"></param>
        /// <param name="aFileVersion_UNUSED"></param>
        /// <param name="aFileSection"></param>
        public void ReadProperties(uopProject aProject, string aFileSpec, double aFileVersion, string aFileSection = "Customer")
        {

            try
            {
                if (aProject != null) aProject.ReadStatus("Reading Customer Properties", 2);

                Reading = true;
              
                PropValSet("ForExport", uopUtils.ReadINI_Boolean(aFileSpec, aFileSection, "ForExport"));
                PropValSet("Name", uopUtils.ReadINI_String(aFileSpec, aFileSection, "Name"));
                PropValSet("Location", uopUtils.ReadINI_String(aFileSpec, aFileSection, "Location"));
                PropValSet("Item", uopUtils.ReadINI_String(aFileSpec, aFileSection, "Item"));
                PropValSet("PO", uopUtils.ReadINI_String(aFileSpec, aFileSection, "PO"));
                PropValSet("Service", uopUtils.ReadINI_String(aFileSpec, aFileSection, "Service"));
                Reading = false;
                if (aProject != null)  aProject.ReadStatus("", 2);

            
            }
            catch (Exception e) { throw new Exception("[uopCustomer.ReadProperties] " + e.Message); }
            finally { Reading = false; }

         
        }

       

        public override void UpdatePartWeight() => base.Weight = 0;
        

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((uopCustomer)aPart);
        }

        public bool IsEqual(uopCustomer aCustomer)
        {
            if (aCustomer == null) return false;
            return aCustomer.Properties.IsEqual(Properties);
        }

      

        private void Notify(uopProperty aProperty)
        {
            if (aProperty != null)  aProperty.ProjectHandle = ProjectHandle;

            eventCustomerPropertyChange.Invoke(aProperty);
        }


    }
}