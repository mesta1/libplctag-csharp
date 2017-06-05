using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LibplctagWrapper;

namespace SlcLoop
{
    class Program
    {
        const int DataTimeout = 5000;

        private static System.Timers.Timer timer;
        private static Libplctag client;
        private static bool _flag;

        static void Main(string[] args)
        {
            client = new Libplctag();
            var tag1 = new Tag("192.168.0.100", CpuType.SLC, "F8:0", DataType.Float32, 1);
            var tag2 = new Tag("192.168.0.100", CpuType.SLC, "N7:0", DataType.Int16, 20);
            var tag3 = new Tag("192.168.0.100", CpuType.SLC, "B3:0", DataType.Int16, 10);
            var tag4 = new Tag("192.168.0.100", CpuType.SLC, "O0:0.0", DataType.Int16, 5);
            var tags = new List<Tag>() {tag1, tag2, tag3, tag4};
            foreach (var tag in tags)
            {
                client.AddTag(tag);
                var status = client.GetStatus(tag);
                if (status != Libplctag.PLCTAG_STATUS_OK)
                {
                    LogError($"{tag.Name} Error setting up tag internal state.  Error {status}");
                    return;
                }
            }

            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += OnTimerTick;
            timer.Enabled = true;


            var noError = true;
            while (noError)
            {
                Console.WriteLine(DateTime.Now);
                noError = RefreshTags(tags, client);
                Thread.Sleep(100);
            }

            Console.ReadKey();
        }

        private static void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            var tag = new Tag("192.168.0.100", CpuType.SLC, "B3:0", DataType.Int16, 1);
            client.AddTag(tag);
            var rc = client.WriteBool(tag, 0, _flag, DataTimeout);
            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                return;
            }
            rc = client.WriteBool(tag, 15, _flag, DataTimeout);
            if (rc != Libplctag.PLCTAG_STATUS_OK)
            {
                LogError($"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                return;
            }
            client.RemoveTag(tag);
            _flag = !_flag;
            timer.Start();
        }



        private static bool RefreshTags(List<Tag> tags, Libplctag client)
        {
            foreach (var tag in tags)
            {
                if (tag.Name.StartsWith("F"))
                {
                    /* get the data */
                    var rc = client.ReadTag(tag, DataTimeout);

                    if (rc != Libplctag.PLCTAG_STATUS_OK)
                    {
                        LogError(
                            $"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                        return false;
                    }

                    /* print out the data */
                    for (int i = 0; i < tag.ElementCount; i++)
                    {
                        Console.WriteLine($"{tag.Name} data[{i}]={client.GetFloat32Value(tag, (i*tag.ElementSize))}");
                    }

                    /* now test a write */
                    for (int i = 0; i < tag.ElementCount; i++)
                    {
                        var val = client.GetFloat32Value(tag, (i*tag.ElementSize));

                        val++;
                        if (val > 1000)
                            val = 0;
                        Console.WriteLine($"{tag.Name} Setting element {i} to {val}");

                        client.SetFloat32Value(tag, (i*tag.ElementSize), val);
                    }

                    rc = client.WriteTag(tag, DataTimeout);

                    if (rc != Libplctag.PLCTAG_STATUS_OK)
                    {
                        LogError(
                            $"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                        return false;
                    }

                    /* get the data again*/
                    rc = client.ReadTag(tag, DataTimeout);

                    if (rc != Libplctag.PLCTAG_STATUS_OK)
                    {
                        LogError(
                            $"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                        return false;
                    }

                    /* print out the data */
                    for (int i = 0; i < tag.ElementCount; i++)
                    {
                        Console.WriteLine($"{tag.Name} data[{i}]={client.GetFloat32Value(tag, (i*tag.ElementSize))}");
                    }
                }
                else
                {
                    /* get the data */
                    var rc = client.ReadTag(tag, DataTimeout);

                    if (rc != Libplctag.PLCTAG_STATUS_OK)
                    {
                        LogError(
                            $"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                        return false;
                    }

                    /* print out the data */
                    for (int i = 0; i < tag.ElementCount; i++)
                    {
                        Console.WriteLine($"{tag.Name} data[{i}]={client.GetInt16Value(tag, (i*tag.ElementSize))}");
                    }

                    if (!tag.Name.Contains("O") && !tag.Name.Contains("I")) // we can't write on I/O
                    {
                        /* now test a write */
                        for (int i = 0; i < tag.ElementCount; i++)
                        {
                            var val = client.GetInt16Value(tag, (i*tag.ElementSize));

                            val++;
                            if (val > 1000)
                            {
                                val = 0;
                            }
                            Console.WriteLine($"{tag.Name} Setting element {i} to {val}");

                            client.SetInt16Value(tag, (i*tag.ElementSize), val);
                        }

                        rc = client.WriteTag(tag, DataTimeout);

                        if (rc != Libplctag.PLCTAG_STATUS_OK)
                        {
                            LogError(
                                $"{tag.Name} ERROR: Unable to write the data! Got error code {rc}: {client.DecodeError(rc)}");
                            return false;
                        }

                        /* get the data again*/
                        rc = client.ReadTag(tag, DataTimeout);

                        if (rc != Libplctag.PLCTAG_STATUS_OK)
                        {
                            LogError(
                                $"{tag.Name} ERROR: Unable to read the data! Got error code {rc}: {client.DecodeError(rc)}");
                            return false;
                        }

                        /* print out the data */
                        for (int i = 0; i < tag.ElementCount; i++)
                        {
                            Console.WriteLine($"{tag.Name} data[{i}]={client.GetInt16Value(tag, (i*tag.ElementSize))}");
                        }
                    }
                }
            }
            return true;
        }

        static void LogError(string error)
        {
            Console.WriteLine(error);
            Console.ReadKey();
        }
    }
}
