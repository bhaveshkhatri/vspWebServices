namespace VspWS.Data
{
    using System;
    using System.Data.Entity;

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

        public AlSys(string connectionString)
            : base(connectionString)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<EhrMessageTrackingInfo> EhrMessageTrackingInfos { get; set; }
    }

    public class EhrMessageTrackingInfo
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public DateTime? RequestReceivedOn { get; set; }
        public DateTime? RequestCompletedOn { get; set; }
        public DateTime? ProcessStartedOn { get; set; }
        public DateTime? ProcessCompletedOn { get; set; }
    }
}