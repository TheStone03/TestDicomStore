using Leadtools.Dicom;
using System.Text;

namespace DicomLibrary
{
	public class MyDicomPrintSCU : DicomPrintScu
	{
		public MyDicomPrintSCU(string path) : base(path) { }

		public override void OnStatus(DicomPrintScuStatus status, DicomCommandStatusType operationStatus)
		{
			string statusCodeType = "Unknown";
			StringBuilder msg = new StringBuilder();
			switch (status) {
				case DicomPrintScuStatus.ReceiveAbort:
					DicomPrintScuAbortInformation printSCUAbortInfo = GetAbortInformation();
					msg.AppendFormat("Source = {0}, Reason = {1}", printSCUAbortInfo.Source, printSCUAbortInfo.Reason);
					Console.WriteLine("Print SCP Aborted the Association: " + msg.ToString());
					break;

				case DicomPrintScuStatus.ReceivePrintFilmSessionRsp:
					DicomCommandStatusType commandStatusType = GetLastOperationStatus();
					if (operationStatus == DicomCommandStatusType.Success) {
						if (commandStatusType == DicomCommandStatusType.Success) {
							statusCodeType = "Success";
						}
						else {
							statusCodeType = "Warning";
						}
					}
					else {
						statusCodeType = "Failure";
					}
					msg.AppendFormat("Status: 0x{0:X4} {1}", commandStatusType, statusCodeType);
					Console.WriteLine($"Received N-ACTION-RSP (Basic Film Session SOP Class): {msg.ToString()}");
					break;
			}
		}

		public override void OnPrinterReport(int eventTypeID, DicomPrinterReportInformation reportInfo)
		{
			StringBuilder msg = new StringBuilder();
			string eventTypeName = "Normal";
			switch (eventTypeID) {
				case 2:
					eventTypeName = "Warning";
					break;

				case 3:
					eventTypeName = "Failure";
					break;
			}

			msg.AppendFormat("Event Type Name: {0}", eventTypeName);

			if (eventTypeID != 1 && reportInfo != null) {
				if (reportInfo.PrinterStatusInfo != null) {
					msg.AppendFormat("{0}Printer Status Info: {1}", Environment.NewLine, reportInfo.PrinterStatusInfo);
				}

				if (reportInfo.FilmDestination != null) {
					msg.AppendFormat("{0}Film Destination: {1}", Environment.NewLine, reportInfo.FilmDestination);
				}

				if (reportInfo.PrinterName != null) {
					msg.AppendFormat("{0}Printer Name: {1}", Environment.NewLine, reportInfo.PrinterName);
				}
			}
			Console.WriteLine($"Printer Status Report: {msg.ToString()}");
		}

		public override void OnPrintJobReport(string printJobInstanceUID, int eventTypeID, DicomPrintJobReportInformation reportInfo)
		{
			StringBuilder msg = new StringBuilder();
			msg.AppendFormat("Print Job SOP Instance UID: {0}", printJobInstanceUID);

			string eventTypeName = "Pending";
			switch (eventTypeID) {
				case 2:
					eventTypeName = "Printing";
					break;

				case 3:
					eventTypeName = "Done";
					break;

				case 4:
					eventTypeName = "Failure";
					break;
			}
			msg.AppendFormat("{0}Event Type Name: {1}", Environment.NewLine, eventTypeName);

			if (reportInfo != null) {
				if (reportInfo.ExecutionStatusInfo != null) {
					msg.AppendFormat("{0}Execution Status Info: {1}", Environment.NewLine, reportInfo.ExecutionStatusInfo);
				}

				if (reportInfo.FilmSessionLabel != null) {
					msg.AppendFormat("{0}Film Session Label: {1}", Environment.NewLine, reportInfo.FilmSessionLabel);
				}

				if (reportInfo.PrinterName != null) {
					msg.AppendFormat("{0}Printer Name: {1}", Environment.NewLine, reportInfo.PrinterName);
				}
			}
			Console.WriteLine($"Print Job Status Report: {msg.ToString()}");
		}


