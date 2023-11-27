using Leadtools.Dicom;

namespace DicomLibrary
{
	public delegate void SCUProcessFunc();

	internal class AssociateClass
	{
		public Scu MyScu;
		public DicomServer DcmServer;
		public string ClientAE;
		public SCUProcessFunc Process;

		public void DoAssociate()
		{
			DicomAssociate associate;

			DicomExceptionCode ret = DicomExceptionCode.Success;

			ret = MyScu.Connect(DcmServer);
			if (ret != DicomExceptionCode.Success) {
				return;
			}

			associate = MyScu.BuildAssociation(DcmServer.AETitle, ClientAE);
			if (associate == null) {
				MyScu.InvokeStatusEvent(StatusType.Error, DicomExceptionCode.Parameter);
				MyScu.Terminate();
				return;
			}

			MyScu.InvokeStatusEvent(StatusType.SendAssociateRequest, DicomExceptionCode.Success);
			try {
				if (MyScu.IsConnected()) {
					MyScu.SendAssociateRequest(associate);
				}
				else {
					MyScu.InvokeStatusEvent(StatusType.Error, DicomExceptionCode.NetConnectionAborted);
					MyScu.Terminate();
					return;
				}
			}
			catch (DicomException de) {
				MyScu.InvokeStatusEvent(StatusType.Error, de.Code);
				MyScu.Terminate();
				return;
			}

			if (!MyScu.Wait()) {
				//
				// Connection timed out
				//
				MyScu.InvokeStatusEvent(StatusType.Timeout, DicomExceptionCode.Success);
				MyScu.Terminate();
			}
			if (!MyScu.Rejected && MyScu.IsAssociated())
				Process();
		}
	}
}
