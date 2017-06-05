using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibplctagWrapper
{
    public class Libplctag : IDisposable
    {
        private readonly Dictionary<string, IntPtr> _tags;

        public Libplctag()
        {
            _tags = new Dictionary<string, IntPtr>();
        }

        public void AddTag(Tag tag)
        {
            var ptr = plc_tag_create(tag.UniqueKey);
            _tags.Add(tag.UniqueKey, ptr);
        }

        public int GetStatus(Tag tag)
        {
            var status = plc_tag_status(_tags[tag.UniqueKey]);
            return status;
        }

        public string DecodeError(int error)
        {
            var ptr = plc_tag_decode_error(error);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public void RemoveTag(Tag tag)
        {
            plc_tag_destroy(_tags[tag.UniqueKey]);
            _tags.Remove(tag.UniqueKey);
        }

        public int ReadTag(Tag tag, int timeout)
        {
            var result = plc_tag_read(_tags[tag.UniqueKey], timeout);
            return result;
        }

        public int WriteTag(Tag tag, int timeout)
        {
            var result = plc_tag_write(_tags[tag.UniqueKey], timeout);
            return result;
        }

        public int WriteBool(Tag tag, int index, bool value, int timeout)
        {
            var readResult = ReadTag(tag, timeout);
            if (readResult != PLCTAG_STATUS_OK)
            {
                return readResult;
            }
            if (tag.ElementSize*8 <= index)
                return PLCTAG_ERR_OUT_OF_BOUNDS;

            if (tag.ElementSize == DataType.Int32)
            {
                var data = GetUint32Value(tag, 0);
                if (value)
                {
                    data = data | (uint)Math.Pow(2, index);
                }
                else
                {
                    data = data ^ (uint)Math.Pow(2, index);
                }
                SetUint32Value(tag, 0, data);
            }
            else if (tag.ElementSize == DataType.Int16)
            {
                var data = GetUint16Value(tag, 0);
                if (value)
                {
                    data = (ushort)(data | (ushort)Math.Pow(2, index));
                }
                else
                {
                    data = (ushort)(data ^ (ushort)Math.Pow(2, index));
                }
                SetUint16Value(tag, 0, data);
            }
            else if (tag.ElementSize == DataType.Int8)
            {
                var data = GetUint8Value(tag, 0);
                if (value)
                {
                    data = (byte)(data | (byte)Math.Pow(2, index));
                }
                else
                {
                    data = (byte)(data ^ (byte)Math.Pow(2, index));
                }
                SetUint8Value(tag, 0, data);
            }
            else
            {
                return PLCTAG_ERR_NOT_ALLOWED;
            }

            return WriteTag(tag, timeout);
        }

        public ushort GetUint16Value(Tag tag, int offset)
        {
            return plc_tag_get_uint16(_tags[tag.UniqueKey], offset);
        }

        public void SetUint16Value(Tag tag, int offset, ushort value)
        {
            plc_tag_set_uint16(_tags[tag.UniqueKey], offset, value);
        }

        public short GetInt16Value(Tag tag, int offset)
        {
            return plc_tag_get_int16(_tags[tag.UniqueKey], offset);
        }

        public void SetInt16Value(Tag tag, int offset, short value)
        {
            plc_tag_set_int16(_tags[tag.UniqueKey], offset, value);
        }

        public byte GetUint8Value(Tag tag, int offset)
        {
            return plc_tag_get_uint8(_tags[tag.UniqueKey], offset);
        }

        public void SetUint8Value(Tag tag, int offset, byte value)
        {
            plc_tag_set_uint8(_tags[tag.UniqueKey], offset, value);
        }

        public sbyte GetInt8Value(Tag tag, int offset)
        {
            return plc_tag_get_int8(_tags[tag.UniqueKey], offset);
        }

        public void SetInt8Value(Tag tag, int offset, sbyte value)
        {
            plc_tag_set_int8(_tags[tag.UniqueKey], offset, value);
        }

        public float GetFloat32Value(Tag tag, int offset)
        {
            return plc_tag_get_float32(_tags[tag.UniqueKey], offset);
        }

        public void SetFloat32Value(Tag tag, int offset, float value)
        {
            plc_tag_set_float32(_tags[tag.UniqueKey], offset, value);
        }

        public uint GetUint32Value(Tag tag, int offset)
        {
            return plc_tag_get_uint32(_tags[tag.UniqueKey], offset);
        }

        public void SetUint32Value(Tag tag, int offset, uint value)
        {
            plc_tag_set_uint32(_tags[tag.UniqueKey], offset, value);
        }

        public int GetInt32Value(Tag tag, int offset)
        {
            return plc_tag_get_int32(_tags[tag.UniqueKey], offset);
        }

        public void SetInt32Value(Tag tag, int offset, int value)
        {
            plc_tag_set_int32(_tags[tag.UniqueKey], offset, value);
        }

        public void Dispose()
        {
            foreach (var tag in _tags)
            {
                plc_tag_destroy(tag.Value);
            }
            _tags.Clear();
        }

        [DllImport("plctag.dll", EntryPoint = "plc_tag_create", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr plc_tag_create([MarshalAs(UnmanagedType.LPStr)] string lpString);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_destroy", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_destroy(IntPtr tag);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_status(IntPtr tag);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_decode_error", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr plc_tag_decode_error(int error);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_read", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_read(IntPtr tag, int timeout);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_write", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_write(IntPtr tag, int timeout);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_uint16", CallingConvention = CallingConvention.Cdecl)]
        static extern ushort plc_tag_get_uint16(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_int16", CallingConvention = CallingConvention.Cdecl)]
        static extern short plc_tag_get_int16(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_uint16", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_uint16(IntPtr tag, int offset, ushort val);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_int16", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_int16(IntPtr tag, int offset, short val);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_uint8", CallingConvention = CallingConvention.Cdecl)]
        static extern byte plc_tag_get_uint8(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_int8", CallingConvention = CallingConvention.Cdecl)]
        static extern sbyte plc_tag_get_int8(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_uint8", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_uint8(IntPtr tag, int offset, byte val);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_int8", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_int8(IntPtr tag, int offset, sbyte val);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_float32", CallingConvention = CallingConvention.Cdecl)]
        static extern float plc_tag_get_float32(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_float32", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_float32(IntPtr tag, int offset, float val);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_uint32", CallingConvention = CallingConvention.Cdecl)]
        static extern uint plc_tag_get_uint32(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_get_int32", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_get_int32(IntPtr tag, int offset);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_uint32", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_uint32(IntPtr tag, int offset, uint val);

        [DllImport("plctag.dll", EntryPoint = "plc_tag_set_int32", CallingConvention = CallingConvention.Cdecl)]
        static extern int plc_tag_set_int32(IntPtr tag, int offset, int val);



        /* library internal status. */
        public const int PLCTAG_STATUS_PENDING = 1;
        public const int PLCTAG_STATUS_OK = 0;

        /* for reference only: use DecodeError to get the string associated to the error code*/
        public const int PLCTAG_ERR_NULL_PTR = -1;
        public const int PLCTAG_ERR_OUT_OF_BOUNDS = -2;
        public const int PLCTAG_ERR_NO_MEM = -3;
        public const int PLCTAG_ERR_LL_ADD = -4;
        public const int PLCTAG_ERR_BAD_PARAM = -5;
        public const int PLCTAG_ERR_CREATE = -6;
        public const int PLCTAG_ERR_NOT_EMPTY = -7;
        public const int PLCTAG_ERR_OPEN = -8;
        public const int PLCTAG_ERR_SET = -9;
        public const int PLCTAG_ERR_WRITE = -10;
        public const int PLCTAG_ERR_TIMEOUT = -11;
        public const int PLCTAG_ERR_TIMEOUT_ACK = -12;
        public const int PLCTAG_ERR_RETRIES = -13;
        public const int PLCTAG_ERR_READ = -14;
        public const int PLCTAG_ERR_BAD_DATA = -15;
        public const int PLCTAG_ERR_ENCODE = -16;
        public const int PLCTAG_ERR_DECODE = -17;
        public const int PLCTAG_ERR_UNSUPPORTED = -18;
        public const int PLCTAG_ERR_TOO_LONG = -19;
        public const int PLCTAG_ERR_CLOSE = -20;
        public const int PLCTAG_ERR_NOT_ALLOWED = -21;
        public const int PLCTAG_ERR_THREAD = -22;
        public const int PLCTAG_ERR_NO_DATA = -23;
        public const int PLCTAG_ERR_THREAD_JOIN = -24;
        public const int PLCTAG_ERR_THREAD_CREATE = -25;
        public const int PLCTAG_ERR_MUTEX_DESTROY = -26;
        public const int PLCTAG_ERR_MUTEX_UNLOCK = -27;
        public const int PLCTAG_ERR_MUTEX_INIT = -28;
        public const int PLCTAG_ERR_MUTEX_LOCK = -29;
        public const int PLCTAG_ERR_NOT_IMPLEMENTED = -30;
        public const int PLCTAG_ERR_BAD_DEVICE = -31;
        public const int PLCTAG_ERR_BAD_GATEWAY = -32;
        public const int PLCTAG_ERR_REMOTE_ERR = -33;
        public const int PLCTAG_ERR_NOT_FOUND = -34;
        public const int PLCTAG_ERR_ABORT = -35;
        public const int PLCTAG_ERR_WINSOCK = -36;
    }
}
