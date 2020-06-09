using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cptf
{
    public class CopyParameters
    {
        /// <summary>
        /// The name of the test data to copy
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The name of the project to copy to
        /// </summary>
        public string Project { get; internal set; }
        /// <summary>
        /// The root folder where the test data is located
        /// </summary>
        public string TestDataRepoRootDir { get; set; }
        /// <summary>
        /// The root folder where the destination folder is located
        /// </summary>
        public string DestinationRootDir { get; set; }

        public CopyParameters()
        { }
        public bool RootDirsExists()
        {
            if ((!Directory.Exists(this.DestinationRootDir)) &
                            (!Directory.Exists(this.TestDataRepoRootDir)))
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}
