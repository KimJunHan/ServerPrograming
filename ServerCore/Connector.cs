 using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

// 서버 코어는 라이브러리로만 사용할 예정이다.
// 서버 코어는 속성으로 클래스 라이브러리 형태로 한다.
// 서버와 더미 클라이언트는 추가-참조에서 servercore를 참조한다.  
// 서버코어는 컨텐츠단인데 세션만 들고와서 쓰고 이벤트시리즈만 재정의해서 사용한다.
namespace ServerCore
{
	// 서버를  분산 처리를 할지 결정해야한다. 
	// 서버끼리 연결할때 커넥터가 필수적으로 들어가야한다.
	
	public class Connector
	{
		Func<Session> _sessionFactory;

		// 펑크세선을 인자로 받는다.
		public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				// 휴대폰 설정
				Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				_sessionFactory = sessionFactory;

				SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.Completed += OnConnectCompleted;
				// 연결하는 부분으 상대방의 주소를 넣어야한다.
				args.RemoteEndPoint = endPoint;
				// 유저토큰으로 원하는 정보를 전달 할 수 있다.
				args.UserToken = socket;

				RegisterConnect(args);
			}
		}

		// ㅅ
		void RegisterConnect(SocketAsyncEventArgs args)
		{
			// 소켓으로 타입변환을 해야한다.
			Socket socket = args.UserToken as Socket;
			// 널이면 린터해준다.
			if (socket == null)
				return;

			bool pending = socket.ConnectAsync(args);
			if (pending == false)
				OnConnectCompleted(null, args);
		}

		void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
				// Func로 만든 세션을 이용하자.
				Session session = _sessionFactory.Invoke();
				session.Start(args.ConnectSocket);
				session.OnConnected(args.RemoteEndPoint);
			}
			else
			{
				Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
			}
		}
	}
}
