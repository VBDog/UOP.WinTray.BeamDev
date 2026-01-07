using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Class to create uniform width for tab headers
    /// </summary>
    public class UniformTabPanel : UniformGrid
    {
        public UniformTabPanel()
        {
            IsItemsHost = true;
            Rows = 1;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        /// <summary>
        /// Measure override to find the width of tab header
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            var totalMaxWidth = Children.OfType<TabItem>().Sum(tab => tab.MaxWidth);
            if (!double.IsInfinity(totalMaxWidth))
                HorizontalAlignment = constraint.Width > totalMaxWidth
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Stretch;

            return base.MeasureOverride(constraint);
        }
    }
}
