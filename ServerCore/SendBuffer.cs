using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

// 리시브 버퍼 만들었으면 샌드 버퍼도 많들어서 데이터를 처리 할 수 있게해야죠.
// 기존의 샌드는 외부에서 보낼 데이터와 받을 버퍼를 만든 다음 send라는 인터페이스를 통해 보냈다.
// 리스브 버퍼는 세션마다 자기의 고유 버퍼를 가지고 있었다.
// 세션과 리시브 버퍼가 1:1 관계였다.
// 샌드 버퍼는 외부에서 만든 다음 필요할때마다 클라이언트가 보내는 방식을 진행한다.
// 버퍼크기가 가변적일때 라이트 버퍼도 아주 크게 잡아두고 잘라서 쓰자.
// 샌드 버퍼는 일회용으로만 사용해야한다.
namespace ServerCore
{
	// 샌드버퍼를 사용하기 쉽게 헬퍼 클래스를 만들자.
	
	public class SendBufferHelper
	{
		// 전역으로 만들면 스레드끼리 서로 경합을 한다.
		// 스레드 로컬로 만들면 전역은 전역인데 나의 로컬에서만 사용할 수 있는 전역으로 하자.
		public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

		public static int ChunkSize { get; set; } = 65535 * 100;

		public static ArraySegment<byte> Open(int reserveSize)
		{
			if (CurrentBuffer.Value == null)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			if (CurrentBuffer.Value.FreeSize < reserveSize)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			return CurrentBuffer.Value.Open(reserveSize);
		}

		public static ArraySegment<byte> Close(int usedSize)
		{
			return CurrentBuffer.Value.Close(usedSize);
		}
	}

	public class SendBuffer
	{
		// [][][][][][][][][u][]
		// 리시브 버퍼에서 라이트에 해당하는게 샌드 버퍼에서 u라고 생각하면된다.
		// 멀티 스레드 환경에서 샌드 버퍼에 락을 안잡아도 안전한 이유가 최초에  한 번 데이터를 주고  버퍼를 수정하는게아니라 
		// 데이터를 읽기만하기 떄문에 안전하다. 데이터를 읽는 놈은 최초에 open으로 한 번 열고나서다.
		// TCP는 100바이트 보냈다고 100바이트 다오는게 아니라 중간 중간 짤려서 올 수 있다.
		// 패킷이 완전체로 왔는지 짤려서 왔는지 확인 할 수 있는 무언가가 필요하다.
		byte[] _buffer;
		int _usedSize = 0;

		// 남은 공간 
		public int FreeSize { get { return _buffer.Length - _usedSize; } }

		public SendBuffer(int chunkSize)
		{
			_buffer = new byte[chunkSize];
		}

		//
		public ArraySegment<byte> Open(int reserveSize)
		{
			// 예약 공간이 더 크면 널을 리턴하자.
			if (reserveSize > FreeSize)
				return null;

			return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
		}

		public ArraySegment<byte> Close(int usedSize)
		{
			ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
			_usedSize += usedSize;
			return segment;
		}
	}
}
