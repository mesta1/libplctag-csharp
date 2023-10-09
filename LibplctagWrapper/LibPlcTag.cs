using libplctag.NativeImport;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LibplctagWrapper
{
    public class Libplctag : IDisposable
    {
        private readonly Dictionary<string, Int32> _tags;

        public Libplctag()
        {
            _tags = new Dictionary<string, Int32>();
        }

        public void AddTag(Tag tag)
        {
            /* use timeout of 0 for legacy support */
            var ptr = plctag.plc_tag_create(tag.UniqueKey, 0);
            _tags.Add(tag.UniqueKey, ptr);
        }

        public void AddTag(Tag tag, int timeout)
        {
            var ptr = plctag.plc_tag_create(tag.UniqueKey, timeout);
            _tags.Add(tag.UniqueKey, ptr);
        }

        public int GetStatus(Tag tag)
        {
            var status = plctag.plc_tag_status(_tags[tag.UniqueKey]);
            return status;
        }

        public string DecodeError(int error)
        {
            return plctag.plc_tag_decode_error(error);
        }

        public void RemoveTag(Tag tag)
        {
            plctag.plc_tag_destroy(_tags[tag.UniqueKey]);
            _tags.Remove(tag.UniqueKey);
        }

        public int ReadTag(Tag tag, int timeout)
        {
#if true
            var result = plctag.plc_tag_read(_tags[tag.UniqueKey], timeout);
            return result;
#else
            if (_tags.Count != 0)
            {
                return plc_tag_read(_tags[tag.UniqueKey], timeout);
            }
            return Libplctag.PLCTAG_ERR_NOT_FOUND;
#endif
        }

        public int WriteTag(Tag tag, int timeout)
        {
            var result = plctag.plc_tag_write(_tags[tag.UniqueKey], timeout);
            return result;
        }

        /* string types */

        public string GetStringValue(Tag tag, int stringStartOffset, int bufferLength)
        {
            StringBuilder buffer = new StringBuilder(bufferLength);
            int result = plctag.plc_tag_get_string(_tags[tag.UniqueKey], stringStartOffset, buffer, bufferLength);

            if (result == 0)
            {
                return buffer.ToString();
            }
            else
            {
                return string.Empty; // Return an empty string in case of an error.
            }
        }
        public void SetStringValue(Tag tag, int offset, string string_val)
        {
            plctag.plc_tag_set_string(_tags[tag.UniqueKey], offset, string_val);
        }


        /* 64-bit types */

        public UInt64 GetUint64Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_uint64(_tags[tag.UniqueKey], offset);
        }

        public void SetUint64Value(Tag tag, int offset, UInt64 value)
        {
            plctag.plc_tag_set_uint64(_tags[tag.UniqueKey], offset, value);
        }

        public Int64 GetInt64Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_int64(_tags[tag.UniqueKey], offset);
        }

        public void SetInt64Value(Tag tag, int offset, Int64 value)
        {
            plctag.plc_tag_set_int64(_tags[tag.UniqueKey], offset, value);
        }

        public double GetFloat64Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_float64(_tags[tag.UniqueKey], offset);
        }

        public void SetFloat64Value(Tag tag, int offset, double value)
        {
            plctag.plc_tag_set_float64(_tags[tag.UniqueKey], offset, value);
        }

        /* 32-bit types */

        public UInt32 GetUint32Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_uint32(_tags[tag.UniqueKey], offset);
        }

        public void SetUint32Value(Tag tag, int offset, UInt32 value)
        {
            plctag.plc_tag_set_uint32(_tags[tag.UniqueKey], offset, value);
        }

        public Int32 GetInt32Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_int32(_tags[tag.UniqueKey], offset);
        }

        public void SetInt32Value(Tag tag, int offset, Int32 value)
        {
            plctag.plc_tag_set_int32(_tags[tag.UniqueKey], offset, value);
        }

        public float GetFloat32Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_float32(_tags[tag.UniqueKey], offset);
        }

        public void SetFloat32Value(Tag tag, int offset, float value)
        {
            plctag.plc_tag_set_float32(_tags[tag.UniqueKey], offset, value);
        }

        /* 16-bit types */

        public UInt16 GetUint16Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_uint16(_tags[tag.UniqueKey], offset);
        }

        public void SetUint16Value(Tag tag, int offset, UInt16 value)
        {
            plctag.plc_tag_set_uint16(_tags[tag.UniqueKey], offset, value);
        }

        public Int16 GetInt16Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_int16(_tags[tag.UniqueKey], offset);
        }

        public void SetInt16Value(Tag tag, int offset, Int16 value)
        {
            plctag.plc_tag_set_int16(_tags[tag.UniqueKey], offset, value);
        }

        /* 8-bit types */

        public byte GetUint8Value(Tag tag, int offset)
        {
#if true
            return plctag.plc_tag_get_uint8(_tags[tag.UniqueKey], offset);
#else
            if (_tags.Count != 0)
            {
                return plc_tag_get_uint8(_tags[tag.UniqueKey], offset);
            }
            return 0;
#endif
        }

        public void SetUint8Value(Tag tag, int offset, byte value)
        {
            plctag.plc_tag_set_uint8(_tags[tag.UniqueKey], offset, value);
        }

        public sbyte GetInt8Value(Tag tag, int offset)
        {
            return plctag.plc_tag_get_int8(_tags[tag.UniqueKey], offset);
        }

        public void SetInt8Value(Tag tag, int offset, sbyte value)
        {
            plctag.plc_tag_set_int8(_tags[tag.UniqueKey], offset, value);
        }

        /* bits */

        /// <summary>
        /// <returns></returns>
        /// To read a tag that aliases a single bit, set 'index' to value < 0
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="index"></param>
        /// <param name="timeout"></param>
        public bool ReadBool(Tag tag, int index, int timeout)
        {
            var readResult = ReadTag(tag, timeout);
            if (readResult != PLCTAG_STATUS_OK)
            {
                return false;
            }

            // workaround for tags aliasing a single bit
            if (index < 0)
            {
                return Convert.ToBoolean(GetUint8Value(tag, 0));
            }

            if (tag.ElementSize * 8 <= index)
            {
                return false;
            }

            switch (tag.ElementSize)
            {
                case DataType.Int64:  // aka LINT
                    return Convert.ToBoolean((GetUint64Value(tag, 0) >> index) & 1UL);

                case DataType.Int32:  // aka DINT
                    return Convert.ToBoolean((GetUint32Value(tag, 0) >> index) & 1U);

                case DataType.Int16:  // aka INT
                    return Convert.ToBoolean((GetUint16Value(tag, 0) >> index) & 1U);

                case DataType.Int8:  // aka SINT
                    return Convert.ToBoolean((GetUint8Value(tag, 0) >> index) & 1U);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Convenience wrapper for ReadBool()
        /// To read a tag that aliases a single bit, set 'bit' to value < 0
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="bit"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool GetBitValue(Tag tag, int bit, int timeout)
        {
            return ReadBool(tag, bit, timeout);
        }

        /// <summary>
        /// <returns></returns>
        /// To write a tag that aliases a single bit, set 'index' to value < 0
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        public int WriteBool(Tag tag, int index, bool value, int timeout)
        {
            var readResult = ReadTag(tag, timeout);
            if (readResult != PLCTAG_STATUS_OK)
            {
                return readResult;
            }

            // workaround for tags aliasing a single bit
            if (index < 0)
            {
                byte dataByte = GetUint8Value(tag, 0);
                bool dataBit = Convert.ToBoolean(dataByte);
                dataBit = value;
                dataByte = Convert.ToByte(dataBit);
                SetUint8Value(tag, 0, dataByte);
                return WriteTag(tag, timeout);
            }

            if (tag.ElementSize*8 <= index)
            {
                return PLCTAG_ERR_OUT_OF_BOUNDS;
            }
            switch (tag.ElementSize)
            {
                case DataType.Int64:  // aka LINT
                    UInt64 data64 = GetUint64Value(tag, 0);
                    if (value)
                    {
                        data64 |= (UInt64) 1 << index;
                    }
                    else
                    {
                        data64 &= ~((UInt64) 1 << index);
                    }
                    SetUint64Value(tag, 0, data64);
                    break;

                case DataType.Int32:  // aka DINT
                    UInt32 data32 = GetUint32Value(tag, 0);
                    if (value)
                    {
                        data32 |= (UInt32) 1 << index;
                    }
                    else
                    {
                        data32 &= ~((UInt32) 1 << index);
                    }
                    SetUint32Value(tag, 0, data32);
                    break;

                case DataType.Int16:  // aka INT
                    UInt16 data16 = GetUint16Value(tag, 0);
                    if (value)
                    {
                        data16 |= (UInt16)(1 << index);
                    }
                    else
                    {
                        data16 &= (UInt16)~(1 << index);
                    }
                    SetUint16Value(tag, 0, data16);
                    break;

                case DataType.Int8:  // aka SINT
                    byte data8 = GetUint8Value(tag, 0);
                    if (value)
                    {
                        data8 |= (byte)(1 << index);
                    }
                    else
                    {
                        data8 &= (byte)~(1 << index);
                    }
                    SetUint8Value(tag, 0, data8);
                    break;

                default:
                    return PLCTAG_ERR_NOT_ALLOWED;
            }
            return WriteTag(tag, timeout);
        }

        /// <summary>
        /// Convenience wrapper for WriteBool()
        /// To write a tag that aliases a single bit, set 'bit' to value < 0
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="bit"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public int SetBitValue(Tag tag, int bit, bool value, int timeout)
        {
            return WriteBool(tag, bit, value, timeout);
        }

        public void Dispose()
        {
            foreach (var tag in _tags)
            {
                plctag.plc_tag_destroy(tag.Value);
            }
            _tags.Clear();
        }

        

        /* library internal status. */
        public const int PLCTAG_STATUS_PENDING = (int)STATUS_CODES.PLCTAG_STATUS_PENDING; // Operation in progress. Not an error.
        public const int PLCTAG_STATUS_OK = (int)STATUS_CODES.PLCTAG_STATUS_OK; // No error.

        public const int PLCTAG_ERR_NOT_ALLOWED = (int)STATUS_CODES.PLCTAG_ERR_NOT_ALLOWED;
        public const int PLCTAG_ERR_OUT_OF_BOUNDS = (int)STATUS_CODES.PLCTAG_ERR_OUT_OF_BOUNDS;

    }
}
