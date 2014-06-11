using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;

namespace SolarApplication
{
    class ConnectionClass
    {
        public SolarDBEntities Entity { get; set; }

        public ConnectionClass()
        {
            Entity = new SolarDBEntities();
        }
    }
}
