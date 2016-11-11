using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient {
    
    public interface Node {
        String name {get; set;}
        String path { get; set; }
        DateTime creationTime { get; set; }
        DateTime lastWriteTime { get; set; }
        String creationTimeToString { get; }
        String lastWriteTimeToString { get; }
        String ImgSrc { get; set; }
        String Lenght { get; set; }
    }
}
