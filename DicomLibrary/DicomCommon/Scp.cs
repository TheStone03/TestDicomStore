﻿using Leadtools.Dicom;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomLibrary
{
	/// <summary>
	/// Summary description for Scp.
	/// </summary>
	public class Scp : Base
	{
		string _CalledAE;
		string _CallingAE;
		private StringCollection _UidExclusionList = new StringCollection();

		public StringCollection UidExclusionList {
			get {
				return _UidExclusionList;
			}
		}

		public string CalledAE {
			get {
				return _CalledAE;
			}
			set {
				_CalledAE = value;
			}
		}

		public string CallingAE {
			get {
				return _CallingAE;
			}
			set {
				_CallingAE = value;
			}
		}

		public Scp()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public DicomExceptionCode Listen(int port, int peers)
		{
			DicomExceptionCode ret = DicomExceptionCode.Success;

			try {
				Listen("", port, peers);
			}
			catch (DicomException de) {
				ret = de.Code;
			}

			return ret;
		}

		public DicomExceptionCode Listen(int port, int peers, DicomNetIpTypeFlags ipType)
		{
			DicomExceptionCode ret = DicomExceptionCode.Success;

			try {
				Listen("*", port, peers, ipType);
			}
			catch (DicomException de) {
				ret = de.Code;
			}

			return ret;
		}

		public override void Init()
		{
			base.Init();
		}

		public bool IsSupported(string uid)
		{
			if (UidExclusionList.IndexOf(uid) != -1)
				return false;

			return (DicomUidTable.Instance.Find(uid) != null);
		}

		public virtual bool IsConnectionValid(string peerAddress, ref DicomAssociateRejectReasonType reject)
		{
			return true;
		}
	}
}
