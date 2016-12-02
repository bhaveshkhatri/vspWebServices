namespace VspWS.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class AlSys : DbContext
    {
        // Your context has been configured to use a 'AlSys' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'VspWS.Data.AlSys' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'AlSys' 
        // connection string in the application configuration file.
        public AlSys()
            : base("name=AlSys")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<EhrMessageTrackingInfo> EhrMessageTrackingInfos { get; set; }
    }

    public class EhrMessageTrackingInfo
    {
        public int Id { get; set; }
        public DateTime RequestReceived { get; set; }
        public DateTime RequestCompleted { get; set; }
        public DateTime ProcessStarted { get; set; }
        public DateTime ProcessCompleted { get; set; }
    }
}