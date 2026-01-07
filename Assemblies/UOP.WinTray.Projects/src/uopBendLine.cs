namespace UOP.WinTray.Projects
{
    /// <summary>
    /// simple object used to represent a bend line in a flat plate manufacturing drawing
    /// </summary>
    public class uopBendLine
    {
        public double Y;
        public double X;

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        public uopBendLine Clone()
        {
            uopBendLine Clone = new uopBendLine
            {
                Y = Y,
                X = X
            };
            return Clone;
        }
    }
}
