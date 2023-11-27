using Leadtools.Dicom;
using Leadtools;

namespace DicomLibrary
{
	public class Dicom
	{
		/// <summary>
		/// TEMP INIZIALIZZAZIONE DEL DICOM
		/// </summary>
		public static void DicomStart()
		{
			// PUT the LICENSE
			RasterSupport.SetLicense("",
					File.ReadAllText(""));

			//Make sure to initialize the DICOM engine, this needs to be done only once  
			//In the whole application
			DicomEngine.Startup();
			DicomNet.Startup();
		}

		/// <summary>
		/// General transaction state
		/// </summary>
		public enum TransactionState
		{
			Success,
			Error,
			Killed,
			Pending
		}

		/// <summary>
		/// Dicom Error Type
		/// </summary>
		public enum DicomErrorMsg
		{
			Success,
			DicomError,
			ConnectionFailed,
			AssociateReject,
			AbstractSyntaxNotSupported,
			Abort,
			StorageFailed,
			FindFailed,
			PrintException,
			PrintFailed,
			PrintUpdateImageBox,
			StorageCommitmentRequestFailed,
			StorageCommitmentFailed,
			Warning,
			ConnectionClosed,
			ProcessTerminated,
			Timeout
		}

		// Error Status Management
		public static class StatusManagementConfig
		{
			public class StatusManagementItem
			{
				private String _ErrorMsg { set; get; }
				private TransactionState _TransactionState { set; get; }
				private DicomErrorMsg _DcmErrorMsg { set; get; }

				public String ErrorMsg {
					get { return _ErrorMsg; }
				}

				public TransactionState TransactionState {
					get { return _TransactionState; }
				}

				public DicomErrorMsg DcmErrorMsg {
					get { return _DcmErrorMsg; }
				}

				/**
				 * Constructor that initializes the class
				 * 
				 * @param[in]	ErrorMsg 
				 * @param[in]	transactionState
				 * @param[in]	dcmErrorMsg
				 */
				public StatusManagementItem(String errorMsg, TransactionState transactionState, DicomErrorMsg dcmErrorMsg)
				{
					_ErrorMsg = errorMsg;
					_TransactionState = transactionState;
					_DcmErrorMsg = dcmErrorMsg;
				}
			}

			// public accessor for status management dictionary readonly
			public static System.Collections.ObjectModel.ReadOnlyDictionary<StatusType, StatusManagementItem> StatusManagement {
				get {
					return new System.Collections.ObjectModel.ReadOnlyDictionary<StatusType, StatusManagementItem>(_StatusManagement);
				}
			}

			// private accessor for the status management dictionary
			private static Dictionary<StatusType, StatusManagementItem> _StatusManagement;

			/**
			 * Class constructor
			 * 
			 * @note			Builds the structure of statuses
			 * @note			WARNING: Change values only if you know what you are doing
			 */
			static StatusManagementConfig()
			{
				_StatusManagement = new Dictionary<StatusType, StatusManagementItem>();
				_StatusManagement.Add(StatusType.Error, new StatusManagementItem("Error occurred. Error code:", TransactionState.Error, DicomErrorMsg.DicomError));
				_StatusManagement.Add(StatusType.ConnectFailed, new StatusManagementItem("Connection failed.", TransactionState.Error, DicomErrorMsg.ConnectionFailed));
				_StatusManagement.Add(StatusType.ConnectSucceeded, new StatusManagementItem("Connection succeeded.\r\n", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendAssociateRequest, new StatusManagementItem(">>Sending association request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveAssociateAccept, new StatusManagementItem("<<Associate Accept Received", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveAssociateReject, new StatusManagementItem("<<Received Associate Reject!", TransactionState.Error, DicomErrorMsg.AssociateReject));
				_StatusManagement.Add(StatusType.AbstractSyntaxNotSupported, new StatusManagementItem("Abstract Syntax NOT supported!", TransactionState.Killed, DicomErrorMsg.AbstractSyntaxNotSupported));
				_StatusManagement.Add(StatusType.ConnectionClosed, new StatusManagementItem("Network Connection closed!", TransactionState.Killed, DicomErrorMsg.ConnectionClosed));
				_StatusManagement.Add(StatusType.ProcessTerminated, new StatusManagementItem("Process has been terminated!", TransactionState.Error, DicomErrorMsg.ProcessTerminated));
				_StatusManagement.Add(StatusType.SendReleaseRequest, new StatusManagementItem(">>Sending release request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveReleaseResponse, new StatusManagementItem("<<Receiving release response", TransactionState.Success, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.Timeout, new StatusManagementItem("Communication timeout. Process will be terminated.", TransactionState.Error, DicomErrorMsg.Timeout));
				_StatusManagement.Add(StatusType.ReceiveAssociateRequest, new StatusManagementItem("<<Associate request received.", TransactionState.Pending, DicomErrorMsg.Success));
				/*_StatusManagement.Add(StatusType.SendAssociateAccept, new StatusManagementItem(">>Sending Associate ACCEPT.", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendAssociateReject, new StatusManagementItem(">>Sending Associate REJECT.", TransactionState.Success, DicomErrorMsg.Success));*/

				// ECHO (SCU)
				_StatusManagement.Add(StatusType.SendCEchoRequest, new StatusManagementItem(">>Sending C-ECHO request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveCEchoResponse, new StatusManagementItem("<<Received C-ECHO response", TransactionState.Pending, DicomErrorMsg.Success));

				// WKL
				_StatusManagement.Add(StatusType.SendCFindRequest, new StatusManagementItem(">>Sending C-FIND request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveCFindResponse, new StatusManagementItem("Operation completed successfully.", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendCMoveRequest, new StatusManagementItem(">>Sending C-MOVE request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveCMoveResponse, new StatusManagementItem("<<Received response", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendCStoreResponse, new StatusManagementItem(">>Sending response...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveCStoreRequest, new StatusManagementItem("<<Received request", TransactionState.Pending, DicomErrorMsg.Success));

				// STORE
				_StatusManagement.Add(StatusType.SendCStoreRequest, new StatusManagementItem(">>Sending C-STORE Request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveCStoreResponse, new StatusManagementItem("<<Received response", TransactionState.Pending, DicomErrorMsg.Success));

				/*// ECHO (SCP)
				_StatusManagement.Add(StatusType.ReceiveCEchoRequest, new StatusManagementItem("<<Received C-ECHO-RQ", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendCEchoResponse, new StatusManagementItem(">>Sending C-ECHO-RES...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveReleaseRequest, new StatusManagementItem("<<Received release request", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendReleaseResponse, new StatusManagementItem(">>Release response sent", TransactionState.Success, DicomErrorMsg.Success));

				// STORAGE COMMITMENT
				_StatusManagement.Add(StatusType.SendNActionRequest, new StatusManagementItem(">>Sending N-ACTION Request...", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveNActionResponse, new StatusManagementItem("<<Received N-ACTION response", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.ReceiveNEventReportRequest, new StatusManagementItem("<<Received N-EVENT-REPORT request.", TransactionState.Pending, DicomErrorMsg.Success));
				_StatusManagement.Add(StatusType.SendNEventReportResponse, new StatusManagementItem(">>Sending N-EVENT-REPORT response.", TransactionState.Pending, DicomErrorMsg.Success));*/
			}
		}
	}
}
