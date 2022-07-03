using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

// 세션이 대리인 역할을 한다.
// 클라이언트쪽에 서버 대리인 역할을 하는게 서버 세션이고
// 서버쪽 클라이언트 대대리인역학을 하는게 클라이언트 세션이다.
// 서버끼리도 db를 통신해야할 수 있어써 세션끼리 이름을 정확하게 나눈다.
namespace DummyClient
{
	class ServerSession : PacketSession
	{
		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");			
		}
		

		public override void OnDisconnected(EndPoint endPoint) 
		{
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
