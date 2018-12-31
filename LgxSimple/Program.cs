using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibplctagWrapper;

namespace LgxSimple
{
    class Program
    {
        const int DataTimeout = 5000;

        static void Main(string[] args)
        {
            var client = new Libplctag();

            const string PLC_IP = "192.168.10.10";
            const string PLC_PATH = "1, 0";
            const CpuType PLC_TYPE = CpuType.LGX;

            var TAG_CONTROL_WORD = new Tag(PLC_IP, PLC_PATH, PLC_TYPE, "CONTROL_WORD", DataType.Int32, 1);
            var TAG_SHEET_LENGTH = new Tag(PLC_IP, PLC_PATH, PLC_TYPE, "SHEET_LENGTH", DataType.Float32, 1);
            var TAG_STATUS_WORD = new Tag(PLC_IP, PLC_PATH, PLC_TYPE, "STATUS_WORD", DataType.DINT, 1);
            var TAG_PC_HEARTBEAT = new Tag(PLC_IP, PLC_PATH, PLC_TYPE, "PC_HEARTBEAT", DataType.Int8, 1);
            var TAG_MOVE_HOME = new Tag(PLC_IP, PLC_PATH, PLC_TYPE, "MOVE_HOME", DataType.SINT, 1);
            var TAG_LINTY = new Tag(PLC_IP, PLC_PATH, PLC_TYPE, "LINTY", DataType.LINT, 1);

            var tags = new List<Tag> {
                TAG_CONTROL_WORD,
                TAG_SHEET_LENGTH,
                TAG_STATUS_WORD,
                TAG_PC_HEARTBEAT,
                TAG_MOVE_HOME,
                TAG_LINTY,
            };

            /* create the tag(s) */
            foreach (var tag in tags)
            {
                client.AddTag(tag);
            }

            /* let the connect succeed we hope */
            foreach (var tag in tags)
            {
                while (client.GetStatus(tag) == Libplctag.PLCTAG_STATUS_PENDING)
                {
                    Thread.Sleep(100);
                }

                if (client.GetStatus(tag) != Libplctag.PLCTAG_STATUS_OK)
                {
                    Console.WriteLine($"Error setting up '{tag.Name}' internal state. Error {client.DecodeError(client.GetStatus(tag))}\n");
                    return;
                }
            }

            /* get the data */
            int rc;
            foreach (var tag in tags)
            {
                rc = client.ReadTag(tag, DataTimeout);
                if (rc != Libplctag.PLCTAG_STATUS_OK)
                {
                    Console.WriteLine($"ERROR: Unable to read the '{tag.Name}' data! Got error code {rc}: {client.DecodeError(client.GetStatus(tag))}\n" );
                    return;
                }
            }

            /* print out the data */
            Console.WriteLine("\n\n{{{{{{{{{{{{{{{{{{{{{\n");

            Console.WriteLine($"initial 'TAG_CONTROL_WORD' = { client.GetUint32Value(TAG_CONTROL_WORD, 0)}\n");
            Console.WriteLine($"initial 'SHEET_LENGTH' = { client.GetFloat32Value(TAG_SHEET_LENGTH, 0)}\n");
            Console.WriteLine($"initial 'STATUS_WORD' = { client.GetUint32Value(TAG_STATUS_WORD, 0)}\n");
            Console.WriteLine($"initial 'PC_HEARTBEAT' = { client.GetUint8Value(TAG_PC_HEARTBEAT, 0)}\n");
            Console.WriteLine($"initial 'MOVE_HOME' = { client.GetBitValue(TAG_MOVE_HOME, -1, DataTimeout)}\n");
            Console.WriteLine($"initial 'LINTY' = { client.GetUint64Value(TAG_LINTY, 0)}\n");
            Console.WriteLine($"initial 'CONTROL_WORD.2 (JOG_REV)' = { client.GetBitValue(TAG_CONTROL_WORD, 2, DataTimeout)}\n");

            Console.WriteLine("---------------------\n");

            /* now test a write */

            /* TAG_CONTROL_WORD */
            var controlVal = client.GetUint32Value(TAG_CONTROL_WORD, 0);
            if (controlVal == 0)
                controlVal = 1;
            else
                controlVal = controlVal * 2;
            Console.WriteLine($"setting 'TAG_CONTROL_WORD' = {controlVal}\n");
            client.SetUint32Value(TAG_CONTROL_WORD, 0, controlVal);

            /* TAG_SHEET_LENGTH */
            float shellLenVal = client.GetFloat32Value(TAG_SHEET_LENGTH, 0);
            shellLenVal = shellLenVal + (float)0.25;
            Console.WriteLine($"setting 'SHEET_LENGTH' = {shellLenVal}\n");
            client.SetFloat32Value(TAG_SHEET_LENGTH, 0, shellLenVal);

            /* TAG_STATUS_WORD */
            var statusVal = client.GetUint32Value(TAG_STATUS_WORD, 0);
            if (statusVal == 0)
                statusVal = 1;
            else
                statusVal = statusVal * 2;
            Console.WriteLine($"setting 'STATUS_WORD' = {statusVal}\n");
            client.SetUint32Value(TAG_STATUS_WORD, 0, statusVal);

            /* TAG_PC_HEARTBEAT */
            byte pcHeartbeatByteVal = client.GetUint8Value(TAG_PC_HEARTBEAT, 0);
            bool pcHeartbeatBitVal = Convert.ToBoolean(pcHeartbeatByteVal);
            pcHeartbeatBitVal = !pcHeartbeatBitVal;
            pcHeartbeatByteVal = Convert.ToByte(pcHeartbeatBitVal);
            Console.WriteLine($"setting 'PC_HEARTBEAT' = {pcHeartbeatByteVal}\n");
            client.SetUint8Value(TAG_PC_HEARTBEAT, 0, pcHeartbeatByteVal);

            /* TAG_MOVE_HOME */
            bool moveHomeVal = client.GetBitValue(TAG_MOVE_HOME, -1, DataTimeout);
            moveHomeVal = !moveHomeVal;
            Console.WriteLine($"setting 'MOVE_HOME' = {moveHomeVal}\n");
            rc = client.SetBitValue(TAG_MOVE_HOME, -1, moveHomeVal, DataTimeout);
            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to write the 'MOVE_HOME' data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                return;
            }

