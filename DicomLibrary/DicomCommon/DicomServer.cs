using System.Net;

namespace DicomLibrary
{
	/// <summary>
	/// Summary description for DicomServer.
	/// </summary>
	public class DicomServer
	{
		private string _AETitle = "";
		private int _Port = 104;
		private IPAddress _Address = IPAddress.Parse("127.0.0.1");
		private int _Timeout = 30;

		#region Server Properties

		/// <summary>
		/// Called AE Title.
		/// </summary>
		public string AETitle {
			get {
				return _AETitle;
			}
			set {
				_AETitle = value;
			}
		}

		/// <summary>
		/// Port of server.
		/// </summary>
		public int Port {
			get {
				return _Port;
			}
			set {
				_Port = value;
			}
		}

		/// <summary>
		/// IPAddress of server
		/// </summary>
		public IPAddress Address {
			get {
				return _Address;
			}
			set {
				_Address = value;
			}
		}

		/// <summary>
		/// Ammount of time in milliseconds to wait for a response from
		/// the server. Assign zero to wait indefinitely.
		/// </summary>
		public int Timeout {
			get {
				return _Timeout;
			}
			set {
				_Timeout = value;
			}
		}
		#endregion

		public DicomServer()
		{

		}
	}
}