		void PerformBasicPM()
		{

			//It's assumed that support for medical communication is already unlocked 

			//Make sure to initialize the DICOM engine, this needs to be done only once  
			//In the whole application 
			DicomEngine.Startup();

			//Make sure to initialize the DICOM Communication engine, this needs to be done only once  
			//In the whole application 
			DicomNet.Startup();

			//The DicomPrintSCU is disposable, this is why we are using the "using" keyword 
			using (MyDicomPrintSCU printSCU = new MyDicomPrintSCU(null)) {
				printSCU.SetTimeout(60);

				try {
					// Establish the Association 
					bool ret = printSCU.Associate("10.1.1.209", 7104, "PrintSCP", "PrintSCU",
										 DicomPrintScuPrintManagementClassFlags.BasicGrayscalePmMetaSopClass |
										 DicomPrintScuPrintManagementClassFlags.BasicColorPmMetaSopClass |
										 DicomPrintScuPrintManagementClassFlags.BasicAnnotationBoxSopClass |
										 DicomPrintScuPrintManagementClassFlags.BasicPrintImageOverlayBoxSopClass |
										 DicomPrintScuPrintManagementClassFlags.PresentationLutSopClass |
										 DicomPrintScuPrintManagementClassFlags.PrintJobSopClass |
										 DicomPrintScuPrintManagementClassFlags.PrinterConfigurationRetrievalSopClass);
					//The method will return false if the association was rejected, 
					//if some other error occurred then an exception will be thrown 
					if (ret == false) {
						DicomPrintScuAssociateRejectInformation associateRejectInfo = printSCU.GetAssociateRejectInformation();
						if (associateRejectInfo != null) {
							StringBuilder msg = new StringBuilder();
							msg.AppendFormat("Source = {0}, Reason = {1}", associateRejectInfo.Source, associateRejectInfo.Reason);
							Console.WriteLine(msg.ToString());
							return;
						}
					}

					// Display some printer info 
					GetPrinterInfo(printSCU);

					// Display some printer configuration info 
					GetPrinterConfigInfo(printSCU);

					// Create a Film Session 
					DicomFilmSessionParameters filmSessionParameters = printSCU.GetDefaultFilmSessionParameters();
					filmSessionParameters.NumberOfCopies = 1;

					//Over here we can set some other film session parameters before creating the film session. 
					//To set these parameters we can access one or more of these properties: 
					//DicomFilmSessionParameters.NumberOfCopies 
					//DicomFilmSessionParameters.MemoryAllocation 
					//DicomFilmSessionParameters.OwnerID 
					//DicomFilmSessionParameters.PrintPriority 
					//DicomFilmSessionParameters.MediumType 
					//DicomFilmSessionParameters.FilmDestination 
					//DicomFilmSessionParameters.FilmSessionLabel 

					printSCU.CreateFilmSession(filmSessionParameters, true);
					Console.WriteLine($"Film Session SOP Instance UID: {printSCU.GetFilmSessionInstanceUid()}");

					// Update the Film Session to specify a "MED" Print Priority 
					filmSessionParameters = printSCU.GetDefaultFilmSessionParameters();
					filmSessionParameters.PrintPriority = "MED";
					printSCU.UpdateFilmSession(filmSessionParameters);


					DicomFilmBoxParameters filmBoxParameters = printSCU.GetDefaultFilmBoxParameters();

					if (printSCU.IsClassSupported(DicomPrintScuPrintManagementClassFlags.BasicAnnotationBoxSopClass)) {
						filmBoxParameters.AnnotationDisplayFormatID = "SomeID";
					}

					//Over here we can set some other film box parameters before creating the film box. 
					//To set these parameters we can access one or more of these properties: 
					//DicomFilmBoxParameters.ImageDisplayFormat 
					//DicomFilmBoxParameters.FilmOrientation 
					//DicomFilmBoxParameters.FilmSizeID 
					//DicomFilmBoxParameters.ConfigurationInformation 
					//DicomFilmBoxParameters.AnnotationDisplayFormatID 
					//DicomFilmBoxParameters.SmoothingType 
					//DicomFilmBoxParameters.BorderDensity 
					//DicomFilmBoxParameters.EmptyImageDensity 
					//DicomFilmBoxParameters.Trim 
					//DicomFilmBoxParameters.RequestedResolutionID 
					//DicomFilmBoxParameters.MaxDensity 
					//DicomFilmBoxParameters.MinDensity 
					//DicomFilmBoxParameters.Illumination 
					//DicomFilmBoxParameters.ReflectedAmbientLight 
					//DicomFilmBoxParameters.MagnificationType 

					// Create a Film Box 
					printSCU.CreateFilmBox(filmBoxParameters, null);
					Console.WriteLine($"Film Box SOP Instance UID: {printSCU.GetFilmBoxInstanceUid()}");

					// Create a Presentation LUT 
					if (printSCU.IsClassSupported(DicomPrintScuPrintManagementClassFlags.PresentationLutSopClass)) {
						// Make sure that you have a valid presentation state dataset, 
						// otherwise leave this code commented out 

						/* 
						using (DicomDataSet presentationLUTDataset = new DicomDataSet()) 
						{ 
						   //Load DICOM File 
						   presentationLUTDataset.Load(LeadtoolsExamples.Common.ImagesPath.Path + "plut_Pre.dcm", DicomDataSetLoadFlags.None); 
						   printSCU.CreatePresentationLUT(presentationLUTDataset, null); 

						   string presLUTInstanceUID = printSCU.GetPresentationLutInstanceUid(); 
						   if(presLUTInstanceUID != null) 
						   { 
							  Console.WriteLine($"Pres LUT SOP Instance UID: {presLUTInstanceUID}"); 
							  printSCU.UpdateFilmBox(null, presLUTInstanceUID); 
						   } 
						 } 
						 */
					}

					using (DicomDataSet imageDataset = new DicomDataSet()) {
						//Load DICOM File 
						imageDataset.Load(Path.Combine(LEAD_VARS.ImagesDir, "IMAGE3.dcm"), DicomDataSetLoadFlags.LoadAndClose);
						// Update the Image Box. Since the Image Display Format of the Film Box was 
						// set to "STANDARD\1,1", then we are supposed to have one Image Box created 
						// by the Print SCP. 
						if (printSCU.GetImageBoxesCount() > 0) {
							Console.WriteLine($"Image Box SOP Instance UID: {printSCU.GetImageBoxInstanceUid(0)}");

							DicomImageBoxParameters imageBoxParameters = printSCU.GetDefaultImageBoxParameters();
							imageBoxParameters.ImagePosition = 1;

							//Over here we can set some other image box parameters before updating the image box. 
							//To set these parameters we can access one or more of these properties: 
							//DicomImageBoxParameters.MinDensity 
							//DicomImageBoxParameters.MaxDensity 
							//DicomImageBoxParameters.RequestedImageSize 
							//DicomImageBoxParameters.Polarity 
							//DicomImageBoxParameters.MagnificationType 
							//DicomImageBoxParameters.SmoothingType 
							//DicomImageBoxParameters.ConfigurationInformation 
							//DicomImageBoxParameters.RequestedDecimateCropBehavior 

							printSCU.UpdateImageBox(printSCU.GetImageBoxInstanceUid(0),
													imageDataset,
													0,
													imageBoxParameters,
													null,
													null);
							// We don't need them any more 
							printSCU.FreeImageBoxesInstanceUids();
						}
					}

					// Update the Annotation Boxes (if there are any)  
					int annotationBoxCount = printSCU.GetAnnotationBoxesCount();
					for (int i = 0; i < annotationBoxCount; i++) {
						printSCU.UpdateAnnotationBox(printSCU.GetAnnotationBoxInstanceUid(i), i + 1, "Some Text");
					}
					printSCU.FreeAnnotationBoxesInstanceUids(); // We don't need them any more 

					// Print the Film Session (or the Film Box; there is no difference since we 
					// have a single Film Box in the Film Session)  
					printSCU.PrintFilmSession();
					// We can also call this 
					//PrintSCU.PrintFilmBox(); 

					// Display some info about the Print Job 
					if (printSCU.IsClassSupported(DicomPrintScuPrintManagementClassFlags.PrintJobSopClass)) {
						GetPrintJobInfo(printSCU, printSCU.GetPrintJobInstanceUid());
					}

					// Delete the Film Box (anyway, it would be deleted when the Film Session 
					// is deleted)  
					printSCU.DeleteFilmBox();

					// Delete the Film Session 
					printSCU.DeleteFilmSession();

					// We can also call printSCU.DeletePresentationLUT and printSCU.DeleteOverlayBox 
					// to Delete the Presentation LUT and the Image Overlay Box  

				}
				catch (DicomException ex) {
					Console.WriteLine(string.Format("An error occurred, code: {0}", ex.Code));
					return;
				}
				finally {
					// Release the Association and close the connection 
					printSCU.Release();
				}
			}
			DicomNet.Shutdown();
			DicomEngine.Shutdown();
		}

