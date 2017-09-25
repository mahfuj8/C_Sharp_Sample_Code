using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication6
{
    public class ModifiedCustomersQueryBO : System.Collections.CollectionBase
    {

        public ModifiedCustomersQueryBO()
        {
            base.InnerList.Clear();
        }

        public virtual void Add(string item)
        {
            base.InnerList.Add(item);
        }

        public virtual string this[int index]
        {
            get
            {
                return (string)(base.InnerList[index]);
            }
            set
            {

                base.InnerList[index] = value;
            }
        }

    }
}
