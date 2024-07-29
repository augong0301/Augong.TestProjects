using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.CSharp.Contract
{
	public interface IClassTest<T> where T : class, ITest
	{
		void DoTest();
	}
}
