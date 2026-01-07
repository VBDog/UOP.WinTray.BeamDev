using Honeywell.UOP.WinTray.API.Calculations.Classes;
using Honeywell.UOP.WinTray.API.Calculations.Enums;
using Honeywell.UOP.WinTray.BusinessLogic;
using Honeywell.UOP.WinTray.Events.EventAggregator;
using Honeywell.UOP.WinTray.Messages;
using Honeywell.UOP.WinTray.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Honeywell.UOP.WinTray.Utilities.Constants.CommonConstants;

namespace Honeywell.UOP.WinTray.ViewModels
{
    /// <summary>
    /// View model for TrayInfo Properties
    /// </summary>
    public class TrayInfoViewModel : MDSpoutViewModelBase,                                    
                                        IEventSubscriber<SelectedTrayRangeMessageNew>, 
                                        IEventSubscriber<EditOperationCompletedMessage>, 
                                        IEventSubscriber<UnitConversionRequestMessage>
    {
        #region Constructors
        private readonly IStorageHelper<mdProject> storageHelper;
        public TrayInfoViewModel(IStorageHelper<mdProject> storageHelper, IPropertyHelper propertyService, IEventAggregator eventAggregator) : base(propertyService, eventAggregator)
        {
            this.storageHelper = storageHelper;
        }

        #endregion Constructors

        #region Event Handlers
  

        /// <summary>
        /// Event handler called when the user changes from onr Tray Range to another  
        /// </summary>
        /// <param name="selectedTrayRangeMessageNew"></param>
        public void OnAggregateEvent(SelectedTrayRangeMessageNew selectedTrayRangeMessageNew)
        {
            this.Properties = new ObservableCollection<WTProperty>(selectedTrayRangeMessageNew.Project.TrayInfoProps);
        }
        /// <summary>
        /// Event handler called when edit operation completed message
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(EditOperationCompletedMessage editOperationCompletedMessage)
        {
            this.Properties = new ObservableCollection<WTProperty>(ChangeUnits(storageHelper.ConstructOrGetMDSpoutProject(storageHelper.SelectedTrayRange.UniqueId).TrayInfoProps));
        }
        /// <summary>
        /// Event handler called when the unit conversion is requested
        /// </summary>
        /// <param name="message"></param>
        public override void OnAggregateEvent(UnitConversionRequestMessage message)
        {
            this.Properties = new ObservableCollection<WTProperty>(ChangeUnits(storageHelper.ConstructOrGetMDSpoutProject(storageHelper.SelectedTrayRange.UniqueId).TrayInfoProps));
        }

        #endregion

        #region Methods
        private List<WTProperty> ChangeUnits(List<WTProperty> properties)
        {
            List<WTProperty> resultantProps = new List<WTProperty>();
            if (storageHelper.MainObject.DisplayUnits == API.Calculations.Enums.uppUnitFamilies.uopEnglish)
            {
                resultantProps.Add(new WTProperty(FBA, FBA, (storageHelper.MainObject.SelectedRange.TrayAssembly.TotalFreeBubblingArea / FTSQUARECOFF).ToString(FORMAT_F3) + FT_SQUARE));
                resultantProps.Add(new WTProperty(ACTIVEAREA, ACTIVEAREA, (storageHelper.MainObject.SelectedRange.TrayAssembly.FunctionalActiveArea / FTSQUARECOFF).ToString(FORMAT_F3) + FT_SQUARE));
                resultantProps.Add(new WTProperty(TOTWEIRLEN, TOTWEIRLEN, (storageHelper.SelectedTrayRange.TrayAssembly.Downcomers.TotalWeirLength / INCHTOFEET).ToString(FORMAT_F3) + SPACE + FEET));
                resultantProps.Add(new WTProperty(TOTALAREA, TOTALAREA, (storageHelper.SelectedTrayRange.TrayAssembly.Downcomers.TotalBottomArea / FTSQUARECOFF).ToString(FORMAT_F3) + SPACE + FT_SQUARE));
                resultantProps.Add(new WTProperty(TOTAL_SPOUTAREA_IDEAl, TOTAL_SPOUTAREA_IDEAl, storageHelper.SelectedTrayRange.TrayAssembly.TheoreticalSpoutArea.ToString(FORMAT_F3) + SPACE + INCH_SQUARE));
                resultantProps.Add(new WTProperty(TOTAL_SPOUTAREA_ACTUAL, TOTAL_SPOUTAREA_ACTUAL, storageHelper.SelectedTrayRange.TrayAssembly.TotalSpoutArea.ToString(FORMAT_F3) + SPACE + INCH_SQUARE));
            }
            else if (storageHelper.MainObject.DisplayUnits == API.Calculations.Enums.uppUnitFamilies.uopMetric)
            {
                resultantProps.Add(new WTProperty(FBA, FBA, (storageHelper.MainObject.SelectedRange.TrayAssembly.TotalFreeBubblingArea * METRICAREACOFF).ToString(FORMAT_F3) + MT_SQUARE));
                resultantProps.Add(new WTProperty(ACTIVEAREA, ACTIVEAREA, (storageHelper.MainObject.SelectedRange.TrayAssembly.FunctionalActiveArea * METRICAREACOFF).ToString(FORMAT_F3) + MT_SQUARE));
                resultantProps.Add(new WTProperty(TOTWEIRLEN, TOTWEIRLEN, (storageHelper.SelectedTrayRange.TrayAssembly.Downcomers.TotalWeirLength * INCHTOMT).ToString(FORMAT_F3) + SPACE + METER));
                resultantProps.Add(new WTProperty(TOTALAREA, TOTALAREA, (storageHelper.SelectedTrayRange.TrayAssembly.Downcomers.TotalBottomArea * METRICAREACOFF).ToString(FORMAT_F3) + SPACE + MT_SQUARE));
                resultantProps.Add(new WTProperty(TOTAL_SPOUTAREA_IDEAl, TOTAL_SPOUTAREA_IDEAl, (storageHelper.SelectedTrayRange.TrayAssembly.TheoreticalSpoutArea * Math.Pow(AREACOFFIFUNITSISMETRIC, 2)).ToString(FORMAT_F3) + SPACE + CENTIMETER_SQUARE));
                resultantProps.Add(new WTProperty(TOTAL_SPOUTAREA_ACTUAL, TOTAL_SPOUTAREA_ACTUAL, (storageHelper.SelectedTrayRange.TrayAssembly.TotalSpoutArea * Math.Pow(AREACOFFIFUNITSISMETRIC, 2)).ToString(FORMAT_F3) + SPACE + CENTIMETER_SQUARE));
            }
            string color = storageHelper.SelectedTrayRange.TrayAssembly.ErrorPercentage >= storageHelper.SelectedTrayRange.TrayAssembly.ErrorLimit || storageHelper.SelectedTrayRange.TrayAssembly.ErrorPercentage <= -storageHelper.SelectedTrayRange.TrayAssembly.ErrorLimit ? RED : BLACK;
            resultantProps.Add(new WTProperty(ERROR, ERROR, storageHelper.SelectedTrayRange.TrayAssembly.ErrorPercentage.ToString(FORMAT_F2) + SPACE + PERCENT, color));
            string val = string.Empty;
            if (storageHelper.MainObject.ProjectType == uppProjectTypes.uopProjMDSpout)
            {
                val = storageHelper.MainObject.DisplayUnits == uppUnitFamilies.uopEnglish ? (storageHelper.SelectedTrayRange.TrayAssembly.FunctionalPanelWidth).ToString(FORMAT_F3) + SPACE + INCH : (storageHelper.SelectedTrayRange.TrayAssembly.FunctionalPanelWidth * INCHTOMM).ToString(FORMAT_F3) + SPACE + MILLIMETER;
                resultantProps.Add(new WTProperty(FUNC_PANEL_WIDTH, FUNC_PANEL_WIDTH, val));
            }
            else if (storageHelper.MainObject.ProjectType == uppProjectTypes.uopProjMDDraw)
            {
                val = storageHelper.MainObject.DisplayUnits == uppUnitFamilies.uopEnglish ? (storageHelper.SelectedTrayRange.TrayAssembly.DeckSectionWidth(true)).ToString(FORMAT_F3) + SPACE + INCH : (storageHelper.SelectedTrayRange.TrayAssembly.DeckSectionWidth(true) * INCHTOMM).ToString(FORMAT_F3) + SPACE + MILLIMETER;
                resultantProps.Add(new WTProperty(FUNC_PANEL_WIDTH, PANEL_WIDTH, val));
            }
            var prop = storageHelper.SelectedTrayRange.TrayAssembly.DesignOptions.CurrentProperties.Item(APPANPERFDIAMETER);
            resultantProps.Add(new WTProperty(prop.Name, prop.DisplayName, prop.UnitSignature(aUnitFamily: storageHelper.MainObject.DisplayUnits)));
            properties.ForEach(ele =>
            {
                if (resultantProps.Find(element => element.Name == ele.Name) == null)
                {
                    resultantProps.Add(ele);
                }
            });
            return resultantProps;
        }
        #endregion
    }
}