		void GetPrinterInfo(DicomPrintScu printSCU)
		{
			// Query the Print SCP for the information 
			try {
				DicomPrinterInformation printerInfo = printSCU.GetPrinterInformation(null, true);
				StringBuilder msg = new StringBuilder();
				msg.AppendFormat("Printer Status: {0}{2}Printer Status Info: {1}", printerInfo.PrinterStatus, printerInfo.PrinterStatusInfo, Environment.NewLine);

				//Over here we can also investigate other printer information by accessing the following  
				//properties from the DicomPrinterInformation class: TimeOfLastCalibration, DateOfLastCalibration, 
				//SoftwareVersions, DeviceSerialNumber, ManufacturerModelName , Manufacturer, CreationDate, and CreationTime. 

				Console.WriteLine($"Printer Info: {msg.ToString()}");
			}
			catch (DicomException ex) {
				Console.WriteLine(string.Format("Failed to get printer information, Error code: {0}", ex.Code));
			}
		}

		void GetPrinterConfigInfo(DicomPrintScu printSCU)
		{
			// Assume that the Association is already established 

			try {
				// Query the Print SCP for the printer configuration information 
				DicomDataSet printerConfiguration = printSCU.GetPrinterConfiguration();

				// We will display only the Printer Name and Memory Bit Depth 
				// in the first Item 

				DicomElement element = printerConfiguration.FindFirstElement(null, DicomTag.PrinterConfigurationSequence, false);
				if (element == null)
					return;

				element = printerConfiguration.GetChildElement(element, true);
				if (element == null)
					return;

				element = printerConfiguration.GetChildElement(element, true);
				if (element == null)
					return;

				StringBuilder msg = new StringBuilder();
				msg.Append("Printer Name: ");

				DicomElement printerName = printerConfiguration.FindFirstElement(element, DicomTag.PrinterName, true);
				if (printerName != null) {
					string name = printerConfiguration.GetStringValue(printerName, 0);
					if (name != null) {
						msg.AppendFormat("{0}{1}", name, Environment.NewLine);
					}
					else {
						msg.AppendFormat("N/A{0}", Environment.NewLine);
					}
				}

				msg.Append("Memory Bit Depth: ");

				DicomElement memoryBitDepth = printerConfiguration.FindFirstElement(element, DicomTag.MemoryBitDepth, true);
				if (memoryBitDepth != null) {
					short[] value = printerConfiguration.GetShortValue(memoryBitDepth, 0, 1);
					if (value.Length > 0) {
						msg.AppendFormat("{0}", value[0]);
					}
					else {
						msg.Append("N/A");
					}
				}
				Console.WriteLine($"Printer Config Info: {msg.ToString()}");

			}
			catch (DicomException ex) {
				Console.WriteLine(string.Format("Failed to get Printer Configuration Info, Error code: {0}", ex.Code));
			}
		}

		void GetPrintJobInfo(DicomPrintScu printSCU, string printJobInstanceUID)
		{
			// Query the Print SCP for the Print Job information 
			try {
				DicomPrintJobInformation printJobInfo = printSCU.GetPrintJobInformation(printJobInstanceUID, null);
				StringBuilder msg = new StringBuilder();

				msg.AppendFormat("Execution Status: {0}{2}Execution Status Info: {1}",
				   printJobInfo.ExecutionStatus,
				   printJobInfo.ExecutionStatusInfo,
				   Environment.NewLine);

				Console.WriteLine($"Print Job Info: {msg.ToString()}");
			}
			catch (DicomException ex) {
				Console.WriteLine(string.Format("Failed to get Print Job information, Error code: {0}", ex.Code));
			}
		}

	}

	static class LEAD_VARS
	{
		public const string ImagesDir = @"C:\LEADTOOLS21\Resources\Images";
	}
}
