using VspWS.Data;

namespace VspWS.DataAccess
{
    public class AlSysDAL
    {
        private AlSys _context;

        public AlSysDAL()
        {
            this._context = new AlSys();
        }
    }
}
