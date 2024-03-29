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
using System.Text;
using MongoDB.Driver.Conditions;
using MongoDB.Driver.Conversion;

namespace MongoDB.Driver.IO
{
    /// <summary>
    /// Equivalent of System.IO.BinaryWriter, but with either endianness, depending on
    /// the EndianBitConverter it is constructed with.
    /// </summary>
    public class EndianBinaryWriter : IDisposable
    {
        #region Fields not directly related to properties
        /// <summary>
        /// Whether or not this writer has been disposed yet.
        /// </summary>
        bool disposed = false;
        /// <summary>
        /// Buffer used for temporary storage during conversion from primitives
        /// </summary>
        byte[] buffer = new byte[16];
        /// <summary>
        /// Buffer used for Write(char)
        /// </summary>
        char[] charBuffer = new char[1];
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new binary writer with the given bit converter, writing
        /// to the given stream, using UTF-8 encoding.
        /// </summary>
        /// <param name="bitConverter">Converter to use when writing data</param>
        /// <param name="stream">Stream to write data to</param>
        public EndianBinaryWriter(EndianBitConverter bitConverter,
            Stream stream)
            : this(bitConverter, stream, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Constructs a new binary writer with the given bit converter, writing
        /// to the given stream, using the given encoding.
        /// </summary>
        /// <param name="bitConverter">Converter to use when writing data</param>
        /// <param name="stream">Stream to write data to</param>
        /// <param name="encoding">Encoding to use when writing character data</param>
        public EndianBinaryWriter(EndianBitConverter bitConverter, Stream stream, Encoding encoding)
        {
            if (bitConverter == null)
            {
                throw new ArgumentNullException("bitConverter");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream isn't writable", "stream");
            }
            this.stream = stream;
            this.bitConverter = bitConverter;
            this.encoding = encoding;
        }
        #endregion

        #region Properties
        EndianBitConverter bitConverter;
        /// <summary>
        /// The bit converter used to write values to the stream
        /// </summary>
        /// <value>The bit converter.</value>
        public EndianBitConverter BitConverter
        {
            get { return bitConverter; }
        }

        Encoding encoding;
        /// <summary>
        /// The encoding used to write strings
        /// </summary>
        /// <value>The encoding.</value>
        public Encoding Encoding
        {
            get { return encoding; }
        }

        Stream stream;
        /// <summary>
        /// Gets the underlying stream of the EndianBinaryWriter.
        /// </summary>
        /// <value>The base stream.</value>
        public Stream BaseStream
        {
            get { return stream; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Closes the writer, including the underlying stream.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public void Flush()
        {
            CheckDisposed();
            stream.Flush();
        }

        /// <summary>
        /// Seeks within the stream.
        /// </summary>
        /// <param name="offset">Offset to seek to.</param>
        /// <param name="origin">Origin of seek operation.</param>
        public void Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            stream.Seek(offset, origin);
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get { CheckDisposed(); return Convert.ToInt32(stream.Position); } }

        /// <summary>
        /// Writes a boolean value to the stream. 1 byte is written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(bool value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 1);
        }

        /// <summary>
        /// Writes a 16-bit signed integer to the stream, using the bit converter
        /// for this writer. 2 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(short value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 2);
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the stream, using the bit converter
        /// for this writer. 4 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(int value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 4);
        }

        /// <summary>
        /// Writes the straight.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteStraight(int value)
        {
            WriteInternal(System.BitConverter.GetBytes(value), 4);
        }

        /// <summary>
        /// Writes a 64-bit signed integer to the stream, using the bit converter
        /// for this writer. 8 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(long value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 8);
        }

