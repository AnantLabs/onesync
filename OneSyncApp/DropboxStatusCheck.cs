using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OneSync
{
    public enum DropboxStatus
    {
        NOT_RUNNING,
        NOT_IN_DROPBOX,
        UP_TO_DATE,
        SYNCHRONIZING,
        SYNC_PROBLEM
    }

    public class DropboxStatusCheck
    {
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

        [DllImport("kernel32.dll")]
        static extern bool CallNamedPipe(string lpNamedPipeName, byte[] lpInBuffer,
           uint nInBufferSize, [Out] byte[] lpOutBuffer, uint nOutBufferSize,
           out uint lpBytesRead, uint nTimeOut);

        public static DropboxStatus ReturnDropboxStatus(string path)
        {
            uint processId = GetCurrentProcessId();
            uint threadId = GetCurrentThreadId();
            uint sessionID;
            bool success1 = ProcessIdToSessionId(processId, out sessionID);
            uint request_type = 1;
            uint wtf = 0x3048302;

            byte[] p1 = System.BitConverter.GetBytes(wtf);
            byte[] p2 = System.BitConverter.GetBytes(processId);
            byte[] p3 = System.BitConverter.GetBytes(threadId);
            byte[] p4 = System.BitConverter.GetBytes(request_type);

            string pipename = @"\\.\PIPE\DropboxPipe_" + Convert.ToString(sessionID);
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] b = encoding.GetBytes(path);

            byte[] b1 = new byte[16382];
            p1.CopyTo(b1, 0);
            p2.CopyTo(b1, p1.Length);
            p3.CopyTo(b1, p2.Length + 4);
            p4.CopyTo(b1, p3.Length + 8);
            b.CopyTo(b1, p4.Length + 12);

            byte[] b2 = new byte[16382];
            uint bytesRead;

            string s = System.Text.ASCIIEncoding.ASCII.GetString(b1);
            bool success2 = CallNamedPipe(pipename, b1, (uint)b1.Length, b2, (uint)b2.Length,
                out bytesRead, 1000);
            int readVal = (int)b2[4];

            if (!success2)
            {
                return DropboxStatus.NOT_RUNNING;
            }
            else
            {
                DropboxStatus status;
                switch (readVal)
                {
                    case 48: status = DropboxStatus.NOT_IN_DROPBOX; break;
                    case 49: status = DropboxStatus.UP_TO_DATE; break;
                    case 50: status = DropboxStatus.SYNCHRONIZING; break;
                    default: status = DropboxStatus.SYNC_PROBLEM; break;
                }
                return status;
            }
        }
    }
}
