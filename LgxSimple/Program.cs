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
            var tag = new Tag("10.206.1.39", "1, 0", CpuType.LGX, "TestDINTArray[0]", DataType.Int32, 10);

            /* create the tag */
            client.AddTag(tag);

            /* let the connect succeed we hope */
            while (client.GetStatus(tag) == Libplctag.PLCTAG_STATUS_PENDING)
            {
                Thread.Sleep(100);
            }

            if (client.GetStatus(tag) != Libplctag.PLCTAG_STATUS_OK)
            {
                Console.WriteLine($"Error setting up tag internal state. Error {client.DecodeError(client.GetStatus(tag))}\n");
                return;
            }

            /* get the data */
            var rc = client.ReadTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                Console.WriteLine($"ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(client.GetStatus(tag))}\n" );
                return;
            }

            /* print out the data */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                Console.WriteLine($"data[{i}]={ client.GetInt32Value(tag, (i * tag.ElementSize))}\n");
            }

            /* now test a write */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                var val = client.GetInt32Value(tag, (i * tag.ElementSize));

                val = val + 1;

                Console.WriteLine($"Setting element {i} to {val}\n");

                client.SetInt32Value(tag, (i * tag.ElementSize), val);
            }

            rc = client.WriteTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                return;
            }

            /* get the data again*/
            rc = client.ReadTag(tag, DataTimeout);

            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}\n" );
                return;
            }

            /* print out the data */
            for (int i = 0; i < tag.ElementCount; i++)
            {
                Console.WriteLine($"data[{0}]={1}\n", i, client.GetInt32Value(tag, (i * tag.ElementSize)));
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
