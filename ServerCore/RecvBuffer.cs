using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
	public class RecvBuffer
	{
		// [r][][w][][][][][][][]
		// 리시브 버퍼를 따로 만들어주는게 코드 정리하기 편하다.
		// 일부분만 처리할 수 있게했다.
		ArraySegment<byte> _buffer;
		// 마우스 커서라고 생각하면된다. 시작하는 위치다.
		// 리드 라이트 위치다. 
		int _readPos;
		int _writePos;

		//TCP의 특성 때문에 100 바이트르 보내도 한 80 바이트만 온다
		//나중에 20 바이트를 다시 받아야한다.
		public RecvBuffer(int bufferSize)
		{
			_buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
		}
		// 실제 데이터 사이즈
		public int DataSize { get { return _writePos - _readPos; } }
		// 프리사이즈는 버퍼에 남은 공간이다.
		//_buffer.Count로 총 버퍼 크기를 가져오면된다. 
		public int FreeSize { get { return _buffer.Count - _writePos; } }

		// 어디서부터 읽을지르 선택한다.
		public ArraySegment<byte> ReadSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
		}

		public ArraySegment<byte> WriteSegment
		{
			//_buffer.Offset + _writePos 쓰기위한 시작 위치다.
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
		}

		// 중간 중간에 버퍼 정리를 해야한다. 
		public void Clean()
		{
			int dataSize = DataSize;
			if (dataSize == 0)
			{
				// 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
				_readPos = _writePos = 0;
			}
			else
			{
				// 남은 찌끄레기가 있으면 시작 위치로 복사 남은 데이터가 있으면
				// dataSize 데이터 사이즈만큼 크기로 복사하고 
				// _buffer.Array를 _buffer.Offset + _readPos에서 
				//  _buffer.Array로, _buffer.Offset의(처음) 위치로 이동한다.
				Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
				_readPos = 0; // r은 시작위치로
				_writePos = dataSize; // w는 데이터 사이즈 위치로 이동한다.
			}
		}

		// 리드가 완료 됐을때 커서위치를 이동해야한다.
		public bool OnRead(int numOfBytes)
		{
			if (numOfBytes > DataSize)
				return false;
			// 리드포지션을 넘버오브바이트만큼 앞으로 이동한다.
			_readPos += numOfBytes;
			return true;
		}

		public bool OnWrite(int numOfBytes)
		{
			if (numOfBytes > FreeSize)
				return false;

			_writePos += numOfBytes;
			return true;
		}
	}
}
