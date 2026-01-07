using System;

namespace UOP.WinTray.UI.Events.EventAggregator
{

    /// <summary>
    /// Base object implementing the Dispose pattern
    /// </summary>
    internal class DisposableObject : IDisposable
	{
		private bool isDisposed = false;

		/// <summary>
		/// Public implementation of Dispose pattern callable by consumers.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing:true);
			GC.SuppressFinalize(obj:this);
		}

		/// <summary>
		/// Protected implementation of Dispose pattern.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed) {
				return;
			}

			this.isDisposed = true;
		}
	}

}