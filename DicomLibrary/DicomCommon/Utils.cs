using Leadtools.Dicom;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DicomLibrary
{
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public static implicit operator System.Drawing.Point(POINT p)
		{
			return new System.Drawing.Point(p.X, p.Y);
		}

		public static implicit operator POINT(System.Drawing.Point p)
		{
			return new POINT(p.X, p.Y);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MSG
	{
		public IntPtr hwnd;
		public uint message;
		public IntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}

	public enum WaitReturn
	{
		Complete,
		Timeout,
		NetConnectionRefused,
	}

	/// <summary>
	/// Summary description for Scu.
	/// </summary>
	public class Utils
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool PeekMessage(out MSG lpMsg, HandleRef hWnd,
									   uint wMsgFilterMin, uint wMsgFilterMax,
									   uint wRemoveMsg);

		[DllImport("user32.dll")]
		static extern bool TranslateMessage([In] ref MSG lpMsg);
		[DllImport("user32.dll")]
		static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

		const uint PM_REMOVE = 1;

		/*public static WaitReturn WaitForComplete(double mill, WaitHandle wh)
		{
			TimeSpan goal = new TimeSpan(DateTime.Now.AddMilliseconds(mill).Ticks);
			MSG msg = new MSG();
			HandleRef h = new HandleRef(null, IntPtr.Zero);

			do {
				if (PeekMessage(out msg, h, 0, 0, PM_REMOVE)) {
					TranslateMessage(ref msg);
					DispatchMessage(ref msg);
				}

				if (wh.WaitOne(new TimeSpan(1), false)) {
					return WaitReturn.Complete;
				}

				if (goal.CompareTo(new TimeSpan(DateTime.Now.Ticks)) < 0) {
					return WaitReturn.Timeout;
				}

			} while (true);
		}*/

		public static WaitReturn WaitForComplete(double mill, WaitHandle completeHandle)
		{
			TimeSpan goal = new TimeSpan(DateTime.Now.AddMilliseconds(mill).Ticks);
			MSG msg = new MSG();
			HandleRef h = new HandleRef(null, IntPtr.Zero);

			WaitHandle[] waitHandles = new WaitHandle[1] { completeHandle };

			do {
				if (PeekMessage(out msg, h, 0, 0, PM_REMOVE)) {
					TranslateMessage(ref msg);
					DispatchMessage(ref msg);
				}

				int index = WaitHandle.WaitAny(waitHandles, new TimeSpan(1), false);

				if (index == WaitHandle.WaitTimeout) {
					if (goal.CompareTo(new TimeSpan(DateTime.Now.Ticks)) < 0) {
						return WaitReturn.Timeout;
					}
				}

				else {
					Debug.Assert(index == 0 || index == 1);
					AutoResetEvent autoEvent = waitHandles[index] as AutoResetEvent;
					if (autoEvent == completeHandle) {
						return WaitReturn.Complete;
					}
				}

			} while (true);
		}

		public static WaitReturn WaitForComplete2(double mill, WaitHandle completeHandle, WaitHandle failureHandle)
		{
			TimeSpan goal = new TimeSpan(DateTime.Now.AddMilliseconds(mill).Ticks);
			MSG msg = new MSG();
			HandleRef h = new HandleRef(null, IntPtr.Zero);

			WaitHandle[] waitHandles = new WaitHandle[] { completeHandle, failureHandle };

			do {
				if (PeekMessage(out msg, h, 0, 0, PM_REMOVE)) {
					TranslateMessage(ref msg);
					DispatchMessage(ref msg);
				}

				int index = WaitHandle.WaitAny(waitHandles, new TimeSpan(1), false);

				if (index == WaitHandle.WaitTimeout) {
					if (goal.CompareTo(new TimeSpan(DateTime.Now.Ticks)) < 0) {
						return WaitReturn.Timeout;
					}
				}
				else {
					Debug.Assert(index == 0 || index == 1 || index == 2);
					AutoResetEvent autoEvent = waitHandles[index] as AutoResetEvent;
					if (autoEvent == completeHandle) {
						return WaitReturn.Complete;
					}
					else if (autoEvent == failureHandle) {
						return WaitReturn.NetConnectionRefused;
					}
				}

			} while (true);
		}

		public static void EngineStartup()
		{
			DicomEngine.Startup();
		}

		public static void EngineShutdown()
		{
			DicomEngine.Shutdown();
		}

		public static void DicomNetStartup()
		{
			DicomNet.Startup();
		}

		public static void DicomNetShutdown()
		{
			DicomNet.Shutdown();
		}

		/// <summary>
		/// Helper method to get string value from a DICOM dataset.
		/// </summary>
		/// <param name="dcm">The DICOM dataset.</param>
		/// <param name="tag">Dicom tag.</param>
		/// <returns>String value of the specified DICOM tag.</returns>
		public static string GetStringValue(DicomDataSet dcm, long tag, bool tree)
		{
			DicomElement element;

			element = dcm.FindFirstElement(null, tag, tree);
			if (element != null) {
				if (dcm.GetElementValueCount(element) > 0) {
					return dcm.GetConvertValue(element);
				}
			}

			return "";
		}

		public static string GetStringValue(DicomDataSet dcm, long tag)
		{
			return GetStringValue(dcm, tag, true);
		}

		public static StringCollection GetStringValues(DicomDataSet dcm, long tag)
		{
			DicomElement element;
			StringCollection sc = new StringCollection();

			element = dcm.FindFirstElement(null, tag, true);
			if (element != null) {
				if (dcm.GetElementValueCount(element) > 0) {
					string s = dcm.GetConvertValue(element);
					string[] items = s.Split('\\');

					foreach (string value in items) {
						sc.Add(value);
					}
				}
			}

			return sc;
		}

		public static byte[] GetBinaryValues(DicomDataSet dcm, long tag)
		{
			DicomElement element;

			element = dcm.FindFirstElement(null, tag, true);
			if (element != null) {
				if (element.Length > 0) {
					return dcm.GetBinaryValue(element, (int)element.Length);
				}
			}

			return null;
		}

		public static bool IsTagPresent(DicomDataSet dcm, long tag)
		{
			DicomElement element;

			element = dcm.FindFirstElement(null, tag, true);
			return (element != null);
		}

		public static bool IsAscii(string value)
		{
			return Encoding.UTF8.GetByteCount(value) == value.Length;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dcm"></param>
		/// <param name="tag"></param>
		/// <param name="tagValue"></param>
		/// <returns></returns>
		public static DicomExceptionCode SetTag(DicomDataSet dcm, long tag, object tagValue, bool tree)
		{
			DicomExceptionCode ret = DicomExceptionCode.Success;
			DicomElement element;

			if (tagValue == null)
				return DicomExceptionCode.Parameter;

			element = dcm.FindFirstElement(null, tag, tree);
			if (element == null) {
				element = dcm.InsertElement(null, false, tag, DicomVRType.UN, false, 0);
			}

			if (element == null)
				return DicomExceptionCode.Parameter;

			try {
				string s = tagValue.ToString();
				if (IsAscii(s))
					dcm.SetConvertValue(element, s, 1);
				else
					dcm.SetStringValue(element, s, DicomCharacterSetType.UnicodeInUtf8);
			}
			catch (DicomException de) {
				ret = de.Code;
			}

			return ret;
		}

		public static DicomExceptionCode SetTag(DicomDataSet dcm, long tag, object tagValue)
		{
			return SetTag(dcm, tag, tagValue, true);
		}

		public static void SetTag(DicomDataSet dcm, long Sequence, long Tag, object TagValue)
		{
			DicomElement seqElement = dcm.FindFirstElement(null, Sequence, true);
			DicomElement seqItem = null;
			DicomElement item = null;

			if (seqElement == null) {
				seqElement = dcm.InsertElement(null, false, Tag, DicomVRType.SQ, true, -1);
			}

			seqItem = dcm.GetChildElement(seqElement, false);
			if (seqItem == null) {
				seqItem = dcm.InsertElement(seqElement, true, DicomTag.SequenceDelimitationItem, DicomVRType.SQ, true, -1);
			}

			item = dcm.GetChildElement(seqItem, true);
			while (item != null) {
				if (item.Tag == Tag)
					break;

				item = dcm.GetNextElement(item, true, true);
			}

			if (item == null) {
				item = dcm.InsertElement(seqItem, true, Tag, DicomVRType.UN, false, -1);
			}
			dcm.SetConvertValue(item, TagValue.ToString(), 1);
		}

		public static DicomExceptionCode SetTag(DicomDataSet dcm, long tag, byte[] tagValue)
		{
			DicomExceptionCode ret = DicomExceptionCode.Success;
			DicomElement element;

			if (tagValue == null)
				return DicomExceptionCode.Parameter;

			element = dcm.FindFirstElement(null, tag, true);
			if (element == null) {
				element = dcm.InsertElement(null, false, tag, DicomVRType.UN, false, 0);
			}

			dcm.SetBinaryValue(element, tagValue, tagValue.Length);

			return ret;
		}

		public static void CreateTag(DicomDataSet dcm, long tag)
		{
			DicomElement element = dcm.FindFirstElement(null, tag, true);
			if (element == null) {
				element = dcm.InsertElement(null, false, tag, DicomVRType.UN, false, 0);
			}
		}

		public static DicomExceptionCode InsertKeyElement(DicomDataSet dcmRsp, DicomDataSet dcmReq, long tag)
		{
			DicomExceptionCode ret = DicomExceptionCode.Success;
			DicomElement element;

			try {
				element = dcmReq.FindFirstElement(null, tag, true);
				if (element != null) {
					dcmRsp.InsertElement(null, false, tag, DicomVRType.UN, false, 0);
				}
			}
			catch (DicomException de) {
				ret = de.Code;
			}

			return ret;
		}


		public static DicomExceptionCode SetKeyElement(DicomDataSet dcmRsp, long tag, object tagValue, bool tree)
		{
			DicomExceptionCode ret = DicomExceptionCode.Success;
			DicomElement element;

			if (tagValue == null)
				return DicomExceptionCode.Parameter;

			try {
				element = dcmRsp.FindFirstElement(null, tag, tree);
				if (element != null) {
					string s = tagValue.ToString();
					if (IsAscii(s))
						dcmRsp.SetConvertValue(element, s, 1);
					else
						dcmRsp.SetStringValue(element, s, DicomCharacterSetType.UnicodeInUtf8);
				}
			}
			catch (DicomException de) {
				ret = de.Code;
			}

			return ret;
		}

		public static DicomExceptionCode SetKeyElement(DicomDataSet dcmRsp, long tag, object tagValue)
		{
			return SetKeyElement(dcmRsp, tag, tagValue, true);
		}

		public static UInt16 GetGroup(long tag)
		{
			return ((UInt16)(tag >> 16));
		}

		public static int GetElement(long tag)
		{
			return ((UInt16)(tag & 0xFFFF));
		}
	}
}
