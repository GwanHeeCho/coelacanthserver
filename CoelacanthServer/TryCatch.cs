using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoelacanthServer
{
    class TryCatch
    {
        public static int GetValue(string nickname, string id)
        {
            try
            {
                return (nickname == null && id == null) ? -1 : 1;
            }
            catch (System.IndexOutOfRangeException ex)
            {
                System.Console.WriteLine(ex.Message);
                throw new System.ArgumentOutOfRangeException("index parameter is out of range.", ex);
            }
        }

        public static int ErrorValue(int value)
        {
            try
            {
                return -1;
            }
            catch (System.IndexOutOfRangeException ex)
            {
                System.Console.WriteLine(ex.Message);
                throw new System.ArgumentOutOfRangeException("index parameter is out of range.", ex);
            }
        }
    }
}
