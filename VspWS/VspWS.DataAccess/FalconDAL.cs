using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VspWS.Data;

namespace VspWS.DataAccess
{
    public class FalconDAL
    {
        private Falcon _context;

        public FalconDAL()
        {
            this._context = new Falcon();
        }

        public void AddIntegrationMessage(IntegrationMessage message)
        {
            this._context.IntegrationMessages.Add(message);
            this._context.SaveChanges();
        }
    }
}
