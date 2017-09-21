using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibplctagWrapper;

namespace Slc500Strings
{
    class Program
    {
        const int DataTimeout = 5000;

        static void Main(string[] args)
        {
            var client = new Libplctag();
            var tag = new Tag("192.168.0.100", CpuType.SLC, "ST48:0", DataType.String, 1);
            client.AddTag(tag);

            if (client.GetStatus(tag) != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"{tag.Name} Error setting up tag internal state. Error {client.DecodeError(client.GetStatus(tag))}\n");
                return;
            }

            /* get the data */
            var rc = client.ReadTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}\n");
                return;
            }

            /* print out the data */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                var sb = new StringBuilder();
                for (int j = 0; j < tag.ElementSize; j++)
                {
                    sb.Append((char)client.GetUint8Value(tag, (i * tag.ElementSize) + j));
                }

                Console.WriteLine($"string {i} ({82} chars) = {sb.ToString()}\n");
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
