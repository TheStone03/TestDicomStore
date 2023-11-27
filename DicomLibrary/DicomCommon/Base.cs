using Leadtools.Dicom;
using System.Net;

namespace DicomLibrary
{
	/// <summary>
	/// Status type codes.
	/// </summary>
	public enum StatusType
	{
		/// <summary>
		/// Dicom error has occurred.
		/// </summary>
		Error,

		/// <summary>
		/// Process has been terminated.
		/// </summary>
		ProcessTerminated,

		/// <summary>
		/// Connection has been successfully received.
		/// </summary>
		ConnectReceived,

		/// <summary>
		/// Connection attempt failed.
		/// </summary>
		ConnectFailed,

		/// <summary>
		/// Connection attemp succeeded.
		/// </summary>
		ConnectSucceeded,

		/// <summary>
		/// Connection has been closed.
		/// </summary>
		ConnectionClosed,

		/// <summary>
		/// Associate request sent.
		/// </summary>
		SendAssociateRequest,

		/// <summary>
		/// Received associate accept.
		/// </summary>
		ReceiveAssociateAccept,

		/// <summary>
		/// Received associate reject.
		/// </summary>
		ReceiveAssociateReject,

		/// <summary>
		/// The abstract syntax is not supported on the association.
		/// </summary>
		AbstractSyntaxNotSupported,

		/// <summary>
		/// C-STORE-REQ sent.
		/// </summary>
		SendCStoreRequest,

		/// <summary>
		/// Received a C-STORE-RESP
		/// </summary>
		ReceiveCStoreResponse,

		/// <summary>
		/// Release request sent.
		/// </summary>
		SendReleaseRequest,

		/// <summary>
		/// Received release response.
		/// </summary>
		ReceiveReleaseResponse,

		SendCEchoRequest,

		ReceiveCEchoResponse,

		SendCFindRequest,

		ReceiveCFindResponse,

		SendCMoveRequest,

		ReceiveCMoveResponse,

		SendCStoreResponse,

		ReceiveCStoreRequest,

		/// <summary>
		/// Connection timeout.
		/// </summary>
		Timeout,

		ReceiveAssociateRequest,
		SenAssociateAccept,

		/// <summary>
		/// Secure Link Ready.
		/// </summary>
		SecureLinkReady,
	}

	/// <summary>
	/// 
	/// </summary>
	public class StatusEventArgs : EventArgs
	{
		internal StatusType _Type;
		internal DicomExceptionCode _Error = DicomExceptionCode.Success;
		internal string _CallingAE;
		internal string _CalledAE;
		internal IPAddress _PeerIP;
		internal int _PeerPort;
		internal int _NumberCompleted;
		internal int _NumberRemaining;
		internal DicomCommandStatusType _Status;
		internal DicomAssociateRejectResultType _Result;
		internal DicomAssociateRejectSourceType _Source;
		internal DicomAssociateRejectReasonType _Reason;
		internal byte _PresentationID;
		internal int _MessageID;
		internal string _AffectedClass;

		/// <summary>
		/// 
		/// </summary>
		public StatusEventArgs()
		{
			_Type = Type;
		}

		/// <summary>
		/// Status type
		/// </summary>
		public StatusType Type {
			get {
				return _Type;
			}
		}

		/// <summary>
		/// Status/Error code
		/// </summary>
		public DicomExceptionCode Error {
			get {
				return _Error;
			}
		}

		public string CallingAE {
			get {
				return _CallingAE;
			}
		}

		public string CalledAE {
			get {
				return _CalledAE;
			}
		}

		public IPAddress PeerIP {
			get {
				return _PeerIP;
			}
		}

		public int PeerPort {
			get {
				return _PeerPort;
			}
		}

		public int NumberCompleted {
			get {
				return _NumberCompleted;
			}
		}

		public int NumberRemaining {
			get {
				return _NumberRemaining;
			}
		}

		public DicomCommandStatusType Status {
			get {
				return _Status;
			}
		}

		public DicomAssociateRejectResultType Result {
			get {
				return _Result;
			}
		}

		public DicomAssociateRejectSourceType Source {
			get {
				return _Source;
			}
		}

		public DicomAssociateRejectReasonType Reason {
			get {
				return _Reason;
			}
		}

		public byte PresentationID {
			get {
				return _PresentationID;
			}
		}

		public int MessageID {
			get {
				return _MessageID;
			}
		}

		public string AffectedClass {
			get {
				return _AffectedClass;
			}
		}
	}

	public delegate void StatusEventHandler(object sender, StatusEventArgs e);

	/// <summary>
	/// Base class for dicom communications.  Defines all the processes that
	/// are common to SCP & SCU applications.
	/// </summary>
	public class Base : DicomNet
	{
		private AutoResetEvent _Event = new AutoResetEvent(false);
		private AutoResetEvent _resetTimeoutEvent = new AutoResetEvent(false);
		private AutoResetEvent _FailureEvent = new AutoResetEvent(false);

