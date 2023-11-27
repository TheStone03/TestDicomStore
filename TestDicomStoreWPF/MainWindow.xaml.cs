using DicomLibrary;
using Leadtools.Dicom;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace TestDicomStoreWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Dicom.DicomStart();

		}

		private void _PrintBtn_Click(object sender, RoutedEventArgs e)
		{
			
			DicomServer dicomServer = new DicomServer();
			dicomServer.Address = IPAddress.Parse("10.0.39.195");
			dicomServer.AETitle = "ATS_PRINTER";
			dicomServer.Port = 4007;
			dicomServer.Timeout = 90;
			try {

				CEcho cecho = new CEcho();
				cecho.Status += _Cecho_Status;
				cecho.Echo(dicomServer, "PIETRO_TEST");

				using (MyDicomPrintSCU printSCU = new MyDicomPrintSCU(null)) {
					bool ret = printSCU.Associate(dicomServer.Address.ToString(), dicomServer.Port, dicomServer.AETitle, "PIETRO_TEST",
										 DicomPrintScuPrintManagementClassFlags.BasicGrayscalePmMetaSopClass |
										 DicomPrintScuPrintManagementClassFlags.BasicColorPmMetaSopClass |
										 DicomPrintScuPrintManagementClassFlags.BasicAnnotationBoxSopClass |
										 DicomPrintScuPrintManagementClassFlags.BasicPrintImageOverlayBoxSopClass |
										 DicomPrintScuPrintManagementClassFlags.PresentationLutSopClass |
										 DicomPrintScuPrintManagementClassFlags.PrintJobSopClass |
										 DicomPrintScuPrintManagementClassFlags.PrinterConfigurationRetrievalSopClass);
				}
			}
			catch (Exception ex) {
				Debug.WriteLine("ERROR " + ex.Message);
			}
		}

		private void _Cecho_Status(object sender, StatusEventArgs e)
		{
			Debug.WriteLine(e.Type + " EChoStatus: " + e.Status + " Printer AE: " + e.CalledAE);
		}
	}
}