            /* TAG_LINTY */
            var lintyVal = client.GetUint64Value(TAG_LINTY, 0);
            if (lintyVal == 0)
                lintyVal = 1;
            else
                lintyVal = lintyVal * 2;
            Console.WriteLine($"setting 'LINTY' = {lintyVal}\n");
            client.SetUint64Value(TAG_LINTY, 0, lintyVal);

            /* TAG_CONTROL_WORD.2 (JOG_REV) */
            bool jogRevVal = client.GetBitValue(TAG_CONTROL_WORD, 2, DataTimeout);
            jogRevVal = !jogRevVal;
            Console.WriteLine($"setting 'CONTROL_WORD.2 (JOG_REV)' = {jogRevVal}\n");
            rc = client.SetBitValue(TAG_CONTROL_WORD, 2, jogRevVal, DataTimeout);
            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to write the 'CONTROL_WORD.2 (JOG_REV)' data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                return;
            }

            foreach (var tag in tags)
            {
                rc = client.WriteTag(tag, DataTimeout);
                if (rc != Libplctag.PLCTAG_STATUS_OK)
                {
                    if (rc == Libplctag.PLCTAG_ERR_NOT_ALLOWED)
                    {
                        Console.WriteLine($"READ-ONLY TAG: Unable to write the '{tag.Name}' data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                        continue;
                    }
                    LogError($"ERROR: Unable to write the '{tag.Name}' data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                    return;
                }
            }

            Console.WriteLine("=====================\n");

            /* get the data again */
            foreach (var tag in tags)
            {
                rc = client.ReadTag(tag, DataTimeout);
                if (rc != Libplctag.PLCTAG_STATUS_OK)
                {
                    LogError($"ERROR: Unable to read the '{tag.Name}' data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                    return;
                }
            }

            /* print out the data */
            Console.WriteLine($"latest 'TAG_CONTROL_WORD' = { client.GetUint32Value(TAG_CONTROL_WORD, 0) }\n");
            Console.WriteLine($"latest 'SHEET_LENGTH' = { client.GetFloat32Value(TAG_SHEET_LENGTH, 0) }\n");
            Console.WriteLine($"latest 'STATUS_WORD' = { client.GetUint32Value(TAG_STATUS_WORD, 0) }\n");
            Console.WriteLine($"latest 'PC_HEARTBEAT' = { client.GetUint8Value(TAG_PC_HEARTBEAT, 0) }\n");
            Console.WriteLine($"latest 'MOVE_HOME' = { client.GetBitValue(TAG_MOVE_HOME, -1, DataTimeout) }\n");
            Console.WriteLine($"latest 'LINTY' = { client.GetUint8Value(TAG_LINTY, 0) }\n");
            Console.WriteLine($"latest 'CONTROL_WORD.2 (JOG_REV)' = { client.GetBitValue(TAG_CONTROL_WORD, 2, DataTimeout) }\n");

            Console.WriteLine("}}}}}}}}}}}}}}}}}}}}}\n");

            client.Dispose();

            //Console.ReadKey();
        }

        static void LogError(string error)
        {
            Console.WriteLine(error);
            Console.ReadKey();
        }
    }
}