		private string _ImplementationClass = "1.2.840.114257.1123456";
		private string _privateKeyPassword = "";
		private bool _isSecureTLS = false;

		public event StatusEventHandler Status;

		public Base()
		   : base(null, DicomNetSecurityMode.None)
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region Secure TLS Communication

		public Base(string clientCertificatePath,
					string clientKeyPath,
					string clientKeyPassword)
		   : base(null, DicomNetSecurityMode.Tls)
		{
			PrivateKeyPassword = clientKeyPassword;
			SetTlsCipherSuiteByIndex(0, DicomTlsCipherSuiteType.DheRsaWith3DesEdeCbcSha);///@TODO Deprecated do not use
			SetTlsClientCertificate(clientCertificatePath, DicomTlsCertificateType.Pem, clientKeyPath);
			_isSecureTLS = true;
		}

		public bool IsSecureTLS {
			get {
				return _isSecureTLS;
			}
		}

		public string PrivateKeyPassword {
			get {
				return _privateKeyPassword;
			}
			set {
				_privateKeyPassword = value;
			}
		}

		protected override string OnPrivateKeyPassword(bool encryption)
		{
			return PrivateKeyPassword;
		}
		#endregion

		protected void SendStatus(StatusEventArgs e)
		{
			Status?.Invoke(this, e);
		}

		public void InvokeStatusEvent(StatusType sType, DicomExceptionCode error)
		{
			StatusEventArgs se = new StatusEventArgs();

			se._Type = sType;
			se._Error = error;
			SendStatus(se);
		}

		public void InvokeStatusEvent(StatusType sType, DicomExceptionCode error, string callingAE, string calledAE)
		{
			StatusEventArgs se = new StatusEventArgs();

			se._Type = sType;
			se._Error = error;
			se._CallingAE = callingAE;
			se._CalledAE = calledAE;
			SendStatus(se);
		}

		public void InvokeStatusEvent(StatusType sType, DicomExceptionCode error, int completed, int remaining, DicomCommandStatusType status)
		{
			StatusEventArgs se = new StatusEventArgs();

			se._Type = sType;
			se._Error = error;
			se._NumberCompleted = completed;
			se._NumberRemaining = remaining;
			se._Status = status;
			SendStatus(se);
		}

		public void InvokeStatusEvent(StatusType sType, DicomCommandStatusType status)
		{
			StatusEventArgs se = new StatusEventArgs();

			se._Error = (status == DicomCommandStatusType.Success) ? DicomExceptionCode.Success : DicomExceptionCode.NetFailure;
			se._Status = status;
			se._Type = sType;

			SendStatus(se);
		}

		public void InvokeStatusEvent(StatusType sType, DicomCommandStatusType status, byte presentationID, int messageID, string affectedClass)
		{
			StatusEventArgs se = new StatusEventArgs();

			se._Error = (status == DicomCommandStatusType.Success) ? DicomExceptionCode.Success : DicomExceptionCode.NetFailure;
			se._Status = status;
			se._Type = sType;
			se._PresentationID = presentationID;
			se._MessageID = messageID;
			se._AffectedClass = affectedClass;

			SendStatus(se);
		}


		public void InvokeStatusEvent(StatusEventArgs e)
		{
			SendStatus(e);
		}

		public void InvokeStatusEvent(StatusType sType, DicomAssociateRejectResultType result, DicomAssociateRejectSourceType source, DicomAssociateRejectReasonType reason)
		{
			StatusEventArgs se = new StatusEventArgs();

			se._Result = result;
			se._Reason = reason;
			se._Source = source;
			se._Error = DicomExceptionCode.Success;
			se._Type = sType;
			SendStatus(se);

		}

		/// <summary>
		/// Derived classes should override this method to perform any 
		/// per instance initialization.
		/// </summary>
		public virtual void Init()
		{

		}

		/// <summary>
		/// Implementation Class.
		/// </summary>
		public string ImplementationClass {
			get {
				return _ImplementationClass;
			}
			set {
				_ImplementationClass = value;
			}
		}

		private string _ImplementationVersionName = "";

		/// <summary>
		/// Implementation Version Name.
		/// </summary>
		public string ImplementationVersionName {
			get {
				return _ImplementationVersionName;
			}
			set {
				_ImplementationVersionName = value;
			}
		}

		/// <summary>
		/// Event used to control communication flow in the dicom
		/// communication thread.
		/// </summary>
		public AutoResetEvent Event {
			get {
				return _Event;
			}
		}

		public AutoResetEvent FailureEvent {
			get {
				return _FailureEvent;
			}
		}

	}
}