        /// <summary>
        /// Writes a 16-bit unsigned integer to the stream, using the bit converter
        /// for this writer. 2 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(ushort value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 2);
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to the stream, using the bit converter
        /// for this writer. 4 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(uint value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 4);
        }

        /// <summary>
        /// Writes a 64-bit unsigned integer to the stream, using the bit converter
        /// for this writer. 8 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(ulong value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 8);
        }

        /// <summary>
        /// Writes a single-precision floating-point value to the stream, using the bit converter
        /// for this writer. 4 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(float value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 4);
        }

        /// <summary>
        /// Writes a double-precision floating-point value to the stream, using the bit converter
        /// for this writer. 8 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(double value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 8);
        }

        /// <summary>
        /// Writes a decimal value to the stream, using the bit converter for this writer.
        /// 16 bytes are written.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(decimal value)
        {
            bitConverter.CopyBytes(value, buffer, 0);
            WriteInternal(buffer, 16);
        }

        /// <summary>
        /// Writes a signed byte to the stream.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(byte value)
        {
            buffer[0] = value;
            WriteInternal(buffer, 1);
        }

        /// <summary>
        /// Writes an unsigned byte to the stream.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(sbyte value)
        {
            buffer[0] = unchecked((byte)value);
            WriteInternal(buffer, 1);
        }

        /// <summary>
        /// Writes an array of bytes to the stream.
        /// </summary>
        /// <param name="value">The values to write</param>
        public void Write(byte[] value)
        {
            if (value == null)
            {
                throw (new System.ArgumentNullException("value"));
            }
            WriteInternal(value, value.Length);
        }

        /// <summary>
        /// Writes a portion of an array of bytes to the stream.
        /// </summary>
        /// <param name="value">An array containing the bytes to write</param>
        /// <param name="offset">The index of the first byte to write within the array</param>
        /// <param name="count">The number of bytes to write</param>
        public void Write(byte[] value, int offset, int count)
        {
            CheckDisposed();
            stream.Write(value, offset, count);
        }

        /// <summary>
        /// Writes a single character to the stream, using the encoding for this writer.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(char value)
        {
            charBuffer[0] = value;
            Write(charBuffer);
        }

        /// <summary>
        /// Writes an array of characters to the stream, using the encoding for this writer.
        /// </summary>
        /// <param name="value">An array containing the characters to write</param>
        public void Write(char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CheckDisposed();
            byte[] data = Encoding.GetBytes(value, 0, value.Length);
            WriteInternal(data, data.Length);
        }

        /// <summary>
        /// Writes a string to the stream, using the encoding for this writer.
        /// </summary>
        /// <param name="value">The value to write. Must not be null.</param>
        /// <exception cref="ArgumentNullException">value is null</exception>
        public void WriteLengthPrefixedNullTerminated(string value)
        {
            Condition.Requires(value, "value").IsNotNull();
            CheckDisposed();
            byte[] data = Encoding.GetBytes(value);
            Write(data.Length + 1);
            WriteInternal(data, data.Length);
            Write((byte)0);
        }

        /// <summary>
        /// Writes a string to the stream, using the encoding for this writer.
        /// </summary>
        /// <param name="value">The value to write. Must not be null.</param>
        /// <exception cref="ArgumentNullException">value is null</exception>
        public void WriteNullTerminated(string value)
        {
            Condition.Requires(value, "value").IsNotNull();
            CheckDisposed();
            byte[] data = Encoding.GetBytes(value);
            WriteInternal(data, data.Length);
            Write((byte)0);
        }

        /// <summary>
        /// Writes a 7-bit encoded integer from the stream. This is stored with the least significant
        /// information first, with 7 bits of information per byte of value, and the top
        /// bit as a continuation flag.
        /// </summary>
        /// <param name="value">The 7-bit encoded integer to write to the stream</param>
        public void Write7BitEncodedInt(int value)
        {
            CheckDisposed();
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Value must be greater than or equal to 0.");
            }
            int index = 0;
            while (value >= 128)
            {
                buffer[index++] = (byte)((value & 0x7f) | 0x80);
                value = value >> 7;
                index++;
            }
            buffer[index++] = (byte)value;
            stream.Write(buffer, 0, index);
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Checks whether or not the writer has been disposed, throwing an exception if so.
        /// </summary>
        void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("EndianBinaryWriter");
            }
        }

        /// <summary>
        /// Writes the specified number of bytes from the start of the given byte array,
        /// after checking whether or not the writer has been disposed.
        /// </summary>
        /// <param name="bytes">The array of bytes to write from</param>
        /// <param name="length">The number of bytes to write</param>
        void WriteInternal(byte[] bytes, int length)
        {
            CheckDisposed();
            stream.Write(bytes, 0, length);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes of the underlying stream.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Flush();
                disposed = true;
                ((IDisposable)stream).Dispose();
            }
        }
        #endregion
    }
}
