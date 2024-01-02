using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLConverter
{



    public abstract class PartialBuilder<T> where T : new()
    {
        protected PartialBuilder()
        {
            this.machine = Runner();
            _Output = new();
        }
        public T output => this._Output;
        protected T _Output;
        public bool MoveNext(string s)
        {
            this._Text = s;
            return this.machine.MoveNext();
        }
        IEnumerator machine;

        protected abstract IEnumerator Runner();

        protected string _Text;
    }

}
