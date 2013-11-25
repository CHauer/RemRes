using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemResLib.Execution
{
    public class ExecutionSystem
    {
        /// <summary>
        /// The execution system object
        /// </summary>
        private static ExecutionSystem executionSystemObj;

        #region Constructor  

        /// <summary>
        /// Prevents a default instance of the <see cref="ExecutionSystem"/> class from being created.
        /// </summary>
        private ExecutionSystem()
        {

        }

        #endregion

        #region Singelton

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static ExecutionSystem GetInstance()
        {
            if(executionSystemObj == null)
            {
                executionSystemObj = new ExecutionSystem();
            }

            return executionSystemObj;
        }

        #endregion

    }
}
