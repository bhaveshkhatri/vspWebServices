namespace VspWS.Data
{
    using System;
    using System.Data.Entity;

    public class Falcon : DbContext
    {
        // Your context has been configured to use a 'Falcon' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'VspWS.Data.Falcon' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'Falcon' 
        // connection string in the application configuration file.
        public Falcon()
            : base("name=Falcon")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<IntegrationMessage> IntegrationMessages { get; set; }
    }

    public class IntegrationMessage
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string Body { get; set; }
        public DateTime? RequestStartedOn { get; set; }
        public DateTime? RequestCompletedOn { get; set; }
    }
}