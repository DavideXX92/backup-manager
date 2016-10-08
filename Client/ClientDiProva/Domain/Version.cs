using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class Version
    {
        public Dir dirTree { get; set; }
        public int idVersion { get; set; }
        public DateTime dateCreation { get; set; }
        public DateTime dateClosed { get; set; }

        public Version()
        {
        }

        public Version(Dir dirTree)
        {
            this.dirTree = dirTree;
        }

        public Version(int version)
        {
            this.idVersion = version;
        }

        public Version(int version, Dir dirTree)
        {
            this.idVersion = version;
            this.dirTree = dirTree;
        }

        public Version(int version, DateTime dateCreation)
        {
            this.idVersion = version;
            this.dateCreation = dateCreation;
        }

        public Version(int version, DateTime dateCreation, DateTime dateClosed)
        {
            this.idVersion = version;
            this.dateCreation = dateCreation;
            this.dateClosed = dateClosed;
        }

    }
}
