using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibplctagWrapper;

namespace SlcSimple
{
    class Program
    {
        const int DataTimeout = 5000;

        static void Main(string[] args)
        {
            var client = new Libplctag();
            var tag = new Tag("192.168.0.100", CpuType.SLC, "B3:0", DataType.Int16, 1);
            client.AddTag(tag);

            var status = client.GetStatus(tag);
            if (status != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"Error setting up tag internal state.  Error {status}");
                return;
            }

            /* get the data */
            var rc = client.ReadTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");

                return;
            }

            /* print out the data */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                Console.WriteLine($"data[{i}]={client.GetFloat32Value(tag, (i * tag.ElementSize))}");
            }

            /* now test a write */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                var val = client.GetFloat32Value(tag, (i * tag.ElementSize));

                val++;

                Console.WriteLine($"Setting element {i} to {val}");

                client.SetFloat32Value(tag, (i * tag.ElementSize), val);
            }

            rc = client.WriteTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                return;
            }

            /* get the data again*/
            rc = client.ReadTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                return;
            }

            /* print out the data */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                Console.WriteLine($"data[{i}]={client.GetFloat32Value(tag, (i * tag.ElementSize))}");
            }

            client.Dispose();

            Console.ReadKey();
        }

        static void LogError(string error)
        {
            Console.WriteLine(error);
            Console.ReadKey();
        }
    }
}
