// "Miscellaneous Utility Library" Software Licence
//
// Version 1.0
//
// Copyright (c) 2004-2008 Jon Skeet and Marc Gravell.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
//
// 3. The end-user documentation included with the redistribution, if
// any, must include the following acknowledgment:
//
// "This product includes software developed by Jon Skeet
// and Marc Gravell. Contact skeet@pobox.com, or see 
// http://www.pobox.com/~skeet/)."
//
// Alternately, this acknowledgment may appear in the software itself,
// if and wherever such third-party acknowledgments normally appear.
//
// 4. The name "Miscellaneous Utility Library" must not be used to endorse 
// or promote products derived from this software without prior written 
// permission. For written permission, please contact skeet@pobox.com.
//
// 5. Products derived from this software may not be called 
// "Miscellaneous Utility Library", nor may "Miscellaneous Utility Library"
// appear in their name, without prior written permission of Jon Skeet.
//
// THIS SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESSED OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL JON SKEET BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE. 

using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CSV.Tests")]

namespace MatthiWare.Csv
{
    internal class NonClosableStream : Stream
    {
        private readonly Stream m_baseStream;
        private bool m_closed = false;

        #region Properties

        public override bool CanRead => CheckClosed(m_baseStream.CanRead);

        public override bool CanSeek => CheckClosed(m_baseStream.CanSeek);

        public override bool CanWrite => CheckClosed(m_baseStream.CanWrite);

        public override long Length => CheckClosed(m_baseStream.Length);

        public override long Position { get => CheckClosed(m_baseStream.Position); set => m_baseStream.Position = CheckClosed(value); }

        public Stream BaseStream => m_baseStream;

        #endregion

        public NonClosableStream(Stream input)
        {
            m_baseStream = input;
        }

        #region Methods

        private bool CheckClosed(bool input) => m_closed ? false : input;

        private void CheckClosed()
        {
            if (m_closed)
                throw new InvalidOperationException("Stream has been closed or disposed");
        }

        private long CheckClosed(long input)
        {
            CheckClosed();

            return input;
        }

#if !DOT_NET_STD

        public override void Close()
        {
            Dispose(true);
        }

#endif

        public override void Flush()
        {
            CheckClosed();

            m_baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckClosed();

            return m_baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckClosed();

            return m_baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CheckClosed();

            m_baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckClosed();

            m_baseStream.Write(buffer, offset, count);
        }

#if !DOT_NET_STD

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckClosed();

            return m_baseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckClosed();

            return m_baseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            CheckClosed();

            return m_baseStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            CheckClosed();

            m_baseStream.EndWrite(asyncResult);
        }

#endif

        /// <summary>
        /// Disposing this <see cref="Stream"/> will not really dispose it.
        /// The stream will just be marked as closed/disposed and will not be usable anymore.
        /// The underlying stream however will still be usable as it has not been disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!m_closed)
                m_baseStream.Flush();

            m_closed = true;
        }

        #endregion
    }
}
