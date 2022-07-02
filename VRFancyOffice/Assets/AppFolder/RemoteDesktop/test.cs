using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;

class test {
	private static Socket fileSocket;
	private static int fileRecvBufIt, fileRecvBufLen;
	private static byte[] fileRecvBuf = new byte[1024];
	public static void Main() {
		recvFile("127.0.0.1", 5902, "C:\\Users\\skipl\\Desktop\\freepiano", "C:\\Users\\skipl\\Desktop\\Temp");
	}
    public static void recvFile(string fileIP, int filePort, string srcPath, string dstPath) {
        fileSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        fileSocket.Connect(fileIP, filePort);
		byte[] srcPathBytes = Encoding.UTF8.GetBytes(srcPath);
		byte[] srcPathBytesLength = new byte[4] { (byte)(srcPathBytes.Length >> 24), (byte)(srcPathBytes.Length >> 16), (byte)(srcPathBytes.Length >> 8), (byte)srcPathBytes.Length };
		fileSocket.Send(srcPathBytesLength);
        fileSocket.Send(srcPathBytes);
		fileRecvBufIt = 0; fileRecvBufLen = 0;
		recvFile(dstPath);
		fileSocket.Close();
	}
	private static void recvFile(string dstPath) {
		Console.WriteLine("Current path is " + dstPath);
		if (!Directory.Exists(dstPath))
			Directory.CreateDirectory(dstPath);
		while (true) {
			int op = getFileByte();
			switch (op) {
				case 1: {
					int len = getFileByte();
					byte[] fileName = new byte[len];
					for (int i = 0; i < len; ++i)
						fileName[i] = (byte)getFileByte();
					string dstFileName = Path.Combine(dstPath, Encoding.UTF8.GetString(fileName));
					recvFile(dstFileName);
					break;
				}
				case 2: {
					int len = getFileByte();
					byte[] fileName = new byte[len];
					for (int i = 0; i < len; ++i)
						fileName[i] = (byte)getFileByte();
					string dstFileName = Path.Combine(dstPath, Encoding.UTF8.GetString(fileName));
					FileStream fs = new FileStream(dstFileName, FileMode.Create, FileAccess.Write, FileShare.None);
					len = 0;
					for (int i = 0; i < 4; ++i)
						len = len << 8 | getFileByte();
					byte[] outputMessage = new byte[len];
					for (int i = 0; i < len; ++i)
						outputMessage[i] = (byte)getFileByte();
					fs.Write(outputMessage, 0, len);
					fs.Close();
					break;
				}
				case 3:
					return;
			}
		}
    }
	private static int getFileByte() {
		if (fileRecvBufIt >= fileRecvBufLen) {
			fileRecvBufIt = 0; fileRecvBufLen = fileSocket.Receive(fileRecvBuf);
			if (fileRecvBufLen == 0)
				return -1;
		}
		return ((int)fileRecvBuf[fileRecvBufIt++]) & 255;
	}
}
