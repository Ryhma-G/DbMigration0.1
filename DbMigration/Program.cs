using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Cassandra;
using Cassandra.Data.Linq;

namespace DbMigration
{
    //measurement table context
   public class MeasurementContext : DbContext
    {
        public MeasurementContext() : base("SavoniaMeasurementsV2Entities") { }
        public DbSet<Measurement> Measurement { get; set; }
    }
    //Data table context
    public class DataContext : DbContext
    {
        public DataContext() : base("SavoniaMeasurementsV2Entities") { }
        public DbSet<Data> Data { get; set; }
    }
    public class BigContext : DbContext
    {
        public BigContext() : base("SavoniaMeasurementsV2Entities") { }
        public DbSet<Measurement> Measurement { get; set; }
        public DbSet<Data> Data { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {

            //Cassandra connection
            var cluster = Cluster.Builder()
                                .AddContactPoints("Samidb.ky.local")
                                .WithPort(9042)

                                .Build();

            // Connect to the nodes using a keyspace
            var session = cluster.Connect("samiv4");

            
            /*
                        using (var context = new BigContext())
                        {
                            string TimeBucket = DateTime.Now.ToString("yyyy-MM");
                            var cqlquery = session.Prepare("INSERT INTO samiv4.measurementdata(providerid, sensortag, timebucket, timestamp)  values(?,?,?,?); ");
                            var batch = new BatchStatement();

                            try
                            {
                                //fetch data from Data table
                                var data = (from da in context.Data where da.MeasurementID >= 0 && da.MeasurementID <= 1 select da);
                                var measurement = (from me in context.Measurement where me.ID >= 1 && me.ID < 10 select me);

                                foreach (var Drow in data)
                                {
                                     Console.WriteLine("ID: " + Drow.MeasurementID + " Tag: " + Drow.Tag + " Value: " + Drow.Value + " LongValue: " + Drow.LongValue + " TextValue: " + Drow.TextValue +
                                       " BinaryValue: " + Drow.BinaryValue + " XmlValue: " + Drow.XmlValue);
                                    batch.Add(cqlquery.Bind(Drow.Tag));

                                    foreach (var Mrow in measurement)
                                    {
                                        Console.WriteLine(" ID: " + Mrow.ID + " ProviderID: " + Mrow.ProviderID + " Object: " + Mrow.Object + " Tag: " + Mrow.Tag + " Timestamp: " + Mrow.Timestamp + " Note: " + Mrow.Note +
                                          " Location: " + Mrow.Location + " RowCreateTimestamp: " + Mrow.RowCreatedTimestamp + " KeyId: " + Mrow.KeyId);

                                        batch.Add(cqlquery.Bind(Mrow.ProviderID, TimeBucket, Mrow.Timestamp));
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }



                            try
                            {
                                //fetch data from Measurement table
                                var measurement = (from me in context.Measurement where me.ID >= 1 && me.ID < 10 select me);
                                /*
                                foreach (var Mrow in measurement)
                                {
                                    //Console.WriteLine(" ID: " + row.ID + " ProviderID: " + row.ProviderID + " Object: " + row.Object + " Tag: " + row.Tag + " Timestamp: " + row.Timestamp + " Note: " + row.Note +
                                      //  " Location: " + row.Location + " RowCreateTimestamp: " + row.RowCreatedTimestamp + " KeyId: " + row.KeyId);

                                        batch.Add(cqlquery.Bind(Mrow.ProviderID, Mrow.Tag, TimeBucket, Mrow.Timestamp));
                                }
                                session.Execute(batch);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }
            */

            var cqlquery = session.Prepare("INSERT INTO samiv4.measurementdata(providerid, sensortag, timebucket, timestamp, measurementobject, measurementtag, location) values(?,?,?,?,?,?,?); ");
            //var cqlquery2 = session.Prepare("INSERT INTO samiv4.measurementdata(sensortag) values(?); ");
            var batch = new BatchStatement();
            // var batch2 = new BatchStatement();

            using (var context3 = new BigContext())
            {
                string TimeBucket = DateTime.Now.ToString("yyyy-MM");

                try
                {
                    //fetch data from measurement and data tables
                    var entrypoint = (from m in context3.Measurement
                                      join d in context3.Data on m.ID equals d.MeasurementID
                                      where m.ID == d.MeasurementID
                                      select new
                                      {
                                          providerid = m.ProviderID,
                                          sensortag = d.Tag,
                                          timebucket = TimeBucket,
                                          timestamp = m.Timestamp,
                                          measurementobject = m.Object,
                                          measurementtag = m.Tag,
                                          location = m.Location,
                                          doublevalue = d.LongValue,
                                          decimalvalue = d.Value,
                                          longvalue = d.LongValue,
                                          bigintvalue = m.ID,
                                          listvalue = m.Tag,
                                          mapvalue = m.RowCreatedTimestamp,
                                          note = m.Note,
                                          textvalue = d.TextValue,
                                          binaryvalue = d.BinaryValue
                                      }).Take(700);

                    foreach(var row in entrypoint)
                    {
                        Console.WriteLine("providerid: " + row.providerid + " sensortag: " + row.sensortag + " timebucket: " + TimeBucket + " timestamp: " + row.timestamp);
                        batch.Add(cqlquery.Bind(row.providerid, row.sensortag, row.timebucket, row.timestamp, row.measurementobject, row.measurementtag, row.location));
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                /*
                using (var context2 = new MeasurementContext())
                {


                    string TimeBucket = DateTime.Now.ToString("yyyy-MM");
                    // var cqlquery = session.Prepare("INSERT INTO samiv4.measurementdata(providerid, sensortag, timebucket, timestamp) values(?,?,?,?); ");
                    //var batch = new BatchStatement();

                    try
                    {
                        //fetch data from Measurement table
                        //var measurement = (from me in context2.Measurement where me.ID >= 1 && me.ID < 2 select me);

                        foreach (var row in measurement)
                        {
                            Console.WriteLine(" ID: " + row.ID + " ProviderID: " + row.ProviderID + " Object: " + row.Object + " Tag: " + row.Tag + " Timestamp: " + row.Timestamp + " Note: " + row.Note +
                                " Location: " + row.Location + " RowCreateTimestamp: " + row.RowCreatedTimestamp + " KeyId: " + row.KeyId);

                            batch.Add(cqlquery.Bind(row.ProviderID, row.Tag, TimeBucket, row.Timestamp));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                Console.ReadKey();

                using (var context = new DataContext())
                {

                    try
                    {
                        //fetch data from Data table
                        var data = (from da in context.Data where da.MeasurementID >= 0 && da.MeasurementID <= 1 select da);

                        foreach (var row in data)
                        {
                            Console.WriteLine("ID: " + row.MeasurementID + " Tag: " + row.Tag + " Value: " + row.Value + " LongValue: " + row.LongValue + " TextValue: " + row.TextValue +
                                " BinaryValue: " + row.BinaryValue + " XmlValue: " + row.XmlValue);

                            // batch2.Add(cqlquery2.Bind(row.Tag));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }*/
            }
            Console.ReadKey();

            session.Execute(batch);

         

            // Get name of a Cluster
            Console.WriteLine("The cluster's name is: " + cluster.Metadata.ClusterName);

            // Execute a query on a connection synchronously
            var rs = session.Execute("SELECT * FROM samiv4.measurementdata");

            

            // Iterate through the RowSet (read)
            foreach (var row in rs)
             {
                var value = row.GetValue<int>("providerid");
                var value2 = row.GetValue<string>("sensortag");
                Console.WriteLine(value + "  " + value2);
                 // Do something with the value
             }
             Console.ReadKey();
             
            
            
    }
    }
}
